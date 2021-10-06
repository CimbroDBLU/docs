using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using dblu.Docs.Models;
using dblu.Portale.Plugin.Docs.Extensions;
using dblu.Portale.Plugin.Docs.ViewModels;
using dblu.Portale.Plugin.Docs.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NToastNotify;
using Microsoft.Extensions.Logging;
using MimeKit;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using dblu.Docs.Classi;
using System.Threading;
using System.Linq;
using dblu.Docs.Interfacce;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;
using dblu.Portale.Core.Infrastructure;
using dblu.Docs.Extensions;
using dblu.Portale.Plugin.Docs.Models;
using AspNetCore;
using BPMClient;
using dblu.Portale.Plugin.Docs.Class;

namespace dblu.Portale.Plugin.Documenti.Controllers
{
    [Authorize]
    public class ZipViewController : Controller
    {

        private readonly IToastNotification _toastNotification;
        private ZipService _zipsvc;
        private IConfiguration _config;
        private ISoggettiService _soggetti;
        private PrintService _printService;

        public ZipViewController(ZipService zipservice,
            IToastNotification toastNotification,
            IConfiguration config,
            ISoggettiService soggetti,PrintService printService)
        {
            _zipsvc = zipservice;
            _toastNotification = toastNotification;
            _config = config;
            _soggetti = soggetti;
            _printService = printService;
        }


        [HasPermission("50.1.3")]
        public ActionResult ZipTask()
        {
            return View();
        }

        [HasPermission("50.1.3")]
        public ActionResult Task_Read([DataSourceRequest] DataSourceRequest request, string Ruolo)
        {

            List<ZipViewModel> lista = _zipsvc.GetFileTask(Ruolo, 0, 1000);
            return Json(lista.ToDataSourceResult(request));
        }

        [AcceptVerbs("Post")]
        [HasPermission("50.1.3|50.1.4")]
        public async Task<ZipViewModel> CaricaDettaglio(string Id)
        {
            ZipViewModel m = await _zipsvc.GetZipViewModel(Id);

            return m;
        }

        [HasPermission("50.1.3|50.1.4")]
        public ActionResult ListaZipElementi([DataSourceRequest] DataSourceRequest request, string IdFascicolo, string IdAllegato)
        {
            List<EmailElementi> lista = _zipsvc.ListaElementiZip(IdFascicolo, IdAllegato);
            return Json(lista.ToDataSourceResult(request));
        }

        [AcceptVerbs("Post")]
        [HasPermission("50.1.3|50.1.4")]

        public async Task<ActionResult<bool>> ZipFileCompleto(string IdTask, string IdAllegato)
        {

            bool res = false;

            if (string.IsNullOrEmpty(IdAllegato) == false)
            {
                var all = _zipsvc._allMan.Get(IdAllegato);
                if (all != null)
                {


                    all.Stato = StatoAllegato.Chiuso;
                    //_mailService._context.SaveChanges();
                    res =  _zipsvc._allMan.Salva(all, false);

                    //-------- Memorizzo l'operazione----------------------
                    LogDoc log = new LogDoc()
                    {
                        Data = DateTime.Now,
                        IdOggetto = Guid.Parse(IdAllegato),
                        TipoOggetto = TipiOggetto.ALLEGATO,
                        Utente = User.Identity.Name,
                        Operazione = TipoOperazione.Chiuso
                    };
                    _zipsvc._logMan.Salva(log, true);
                    //-------- Memorizzo l'operazione----------------------

                }
            }

                if (string.IsNullOrEmpty(IdTask) == false)
                {
                    BPMTask tsk = new BPMTask();
                    Dictionary<string, BPMClient.VariableValue> fVars = await tsk.GetTaskFormVariables(_zipsvc._bpm._eng, IdTask);
                    BPMTaskDto tdto = tsk.Get(_zipsvc._bpm._eng, IdTask);
                    fVars.Add("_Annullato", new VariableValue { type = VariableType.Boolean, value = false });
                    res = tsk.SubmitTaskForm(_zipsvc._bpm._eng, IdTask, fVars);

                }

            if (res) {
                _toastNotification.AddSuccessToastMessage("Documento completato.");
            }
            else {
                _toastNotification.AddErrorToastMessage("Documento NON completato.");
            }

            return Json(res);
        }

        [AcceptVerbs("Post")]
        [HasPermission("50.1.3|50.1.4")]
        public IActionResult editDettaglioElemento(Guid IdElemento)
        {

            string NomeView = @"/Pages/Action/Partials/Form/EditElemento.cshtml";
            var e = _zipsvc._elmMan.Get(IdElemento, 0);
            if (e != null)
            {
                //var x = _mailService.MarcaAllegati(e);
                e.IdFascicoloNavigation = _zipsvc._fasMan.Get(e.IdFascicolo);

                if (!string.IsNullOrEmpty(e.TipoNavigation.ViewAttributi))
                {
                    NomeView = e.TipoNavigation.ViewAttributi;
                }
            }
            return PartialView(NomeView, e);
        }

        [AcceptVerbs("Post")]
        [HasPermission("50.1.3|50.1.4")]
        public async Task<ActionResult<bool>> AllegaAElementoFascicolo(
             string IdAllegato,
             string IdFascicolo,
             string IdElemento,
             string elencoFile,
             bool AllegaEmail,
             string Descrizione)
        {
            if (IdAllegato != null && IdFascicolo != null && IdElemento != null)
            {
                BPMDocsProcessInfo Info = _zipsvc.GetProcessInfo(TipiOggetto.ELEMENTO, AzioneOggetto.MODIFICA);
                bool fl = await _zipsvc.AllegaAElementoFascicolo(IdAllegato,
                    IdFascicolo,
                    IdElemento,
                    elencoFile,
                    AllegaEmail,
                    Descrizione,
                    User, 
                    Info,
                    null );

                if (fl)
                {
                    _toastNotification.AddSuccessToastMessage("File allegati.");
                    return Json(true);
                }
            }
            return Json(false);
        }

        public ActionResult GetSoggetto(string codice)
        {
            try
            {
                var soggetto = _soggetti.GetSoggetto(codice);
                return Json(soggetto);

            }
            catch (Exception ex)
            {
                return BadRequest();
            }

        }

        [AcceptVerbs("Post")]
        [HasPermission("50.1.3|50.1.4")]
        public async Task<bool> NotificaAssociazione(string CodiceSoggetto, string IdAllegato)
        {
            bool res = false;
            try
            {
                res = await _soggetti.NotificaAssociazione(User.Identity.Name, CodiceSoggetto, IdAllegato);
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage($"Notifica Associazione Soggetto: {ex.Message}");
            }
            return res;
        }

        [AcceptVerbs("Post")]
        [HasPermission("50.1.3|50.1.4")]

        public async Task<ActionResult<bool>> ZipFileAnnulla(string IdTask)  //, string IdAllegato
        {
            string IdAllegato = null;
            bool res = false;
            if (string.IsNullOrEmpty(IdTask) == false)
            {
                BPMClient.BPMVariable var = new BPMClient.BPMVariable();
                Dictionary<string, BPMClient.VariableValue> variables = var.GetAll(_zipsvc._bpm._eng, IdTask).Result;
                if (variables.ContainsKey("_IdAllegato"))
                {
                    IdAllegato = variables["_IdAllegato"].GetValue<string>();
                }
            }

            if (string.IsNullOrEmpty(IdAllegato) == false && string.IsNullOrEmpty(IdTask) == false)
            {
                try
                {


                var all = _zipsvc._allMan.Get(IdAllegato);
                if (all != null)
                {
                    all.Stato = StatoAllegato.Annullato;
                    res = _zipsvc._allMan.Salva(all, false);

                    //-------- Memorizzo l'operazione----------------------
                    LogDoc log = new LogDoc()
                    {
                        Data = DateTime.Now,
                        IdOggetto = Guid.Parse(IdAllegato),
                        TipoOggetto = TipiOggetto.ALLEGATO,
                        Utente = User.Identity.Name,
                        Operazione = TipoOperazione.Annullato
                    };
                    _zipsvc._logMan.Salva(log, true);
                    //-------- Memorizzo l'operazione----------------------

                }

                if (string.IsNullOrEmpty(IdTask) == false)
                {
                    BPMTask tsk = new BPMTask();
                    Dictionary<string, BPMClient.VariableValue> fVars = await tsk.GetTaskFormVariables(_zipsvc._bpm._eng, IdTask);
                    BPMTaskDto tdto = tsk.Get(_zipsvc._bpm._eng, IdTask);
                        if (fVars.ContainsKey("_Annulla"))
                        {
                            fVars["_Annulla"].value = "true"; 
                        }
                        else { 
                    fVars.Add("_Annulla", new VariableValue { type = VariableType.Boolean, value = true });
                        }
                    res = tsk.SubmitTaskForm(_zipsvc._bpm._eng, IdTask, fVars);
                    }
                }
                catch (Exception ex) {
                    _zipsvc._logger.LogError($"ZipFileAnnulla: {ex.Message}");
                    _toastNotification.AddErrorToastMessage("Documento NON annullato.");
                    res = false;
                }
            }
            if (res)
            {
                _toastNotification.AddSuccessToastMessage("Documento annullato.");
            }
            else
            {
                _toastNotification.AddErrorToastMessage("Documento NON annullato.");
            }

            return Json(res);
        }

        [HasPermission("50.1.3|50.1.4")]
        public async Task<ActionResult> SoggettoElementiAperti([DataSourceRequest] DataSourceRequest request, string CodiceSoggetto)
        {
            List<ISoggettoElementiAperti> lista = await _soggetti.GetElementiAperti(CodiceSoggetto);
            return Json(lista.ToDataSourceResult(request));
        }

        public async Task<ActionResult<Elementi>> CreaElementoFascicoloAsync(
             string IdAllegato,
             string IdFascicolo,
             string IdElemento,
             string Categoria,
             string TipoElemento,
             string CodiceSoggetto,
             string NomeSoggetto,
             string ElencoFile,
             bool AllegaZip,
             string Descrizione)
        {
            try
            {
                if (Categoria is null)
                {
                    _toastNotification.AddErrorToastMessage("Scegli una categoria per il fascicolo!");
                    return BadRequest();
                }
                if (TipoElemento is null)
                {
                    _toastNotification.AddErrorToastMessage("Scegli il tipo elemento!");
                    return BadRequest();
                }

                var e = await _zipsvc.CreaElementoFascicoloAsync(IdAllegato, IdFascicolo, IdElemento, Categoria, TipoElemento, CodiceSoggetto, NomeSoggetto, ElencoFile, AllegaZip, Descrizione, User);
                if (e is null)
                {
                    _toastNotification.AddErrorToastMessage("Errore nella creazione del fascicolo!");
                    return BadRequest();
                }
                _toastNotification.AddSuccessToastMessage("Elemento creato correttamente!");
                return Ok(e);
            }
            catch (Exception)
            {
                _toastNotification.AddErrorToastMessage("Errore nella creazione del fascicolo!");
                return BadRequest();
            }

 

        }    
        
        
        [HasPermission("50.1.3|50.1.4")]
        [HttpGet]
        public ActionResult StampaRiepilogo(string IdAllegato)
        {
            Allegati a = new Allegati();
            a.Id = Guid.Parse(IdAllegato);

            return View("PreviewRiepilogo", a);
        }


        [AcceptVerbs("Post")]
        [HasPermission("50.1.3|50.1.4")]
        public async Task<ActionResult<RisultatoAzione>> CancellaElemento(string IdElemento, short Revisione)
        {
            RisultatoAzione res = new RisultatoAzione();
            if (IdElemento != null && ModelState.IsValid)
            {
                res = await Task.FromResult(_zipsvc.CancellaElemento(IdElemento, Revisione, User));

                if (res.Successo)
                {
                    _toastNotification.AddSuccessToastMessage("Elemento cancellato.");
                }
                return Ok(res);
            }
            else
            {
                res.Successo = false;
                res.Messaggio = "Elemento non valido.";
                _toastNotification.AddErrorToastMessage(res.Messaggio);
            }
            return BadRequest(res);
        }

        #region ZipInArrivo
        
        [HasPermission("50.1.4")]
        public ActionResult ZipInArrivo(string Ruolo, string Tipo)
        {
            return View(new ZipInArrivoViewModel
                {
                    Ruolo = Ruolo,
                    TipoAll = Tipo
                }
            );
        }


        [AcceptVerbs("Post")]
        [HasPermission("50.1.4")]
        public async Task<ZipViewModel> InArrivo_CaricaDettaglio(string Id)
        {
            ZipViewModel m = await _zipsvc.GetZipViewModel("", Id);
            //_toastNotification.AddSuccessToastMessage("dettaglio caricato");
            return m;
        }

        [HasPermission("50.1.4")]
        public ActionResult InArrivo_Read([DataSourceRequest] DataSourceRequest request, string Tipo, string Origine = "")
        {

            IEnumerable<Allegati> lista = _zipsvc._allMan.GetZipInArrivo(Tipo, Origine);
            return Json(lista.ToDataSourceResult(request));
        }


        [AcceptVerbs("Post")]
        [HasPermission("50.1.4")]
        public ActionResult<bool> InArrivo_Cancella(string Id)
        {
            if (Id != null && ModelState.IsValid)
            {
                // if (_mailService._allMan.Cancella(Id)) {
                var all = _zipsvc._allMan.Get(Id);
                if (all != null)
                {
                    all.Stato = StatoAllegato.Annullato;
                    _zipsvc._allMan.Salva(all, false);

                    //-------- Memorizzo l'operazione----------------------
                    LogDoc log = new LogDoc()
                    {
                        Data = DateTime.Now,
                        IdOggetto = Guid.Parse(Id),
                        TipoOggetto = TipiOggetto.ALLEGATO,
                        Utente = User.Identity.Name,
                        Operazione = TipoOperazione.Cancellato
                    };
                    _zipsvc._logMan.Salva(log, true);
                    //-------- Memorizzo l'operazione----------------------

                    _toastNotification.AddSuccessToastMessage("Documento eliminato.");
                    return Json(true);
                }
            }
            return Json(false);
        }


        [AcceptVerbs("Post")]
        [HasPermission("50.1.4")]
        public async Task<ActionResult<bool>> InArrivo_Sposta(
           string IdAllegato,
           string Cartella)
        {
            RisultatoAzione r = new RisultatoAzione();
            if (!string.IsNullOrEmpty(IdAllegato) && Cartella!=null)
            {
                r = await Task.FromResult(_zipsvc.SpostaZip(
                    IdAllegato,
                    Cartella,
                    User));
            }
            else
            {
                r.Successo = false;
                r.Messaggio = "Cartella non valida";
            }
            if (r.Successo)
            {
                _toastNotification.AddSuccessToastMessage("Documento spostato.");
            }
            else
            {
                _toastNotification.AddErrorToastMessage($"Spostamento fallito: {r.Messaggio}");
            }
            return Json(r.Successo);
        }

        [AcceptVerbs("Post")]
        [HasPermission("50.1.3")]
        public async Task<ActionResult<bool>> StampaRiepilogoServer(string IdAllegato,string Printer)
        {
            try
            {
                string TempFile = Path.Combine(Path.GetTempPath(), IdAllegato.ToString() + ".pdf");
                using (FileStream FS = new FileStream(TempFile, FileMode.Create))
                {
                    MemoryStream MS = await _zipsvc.GetPdfRiepilogo(IdAllegato);
                    MS.WriteTo(FS);
                    FS.Close();
                }
                if (await _printService.AddJob(User.Identity.Name, TempFile,Printer) == 0)
                {
                    System.IO.File.Delete(TempFile);
                    _toastNotification.AddSuccessToastMessage("Documento inviato alla stampante");

                    if (!string.IsNullOrEmpty(IdAllegato))

                        _zipsvc._logMan.Salva(new LogDoc()
                        {
                            Data = DateTime.Now,
                            IdOggetto = Guid.Parse(IdAllegato),
                            TipoOggetto = TipiOggetto.ALLEGATO,
                            Utente = User.Identity.Name,
                            Operazione = TipoOperazione.Stampato
                        }, true);
                    foreach (Elementi e in _zipsvc._elmMan.GetElementiDaAllegato(Guid.Parse(IdAllegato)))
                    {
                        _zipsvc._logMan.Salva(new LogDoc()
                        {
                            Data = DateTime.Now,
                            IdOggetto = e.Id,
                            TipoOggetto = TipiOggetto.ELEMENTO,
                            Utente = User.Identity.Name,
                            Operazione = TipoOperazione.Stampato
                        }, true);
                    }
                }
                else _toastNotification.AddErrorToastMessage("Impossibile accodare il documento alla stampante");
            }
            catch (Exception ex)
            {
                return await Task.FromResult(false);
            }
            return await Task.FromResult(true);
        }



        [AcceptVerbs("Post")]
        [HasPermission("50.1.4")]
        public async Task<ActionResult<bool>> LogRiepilogo(
            string IdAllegato)
        {
            try
            {
                if (!string.IsNullOrEmpty(IdAllegato))

                    _zipsvc._logMan.Salva(new LogDoc()
                    {
                        Data = DateTime.Now,
                        IdOggetto = Guid.Parse(IdAllegato),
                        TipoOggetto = TipiOggetto.ALLEGATO,
                        Utente = User.Identity.Name,
                        Operazione = TipoOperazione.Stampato
                    }, true);
                foreach (Elementi e in _zipsvc._elmMan.GetElementiDaAllegato(Guid.Parse(IdAllegato)))
                {
                    _zipsvc._logMan.Salva(new LogDoc()
                    {
                        Data = DateTime.Now,
                        IdOggetto = e.Id,
                        TipoOggetto = TipiOggetto.ELEMENTO,
                        Utente = User.Identity.Name,
                        Operazione = TipoOperazione.Stampato
                    }, true);
                }
            }
            catch (Exception ex)
            {
                return await Task.FromResult(false);
            }
            return await Task.FromResult(true);
        }

        [AcceptVerbs("Post")]
        [HasPermission("50.1.4")]
        public async Task<ActionResult<bool>> InArrivo_Stampato(
            string IdAllegato,
            string IdElemento)
        {
            try
            {
                if (!string.IsNullOrEmpty(IdAllegato))
                    _zipsvc._logMan.Salva(new LogDoc()
                    {
                        Data = DateTime.Now,
                        IdOggetto = Guid.Parse(IdAllegato),
                        TipoOggetto = TipiOggetto.ALLEGATO,
                        Utente = User.Identity.Name,
                        Operazione = TipoOperazione.Stampato
                    }, true);

                if (!string.IsNullOrEmpty(IdElemento))
                    _zipsvc._logMan.Salva(new LogDoc()
                    {
                        Data = DateTime.Now,
                        IdOggetto = Guid.Parse(IdElemento),
                        TipoOggetto = TipiOggetto.ELEMENTO,
                        Utente = User.Identity.Name,
                        Operazione = TipoOperazione.Stampato
                    }, true);
            }
            catch (Exception ex)
            {
                _zipsvc._logger.LogError($"InArrivo_Stampato: {ex.Message}");
                return await Task.FromResult(false);
            }
            return await Task.FromResult(true);
        }

        [AcceptVerbs("Post")]
        [HasPermission("50.1.4")]
        public async Task<ActionResult<bool>> InArrivo_Stampa(string pdf)
        {
            try
            {
                PdfEditAction pdfact = new PdfEditAction();
                pdfact = JsonConvert.DeserializeObject<PdfEditAction>(pdf);

                // string TempFile =Path.Combine(Path.GetTempPath(), IdAllegato.ToString() + ".pdf");
                //pdfact.TempFolder = Path.Combine(_hostingEnvironment.WebRootPath, "_tmp");
                pdfact.TempFolder = Path.GetTempPath();
                string TempFile = Path.Combine(pdfact.TempFolder, $"{pdfact.IdAllegato}.stp.pdf");
                using (FileStream FS = new FileStream(TempFile, FileMode.Create))
                {
                    MemoryStream MS = await _zipsvc.GetPdfCompletoAsync(pdfact.IdAllegato, pdfact.IdElemento, false);
                    MS.WriteTo(FS);
                    FS.Close(); 
                }
                if (await _printService.AddJob(User.Identity.Name, TempFile, pdfact.Printer) ==0)
                {
                    System.IO.File.Delete(TempFile);
                    _toastNotification.AddSuccessToastMessage("Documento inviato alla stampante");

                    //Invio di conferma
                    if (!string.IsNullOrEmpty(pdfact.IdAllegato))
                        _zipsvc._logMan.Salva(new LogDoc()
                        {
                            Data = DateTime.Now,
                            IdOggetto = Guid.Parse(pdfact.IdAllegato),
                            TipoOggetto = TipiOggetto.ALLEGATO,
                            Utente = User.Identity.Name,
                            Operazione = TipoOperazione.Stampato
                        }, true);

                    if (!string.IsNullOrEmpty(pdfact.IdElemento))
                        _zipsvc._logMan.Salva(new LogDoc()
                        {
                            Data = DateTime.Now,
                            IdOggetto = Guid.Parse(pdfact.IdElemento),
                            TipoOggetto = TipiOggetto.ELEMENTO,
                            Utente = User.Identity.Name,
                            Operazione = TipoOperazione.Stampato
                        }, true);
                }
                else
                    _toastNotification.AddErrorToastMessage("Impossibile accodare il documento alla stampante");
            }
            catch (Exception ex)
            {
                _zipsvc._logger.LogError($"InArrivo_Stampa: {ex.Message}");
                return await Task.FromResult(false);
            }
            return await Task.FromResult(true);
        }


        [HttpGet]
        [HasPermission("50.1.4")]
        public async Task<FileResult> ApriFile(string IdAllegato, string NomeFile)
        {

            MemoryStream ff = await _zipsvc.GetFileFromZipAsync(IdAllegato, NomeFile);
            string t = _zipsvc._allMan.GetContentType(NomeFile);
            if (string.IsNullOrEmpty(t))
            {
                t = "text/csv";
                NomeFile = NomeFile + ".txt";
            }
            return File(ff, t, NomeFile);
        }


        /// <summary>
        /// Get a file included into a ZIP as a stream in GET
        /// </summary>
        /// <param name="IdAllegato">Attachment in with file is container</param>
        /// <param name="NomeFile">Name of the file to get</param>
        /// <returns>
        /// The stream required
        /// </returns>
        [HttpGet]
        [HasPermission("50.1.3")]
        public async Task<ActionResult> OpenFile(string IdAllegato, string NomeFile)
        {

            MemoryStream ff = new MemoryStream();
            try
            {
                ff = await _zipsvc.GetFileFromZipAsync(IdAllegato, NomeFile);
                string t = _zipsvc._allMan.GetContentType(NomeFile);
                if (string.IsNullOrEmpty(t))
                    t = "application/octet";
                return File(ff, t);
            }
            catch (Exception e)
            {

            }
            return NotFound();
        }

        #endregion


        #region   Zip Processati

        [HasPermission("50.1.4")]
        public ActionResult ZipProcessati(string Ruolo, string Tipo)
        {
            return View(new ZipProcessatiViewModel
            {
                Ruolo = Ruolo,
                TipoAll = Tipo
            }
            );
            //return View();
        }


        [HasPermission("50.1.4")]
        public ActionResult Processati_Read([DataSourceRequest] DataSourceRequest request, string Tipo, string Origine = "")
        {

            IEnumerable<Allegati> lista = _zipsvc._allMan.GetEmailProcessate(Tipo, Origine);
            return Json(lista.ToDataSourceResult(request));
        }

        [AcceptVerbs("Post")]
        [HasPermission("50.1.4")]
        public async Task<ActionResult<RisultatoAzione>> Processati_Riapri(
             string IdAllegato)
        {
            RisultatoAzione r = new RisultatoAzione();
            if (!string.IsNullOrEmpty(IdAllegato))
            {
                r = await Task.FromResult(_zipsvc.RiapriZip(IdAllegato, User));
            }
            else
            {
                r.Successo = false;
                r.Messaggio = "File non valido";
            }
            if (r.Successo)
            {
                _toastNotification.AddSuccessToastMessage("Documento riaperto.");
            }
            else
            {
                _toastNotification.AddErrorToastMessage($"Riapertura fallita: {r.Messaggio}");
            }
            return Ok(r);
        }

        [AcceptVerbs("Post")]
        [HasPermission("50.1.4")]
        public async Task<ActionResult<RisultatoAzione>> Processati_Cancella(string IdAllegato)
        {
            RisultatoAzione res = new RisultatoAzione();
            if (IdAllegato != null && ModelState.IsValid)
            {
                res = await _zipsvc.CancellaZip(IdAllegato, User);
                _toastNotification.AddSuccessToastMessage("File eliminato.");
                return Ok(res);
            }
            else
            {
                res.Successo = false;
                res.Messaggio = "File non valido.";
                _toastNotification.AddErrorToastMessage(res.Messaggio);

            }
            return BadRequest(res);
        }


        #endregion
    }


}


