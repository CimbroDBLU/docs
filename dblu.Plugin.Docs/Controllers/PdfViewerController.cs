using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Syncfusion.EJ2.PdfViewer;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using dblu.Portale.Plugin.Docs.Data;
using dblu.Portale.Plugin.Docs.Services;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;
using NToastNotify;
using Microsoft.Extensions.Logging;
using dblu.Docs.Models;
using dblu.Docs.Classi;
using Microsoft.AspNetCore.Authorization;
using dblu.Portale.Core.Infrastructure;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Drawing;
using Syncfusion.Pdf.Parsing;
using System.Text;
using Syncfusion.Pdf.Tables;
using Syncfusion.Pdf.Grid;
using System.Data;
using Syncfusion.Pdf.Interactive;
using dblu.Portale.Plugin.Docs.Models;

namespace dblu.Portale.Plugin.Documenti
{


    public partial class PdfViewerController : Controller
    {
        private IMemoryCache _cache;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private IConfiguration _config;
        private readonly IToastNotification _toastNotification;
        public readonly ILogger _logger;
        private PdfEditService _pdfsvc;

        private AllegatiService _allegatiService;
        private readonly dbluDocsContext _context;
        public PdfViewerController(IWebHostEnvironment hostingEnvironment, 
            IMemoryCache cache, IConfiguration config,   AllegatiService allegatiService, IToastNotification toastNotification,
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
            _toastNotification = toastNotification;
            _pdfsvc = pdfsvc;
        }
        // GET: /<controller>/
        public IActionResult PdfViewerFeatures()
        {
            return View();
        }

        [AcceptVerbs("Post")]
        [HttpPost]
        [Route("api/[controller]/Load")]
        [Authorize]
        [HasPermission("50.1.2")]
        public IActionResult Load([FromBody] Dictionary<string, string> jsonObject)
        {
            PdfRenderer pdfviewer = new PdfRenderer(_cache);
            MemoryStream stream = new MemoryStream();
            object jsonResult = new object();
            if (jsonObject != null && jsonObject.ContainsKey("document"))
            {
                if (bool.Parse(jsonObject["isFileName"]))
                {
                    string documentPath = GetDocumentPath(jsonObject["document"]);
                    if (!string.IsNullOrEmpty(documentPath))
                    {
                        if (documentPath.Contains("\\"))
                        {
                            byte[] bytes = System.IO.File.ReadAllBytes(documentPath);
                            stream = new MemoryStream(bytes);
                        }
                        else
                        {
                        var id = jsonObject["document"];
                        var allMan = new AllegatiManager(_context.Connessione, _logger);
                        
                        var m = allMan.GetFileAsync(id).Result;

                        //byte[] bytes = System.IO.File.ReadAllBytes(documentPath);
                        stream = m; // new MemoryStream(bytes);
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
            catch (Exception)
            {
                _toastNotification.AddErrorToastMessage("Errore in esportazione delle note!");

            }


#if TEST
            return Content(jsonResult);
#else
            return Ok();
#endif

        }


        [AcceptVerbs("Post")]
        [HttpPost]
        [Route("api/[controller]/ImportAnnotations")]
        public IActionResult ImportAnnotations([FromBody] Dictionary<string, string> jsonObject)
        {
            PdfRenderer pdfviewer = new PdfRenderer(_cache);
            string jsonResult = string.Empty;
            if (jsonObject != null && jsonObject.ContainsKey("document"))
            {

               jsonResult =  _allegatiService.GetNote2(jsonObject);

#if TEST
                string documentPath = Path.Combine(_hostingEnvironment.WebRootPath, notepath, jsonObject["document"] + ".json");   //GetDocumentPath(jsonObject["document"]);
                if (!string.IsNullOrEmpty(documentPath))
                {
                    jsonResult = System.IO.File.ReadAllText(documentPath);
                }
                else
                {
                    return Content(jsonObject["document"] + " is not found");
                }
#endif
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
            
            header.Graphics.DrawString("Note Documento" , bigFont, PdfBrushes.White, new PointF(10, 10),format);
            //Draw description

            var brush = new PdfSolidBrush(Color.Black);
            font = new PdfStandardFont(PdfFontFamily.Helvetica, 8, PdfFontStyle.Bold);

            format = new PdfStringFormat();
            format.Alignment = PdfTextAlignment.Left;
            format.LineAlignment = PdfVerticalAlignment.Bottom;
            header.Graphics.DrawString(al.Descrizione, font, brush, new RectangleF(10, 0, header.Width, header.Height - 12), format);

            return header;
        }

        private void AddFooter(string text,PdfDocument doc)
        {
            RectangleF rect = new RectangleF(0, 0, doc.Pages[0].GetClientSize().Width, 50);

            //Create a page template
            PdfPageTemplateElement footer = new PdfPageTemplateElement(rect);
            PdfFont font = new PdfStandardFont(PdfFontFamily.Helvetica, 8);

            PdfSolidBrush brush = new PdfSolidBrush(Color.Gray);

            PdfPen pen = new PdfPen(Color.DarkBlue, 3f);
            font = new PdfStandardFont(PdfFontFamily.Helvetica, 8, PdfFontStyle.Bold);
            PdfStringFormat format = new PdfStringFormat();
            format.Alignment = PdfTextAlignment.Center;
            format.LineAlignment = PdfVerticalAlignment.Middle;
            footer.Graphics.DrawString(text, font, brush, new RectangleF(0, 18, footer.Width, footer.Height), format);

            format = new PdfStringFormat();
            format.Alignment = PdfTextAlignment.Right;
            format.LineAlignment = PdfVerticalAlignment.Bottom;

            doc.Template.Bottom = footer;
           
        }

        private string AddNote(Allegati al, MemoryStream m)
        {


            PdfLoadedDocument loadedDocument = new PdfLoadedDocument(m);
            PdfDocument document = new PdfDocument();
           

             
            int startIndex = 0;

            int endIndex = loadedDocument.Pages.Count - 1;

            //Import all the pages to the new PDF document.

            document.ImportPageRange(loadedDocument, startIndex, endIndex);

            this.AddPagina(document, al);  
            this.AddFooter(al.Descrizione, document);

            


            //Creating the stream object

            MemoryStream stream = new MemoryStream();

            //Save the document into stream

            document.Save(stream);

            var str = Convert.ToBase64String(stream.ToArray());

            //If the position is not set to '0' then the PDF will be empty.

            stream.Position = 0;

            //Close the document.

            document.Close(true);


            return str;

        }

        private void AddPagina(PdfDocument document, Allegati al)
        {

            //
          
            PdfSection section = document.Sections.Add();

            //Create page settings to the section
            section.PageSettings.Rotate = PdfPageRotateAngle.RotateAngle0;
            section.PageSettings.Size = PdfPageSize.A4;
            

            //Add page to the section and initialize graphics for the page
            PdfPage page = section.Pages.Add();
            PdfGraphics g = page.Graphics;

            page.Section.Template.Top = AddHeader(document, al);



            #region Tabella

             
            PdfGrid grid = new PdfGrid();
           List<PdfNoteTable> data = new List<PdfNoteTable>();
            
            var datasource = _allegatiService.GetNoteList(al.Id);
            foreach (var item in datasource)
            {
                data.Add(new PdfNoteTable { Data= item.datemodifica, Utente= item.utentemodifica, Contenuto = item.contenuto });
            }

            grid.DataSource = data;
            grid.ApplyBuiltinStyle(PdfGridBuiltinStyle.GridTable4Accent1);
            grid.Style.CellPadding = new PdfPaddings(10, 10, 10, 10);
          
           
            PdfGridCellStyle cellStyle = new PdfGridCellStyle();
            cellStyle.Borders.All = PdfPens.White;
            PdfGridRow header = grid.Headers[0];
            


            //Creates the header style
            PdfGridCellStyle headerStyle = new PdfGridCellStyle();
            headerStyle.Borders.All = new PdfPen(new PdfColor(126, 151, 173));
            headerStyle.BackgroundBrush = new PdfSolidBrush(new PdfColor(126, 151, 173));
            headerStyle.TextBrush = PdfBrushes.White;
            headerStyle.Font = new PdfStandardFont(PdfFontFamily.TimesRoman, 14f, PdfFontStyle.Regular);
            headerStyle.CellPadding = new PdfPaddings(5, 5, 5, 5);

            grid.BeginCellLayout += Grid_BeginCellLayout;
            //Adds cell customizations
            for (int i = 0; i < header.Cells.Count; i++)
            {
                header.Cells[i].StringFormat = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);
            }


            header.ApplyStyle(headerStyle);
            cellStyle.Borders.Bottom = new PdfPen(new PdfColor(217, 217, 217), 0.70f);
            cellStyle.Font = new PdfStandardFont(PdfFontFamily.TimesRoman, 14f);
   cellStyle.StringFormat = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);
            PdfGridLayoutFormat layoutFormat = new PdfGridLayoutFormat();

            layoutFormat.Layout = PdfLayoutType.Paginate;
           
            //Draws the grid to the PDF page.
            PdfGridLayoutResult gridResult = grid.Draw(page, new RectangleF(new PointF(0, page.Section.Template.Top.Size.Height + 40), new SizeF(page.Graphics.ClientSize.Width, page.Graphics.ClientSize.Height - 80)), layoutFormat);


            #endregion

        }

        
        private void Grid_BeginCellLayout(object sender, PdfGridBeginCellLayoutEventArgs args)
        {

        }

        private void AddTabella(PdfDocument document)
        {
            throw new NotImplementedException();
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

        public Dictionary<string, string> JsonConverter(jsonObjects results)
        {
            Dictionary<string, object> resultObjects = new Dictionary<string, object>();
            resultObjects = results.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .ToDictionary(prop => prop.Name, prop => prop.GetValue(results, null));
            var emptyObjects = (from kv in resultObjects
                where kv.Value != null
                select kv).ToDictionary(kv => kv.Key, kv => kv.Value);
            Dictionary<string, string> jsonResult = emptyObjects.ToDictionary(k => k.Key, k => k.Value.ToString());
            return jsonResult;
        }

        // metodo demo per prendere un pdf dal db
        // viene trasformato in base64 e rimandato al client
        public object GetDocument()
        {
            string documentID = "Xyz.pdf";
            string constr = _config["ConnectionStrings:CamundaDbConnection"];
           SqlConnection con = new SqlConnection(constr);
            //Searches for the PDF document from the Database 
            var query = "select Data from PdfDocuments where DocumentName = '" + documentID + "'";
            SqlCommand cmd = new SqlCommand(query);
            cmd.Connection = con;
            con.Open();
            SqlDataReader read = cmd.ExecuteReader();
            read.Read();
            // Reads the PDF document data as byte array from the database 
            byte[] byteArray = (byte[])read["Data"];
            //converts byte array into base64 string 
            return "data:application/pdf;base64," + Convert.ToBase64String(byteArray);

        }


        [AcceptVerbs("Post")]
        [HasPermission("50.1.2")]
        public async Task<PdfEditAction> EditorPdf(string param)
        {
            PdfEditAction pdf = new PdfEditAction();
            try
            {
                if (!string.IsNullOrEmpty(param))
                {
                    pdf = JsonConvert.DeserializeObject<PdfEditAction>(param);
                }

                PdfEditAction spdf = null;
                string sobj = HttpContext.Session.GetString("PdfViewerEditAction");
                if (!string.IsNullOrEmpty(sobj))
                {
                    spdf = JsonConvert.DeserializeObject<PdfEditAction>(sobj);
                }

                switch (pdf.Azione)
                {
                    case Azioni.Carica:
                        if (spdf != null)
                        {
                            if (!string.IsNullOrEmpty(spdf.FilePdf) && System.IO.File.Exists(spdf.FilePdf))
                            {
                                System.IO.File.Delete(spdf.FilePdf);
                            }
                        }
                        
                        pdf.FilePdf = "";
                        string NomePdf = Path.Combine(_hostingEnvironment.WebRootPath, "_tmp", $"{pdf.IdAllegato}.pdf");
                        if (System.IO.File.Exists(NomePdf)){
                            System.IO.File.Delete(NomePdf);
                        }
                       
                        var allMan = new AllegatiManager(_context.Connessione, _logger);
                        var m = allMan.GetFileAsync(pdf.IdAllegato).Result;
                        using (FileStream fspdf = new FileStream(NomePdf, FileMode.CreateNew, FileAccess.ReadWrite))
                        {
                            m.Position = 0;
                            m.CopyTo(fspdf);
                        }
                        
                        if (System.IO.File.Exists(NomePdf))
                            pdf.FilePdf = NomePdf;

                        break;
                    case Azioni.CaricaRiepilogo:
                        break;
                    case Azioni.RuotaPagina90:
                    case Azioni.RuotaPagina270:
                    case Azioni.CancellaPagina:

                        _pdfsvc.Modifica(pdf);
                        break;
                    case Azioni.Salva:

                        if (System.IO.File.Exists(pdf.FilePdf))
                        {
                           var allMan1 = new AllegatiManager(_context.Connessione, _logger);
                            var ms = new MemoryStream(System.IO.File.ReadAllBytes(pdf.FilePdf));
                            ms.Position = 0;
                            await allMan1.SalvaFileAsync(pdf.IdAllegato, ms);
                        }
                        break;
                    default:
                        break;
                }
                HttpContext.Session.SetString("PdfViewerEditAction", JsonConvert.SerializeObject(pdf));

            }
            catch (Exception ex)
            {
                _logger.LogError($"EditorPdf : {ex.Message}");
            }
            return await Task.FromResult(pdf);
        }


    }


    public class PdfNoteTable
    {
        public string Utente { get; set; }
        public DateTime Data { get; set; }
        public string Contenuto { get; set; }
    }

    public class jsonObjects
    {
        public string document { get; set; }
        public string password { get; set; }
        public string zoomFactor { get; set; }
        public string isFileName { get; set; }
        public string xCoordinate { get; set; }
        public string yCoordinate { get; set; }
        public string pageNumber { get; set; }
        public string documentId { get; set; }
        public string hashId { get; set; }
        public string sizeX { get; set; }
        public string sizeY { get; set; }
        public string startPage { get; set; }
        public string endPage { get; set; }
    }
}

