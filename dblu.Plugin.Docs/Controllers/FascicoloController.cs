using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using dblu.Docs.Interfacce;
using dblu.Docs.Models;
using dblu.Portale.Core.Infrastructure;
using dblu.Portale.Core.Infrastructure.Class;
using dblu.Portale.Plugin.Docs.Data;
using dblu.Portale.Plugin.Docs.Extensions;
using dblu.Portale.Plugin.Docs.Models;
using dblu.Portale.Plugin.Docs.Services;
using dblu.Portale.Plugin.Docs.ViewModels;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NToastNotify;
using FileResult = dblu.Portale.Plugin.Docs.Models.FileRisultato;


namespace dblu.Portale.Plugin.Documenti.Controllers
{
    public class FascicoloController : Controller
    {
        private readonly IToastNotification _toastNotification;
        private AllegatiService _doc;
        private IConfiguration _config;
        private ISoggettiService _soggetti;

        
        public FascicoloController(AllegatiService doc,
            IToastNotification toastNotification,
            IConfiguration config,
            ISoggettiService soggetti)
        {
            _doc = doc;
            _toastNotification = toastNotification;
            _config = config;
            _soggetti = soggetti;
        }


        public IActionResult GetSoggetti([DataSourceRequest] DataSourceRequest request)
        {
            var soggetti = _soggetti.GetSoggetti();
            if (soggetti.Count == 0)
                _toastNotification.AddInfoToastMessage("Nessun Cliente!");

            return Json(soggetti.ToDataSourceResult(request));
        }


        //ritorna la lista di allegati associati ad un elemento
        [HttpGet("/Docs/Allegati/{id}")]
        [Authorize]
        [HasPermission("50.1.2")]
        public IActionResult GetAllegatiElemento([DataSourceRequest] DataSourceRequest request,string id)
        {
            var elemento = Guid.Parse(id);

            var allegati = _doc.GetAllegatiElemento(elemento);

            return new JsonResult(allegati.ToDataSourceResult(request));
        }



        [HttpPost("/Docs/Allegati")]
        [Authorize]
        [HasPermission("50.1.2")]
        public IActionResult GetAllegatiElemento([DataSourceRequest] DataSourceRequest request,Guid elemento)
        {
            var allegati = _doc.GetAllegatiElemento(elemento);
            return Json(allegati.ToDataSourceResult(request));
        }

        [AcceptVerbs("Post")]
        [Authorize]
        [HasPermission("50.1.2")]
        public IActionResult UpdateAllegati([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<dblu.Docs.Models.Allegati> Allegati)
        {
            if (Allegati != null )
            {
                foreach (var allegato in Allegati)
                {
                    if (allegato.Chiave1 == null) allegato.Chiave1 = "";
                    if (allegato.Chiave2 == null) allegato.Chiave2 = "";
                    if (allegato.Chiave3 == null) allegato.Chiave3 = "";
                    if (allegato.Chiave4 == null) allegato.Chiave4 = "";
                    if (allegato.Chiave5 == null) allegato.Chiave5 = "";
                    allegato.UtenteUM = HttpContext.User.Identity.Name;
                    allegato.DataUM = DateTime.Now;
                    if (_doc.SaveAllegato(allegato) == false)
                    {
                        _toastNotification.AddInfoToastMessage("Errore nel salvataggio!");
                    }
                }
            }

            return Json(Allegati.ToDataSourceResult(request, ModelState));

        }


        //lista di elementi associati ad un fascicolo
        [HttpGet("/Docs/Elementi/{id}")]
        [Authorize]
        [HasPermission("50.1.2")]
        public IActionResult GetElementi([DataSourceRequest] DataSourceRequest request,Guid id)
        {

            var elementi = _doc.GetElementi(id);

            return Json(elementi.ToDataSourceResult(request));
           //return Json(elementi);
        }

        [HttpPost("/Docs/Elementi")]
        [Authorize]
        [HasPermission("50.1.2")]
        public IActionResult GetElementiPost([DataSourceRequest] DataSourceRequest request, Guid id)
        {

            var elementi = _doc.GetElementi(id);

            return Json(elementi.ToDataSourceResult(request));
            
        }


        //navida dentro la view pel la visualizzazione dell'allegato
        [HttpGet("/Docs/Allegato/{id}")]
        [Authorize]
        [HasPermission("50.1.2")]
        public IActionResult AllegatoEdit(string id)
        {
            //var elemento = Guid.Parse(id);

            Allegati allegato = _doc._allMan.Get(id);

            var model = new DocumentViewModel();
            if (allegato != null)
            {
                model = new DocumentViewModel
                {
                    Allegato = allegato
                };

            }
            return View(model);

        }


        //naviga dentro la view per la visualizzazione del fascicolo
        [HttpGet("/Docs/Fascicolo/{id}")]
        [Authorize]
        [HasPermission("50.1.2")]
        public IActionResult Index(string id)
        {
            var fascicolo = Guid.Parse(id);
            var objfascicolo = _doc.GetFascicolo(fascicolo);

            var address = new Uri("http://localhost:5000"); //uri di prova
            try
            {
               
            var url = Request.Headers["Referer"].ToString();
                string sc = HttpContext.Session.GetString("FascicoloReferer");
                if (!string.IsNullOrEmpty(sc))
                {
                    address = new Uri(sc);
                    HttpContext.Session.Remove("FascicoloReferer");// ("FascicoloReferer", url);
                }
                else
                {
                    HttpContext.Session.SetString("FascicoloReferer", url);
                address = new Uri(url);
                }
             
            }
            catch { }


            var model = new FascicoloViewModel();
            if (objfascicolo is null)
            {
                _toastNotification.AddErrorToastMessage("Fascicolo " + id + " non trovato");

                model = new FascicoloViewModel()
                {
                    Fascicolo = new Fascicoli()
                    {
                        Id = Guid.Empty,
                        Descrizione = "Errore",
                        DataC = DateTime.Now,
                        DataUM = DateTime.Now,
                        UtenteC = "Errore",
                        UtenteUM = "Errore"
                    },
                    UrlRefer = ""
                };
                return View(model);
            }
            else
            {
                _toastNotification.AddInfoToastMessage("Richiesto fascicolo " + id);
                model = new FascicoloViewModel
                {
                    Fascicolo = objfascicolo,
                    UrlRefer = address.LocalPath,
                    IsInsideTask = isInsideTask(address.LocalPath)
                    
                };


                HttpContext.Session.SetString("Referer", model.UrlRefer);
            }

            return View(model);
        }

        //funzione privata che controlla se la richiesta arriva da dentro un task o i modo libero
        private bool isInsideTask(string localPath)
        {
            return localPath.Contains("/TaskView/");
        }



      
        [HttpGet("/Docs/Fascicolo")]
        [Authorize]
        [HasPermission("50.1.2")]
        public IActionResult Fascicoli()
        {
            return View();
        }

        //ritorna tutti i fascicoli 

    

        public ActionResult GetFascicoliV([DataSourceRequest] DataSourceRequest request)
        {
            var fascicoli = _doc.GetFascicoliV();
            return Json(fascicoli.ToDataSourceResult(request));
        }

        public ActionResult GetFascicoli([DataSourceRequest] DataSourceRequest request)
        {
            var fascicoli = _doc.GetFascicoli();
            return Json(fascicoli.ToDataSourceResult(request));
        }

        //TODO filtro per cliente \ soggetto
        public ActionResult GetElementiSoggetto([DataSourceRequest] DataSourceRequest request, string soggetto)
        {

//#if DEBUG
            ////TODO FILTRO PER SOGGETTO CHE IL DB NON è PRONTO
            //var fascicoli = _doc.GetFascicoli();
            ////var fascicoli = _doc.GetFascicoliElementi();
            //return Json(fascicoli.ToDataSourceResult(request));
//#else
            //TODO FILTRO PER SOGGETTO CHE IL DB NON è PRONTO
            var el = _doc.GetElementiSoggetto(soggetto);
            // var fascicoli = _doc.GetFascicoliElementi(soggetto);
            return Json(el.ToDataSourceResult(request));
//#endif

        }

        [HttpPost]
        public ActionResult CercaElementiSoggetto(string soggetto)
        {
            var model = _doc.GetElementiSoggetto(soggetto);
            return View("CercaFascicoli", model);
        }


        //upload un file per allegarlo ad un elemento
        [HttpPost]
        [RequestSizeLimit(40000000)]
        public async Task<IActionResult> Upload(IEnumerable<IFormFile> files, string idelemento, string idfascicolo, string descrizione)
        {
            try
            {
                if (idelemento is null)
                    return BadRequest();

                if (files != null)
                {
                     _doc.DocumentUpload(files,idelemento,idfascicolo,descrizione, HttpContext.User.Identity.Name);
                    _toastNotification.AddSuccessToastMessage("Allegato caricato correttamente!");
                }
            }
            catch (Exception)
            {
               // _toastNotification.AddErrorToastMessage("Errore nel caricamento dell'allegato!");
                return BadRequest();
            }

            return Ok();
            //return RedirectToAction("Index");
        }


        [HttpPost]
        public IActionResult Allegato(string id)
        {
            //TODO check se esiste o no l'allegato

            ///var redirectUrl = "";
            //redirectUrl = Url.Action("Produzione", "Longon", new { area = "Longon" });
            //return View();

            return Json(new
            {
                ID = id,
            });
        }

        //naviga nella view di editing del fascicolo
        [HttpGet("/Fascicolo/Editor")]
        [Authorize]
        [HasPermission("50.1.2")]
        public IActionResult EditorFascicolo(Guid id)
        {
            var model = new EditorFascicoloViewModel() { Oggetto = _doc.GetFascicolo(id) };
            return PartialView("EditorFascicolo", model);
        }

        //naviga nella view di editing del elemento

        [HttpGet("/Fascicolo/EditorElemento")]
        [Authorize]
        [HasPermission("50.1.2")]
        public IActionResult EditorElemento(Guid id, short rev)
        {

            var address = new Uri("http://localhost:5000"); //uri di prova
            var url = Request.Headers["Referer"].ToString();
            if (url.Length > 0)
                address = new Uri(url);

            var model = new EditorElementoViewModel() { 
                Oggetto = _doc._elmMan.Get(id, rev) ,
                UrlRefer = address.LocalPath,
                IsInsideTask = isInsideTask(address.LocalPath),
                Modulo = "50.1.2",
                User= HttpContext.User 
            };
            return View("EditorElemento", model);
           
        }


        [HttpPost]
        public IActionResult leggiAllegati(IFormFile files, string idelemento, string idfascicolo, string descrizione, string rev)
        {

            try
            {
                if (idelemento is null)
                    return BadRequest();

                if (files != null)
                {
                    _doc.DocumentUploadX(files, idelemento, idfascicolo, descrizione, HttpContext.User.Identity.Name);
                    _toastNotification.AddSuccessToastMessage("Allegato caricato correttamente!");
                }
            }
            catch (Exception)
            {
                // _toastNotification.AddErrorToastMessage("Errore nel caricamento dell'allegato!");
                return BadRequest();
            }


            return Ok();


        }


        [HttpPost]
        [Authorize]
        [HasPermission("50.1.2", 1)]
        public IActionResult AddAllegato(IFormFile files, string idelemento, string idfascicolo, string descrizione, string rev)
        {

            try
            {
                if (idelemento is null)
                    return BadRequest();

                if (files != null)
                {
                    _doc.DocumentUploadX(files, idelemento, idfascicolo, descrizione, HttpContext.User.Identity.Name);
                    _toastNotification.AddSuccessToastMessage("Allegato caricato correttamente!");
                }
            }
            catch (Exception)
            {
                // _toastNotification.AddErrorToastMessage("Errore nel caricamento dell'allegato!");
                return BadRequest();
            }

           
            return Ok();

          
        }
      
        [HttpPost]
        [Authorize]
        [HasPermission("50.1.2", 1)]
        public IActionResult CancellaAllegato(string IdAllegato)
        {
            try
            {
                if (IdAllegato is null)
                    return BadRequest();

                if (!_doc._allMan.Cancella(IdAllegato)) {
                    return BadRequest();
                }

            }
            catch (Exception)
            {
                 return BadRequest();
            }


            return Ok();


        }


        #region NOTE 
        //-------------------------------------------------------------------------------------------------------------------------
        /// GESTIONE NOTE NELLA VIEW DELL'ALLEGATO


        [AcceptVerbs("Post")]
        public ActionResult DestroyNoteGrid(Note models, Guid idAllegato) {
          
            JToken note  = _doc.GetNoteGrid(idAllegato);

            try
            {
                var jnote = note["note"] as JArray;

                dynamic nnote = new JObject();
                nnote.note = new JArray() as dynamic;
                foreach (var item in jnote)
                {
                    var guid = Guid.Parse(item["guid"].ToString());
                    var guidToremove = models.id;
                    if (guid.CompareTo(guidToremove) == 0)
                    {

                    }
                    else
                    {
                        nnote.note.Add(item);
                    }
                }

                _doc.SaveNoteJ(idAllegato, nnote);
                _toastNotification.AddSuccessToastMessage("Note salvate!");
                return Ok();

            }
            catch (Exception)
            {
                _toastNotification.AddErrorToastMessage("Errore in salvataggio note!");
                return BadRequest();
                //throw;
            }
            
        }

        [AcceptVerbs("Post")]
        public ActionResult CreateNoteGrid( Note models, Guid idAllegato)
        {
            try
            {
                var data = Request.Form["date"].ToString();
                DateTime dt = DateTime.Now;
                if (DateTime.TryParse(data, new CultureInfo("en-US"), DateTimeStyles.None, out dt) == false)
                {
                    dt = DateTime.Now;
                };
                models.date = dt;
                SaveNote(models, idAllegato);

                _toastNotification.AddSuccessToastMessage("Note salvate!");
                return Ok();
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage("Errore in salvataggio note!");
                return BadRequest();
            }
        }

        private void SaveNote(Note models, Guid  id)
        {
            var note = _doc.GetNoteGrid(id);

            //metto le vecchie
            dynamic nnote = new JObject();
            nnote.note = new JArray() as dynamic;
            if (note != null)
            {
                var jnote = note["note"] as JArray;
                foreach (var item in jnote)
                {
                    nnote.note.Add(item);
                }
            }

            if (models.id == Guid.Empty)
                models.id = Guid.NewGuid();

            //accodo la nuova
            dynamic jnota = new JObject();
            jnota.guid = models.id != null ? models.id : Guid.NewGuid();
            jnota.data = models.date;
            jnota.datemodifica = models.date;
            jnota.utentemodifica = models.utentemodifica != null ? models.utentemodifica : HttpContext.User.Identity.Name;
            jnota.utente = models.utente != null ? models.utente : HttpContext.User.Identity.Name;
            jnota.contenuto = models.contenuto;
            nnote.note.Add(jnota);

            _doc.SaveNoteJ(id, nnote);
        }

        [AcceptVerbs("Post")]
        public ActionResult UpdateNoteGrid(Note models, Guid idAllegato)
        {

            JToken  note = _doc.GetNoteGrid(idAllegato);

            try
            {
                var jnote = note["note"] as JArray;

                dynamic nnote = new JObject();
                nnote.note = new JArray() as dynamic;
                foreach (var item in jnote)
                {
                    var guid = Guid.Parse(item["guid"].ToString());
                    var guidToUpdate = models.id;
                    if (guid.CompareTo(guidToUpdate) == 0)
                    {
                        item["contenuto"] = models.contenuto;
                        item["utentemodifica"] = models.utentemodifica;
                        item["datemodifica"] = DateTime.Now;
                        nnote.note.Add(item);
                    }
                    else
                    {
                        nnote.note.Add(item);
                    }
                }

                _doc.SaveNoteJ(idAllegato, nnote);
                _toastNotification.AddSuccessToastMessage("Note salvate!");
                return Ok();

            }
            catch (Exception)
            {
                _toastNotification.AddErrorToastMessage("Errore in salvataggio note!");
                return BadRequest();
                //throw;
            }

        }
        public ActionResult GetNoteGrid([DataSourceRequest] DataSourceRequest request, Guid idAllegato)
        {
            var note = _doc.GetNoteGrid(idAllegato);
            if(note is null)
            {
                _toastNotification.AddInfoToastMessage("Nessuna Nota!");
                return Ok();
            }
            var jnote = note["note"] as JArray;

            List<Note> n = new List<Note>();
            foreach (var item in jnote)
            {

                Note x = new Note()
                {
                    id = item["guid"] == null ? Guid.NewGuid() : Guid.Parse(item["guid"].ToString()),
                    contenuto = item["contenuto"].ToString(),
                    date = DateTime.Parse(item["data"].ToString()),
                    utente = item["utente"].ToString(),
                    utentemodifica = item["utentemodifica"] == null ? "" : item["utentemodifica"].ToString()
                };
                n.Add(x);
            }
            
             
            return Json(n.ToDataSourceResult(request));
        }


        [HttpPost]
        public ActionResult ShowNote(Guid id)
        {
            var x = new DocumentViewModel() { Allegato = _doc.GetDocumento(id) };       
            return PartialView("NoteView", x);
        }



        //--------------------------------------------------------------------------------------


        #endregion

        [HttpPost("cerca")]
        public ActionResult<dResult> Cerca(Fascicoli Fascicolo)
        {

            dResult r = new dResult();
            List<Fascicoli> la = _doc._fasMan.CercaFascicoli(Fascicolo);
            r.Success = la.Count > 0;
            r.ReturnData = la;

            return r;
        }

        //[HttpGet("/Docs/Test")]
        //public IActionResult Test()
        //{
        //    return View();
        //}
    }
}
