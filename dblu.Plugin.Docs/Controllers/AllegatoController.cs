using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using BPMClient;
using dblu.Docs.Classi;
using dblu.Docs.Models;
using dblu.Portale.Core.Infrastructure;
using dblu.Portale.Plugin.Docs.Services;
using dblu.Portale.Plugin.Docs.ViewModels;
using dblu.Portale.Services.Camunda;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Configuration;
using NToastNotify;

namespace dblu.Portale.Plugin.Docs.Controllers
{
    public class AllegatoController : Controller
    {
        private AllegatiService _AllegatiService;
        private ZipService _zipService;
        private readonly IToastNotification _toastNotification;
        private IConfiguration _config;
        public AllegatoController(AllegatiService allegatiservice,
            ZipService zipService,
          IToastNotification toastNotification,
          IConfiguration config
        )
        {
            _AllegatiService = allegatiservice;
            _zipService = zipService;
            _toastNotification = toastNotification;
            _config = config;

        }

        [HttpGet("/Allegato/Editor")]
        [Authorize]
        [HasPermission("50.1.2")]
        public IActionResult EditorAllegato(Guid id)
        {

            var address = new Uri("http://localhost:5000"); //uri di prova
            var url = Request.Headers["Referer"].ToString();
            if (url.Length > 0)
                address = new Uri(url);

            var model = new EditorAllegatoViewModel()
            {
                Oggetto = _AllegatiService._allMan.Get(id),
                UrlRefer = address.LocalPath,
                IsInsideTask = isInsideTask(address.LocalPath),
                Modulo = "50.1.2",
                User = HttpContext.User
            };
            if (model.Oggetto == null) model.Oggetto = new dblu.Docs.Models.Allegati();
            return View("EditorAllegato", model);

        }


        //funzione privata che controlla se la richiesta arriva da dentro un task o i modo libero
        private bool isInsideTask(string localPath)
        {
            return localPath.Contains("/TaskView/");
        }

        [HttpGet]
        [Authorize]
        [HasPermission("50.1.2")]
        public async Task<FileResult> ApriFile(string IdAllegato, string NomeFile)
        {

            MemoryStream ff = await _zipService.GetFileFromZipAsync(IdAllegato, NomeFile);
            string t = GetContentType(NomeFile);
            if (string.IsNullOrEmpty(t))
            {
                t = "text/csv";
                NomeFile = NomeFile + ".txt";
            }
            return File(ff, t, NomeFile);
        }

        [HasPermission("50.1.2")]       
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> EliminaFileDaZip(string idAllegato, string NomeFile)
        {

            try
            {
                if (idAllegato is null) return BadRequest();
                if (await _zipService.RemoveFileFromZipAsync(idAllegato, NomeFile))
                                  {
                        _toastNotification.AddSuccessToastMessage("File cancellato correttamente!");
                    }
                    else
                    {
                        return BadRequest();
                    }

               
            }
            catch (Exception)
            {
                return BadRequest();
            }

            return Ok();
        }


        [HttpPost]
        [Authorize]
        [HasPermission("50.1.2")]
        public async Task<IActionResult> AddFileToZip(IFormFile files, string IdAllegato)
        {
            try
            {
                if (IdAllegato is null)
                    return BadRequest();

                if (files != null)
                {
                    MemoryStream memoryStream = files as MemoryStream;
                    if (memoryStream == null)
                    {
                        memoryStream = new MemoryStream();
                        files.CopyTo(memoryStream);
                    }

                    if (await _zipService.AddFileToZipAsync(IdAllegato, files.FileName, memoryStream) ) {
                        _toastNotification.AddSuccessToastMessage("File caricato correttamente!");
                    }
                    else
                    {
                        return BadRequest();
                    }
                    
                }
            }
            catch (Exception)
            {
                return BadRequest();
            }

            return Ok();
        }
        
        [HttpPost]
        [Authorize]
        [HasPermission("50.1.2")]
        public async Task<IActionResult> AnnullaZip(string IdTask, string IdAllegato)
        {
            bool res = false;
            try
            {
                if (string.IsNullOrEmpty(IdTask) || string.IsNullOrEmpty(IdAllegato))
                    return BadRequest();

                Allegati all = _AllegatiService._allMan.Get(IdAllegato);
                if (all != null) {
                    all.Stato = StatoAllegato.Annullato;
                    _zipService._allMan.Salva(all, false);
                    //-------- Memorizzo l'operazione----------------------
                    LogDoc log = new LogDoc()
                    {
                        Data = DateTime.Now,
                        IdOggetto = Guid.Parse(IdAllegato),
                        TipoOggetto = TipiOggetto.ALLEGATO,
                        Utente = User.Identity.Name,
                        Operazione = TipoOperazione.Annullato
                    };
                    _zipService._logMan.Salva(log, true);
                    //-------- Memorizzo l'operazione----------------------

                    BPMTask tsk = new BPMTask();
                    Dictionary<string, BPMClient.VariableValue> fVars = await tsk.GetTaskFormVariables(_zipService._bpm._eng, IdTask);
                    BPMTaskDto tdto = tsk.Get(_zipService._bpm._eng, IdTask);
                    fVars["_Annulla"].SetTypedValue(true);
                    res = tsk.SubmitTaskForm(_zipService._bpm._eng, IdTask, fVars);
                }
            }
            catch 
            {
                return BadRequest();
            }
            if (res)
                return Ok();
            else
                return BadRequest();
        }
        
        public string GetContentType(string path)
        {
            string t = "";
            try
            {
                var types = GetMimeTypes();
                var ext = Path.GetExtension(path).ToLowerInvariant();
                t = types[ext];
            }
            catch
            {
            }
            return t;
        }

        private Dictionary<string, string> GetMimeTypes()
        {
            return new Dictionary<string, string>
                {
                    {".bmp", "image/bmp"},
                    {".txt", "text/plain"},
                    {".pdf", "application/pdf"},
                    {".doc", "application/ms-word"},
                    {".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"},
                    {".odt", "application/vnd.oasis.opendocument.text"},
                    {".xls", "application/vnd.ms-excel"},
                    {".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
                    {".png", "image/png"},
                    {".jpg", "image/jpeg"},
                    {".jpeg", "image/jpeg"},
                    {".gif", "image/gif"},
                    {".tiff", "image/tiff"},
                    {".tif", "image/tiff"},
                    {".csv", "text/csv"},
                    {".html", "text/html"},
                    {".htm", "text/html"},
                    {".dat", "application/dat"},
                    {".svg", "image/svg+xml"},
                    {".eml", "application/eml"},
                    {".zip", "application/zip"},
                    {".rar", "application/rar"},
                    {".7z", "application/7zip"},
                    {".", "text/html"},
                };
            /*
             
            .doc      application/msword
            .dot      application/msword

            .docx     application/vnd.openxmlformats-officedocument.wordprocessingml.document
            .dotx     application/vnd.openxmlformats-officedocument.wordprocessingml.template
            .docm     application/vnd.ms-word.document.macroEnabled.12
            .dotm     application/vnd.ms-word.template.macroEnabled.12

            .xls      application/vnd.ms-excel
            .xlt      application/vnd.ms-excel
            .xla      application/vnd.ms-excel

            .xlsx     application/vnd.openxmlformats-officedocument.spreadsheetml.sheet
            .xltx     application/vnd.openxmlformats-officedocument.spreadsheetml.template
            .xlsm     application/vnd.ms-excel.sheet.macroEnabled.12
            .xltm     application/vnd.ms-excel.template.macroEnabled.12
            .xlam     application/vnd.ms-excel.addin.macroEnabled.12
            .xlsb     application/vnd.ms-excel.sheet.binary.macroEnabled.12

            .ppt      application/vnd.ms-powerpoint
            .pot      application/vnd.ms-powerpoint
            .pps      application/vnd.ms-powerpoint
            .ppa      application/vnd.ms-powerpoint

            .pptx     application/vnd.openxmlformats-officedocument.presentationml.presentation
            .potx     application/vnd.openxmlformats-officedocument.presentationml.template
            .ppsx     application/vnd.openxmlformats-officedocument.presentationml.slideshow
            .ppam     application/vnd.ms-powerpoint.addin.macroEnabled.12
            .pptm     application/vnd.ms-powerpoint.presentation.macroEnabled.12
            .potm     application/vnd.ms-powerpoint.template.macroEnabled.12
            .ppsm     application/vnd.ms-powerpoint.slideshow.macroEnabled.12

            .mdb      application/vnd.ms-access
             
             */
        }


        [HasPermission("50.1.2")]
        [Authorize]
        public async Task<ActionResult> ListaFileInZip([DataSourceRequest] DataSourceRequest request, string IdAllegato)
        {
            IList<EmailAttachments> lista = await _zipService.GetZipFilesAsync(IdAllegato);
            return Json(lista.ToDataSourceResult(request));
        }

        [HttpPost]
        [Authorize]
        [HasPermission("50.1.2")]
        public async Task<IActionResult> SalvaAttributi(string IdAllegato, 
            string form   //form serializzata 
        )

        {
            bool res = false;
         
            try
            {
             
                if ( string.IsNullOrEmpty(IdAllegato))
                    return BadRequest();

                Allegati all = _AllegatiService._allMan.Get(IdAllegato);
                if (all != null)
                {
                    if (!string.IsNullOrEmpty(form)) { 
                        var attr = HttpUtility.ParseQueryString(form);
                        foreach (string n in attr.Keys) {
                            var nome = n;
                            if (nome.StartsWith("ctl_"))
                                nome = n.Substring(4);

                            all.SetAttributo(nome, attr.Get(n));
                        
                        }
                        all.DataUM = DateTime.Now;
                        all.UtenteUM = User.Identity.Name;
                        _zipService._allMan.Salva(all, false);
                        //-------- Memorizzo l'operazione----------------------
                        LogDoc log = new LogDoc()
                        {
                            Data = DateTime.Now,
                            IdOggetto = Guid.Parse(IdAllegato),
                            TipoOggetto = TipiOggetto.ALLEGATO,
                            Utente = User.Identity.Name,
                            Operazione = TipoOperazione.Modificato
                        };
                        _zipService._logMan.Salva(log, true);
                        //-------- Memorizzo l'operazione----------------------
                        res = true;
                     }
                }
            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage($"Allegato non salvato: {ex.Message}" );
                return BadRequest();
            }
            if (res)
            {
                _toastNotification.AddSuccessToastMessage("Allegato salvato");
                return Ok();
            }
            else { 
               
                return BadRequest();
            }
        }

        [HttpPost]
        [Authorize]
        [HasPermission("50.1.2")]
        public async Task<IActionResult> CompletaZip(string IdTask , string RuoliCandidati)
        {

            bool res = false;
            try
            {
                if (string.IsNullOrEmpty(IdTask) )
                    return BadRequest();

                BPMTask tsk = new BPMTask();
                Dictionary<string, BPMClient.VariableValue> fVars = await tsk.GetTaskFormVariables(_zipService._bpm._eng, IdTask);
                BPMTaskDto tdto = tsk.Get(_zipService._bpm._eng, IdTask);
                fVars["_Annulla"].SetTypedValue(false);
                fVars["_RuoliCandidati"].SetTypedValue(RuoliCandidati);
                res = tsk.SubmitTaskForm(_zipService._bpm._eng, IdTask, fVars);

            }
            catch 
            {
                return BadRequest();
            }
            if (res)
                return Ok();
            else
                return BadRequest();
        }
    }
}
