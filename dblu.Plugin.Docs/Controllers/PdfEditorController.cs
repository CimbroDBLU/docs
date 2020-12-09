using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using dblu.Docs.Models;
using dblu.Portale.Core.Infrastructure;
using dblu.Portale.Plugin.Docs.Models;
using dblu.Portale.Plugin.Docs.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NToastNotify;
using Syncfusion.Drawing;
using Syncfusion.EJ2.PdfViewer;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;

namespace dblu.Portale.Plugin.Docs.Controllers
{
    public class PdfEditorController : Controller
    {

        private IMemoryCache _cache;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private IConfiguration _config;
        private readonly IToastNotification _toastNotification;
        public readonly ILogger _logger;
        private PdfEditService _pdfsvc;

        private AllegatiService _allegatiService;
        private MailService _mailService;
        private readonly dbluDocsContext _context;

        private string tempFolder = "";

        public PdfEditorController(IWebHostEnvironment hostingEnvironment,
            IMemoryCache cache, IConfiguration config, 
            AllegatiService allegatiService,
            MailService mailService,
            IToastNotification toastNotification,
            ILoggerFactory loggerFactory,
            dbluDocsContext db,
             PdfEditService pdfsvc
            )
        {
            //_context = new dbluDocsContext(db.Connessione);
            _context = db;
            _logger = loggerFactory.CreateLogger("FileRepository");
            _hostingEnvironment = hostingEnvironment;
            _cache = cache;
            _config = config;
            _allegatiService = allegatiService;
            _mailService = mailService;
            _toastNotification = toastNotification;
            _pdfsvc = pdfsvc;

            tempFolder = Path.Combine(_hostingEnvironment.WebRootPath, "_tmp");
        }

        [AcceptVerbs("Post")]
        [HttpPost]
        [Route("[controller]/editor")]
        [Authorize]
        [HasPermission("50.1.2")]
        public IActionResult editor(string pdf)
        {
            PdfEditAction model = new PdfEditAction();
            if (!string.IsNullOrEmpty(pdf)) {
                model = JsonConvert.DeserializeObject<PdfEditAction>(pdf);
            }
            return View(model);
        }

        // GET: /<controller>/
        public IActionResult PdfViewerFeatures()
        {
            return View();
        }


        //private async Task<MemoryStream> GetPdf(PdfEditAction pdf) {
        //    MemoryStream stream = new MemoryStream();

        //    if (!string.IsNullOrEmpty(pdf.IdElemento))
        //    {
        //        Allegati ae = _allegatiService.GetPdfAllegatoAElemento(pdf);
        //        stream = await _allegatiService._allMan.GetFileAsync(ae.Id.ToString());
        //    }
        //    else
        //    {
        //        byte[] bytes = null;
        //        if (System.IO.File.Exists(pdf.FilePdfModificato))
        //        {
        //            bytes = System.IO.File.ReadAllBytes(pdf.FilePdfModificato);
        //            stream = new MemoryStream(bytes);
        //        }
        //        else
        //        {
        //            if (System.IO.File.Exists(pdf.FilePdf))
        //            {
        //                System.IO.File.Delete(pdf.FilePdf);
        //            }
        //            if (pdf.TipoAllegato == "FILE")
        //            {
        //                stream = await _allegatiService._allMan.GetFileAsync(pdf.IdAllegato.ToString());
        //            }
        //            else
        //            {
        //                pdf = await _mailService.GetFilePdfCompletoAsync(pdf, true);
        //                bytes = System.IO.File.ReadAllBytes(pdf.FilePdf);
        //                stream = new MemoryStream(bytes);
        //            }
        //            System.IO.File.Delete(pdf.FilePdf);
        //        }
        //        if (System.IO.File.Exists(pdf.FilePdf))
        //    {
        //            System.IO.File.Delete(pdf.FilePdf);
        //        }
        //    }


        //    return stream;
        //}

        [AcceptVerbs("Post")]
        [HttpPost]
        [Route("api/[controller]/Load")]
        [Authorize]
        [HasPermission("50.1.2|50.1.3|50.1.4")]
        public async Task<IActionResult> Load([FromBody] Dictionary<string, string> jsonObject)
        {
            PdfRenderer pdfviewer = new PdfRenderer(_cache);
            MemoryStream stream = new MemoryStream();
            object jsonResult = new object();
            if (jsonObject != null && jsonObject.ContainsKey("document"))
            {
                if (bool.Parse(jsonObject["isFileName"]))
                {
                    PdfEditAction pdf = GetAction(jsonObject["document"]);
                    
                    if (!string.IsNullOrEmpty(pdf.FilePdf))
                    {
                        if ( pdf.TipoAllegato!="" )
                        {
                            byte[] bytes = null;
                            switch (pdf.Azione)
                            {
                                case Azioni.Carica:
                                    if (System.IO.File.Exists(pdf.FilePdfInModifica))
                                    {
                                        System.IO.File.Delete(pdf.FilePdfInModifica);
                                    }
                                    if (System.IO.File.Exists(pdf.FileAnnotazioni))
                                    {
                                        System.IO.File.Delete(pdf.FileAnnotazioni);
                                    }
                                    stream = await _pdfsvc.GetPdf(pdf);

                                    break;
                                case Azioni.CaricaRiepilogo:
                                    break;
                                case Azioni.Ricarica:
                                    if (System.IO.File.Exists(pdf.FilePdfInModifica))
                                    {
                                        System.IO.File.Delete(pdf.FilePdfInModifica);
                                    }
                                    if (System.IO.File.Exists(pdf.FilePdfModificato))
                                    {
                                        System.IO.File.Delete(pdf.FilePdfModificato);
                                    }
                                    if (System.IO.File.Exists(pdf.FileAnnotazioni))
                                    {
                                        System.IO.File.Delete(pdf.FileAnnotazioni);
                                    }

                                    stream = await _pdfsvc.GetPdf(pdf);


                                    break;
                                case Azioni.RuotaPagina90:
                                case Azioni.RuotaPagina270:
                                case Azioni.CancellaPagina:
                                    if (!System.IO.File.Exists(pdf.FilePdfInModifica))
                                    {
                                        if (System.IO.File.Exists(pdf.FilePdfModificato))
                                        {
                                            System.IO.File.Copy(pdf.FilePdfModificato, pdf.FilePdfInModifica, true);
                                        }
                                        else
                                        {
                                            stream = await _pdfsvc.GetPdf(pdf);
                                            using (var fileStream = new FileStream(pdf.FilePdfInModifica, FileMode.Create, FileAccess.ReadWrite))
                                            {
                                                stream.CopyTo(fileStream);
                                            }

                                        }
                                    }
                                    if (_pdfsvc.Modifica(pdf))
                                    {
                                        bytes = System.IO.File.ReadAllBytes(pdf.FilePdfInModifica);
                                        stream = new MemoryStream(bytes);
                                    }
                                    break;
                                case Azioni.Salva:
                                    if (System.IO.File.Exists(pdf.FilePdfInModifica))
                                    {
                                        System.IO.File.Move(pdf.FilePdfInModifica, pdf.FilePdfModificato, true);
                                    
                                        bytes = System.IO.File.ReadAllBytes(pdf.FilePdfModificato);
                                        stream = new MemoryStream(bytes);
                                       
                                    }
                                    else
                                    {
                                        stream = await _pdfsvc.GetPdf(pdf);
                                    }
                                    if (!string.IsNullOrEmpty(pdf.IdElemento))
                                        {
                                                Allegati ae = _allegatiService.GetPdfAllegatoAElemento(pdf);
                                        await _allegatiService._allMan.SalvaFileAsync(ae.Id.ToString(), stream);
                                        System.IO.File.Delete(pdf.FilePdfModificato);
                                        stream.Position = 0;

                                            }
                                    else
                                    {
                                        using (var fileStream = new FileStream(pdf.FilePdfModificato, FileMode.Create, FileAccess.ReadWrite))
                                        {
                                            stream.CopyTo(fileStream);
                                        }
                                        if (pdf.TipoAllegato == "FILE") {
                                            await _allegatiService._allMan.SalvaFileAsync(pdf.IdAllegato, stream);
                                            System.IO.File.Delete(pdf.FilePdfModificato);
                                        }
                                            stream.Position = 0;
                                        } 
                                    if (System.IO.File.Exists(pdf.FileAnnotazioni))
                                    {
                                        string json = System.IO.File.ReadAllText(pdf.FileAnnotazioni);
                                        try
                                        {
                                             JToken ann = JsonConvert.DeserializeObject<JToken>(json);
                                            foreach (JToken a in ann["pdfAnnotation"].Children())
                                            {
                                                foreach (JToken n in a.Children())
                                                {
                                                    foreach (JToken t in n.Children().Children())
                                                    {
                                                        for (var ni = 0; ni < t.Count(); ni++)
                                                        {
                                                            var s = t[ni]["AnnotationSettings"];
                                                            s["isLock"] = true;
                                                        }
                                                    }
                                    }
                                            }
                                            json = JsonConvert.SerializeObject(ann);
                                        }
                                        catch (Exception ex)
                                        {


                                    }
                                        _allegatiService.SaveNoteString(pdf, json);
                                        System.IO.File.Delete(pdf.FileAnnotazioni);
                                    }
                                    break;
                                default:
                                    break;
                            }

                            var cacheEntryOptions = new MemoryCacheEntryOptions()
                                .SetSlidingExpiration(TimeSpan.FromSeconds(20));
                            _cache.Set(pdf.CacheEntry, pdf, cacheEntryOptions);
                        }
                        else
                        {
                            if (pdf.FilePdf.Contains("\\") && System.IO.File.Exists(pdf.FilePdf))
                            {
                                byte[] bytes = System.IO.File.ReadAllBytes(pdf.FilePdf);
                                stream = new MemoryStream(bytes);
                                System.IO.File.Delete(pdf.FilePdf);
                            }
                            else
                            {
                                return this.Content(jsonObject["document"] + " is not valid");
                            }
                        }
                    }
                    else
                    {
                        return this.Content(jsonObject["document"] + " is not found");
                    }
                }
                else
                {
                    byte[] bytes = Convert.FromBase64String(jsonObject["document"]);
                    stream = new MemoryStream(bytes);
                }
                //var note = _allegatiService.GetNote(jsonObject);
                //jsonObject.Add("pdfAnnotation", note);
            }
            stream.Position = 0;
            jsonResult = pdfviewer.Load(stream, jsonObject);
            return Content(JsonConvert.SerializeObject(jsonResult));
        }

        [AcceptVerbs("Post")]
        [HttpPost]
        [Route("api/[controller]/RenderPdfPages")]
        [Authorize]
        [HasPermission("50.1.2")]
        public IActionResult RenderPdfPages([FromBody] Dictionary<string, string> jsonObject)
        {
            PdfRenderer pdfviewer = new PdfRenderer(_cache);
            object jsonResult = pdfviewer.GetPage(jsonObject);
            return Content(JsonConvert.SerializeObject(jsonResult));
        }

        [AcceptVerbs("Post")]
        [HttpPost]
        [Route("api/[controller]/RenderAnnotationComments")]
        public IActionResult RenderAnnotationComments([FromBody] Dictionary<string, string> jsonObject)
        {
            PdfRenderer pdfviewer = new PdfRenderer(_cache);
            object jsonResult = pdfviewer.GetAnnotationComments(jsonObject);
            return Content(JsonConvert.SerializeObject(jsonResult));
        }

        [AcceptVerbs("Post")]
        [HttpPost]
        [Route("api/[controller]/Unload")]
        public IActionResult Unload([FromBody] Dictionary<string, string> jsonObject)
        {
            PdfRenderer pdfviewer = new PdfRenderer(_cache);
            pdfviewer.ClearCache(jsonObject);
            return this.Content("Document cache is cleared");
        }

        [AcceptVerbs("Post")]
        [HttpPost]
        [Route("api/[controller]/RenderThumbnailImages")]
        public IActionResult RenderThumbnailImages([FromBody] Dictionary<string, string> jsonObject)
        {
            PdfRenderer pdfviewer = new PdfRenderer(_cache);
            object result = pdfviewer.GetThumbnailImages(jsonObject);
            return Content(JsonConvert.SerializeObject(result));
        }

        [AcceptVerbs("Post")]
        [HttpPost]
        [Route("api/[controller]/Bookmarks")]
        public IActionResult Bookmarks([FromBody] Dictionary<string, string> jsonObject)
        {
            PdfRenderer pdfviewer = new PdfRenderer(_cache);
            object jsonResult = pdfviewer.GetBookmarks(jsonObject);
            return Content(JsonConvert.SerializeObject(jsonResult));
        }

        [AcceptVerbs("Post")]
        [HttpPost]
        [Route("api/[controller]/Download")]
        public IActionResult Download([FromBody] Dictionary<string, string> jsonObject)
        {
            PdfRenderer pdfviewer = new PdfRenderer(_cache);
            string documentBase = pdfviewer.GetDocumentAsBase64(jsonObject);
            return Content(documentBase);
        }

        [AcceptVerbs("Post")]
        [HttpPost]
        [Route("api/[controller]/PrintImages")]
        public IActionResult PrintImages([FromBody] Dictionary<string, string> jsonObject)
        {

            //-------- Memorizzo l'operazione----------------------
            //try
            //{
            //    if (jsonObject["pageNumber"] == "0")
            //    {

            //        Guid idAllegato = Guid.Empty;
            //        //Guid idElemento = Guid.Empty;
            //        string[] id = jsonObject["documentId"].Split(';');
            //        if (id[0] == "riepilogo")
            //        {
            //            idAllegato = Guid.Parse(id[1]);
            //        }
            //        else
            //        {

            //            idAllegato = Guid.Parse(id[0].Replace(".pdf", ""));
            //            //if (id.Length>0)
            //            //    idElemento = Guid.Parse(id[1]);
            //        }
            //        if (idAllegato != Guid.Empty)
            //            _mailService._logMan.Salva(new LogDoc()
            //            {
            //                Data = DateTime.Now,
            //                IdOggetto = idAllegato,
            //                TipoOggetto = TipiOggetto.ALLEGATO,
            //                Utente = User.Identity.Name,
            //                Operazione = TipoOperazione.Stampato
            //            }, true);
            //        //if (idElemento != Guid.Empty)
            //        //    _mailService._logMan.Salva(new LogDoc()
            //        //    {
            //        //        Data = DateTime.Now,
            //        //        IdOggetto = idElemento,
            //        //        TipoOggetto = TipiOggetto.ELEMENTO,
            //        //        Utente = User.Identity.Name,
            //        //        Operazione = TipoOperazione.Stampato
            //        //    }, true);

            //        //-------- Memorizzo l'operazione----------------------
            //    }
            //}
            //catch (Exception ex)
            //{

            //}
            PdfRenderer pdfviewer = new PdfRenderer(_cache);
            object pageImage = pdfviewer.GetPrintImage(jsonObject);
            return Content(JsonConvert.SerializeObject(pageImage));
        }


        private const string notepath = "note";


        [AcceptVerbs("Post")]
        [HttpPost]
        [Route("api/[controller]/ExportAnnotations")]
        public IActionResult ExportAnnotations([FromBody] Dictionary<string, string> jsonObject)
        {

            string jsonResult = string.Empty;

            try
            {
                PdfRenderer pdfviewer = new PdfRenderer(_cache);
                jsonResult = pdfviewer.GetAnnotations(jsonObject);
                if (jsonObject != null && jsonObject.ContainsKey("document"))
                {
                    string documentPath = GetDocumentPath(jsonObject["document"]);
                    if (!string.IsNullOrEmpty(documentPath))
                    {
                        if (documentPath.StartsWith("{"))
                        {
                            PdfEditAction pdf = new PdfEditAction();
                            pdf = JsonConvert.DeserializeObject<PdfEditAction>(documentPath);
                            pdf.TempFolder = Path.Combine(_hostingEnvironment.WebRootPath, "_tmp");
                            byte[] b = Convert.FromBase64String(jsonResult.Split(",")[1]);
                            var json = System.Text.Encoding.UTF8.GetString(b);

                            //_allegatiService.SaveNoteString(pdf, json);
                            System.IO.File.WriteAllText(pdf.FileAnnotazioni, json);

                        }
                        else
                        {
#if TEST
                var path = Path.Combine(_hostingEnvironment.WebRootPath, notepath);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
#endif
                            byte[] b = Convert.FromBase64String(jsonResult.Split(",")[1]);

                            var json = System.Text.Encoding.UTF8.GetString(b);

#if TEST
                System.IO.File.WriteAllText(Path.Combine(path, jsonObject["document"] + ".json"), json);
#endif

                            _allegatiService.SaveNoteString(jsonObject, json);
                            _toastNotification.AddSuccessToastMessage("Note esportate correttamente!");

                        }
                    }
                }

            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage("Errore in esportazione delle note!");

            }
            return Ok();
        }


        [AcceptVerbs("Post")]
        [HttpPost]
        [Route("api/[controller]/ImportAnnotations")]
        public IActionResult ImportAnnotations([FromBody] Dictionary<string, string> jsonObject)
        {
            string jsonResult = string.Empty;
            try
            {
                PdfRenderer pdfviewer = new PdfRenderer(_cache);

                if (jsonObject != null && jsonObject.ContainsKey("document"))
                {
                    string documentPath = jsonObject["document"];
                    if (documentPath.StartsWith("{"))
                    {
                        PdfEditAction pdf = new PdfEditAction();
                        pdf = JsonConvert.DeserializeObject<PdfEditAction>(documentPath);
                        pdf.TempFolder = Path.Combine(_hostingEnvironment.WebRootPath, "_tmp");
                        //byte[] b = Convert.FromBase64String(jsonResult.Split(",")[1]);
                        //var json = System.Text.Encoding.UTF8.GetString(b);
                        if (System.IO.File.Exists(pdf.FileAnnotazioni))
                        {
                            jsonResult = System.IO.File.ReadAllText(pdf.FileAnnotazioni);
                        }
                        else
                        {
                            jsonResult = _allegatiService.GetNote2(pdf);
                        }
                    }
                    else
                    {
                        jsonResult = _allegatiService.GetNote2(jsonObject);
                    }

                }
            }
            catch (Exception ex)
            {
            }

            return Content(jsonResult);
        }
        private static PdfPageTemplateElement AddHeader(PdfDocument doc, Allegati al)
        {
            PdfStandardFont font = new PdfStandardFont(PdfFontFamily.TimesRoman, 8f);


            float margin = 40f;
            PdfFont bigFont = new PdfStandardFont(font, 16f);

            RectangleF bounds = new RectangleF(0, 0, doc.Pages[0].GetClientSize().Width, 50);
            PdfPageTemplateElement header = new PdfPageTemplateElement(bounds);


            PdfBrush orangeBrush = new PdfSolidBrush(new PdfColor(247, 148, 29));

            header.Graphics.DrawRectangle(orangeBrush, new RectangleF(PointF.Empty, new SizeF(header.Graphics.ClientSize.Width, margin)));

            //Draw the text to the PDF page
            var format = new PdfStringFormat();

            header.Graphics.DrawString("Note Documento", bigFont, PdfBrushes.White, new PointF(10, 10), format);
            //Draw description

            var brush = new PdfSolidBrush(Color.Black);
            font = new PdfStandardFont(PdfFontFamily.Helvetica, 8, PdfFontStyle.Bold);

            format = new PdfStringFormat();
            format.Alignment = PdfTextAlignment.Left;
            format.LineAlignment = PdfVerticalAlignment.Bottom;
            header.Graphics.DrawString(al.Descrizione, font, brush, new RectangleF(10, 0, header.Width, header.Height - 12), format);

            return header;
        }

        private PdfEditAction GetAction(string jsonDoc)
        {
            PdfEditAction pdfAct = new PdfEditAction();
            if (!string.IsNullOrEmpty(jsonDoc))
            {
                if (jsonDoc.StartsWith("{"))
                {
                    pdfAct = JsonConvert.DeserializeObject<PdfEditAction>(jsonDoc);
                    pdfAct.TempFolder = tempFolder;
                }
                else {
                    pdfAct.FilePdf = GetDocumentPath(jsonDoc);
                }
            }
            return pdfAct;
        }


        private string GetDocumentPath(string document)
        {
            string documentPath = string.Empty;
            if (!System.IO.File.Exists(document))
            {
                string basePath = _hostingEnvironment.WebRootPath;
                string dataPath = string.Empty;
                dataPath = basePath + @"/doc/";
                if (System.IO.File.Exists(dataPath + document))
                    documentPath = dataPath + document;
                else
                    documentPath = document;
            }
            else
            {
                documentPath = document;
            }
            return documentPath;
        }


        [AcceptVerbs("Post")]
        [HasPermission("50.1.3|50.1.4")]
        public async Task<PdfEditAction> GetPdfEditAction(string param)
        {
            PdfEditAction pdf = new PdfEditAction();
            try
            {
                if (!string.IsNullOrEmpty(param))
                {
                    pdf = JsonConvert.DeserializeObject<PdfEditAction>(param);

                    PdfEditAction chpdf;
                    if (_cache.TryGetValue(pdf.CacheEntry, out chpdf))
                    {
                        pdf = chpdf;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetPdfEditAction: {ex.Message}");
            }
            return await Task.FromResult(pdf);
        }

    }
}
