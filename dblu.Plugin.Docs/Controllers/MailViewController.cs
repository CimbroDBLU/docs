using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using dblu.Docs.Models;
using dblu.Portale.Plugin.Docs.Data;
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
//using Telerik.Web.PDF;
using dblu.Docs.Classi;
using System.Threading;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using dblu.Docs.Interfacce;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;
using dblu.Portale.Core.Infrastructure;
using dblu.Docs.Extensions;
using dblu.Portale.Plugin.Docs.Models;
using System.Web;
using Microsoft.AspNetCore.Hosting;
using dblu.Portale.Plugin.Docs.Class;

namespace dblu.Portale.Plugin.Documenti.Controllers
{
    [Authorize]
    public class  MailViewController : Controller
    {

        private readonly IToastNotification _toastNotification;
        private MailService _mailService;
        private IConfiguration _config;
        private ISoggettiService _soggetti;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private PrintService _printService;
        private PdfEditService _pdfsvc;

        public MailViewController(MailService mailservice,
            IToastNotification toastNotification,
            IConfiguration config ,
            ISoggettiService soggetti,
            IWebHostEnvironment hostingEnvironment, PrintService printService ,
             PdfEditService pdfsvc)
        {
            _mailService = mailservice;
            _toastNotification = toastNotification;
            _config = config;
            _soggetti = soggetti;
            _hostingEnvironment = hostingEnvironment;
            _printService = printService;
            _pdfsvc = pdfsvc;
        }

       
        [HasPermission("50.1.3")]
        public ActionResult GetEmailAttachments([DataSourceRequest] DataSourceRequest request)
        {
            IList<EmailAttachments> l = new List<EmailAttachments>();
            try
            {
                MailViewModel mv = (MailViewModel)TempData["email"];
                foreach (var attachment in mv.Messaggio.Allegati())
                {
                    var fileName = "";
                    if (attachment is MessagePart)
                    {
                        fileName = attachment.ContentDisposition?.FileName;
                        var rfc822 = (MessagePart)attachment;

                        if (string.IsNullOrEmpty(fileName))
                            fileName = "email-allegata.eml";

                        //using (var stream = File.Create(fileName))
                        //    rfc822.Message.WriteTo(stream);
                    }
                    else
                    {
                        var part = (MimePart)attachment;
                        fileName = part.FileName;

                        //using (var stream = File.Create(fileName))
                        //    part.Content.DecodeTo(stream);
                    }
                    var a = new EmailAttachments { Id = attachment.ContentId, NomeFile = fileName, Valido = false };
                }
            }
            catch (Exception ex)
            {
                _mailService._logger.LogError($" GetEmailAttachments : {ex.Message}");
            }
            return Json(l.ToDataSourceResult(request));
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
             bool AllegaEmail, 
             string Descrizione) {
            try
            {
                if(Categoria is null)
                {
                    _toastNotification.AddErrorToastMessage("Scegli una categoria per il fascicolo!");
                    return BadRequest();
                }
                if (TipoElemento is null)
                {
                    _toastNotification.AddErrorToastMessage("Scegli il tipo elemento!");
                    return BadRequest();
                }

                var e = await _mailService.CreaElementoFascicoloAsync(IdAllegato,IdFascicolo, IdElemento, Categoria, TipoElemento, CodiceSoggetto, NomeSoggetto,  ElencoFile, AllegaEmail, Descrizione, User);
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


        //IdFascicolo: $("#IdFascicolo").val(),
        //        IdAllegato: $("#IdAllegato").val(),
        //        Categoria : combobox.value($("#value").val()),
        //        CodiceCliente : $("#CodCliente").val(),
        //        elencoFile: items,
        //        AllegaEmail: m,
        //        Descrizione : des
        public async Task<ActionResult<Elementi>> CreaElementoAsync(string IdFascicolo,  string IdAllegato, string TipoElemento, string CodiceSoggetto, string ElencoFile, bool AllegaEmail, string Descrizione)
        {
            try
            {
                if (TipoElemento is null)
                {
                    _toastNotification.AddErrorToastMessage("Scegli il tipo di elemento!");
                    return BadRequest();
                }

                var f = await _mailService.CreaElementoAsync(IdFascicolo, IdAllegato,  TipoElemento, CodiceSoggetto,  ElencoFile,  AllegaEmail,  Descrizione, User);
                if (f is null)
                {
                    _toastNotification.AddErrorToastMessage("Errore nella creazione dell'elemento!");
                    return BadRequest();
                }
                _toastNotification.AddSuccessToastMessage("Elemento creato correttamente!");
                return Ok(f);
            }
            catch (Exception)
            {
                _toastNotification.AddErrorToastMessage("Errore nella creazione dell'elemento!");
                return BadRequest();
            }

        }


        public async Task<ActionResult<Elementi>> DuplicaElementoAsync(string IdAllegato,
             string IdFascicolo,
             string IdElemento,
             string TipoElemento,
             string CodiceSoggetto,
             string NomeSoggetto,
             string ElencoFile,
             bool AllegaEmail,
             string Descrizione)
        {
            try
            {
                if (TipoElemento is null)
                {
                    _toastNotification.AddErrorToastMessage("Scegli il tipo di elemento!");
                    return BadRequest();
                }

                var f = await _mailService.DuplicaElementoAsync(IdAllegato, IdFascicolo, IdElemento, TipoElemento, CodiceSoggetto, NomeSoggetto, ElencoFile, AllegaEmail, Descrizione, User);
                if (f is null)
                {
                    _toastNotification.AddErrorToastMessage("Errore nella duplicazione dell'elemento!");
                    return BadRequest();
                }
                _toastNotification.AddSuccessToastMessage("Elemento duplicato correttamente!");
                return Ok(f);
            }
            catch (Exception)
            {
                _toastNotification.AddErrorToastMessage("Errore nella duplicazione dell'elemento!");
                return BadRequest();
            }

        }



        [HttpGet]
        [HasPermission("50.1.3")]
        public async Task<FileResult> ApriFile(string IdAllegato, string NomeFile)
        {

            MemoryStream ff = await _mailService.GetFileAsync(IdAllegato, NomeFile);
            string t = _mailService._allMan.GetContentType(NomeFile);
            if (string.IsNullOrEmpty(t)) {
                t = "text/csv";
                NomeFile = NomeFile + ".txt";
            }
            return File(ff, t, NomeFile);
        }

        [HttpGet]
        [HasPermission("50.1.2|50.1.3")]
        public async Task<FileResult> DownloadFile(string IdAllegato, string NomeFile)
        {
            MemoryStream ff = await _mailService._allMan.GetFileAsync(IdAllegato);
            return File(ff, _mailService._allMan.GetContentType(NomeFile), NomeFile);
        }

        private async Task<MemoryStream> GetFileWrapper(string IdAllegato, string NomeFile)
        {
            MemoryStream ff = await _mailService.GetFileAsync(IdAllegato, NomeFile);
            return ff;
        }

        [HttpGet]
        [HasPermission("50.1.3")]
        public IActionResult GetPdf([FromQuery]string IdAllegato, [FromQuery] string NomeFile)
        {
            if (IdAllegato is null || NomeFile is null)
            {
                _toastNotification.AddErrorToastMessage("Error in LoadPDF!");
                return BadRequest();
            }
            MemoryStream ff = _mailService.GetPdfAsync(IdAllegato, NomeFile).Result;  //GetFileWrapper(IdAllegato, NomeFile).Result;
          
            return new FileStreamResult(ff, "application/pdf");
        }

        [HttpGet]
        [HasPermission("50.1.3")]
        public async Task<IActionResult> GetPdfCompleto([FromQuery]string IdAllegato, [FromQuery]string IdElemento)
        {
            try
            {
                if (IdAllegato is null)
                {
                    _toastNotification.AddErrorToastMessage("Error in LoadPDF!");
                    return BadRequest();
                }
                MemoryStream ff = await _mailService.GetPdfCompletoAsync(IdAllegato, IdElemento, false);  //GetFileWrapper(IdAllegato, NomeFile).Result;
                ff.Position = 0;
                return new FileStreamResult(ff, "application/pdf");


            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage("Errore nella lettura della mail.");
                //return null;
                //return BadRequest(ex);
            }

            return null;

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


        [HasPermission("50.1.3")]
        public IActionResult InArrivo(string Ruolo)
        {

            return View(
                 new inArrivoView
                 {
                     Ruolo = Ruolo
                 }
                );


        }

        public ActionResult<int> InArrivo_Count([DataSourceRequest] DataSourceRequest request, string Tipo, string NomeServer= "")
        {
            IEnumerable<Allegati> lista = _mailService._allMan.GetEmailInArrivo(Tipo, NomeServer) ;
            return Json(lista.ToDataSourceResult(request));
        }

        [HasPermission("50.1.3")]
        public ActionResult InArrivo_Read([DataSourceRequest] DataSourceRequest request, string Tipo, string NomeServer="")
        {

            IEnumerable<Allegati> lista = _mailService._allMan.GetEmailInArrivo(Tipo, NomeServer);
            return Json(lista.ToDataSourceResult(request));
    }


        //[AcceptVerbs("Post")]
        //public ActionResult InArrivo_Destroy([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]Allegati a)
        //{
        //    if (a != null && ModelState.IsValid)
        //    {
        //        _mailService._allMan.Cancella(a.Id);
        //    }

        //    return Json(new[] { a }.ToDataSourceResult(request, ModelState));
        //}
        //[AcceptVerbs("Post")]
        //public ActionResult InArrivo_Destroy([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<Allegati> la)
        //{
        //    if (la.Any())
        //    {
        //        foreach (var a in la)
        //        {
        //            _mailService._allMan.Cancella(a.Id);
        //        }
        //    }

        //    return Json(la.ToDataSourceResult(request, ModelState));
        //}



        [AcceptVerbs("Get")]
        [HasPermission("50.1.3")]
        public IActionResult InArrivoApri(Guid Id)
        {
            return View("MailContent",

                new MailVM
                {
                    isInsideTask = false,
                    id = Id
                }
                );  
        }




        [AcceptVerbs("Post")]
        [HasPermission("50.1.3")]
        public ActionResult<bool> InArrivo_Cancella(string Id)
        {
            if (Id != null && ModelState.IsValid)
            {
                // if (_mailService._allMan.Cancella(Id)) {
                var all = _mailService._allMan.Get(Id);
                if (all != null) { 
                    all.Stato = StatoAllegato.Annullato;
                    _mailService._allMan.Salva(all, false);
                    
                    //-------- Memorizzo l'operazione----------------------
                    LogDoc log = new LogDoc()
                    {
                        Data = DateTime.Now,
                        IdOggetto  = Guid.Parse(Id),
                        TipoOggetto = TipiOggetto.ALLEGATO,
                        Utente = User.Identity.Name,
                        Operazione = TipoOperazione.Cancellato
                    };
                    _mailService._logMan.Salva(log, true);
                    //-------- Memorizzo l'operazione----------------------

                    _toastNotification.AddSuccessToastMessage("Email eliminata.");
                    return Json(true);
                }
            }
            return Json(false);
        }

        [AcceptVerbs("Post")]
        [HasPermission("50.1.3")]
		
		public ActionResult<bool> InArrivo_Completa(string Id)
        {
            if (Id != null)
            {
                var all = _mailService._allMan.Get(Id);
                if (all!=null)
                    {
                    all.Stato = StatoAllegato.Chiuso;
                    //_mailService._context.SaveChanges();
                    _mailService._allMan.Salva(all,false);

                    //-------- Memorizzo l'operazione----------------------
                    LogDoc log = new LogDoc()
                    {
                        Data = DateTime.Now,
                        IdOggetto = Guid.Parse(Id),
                        TipoOggetto = TipiOggetto.ALLEGATO,
                        Utente = User.Identity.Name,
                        Operazione = TipoOperazione.Chiuso
                    };
                    _mailService._logMan.Salva(log, true);
                    //-------- Memorizzo l'operazione----------------------

                    _mailService.PulisciFileTemp(Id);

                    
                    _toastNotification.AddSuccessToastMessage("Email processata.");
                    return Json(true);
                }
            }
            return Json(false);
        }

        [AcceptVerbs("Post")]
        [HasPermission("50.1.3")]
        public async Task<LiteMailViewModel> InArrivoCaricaDettaglio(string Id)
        {
            LiteMailViewModel m = new LiteMailViewModel();
            m.IdAllegato = Id;

            MailViewModel mvModel = await _mailService.GetMailViewModel(Guid.Parse(Id));
            m.TestoEmail = mvModel.Messaggio.TextBody;
            m.FileAllegati = mvModel.FileAllegati;
            m.CodiceSoggetto = mvModel.CodiceSoggetto;
            if (mvModel.Soggetto == null)
            {
            m.NomeSoggetto = mvModel.NomeSoggetto;
            }
            else
            {
                m.NomeSoggetto = mvModel.Soggetto.Nome;
            }
         
            m.IdFascicolo = mvModel.IdFascicolo.ToString();
            m.IdElemento = mvModel.IdElemento.ToString();
            if (mvModel.Fascicolo == null)
            {
                m.DescrizioneFascicolo = "";
            }
            else
            {
                m.DescrizioneFascicolo = mvModel.Fascicolo.Descrizione;
            }

            if (mvModel.Elemento == null)
            {
                m.DescrizioneElemento = "";
                m.Stato = StatoElemento.Attivo;
            }
            else {
                m.DescrizioneElemento = mvModel.Elemento.Descrizione;
                m.Stato = mvModel.Elemento.Stato;
            }
            m.ListaTipiElementi = mvModel.ListaTipiElementi;
            //m.ListaEmailElementi =  mvModel.ListaEmailElementi;
            //_toastNotification.AddSuccessToastMessage("dettaglio caricato");
            return m;
        }


        [AcceptVerbs("Post")]
        [HasPermission("50.1.3")]
        public List<TipiElementi> InArrivoSelectServer(string NomeServer)
        {
            List<string> ListaRuoliServ = _mailService.getRuoli(User.Claims,NomeServer);
            List<TipiElementi> ListaTipiElementiServ = _mailService._elmMan.GetAllTipiElementi(ListaRuoliServ);
            
            return ListaTipiElementiServ;
        }



        [AcceptVerbs("Post")]
        [HasPermission("50.1.3")]
        public IActionResult editDettaglioElemento(Guid IdElemento)
        {
            
            string NomeView = @"/Pages/Action/Partials/Form/EditElemento.cshtml";
            var e = _mailService._elmMan.Get(IdElemento, 0);
            if (e != null) {
                //var x = _mailService.MarcaAllegati(e);
                e.IdFascicoloNavigation = _mailService._fasMan.Get(e.IdFascicolo);
                
                if (! string.IsNullOrEmpty(e.TipoNavigation.ViewAttributi)){
                    NomeView = e.TipoNavigation.ViewAttributi;
                }
               // HttpContext.Session.SetString("editDettaglioElemento", JsonConvert.SerializeObject(e));
            }
            return PartialView(NomeView, e);
        }

        [AcceptVerbs("Post")]
        [HasPermission("50.1.3")]
        public IActionResult AllegaAElemento(AllegaAViewModel  aa )
        {
            return PartialView("AllegaA", aa);
        }


        /*
         
                IdAllegato: $("#allegaIdAllegato").val(),
                IdFascicolo: $("#allegaIdFascicolo").val(),
                IdElemento: $("#allegaIdElemento").val(),
                elencoFile: items,
                AllegaEmail: $("#allegatestoemail").val(),
                Descrizione: $("#allegaDescrizione").val(),
         */
        [AcceptVerbs("Post")]
        [HasPermission("50.1.3")]
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
                BPMDocsProcessInfo Info = _mailService.GetProcessInfo(TipiOggetto.ELEMENTO, AzioneOggetto.MODIFICA);
                bool fl = await _mailService.AllegaAElementoFascicolo(IdAllegato,
                    IdFascicolo,
                    IdElemento,
                    elencoFile,
                    AllegaEmail,
                    Descrizione,
                    User,
                    Info,
                    null);
                
                    if (fl){
                    _toastNotification.AddSuccessToastMessage("File allegati.");
                    return Json(true);
                }
            }
            return Json(false);
        }

        [AcceptVerbs("Post")]
        [HasPermission("50.1.3")]
        public async Task<ActionResult<RisultatoAzione>> InArrivo_Inoltra(
            string IdAllegato,
            string email,
            bool chiudi)
        {
            RisultatoAzione r = new RisultatoAzione();
            if (!string.IsNullOrEmpty(IdAllegato) && !string.IsNullOrEmpty(email))
            {
                r = await _mailService.InoltraEmail(
                    IdAllegato,
                    email,
                    chiudi ,
                    User);
            }
            else {
                r.Successo = false;
                r.Messaggio = "Mail o indirizzo non validi";
            }
            if (r.Successo)
            {

                _toastNotification.AddSuccessToastMessage("Email inoltrata.");
            }
            else { 
                _toastNotification.AddErrorToastMessage($"Inoltro fallito: {r.Messaggio}");
            }
            return Json(r.Successo);
        }

        [AcceptVerbs("Post")]
        [HasPermission("50.1.3")]
        public async Task<ActionResult<bool>> InArrivo_Rispondi(
           string IdAllegato,
           string NomeServer,
           string to,
           string cc,
           string oggetto,
           string testo,
           bool allegaEmail,
           bool chiudiEmail)

        {
            RisultatoAzione r = new RisultatoAzione();
            if (!string.IsNullOrEmpty(IdAllegato))
            {
                r = await _mailService.RispondiEmail(
                    IdAllegato,
                    NomeServer,
                    to,
                    cc,
                    oggetto,
                    testo,
                    allegaEmail,
                     chiudiEmail ,
                    User);
            }
            else
            {
                r.Successo = false;
                r.Messaggio = "Mail o indirizzo non validi";
            }
            if (r.Successo)
            {

                _toastNotification.AddSuccessToastMessage("Email inviata.");
            }
            else
            {
                _toastNotification.AddErrorToastMessage($"Invio fallito: {r.Messaggio}");
            }
            return Json(r.Successo);
        }

        [AcceptVerbs("Post")]
        [HasPermission("50.1.3")]
        public async Task<ActionResult<bool>> InArrivo_Sposta(
            string IdAllegato,
            string server)
        {
            RisultatoAzione r = new RisultatoAzione();
            if (!string.IsNullOrEmpty(IdAllegato) && !string.IsNullOrEmpty(server))
            {
                r =  await Task.FromResult( _mailService.SpostaEmail(
                    IdAllegato,
                    server,
                    User));

            }
            else
            {
                r.Successo = false;
                r.Messaggio = "Mail o casella server non validi";
            }
            if (r.Successo)
            {
                _toastNotification.AddSuccessToastMessage("Email spostata.");
            }
            else
            {
                _toastNotification.AddErrorToastMessage($"Spostamento fallito: {r.Messaggio}");
            }
            return Json(r.Successo);
        }



        [AcceptVerbs("Post")]
        [HasPermission("50.1.3")]
        public async Task<ActionResult<bool>> StampaRiepilogoServer(
            string IdAllegato,string Printer)
        {
            try
            {
                string TempFile = Path.Combine(Path.GetTempPath(), IdAllegato.ToString() + ".pdf");
                using (FileStream FS = new FileStream(TempFile, FileMode.Create))
                {
                    MemoryStream MS = await _mailService.GetPdfRiepilogo(IdAllegato);
                    MS.WriteTo(FS);
                    FS.Close();
                }
                if (await _printService.AddJob(User.Identity.Name,TempFile, Printer) == 0)
                {
                    System.IO.File.Delete(TempFile);
                    _toastNotification.AddSuccessToastMessage("Documento inviato alla stampante");

                    if (!string.IsNullOrEmpty(IdAllegato))

                        _mailService._logMan.Salva(new LogDoc()
                        {
                            Data = DateTime.Now,
                            IdOggetto = Guid.Parse(IdAllegato),
                            TipoOggetto = TipiOggetto.ALLEGATO,
                            Utente = User.Identity.Name,
                            Operazione = TipoOperazione.Stampato
                        }, true);
                    foreach (Elementi e in _mailService._elmMan.GetElementiDaAllegato(Guid.Parse(IdAllegato)))
                    {
                        _mailService._logMan.Salva(new LogDoc()
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
        [HasPermission("50.1.3")]
        public async Task<ActionResult<bool>> LogRiepilogo(
    string IdAllegato)
        {
            try
            {
                if (!string.IsNullOrEmpty(IdAllegato))
                   
                    _mailService._logMan.Salva(new LogDoc()
                    {
                        Data = DateTime.Now,
                        IdOggetto = Guid.Parse(IdAllegato),
                        TipoOggetto = TipiOggetto.ALLEGATO,
                        Utente = User.Identity.Name,
                        Operazione = TipoOperazione.Stampato
                    }, true);
                foreach( Elementi e in _mailService._elmMan.GetElementiDaAllegato(Guid.Parse(IdAllegato))) {
                    _mailService._logMan.Salva(new LogDoc()
                    {
                        Data = DateTime.Now,
                        IdOggetto = e.Id,
                        TipoOggetto = TipiOggetto.ELEMENTO,
                        Utente = User.Identity.Name,
                        Operazione = TipoOperazione.Stampato
                    }, true) ;
                }
            }
            catch (Exception ex)
            {
                return await Task.FromResult(false);
            }
            return await Task.FromResult(true);
        }

        [AcceptVerbs("Post")]
        [HasPermission("50.1.3")]
        public async Task<ActionResult<bool>> InArrivo_Stampa(string pdf)
        {
            try
            {
                PdfEditAction pdfact = new PdfEditAction();

                pdfact = JsonConvert.DeserializeObject<PdfEditAction>(pdf);

                pdfact.TempFolder= Path.Combine(_hostingEnvironment.WebRootPath, "_tmp");
                string TempFile = Path.Combine(pdfact.TempFolder, $"{pdfact.IdAllegato}.pdf");
                using (FileStream FS = new FileStream(TempFile, FileMode.Create))
                {
                    MemoryStream MS = await _pdfsvc.GetPdf(pdfact);
                    MS.WriteTo(FS);
                    FS.Close();
                }
                if (await _printService.AddJob(User.Identity.Name, TempFile, pdfact.Printer) == 0)
                {
                    System.IO.File.Delete(TempFile);
                    _toastNotification.AddSuccessToastMessage("Documento inviato alla stampante");

                    //Invio di conferma
                    if (!string.IsNullOrEmpty(pdfact.IdAllegato))
                        _mailService._logMan.Salva(new LogDoc()
                        {
                            Data = DateTime.Now,
                            IdOggetto = Guid.Parse(pdfact.IdAllegato),
                            TipoOggetto = TipiOggetto.ALLEGATO,
                            Utente = User.Identity.Name,
                            Operazione = TipoOperazione.Stampato
                        }, true);

                    if (!string.IsNullOrEmpty(pdfact.IdElemento))
                        _mailService._logMan.Salva(new LogDoc()
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
                _mailService._logger.LogError($"InArrivo_Stampa: {ex.Message}");
                return await Task.FromResult(false);
            }
            return await Task.FromResult(true);
        }



        [AcceptVerbs("Post")]
        [HasPermission("50.1.3")]
        public async Task<ActionResult<bool>> InArrivo_Stampato(
            string IdAllegato,
            string IdElemento)
        {
            try
            {
                if (!string.IsNullOrEmpty(IdAllegato))
                    _mailService._logMan.Salva(new LogDoc()
                    {
                        Data = DateTime.Now,
                        IdOggetto = Guid.Parse(IdAllegato),
                        TipoOggetto = TipiOggetto.ALLEGATO,
                        Utente = User.Identity.Name,
                        Operazione = TipoOperazione.Stampato
                    }, true);
                if (!string.IsNullOrEmpty(IdElemento))
                    _mailService._logMan.Salva(new LogDoc()
                    {
                        Data = DateTime.Now,
                        IdOggetto = Guid.Parse(IdElemento),
                        TipoOggetto = TipiOggetto.ELEMENTO,
                        Utente = User.Identity.Name,
                        Operazione = TipoOperazione.Stampato
                    }, true);
            }
            catch (Exception ex) {
                return await Task.FromResult(false);
            }
            return await Task.FromResult(true);
        }


        [HasPermission("50.1.3")]
        public async Task<ActionResult> SoggettoElementiAperti([DataSourceRequest] DataSourceRequest request, string CodiceSoggetto)
        {
            List<ISoggettoElementiAperti> lista = await _soggetti.GetElementiAperti(CodiceSoggetto);
            return Json(lista.ToDataSourceResult(request));
        }

       [HasPermission("50.1.3")]
        public  ActionResult ListaEmailElementi([DataSourceRequest] DataSourceRequest request, string IdFascicolo)
        {
            List<EmailElementi> lista = _mailService.ListaElementiEmail(IdFascicolo);
            return Json(lista.ToDataSourceResult(request));
        }


        [HasPermission("50.1.3")]
        public ActionResult emailProcessate()
        {
            return View();
        }

        [HasPermission("50.1.3")]
        public ActionResult emailInviate()
        {
            return View();
        }
     
        [HasPermission("50.1.3")]
        public ActionResult Processate_Read([DataSourceRequest] DataSourceRequest request, string Tipo, string NomeServer = "")
        {

            IEnumerable<Allegati> lista = _mailService._allMan.GetEmailProcessate(Tipo, NomeServer);
            return Json(lista.ToDataSourceResult(request));
        }


        [HasPermission("50.1.3")]
        [HttpGet]
        public ActionResult StampaRiepilogo(string IdAllegato)
        {
            Allegati a = new Allegati();
            a.Id = Guid.Parse(IdAllegato);
            return View("PreviewRiepilogo", a);
        }


        [AcceptVerbs("Post")]
        [HasPermission("50.1.3")]
        public async Task<ActionResult<RisultatoAzione>> Processate_Riapri(
            string IdAllegato)
        {
            RisultatoAzione r = new RisultatoAzione();
            if (!string.IsNullOrEmpty(IdAllegato) )
            {
                r = await Task.FromResult( _mailService.RiapriEmail(IdAllegato, User));

            }
            else
            {
                r.Successo = false;
                r.Messaggio = "Mail non valida";
            }
            if (r.Successo)
            {
                _toastNotification.AddSuccessToastMessage("Email riaperta.");
            }
            else
            {
                _toastNotification.AddErrorToastMessage($"Riapertura fallita: {r.Messaggio}");
            }
            return Json(r);
        }

        [AcceptVerbs("Post")]
        [HasPermission("50.1.3")]
        public async Task<bool> NotificaAssociazione( string CodiceSoggetto, string IdAllegato)
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
        [HasPermission("50.1.3")]
        public async Task<ActionResult<RisultatoAzione>> Processate_Cancella(string IdAllegato, bool EliminaDaServer)
        {
            RisultatoAzione res = new RisultatoAzione();
            if (IdAllegato != null && ModelState.IsValid)
            {
               res = await _mailService.CancellaEmail(IdAllegato, EliminaDaServer, User);
               _toastNotification.AddSuccessToastMessage("Email eliminata.");
                return Ok(res);
    }
            else
            {
                res.Successo = false;
                res.Messaggio = "Allegato non valido.";
                _toastNotification.AddErrorToastMessage(res.Messaggio);

            }
            return BadRequest(res);
        }

        [AcceptVerbs("Post")]
        [HasPermission("50.1.3")]
        public async Task<ActionResult<RisultatoAzione>> CancellaElemento(string IdElemento, short Revisione )
        {
            RisultatoAzione res = new RisultatoAzione();
            if (IdElemento != null && ModelState.IsValid)
            {
                res = await Task.FromResult(_mailService.CancellaElemento(IdElemento, Revisione, User));

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

        [HasPermission("50.1.3")]
        public ActionResult Inviate_Read([DataSourceRequest] DataSourceRequest request, string Tipo, string NomeServer = "")
        {

            IEnumerable<AllegatoEmail> lista = _mailService._allMan.GetEmailInviate(Tipo, NomeServer);
            return Json(lista.ToDataSourceResult(request));
        }

        #region Smistamento
        [HasPermission("50.1.5")]
        public IActionResult Smistamento()
        {
            
            return View(
                 new inArrivoView_new
                 {
                     Modulo = "50.1.5"
                 }
                );
        }

        [HasPermission("50.1.5")]
        public ActionResult Smistamento_Read([DataSourceRequest] DataSourceRequest request, string Tipo, string NomeServer = "")
        {

            IEnumerable<Allegati> lista = _mailService._allMan.GetEmailDaSmistare(Tipo, NomeServer);
            return Json(lista.ToDataSourceResult(request));
        }

        [AcceptVerbs("Post")]
        [HasPermission("50.1.5")]

        public IActionResult AggiornaAllegato(Guid IdAllegato, string TipoElemento)
        {

            string NomeView = "EditAttributi";
            var e = new AttributiViewModel();
            //var e = _mailService.CreaElementoVuoto(User, IdAllegato, TipoElemento);
            var elemento = _mailService._elmMan.Nuovo(TipoElemento);
            if (elemento != null)
            {
                //var x = _mailService.MarcaAllegati(e);
                //e.IdFascicoloNavigation = _mailService._fasMan.Get(e.IdFascicolo);
                e.Attributi = elemento.elencoAttributi;
                if (!string.IsNullOrEmpty(elemento.TipoNavigation.ViewAttributi))
                {
                    NomeView = elemento.TipoNavigation.ViewAttributi;
                    //NomeView = "EditorElemento";
                }
            }
            return PartialView(NomeView, e);
        }

        [AcceptVerbs("Post")]
        [HasPermission("50.1.5")]

        public IActionResult SalvaAttributiAggiuntivi(string[] IdAllegato, string form)
        {
                bool res = false;

                try
                {
                    foreach (string item in IdAllegato )
                    {
                    if (string.IsNullOrEmpty(item))
                        return BadRequest();

                    Allegati all = _mailService._allMan.Get(item);
                    if (all != null)
                    {
                        if (!string.IsNullOrEmpty(form))
                        {
                            var attr = HttpUtility.ParseQueryString(form);
                            foreach (string n in attr.Keys)
                            {
                            if (n != "__RequestVerificationToken")
                            {
                                Attributo xx = new Attributo();

                                    xx.Nome = n;
                                    if (n.StartsWith("ctl_"))
                                        xx.Nome = n.Substring(4);

                                if (all.elencoAttributi.Valori.ContainsKey(xx.Nome))
                                {
                                        all.elencoAttributi.Assegna(xx.Nome, attr.Get(n));
                                }
                                else
                                {
                                    xx.Valore = attr.Get(n);
                                    all.elencoAttributi.Add(xx);
                                }
                                    //all.SetAttributo(nome, attr.Get(n));
                                if (xx.Nome.ToLower() == "origine")
                                {
                                    all.Origine = attr.Get(n);
                                }
                                }
                            }
                            PdfEditAction pdfed = new PdfEditAction();
                            pdfed.IdAllegato = item;
                            pdfed.TempFolder = Path.Combine(_hostingEnvironment.WebRootPath, "_tmp");
                            if (System.IO.File.Exists(pdfed.FileAnnotazioni))
                            {
                                all.Note = System.IO.File.ReadAllText(pdfed.FileAnnotazioni);
                            }

                            all.Stato = StatoAllegato.Attivo;
                            all.DataUM = DateTime.Now;
                            all.UtenteUM = User.Identity.Name;
                            _mailService._allMan.Salva(all, false, false);
                            ////-------- Memorizzo l'operazione----------------------
                            //LogDoc log = new LogDoc()
                            //{
                            //    Data = DateTime.Now,
                            //    IdOggetto = Guid.Parse(IdAllegato),
                            //    TipoOggetto = TipiOggetto.ALLEGATO,
                            //    Utente = User.Identity.Name,
                            //    Operazione = TipoOperazione.Modificato
                            //};
                            //_mailService._logMan.Salva(log, true);
                            ////-------- Memorizzo l'operazione----------------------
                            res = true;
                        }
                    }
                }
                    //if (string.IsNullOrEmpty(IdAllegato))
                    //    return BadRequest();

                    //Allegati all = _mailService._allMan.Get(IdAllegato);
                    //if (all != null)
                    //{
                    //    if (!string.IsNullOrEmpty(form))
                    //    {
                    //        var attr = HttpUtility.ParseQueryString(form);
                    //        foreach (string n in attr.Keys)
                    //        {
                    //        if (n != "__RequestVerificationToken")
                    //        {
                    //            Attributo xx = new Attributo();

                    //                xx.Nome = n;
                    //                if (n.StartsWith("ctl_"))
                    //                    xx.Nome = n.Substring(4);

                    //            if (all.elencoAttributi.Valori.ContainsKey(xx.Nome))
                    //            {
                    //                all.elencoAttributi.Valori[xx.Nome].Valore = attr.Get(n);
                    //            }
                    //            else
                    //            {
                    //                xx.Valore = attr.Get(n);
                    //                all.elencoAttributi.Add(xx);
                    //            }
                    //                //all.SetAttributo(nome, attr.Get(n));
                    //            if (xx.Nome.ToLower() == "origine")
                    //            {
                    //                all.Origine = attr.Get(n);
                    //            }
                    //            }
                    //        }
                    //    PdfEditAction pdfed = new PdfEditAction();
                    //    pdfed.IdAllegato = IdAllegato;
                    //    pdfed.TempFolder = Path.Combine(_hostingEnvironment.WebRootPath, "_tmp");
                    //    if (System.IO.File.Exists(pdfed.FileAnnotazioni))
                    //    {
                    //        all.Note = System.IO.File.ReadAllText(pdfed.FileAnnotazioni);
                    //    }

                    //        all.Stato = StatoAllegato.Attivo;
                    //        all.DataUM = DateTime.Now;
                    //        all.UtenteUM = User.Identity.Name;
                    //        _mailService._allMan.Salva(all, false, false);
                    //        ////-------- Memorizzo l'operazione----------------------
                    //        //LogDoc log = new LogDoc()
                    //        //{
                    //        //    Data = DateTime.Now,
                    //        //    IdOggetto = Guid.Parse(IdAllegato),
                    //        //    TipoOggetto = TipiOggetto.ALLEGATO,
                    //        //    Utente = User.Identity.Name,
                    //        //    Operazione = TipoOperazione.Modificato
                    //        //};
                    //        //_mailService._logMan.Salva(log, true);
                    //        ////-------- Memorizzo l'operazione----------------------
                    //        res = true;
                    //    }
                    //}
                }
                catch (Exception ex)
                {
                    _toastNotification.AddErrorToastMessage($"Allegato non inoltrato: {ex.Message}");
                    return BadRequest();
                }
                if (res)
                {
                    _toastNotification.AddSuccessToastMessage("Allegato inoltrato");
                    return Ok();
                }
                else
                {

                    return BadRequest();
                }
   
        }
        #endregion

    }

}


