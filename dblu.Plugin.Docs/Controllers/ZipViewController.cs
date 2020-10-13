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

namespace dblu.Portale.Plugin.Documenti.Controllers
{
    [Authorize]
    public class ZipViewController : Controller
    {

        private readonly IToastNotification _toastNotification;
        private ZipService _zipsvc;
        private IConfiguration _config;
        private ISoggettiService _soggetti;

        public ZipViewController(ZipService zipservice,
            IToastNotification toastNotification,
            IConfiguration config,
            ISoggettiService soggetti)
        {
            _zipsvc = zipservice;
            _toastNotification = toastNotification;
            _config = config;
            _soggetti = soggetti;
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
        [HasPermission("50.1.3")]
        public async Task<ZipViewModel> CaricaDettaglio(string Id)
        {
            ZipViewModel m = await _zipsvc.GetZipViewModel(Id);

            return m;
        }

        [HasPermission("50.1.3")]
        public ActionResult ListaZipElementi([DataSourceRequest] DataSourceRequest request, string IdFascicolo)
        {
            List<EmailElementi> lista = _zipsvc.ListaElementiZip(IdFascicolo);
            return Json(lista.ToDataSourceResult(request));
        }

        [AcceptVerbs("Post")]
        [HasPermission("50.1.3")]

        public async Task<ActionResult<bool>> ZipFileCompleto(string IdTask, string IdAllegato)
        {

            bool res = false;

            if (string.IsNullOrEmpty(IdAllegato) == false && string.IsNullOrEmpty(IdTask) == false)
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

                if (string.IsNullOrEmpty(IdTask) == false)
                {
                    BPMTask tsk = new BPMTask();
                    Dictionary<string, BPMClient.VariableValue> fVars = await tsk.GetTaskFormVariables(_zipsvc._bpm._eng, IdTask);
                    BPMTaskDto tdto = tsk.Get(_zipsvc._bpm._eng, IdTask);
                    fVars.Add("_Annullato", new VariableValue { type = VariableType.Boolean, value = false });
                    res = tsk.SubmitTaskForm(_zipsvc._bpm._eng, IdTask, fVars);

                }
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
        [HasPermission("50.1.3")]
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
                bool fl = await _zipsvc.AllegaAElementoFascicolo(IdAllegato,
                    IdFascicolo,
                    IdElemento,
                    elencoFile,
                    AllegaEmail,
                    Descrizione,
                    User);

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
        [HasPermission("50.1.3")]
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
        [HasPermission("50.1.3")]

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

        [HasPermission("50.1.3")]
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

    }


}


