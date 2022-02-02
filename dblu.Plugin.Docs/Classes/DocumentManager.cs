using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syncfusion.Blazor.PdfViewerServer;
using Syncfusion.Blazor.PdfViewer;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.IO;
using dblu.Portale.Plugin.Docs.Services;
using dblu.Docs.Models;
using Syncfusion.Pdf.Parsing;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Interactive;
using PdfAnnotation = Syncfusion.Pdf.Interactive.PdfAnnotation;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Drawing;
using MimeKit;
using dblu.Docs.Extensions;
using System.IO.Compression;
using dblu.Docs.Classi;

namespace dblu.Portale.Plugin.Docs.Classes
{
    /// <summary>
    /// Type of document to manage
    /// </summary>
    public enum e_SourceType
    {
        /// <summary>
        /// Attachment
        /// </summary>
        Attachment,
        /// <summary>
        /// Item
        /// </summary>
        Item,
        /// <summary>
        /// A stream in Memory,
        /// Please not that in this case DocIdentifier must be : name of document(with extension) and the base 64 of the memory
        /// stream separated by a ;
        /// </summary>
        Memory
    };

    /// <summary>
    /// Docuemnt types recognized by the enging
    /// </summary>
    public enum e_DocType
    {
        /// <summary>
        /// Format not recognized
        /// </summary>
        UNDEFINED,
        /// <summary>
        /// PDF
        /// </summary>
        PDF,
        /// <summary>
        /// IMAGE (PNG, GIF, JPG)
        /// </summary>
        IMAGE,
        /// <summary>
        /// WORD_PROCESSOR (DOC, DOCX)
        /// </summary>
        WORD_PROCESSOR,
        /// <summary>
        /// SPREAD SHEET (XLS,XLSX)
        /// </summary>
        SPREAD_SHEET,
        /// <summary>
        /// EML documents
        /// </summary>
        EMAIL
    };

    /// <summary>
    /// Class that desribe a document processed by Document Manager
    /// </summary>
    public class Document
    {
        /// <summary>
        /// Name of the file
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Description of the document
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Type of the document
        /// </summary>
        public e_DocType DocType { get;set; }

        /// <summary>
        /// The document payload as MemoryStream
        /// </summary>
        public MemoryStream Payload { get; set; }

        /// <summary>
        /// List of origina attachments present in document (if document supports them)
        /// </summary>
        public List<OriginalAttachments> SourceAttachments { get; set; } = new();

        /// <summary>
        /// Indicates if doc is a transofrmation (a PDF generated from an EML or another type of content)
        /// </summary>
        public bool IsTransformation { get; set; } = false;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="nFileName">Name of the file</param>
        /// <param name="nPayload">Document payload as MemoryStream</param>
        /// <param name="nType">Specify how the document has to be managed</param>
        public Document(string nFileName,MemoryStream nPayload, e_DocType nType=e_DocType.UNDEFINED)
        {
            FileName = nFileName;
            Description = FileName;
            Payload = nPayload;
            if (nPayload is not null)
                nPayload.Position = 0;
            DocType = nType;
            if (DocType == e_DocType.UNDEFINED)
                DocType = GetDocType(FileName);
        }

        /// <summary>
        /// Processing the file name (extension) determinates the docuemtn type
        /// </summary>
        /// <param name="nFileName"></param>
        /// <returns></returns>
        public static e_DocType GetDocType(string nFileName)
        {
            string s = nFileName?.ToLowerInvariant()??"";
            if (s.EndsWith(".pdf")) return e_DocType.PDF;
            if (s.EndsWith(".png") || s.EndsWith(".gif") || s.EndsWith(".jpg") || s.EndsWith(".jpeg")) return e_DocType.IMAGE;
            if (s.EndsWith(".doc") || s.EndsWith(".docx")) return e_DocType.WORD_PROCESSOR;
            if (s.EndsWith(".xls") || s.EndsWith(".xlsx")) return e_DocType.SPREAD_SHEET;
            if (s.EndsWith(".eml")) return e_DocType.EMAIL;
            return e_DocType.UNDEFINED;
        }

        /// <summary>
        /// Get the current content as email, 
        /// Returns in particular the html body and the text body, if they are present
        /// </summary>
        /// <returns>Returns html and text body as a tupla</returns>
        public (string,string) AsEmail()
        {
            if (this.DocType == e_DocType.EMAIL || this.DocType == e_DocType.UNDEFINED)
            {
                if (this.Payload != null)
                    this.Payload.Position = 0;

                var message = MimeMessage.Load(this.Payload);
                var html = message.ToHtml();
                var body = message.TextBody;
                return (html,body);
            }
            return ("","");
        }
    };

    /// <summary>
    /// Class that load and process documents
    /// </summary>
    public class DocumentManager
    {
        /// <summary>
        /// Logger interface
        /// </summary>
        private ILogger Logger { get; set; }

        /// <summary>
        /// Attachment service for IO operations
        /// </summary>
        private AllegatiService AttachmentService { get; set; }

        /// <summary>
        /// Mail service for IO operations
        /// </summary>
        private DocumentTransformationService DocumentService { get; set; }

        /// <summary>
        /// Source of current document 
        /// </summary>
        public e_SourceType SourceType { get; set; }

        /// <summary>
        /// Identifier of the docuemtn (Attachment ID, Item ID)
        /// </summary>
        public string DocIdentifier { get; set; }

        /// <summary>
        /// Id of the attachment 
        /// </summary>
        private string AttachId { get; set; }

        /// <summary>
        /// Mark-up to apply to log ot identify the processed files
        /// </summary>
        private string LogMarkup { get; set; }

        /// <summary>
        /// Current Loaded document
        /// </summary>
        private Document Doc { get; set; }

        private string FileRootPath { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="nAttachmentService">Attachment service for IO operations</param>
        /// <param name="nDocumentService">Service for change document type (ex: from email to pdf)</param>
        /// <param name="nLog">Logger interface</param>
        /// <param name="nFileRootPath">File root path</param>
        /// 

        public DocumentManager(AllegatiService nAttachmentService,DocumentTransformationService nDocumentService, ILogger nLog, string nFileRootPath)
        {
            AttachmentService = nAttachmentService;
            DocumentService = nDocumentService;
            Logger = nLog;
            FileRootPath = nFileRootPath;
        }

        /// <summary>
        /// Load a specified document from Database
        /// </summary>
        /// <param name="nType">Type of the content to load</param>
        /// <param name="nContent">Identifier of the content</param>
        /// <returns>
        /// A document loaded
        /// </returns>
        public async Task<Document> Load(e_SourceType nType, string nContent, bool IsTransformationEnable = true)
        {

            if (string.IsNullOrEmpty(nContent)) return null;
            try
            {
                Stopwatch SW = Stopwatch.StartNew();
                SourceType = nType;
                DocIdentifier = nContent;
             

                string FileName = "";

                MemoryStream stream = new();
                
                switch (SourceType)
                {
                    case e_SourceType.Item:
                        List<Allegati> All = AttachmentService.GetAllegatiElemento(Guid.Parse(DocIdentifier));
                        Allegati A = All.Where(x => x.Tipo == "FILE" && Guid.TryParse(Path.GetFileNameWithoutExtension(x.NomeFile), out Guid guidOutput) && x.NomeFile.EndsWith("pdf")).OrderByDescending(c => c.DataC).FirstOrDefault();                        
                        if(A == null)
                            A = All.FirstOrDefault(x => x.Tipo == "FILE");
                        if (A is not null)
                        {
                            AttachId = A.Id.ToString();
                            LogMarkup = $"ELEMENT:{DocIdentifier} ATTACH:{AttachId}"; FileName = A?.NomeFile;                          
                            Doc = await OpenDocument(A, IsTransformationEnable);
                        }
                        break;
                    case e_SourceType.Attachment:
                        Allegati A1 = AttachmentService._allMan.Get(Guid.Parse(DocIdentifier));
                        if (A1 is not null)
                        {
                            LogMarkup = $"ATTACH:{nContent}"; FileName = A1?.NomeFile;
                            Doc = await OpenDocument(A1, IsTransformationEnable);
                        }
                        break;
                    case e_SourceType.Memory:
                        string[]tok= DocIdentifier.Split(";");
                        if (tok.Length == 2)           
                            return new Document($"{tok[0]}",new MemoryStream(Convert.FromBase64String(tok[1])));
                        throw new Exception("DocIdentier not recognized");
                        
                }
                Logger.LogInformation($"DocumentManager.Load[{LogMarkup}]: Loaded in {SW.ElapsedMilliseconds} ms");               
                return Doc;
            }
            catch (Exception ex)
            {
                Logger.LogError($"DocumentManager.Load[{LogMarkup}]: Unecpected error in {ex}");
                return null;
            }

        }
        /// <summary>
        /// Open an attachment doing eventually transformation
        /// </summary>
        /// <param name="A1">Attachment to open</param>
        /// <param name="IsTransformationEnable">Idnicate if transformation is required or not</param>
        /// <returns></returns>
        public async Task<Document> OpenDocument(Allegati A1, bool IsTransformationEnable = true)
        {
            try
            {
                e_DocType DetectedType = e_DocType.UNDEFINED;
                if (IsTransformationEnable)
                    switch (A1.Tipo)
                    {                       
                        case "ZIP":
                            MemoryStream PayloadZip = await AttachmentService._allMan.GetFileAsync(A1.Id.ToString());
                            string TextZIP = $"Da: {A1.elencoAttributi.Get("NomeSoggetto")} \nOggetto: {A1.Descrizione} \ndel: { A1.elencoAttributi.Get("Data")??A1.DataC}\n\n {A1.Testo} ";
                            var zip = DocumentService.PDF_From_ZIP(A1?.NomeFile, new ZipArchive(PayloadZip), TextZIP);
                            return new Document(A1?.NomeFile, zip.Payload, e_DocType.PDF) { Description = A1.Descrizione + " [ZIP]", SourceAttachments = zip.Attachments, IsTransformation = true };
                        case "REQ":
                            MemoryStream PayloadReq = await AttachmentService._allMan.GetFileAsync(A1.Id.ToString());
                            string TextREQ = $"Da: {A1.elencoAttributi.Get("NomeSoggetto")} \nOggetto: {A1.Descrizione} \ndel: { A1.elencoAttributi.Get("Data") ?? A1.DataC}\nRiferimento: {A1.elencoAttributi.Get("Riferimento")??""}\nTipo: {A1.elencoAttributi.Get("Tipo")??""}\n\n {A1.Testo} ";
                            var req = DocumentService.PDF_From_ZIP(A1?.NomeFile, new ZipArchive(PayloadReq), TextREQ);
                            return new Document(A1?.NomeFile, req.Payload, e_DocType.PDF) { Description = A1.Descrizione + " [REQ]", SourceAttachments = req.Attachments, IsTransformation = true };
                        case "EMAIL":
                            MemoryStream Payload = await AttachmentService._allMan.GetFileAsync(A1.Id.ToString());
                            var email = DocumentService.PDF_From_EMail(MimeMessage.Load(Payload), FileRootPath);
                            return new Document(A1?.NomeFile, email.Payload, e_DocType.PDF) { Description = A1.Descrizione + " [EMAIL]", SourceAttachments = email.Attachments, IsTransformation = true };
                        case "FILE":
                        default:
                            //E' un file generico, provo a vedere se e' trasformabile in PDF in base al tipo (estensione)
                            //Le immagini non le traformo perche le faccio vedere cosi come sono
                            var stream = await AttachmentService._allMan.GetFileAsync(A1.Id.ToString());
                            DetectedType = Document.GetDocType(A1?.NomeFile);
                            if (IsTransformationEnable && DetectedType == e_DocType.WORD_PROCESSOR)
                                return new Document(A1?.NomeFile, DocumentService.PDF_From_WordProcessor(A1?.NomeFile, stream).Payload, e_DocType.PDF) { Description = A1?.NomeFile + " [DOCUMENT]", IsTransformation = true };
                            if (IsTransformationEnable && DetectedType == e_DocType.SPREAD_SHEET)
                                return new Document(A1?.NomeFile, DocumentService.PDF_From_SpreadSheet(A1?.NomeFile, stream).Payload, e_DocType.PDF) { Description = A1?.NomeFile + " [DOCUMENT]", IsTransformation = true };
                            return new Document(A1?.NomeFile, stream, DetectedType);
                    }
                else
                {
                    var stream = await AttachmentService._allMan.GetFileAsync(A1.Id.ToString());
                    return new Document(A1?.NomeFile, stream, DetectedType);
                }
               
            }catch(Exception ex)
                {
                Logger.LogError($"DocumentManager.OpenDocument[{LogMarkup}]: Unecpected error in {ex}");
                return null; 
                }
        }

        /// <summary>
        /// Save the document
        /// Override the document with received payload
        /// </summary>
        /// <param name="stream">Document that must be sasved</param>
        /// <returns>
        /// The current saved document
        /// </returns>
        public async Task<Document> Save(MemoryStream stream=null)
        {
            if (Doc.DocType != e_DocType.PDF) return Doc;

            if (stream == null) stream = Doc.Payload;

       
            try
            {
                Stopwatch SW = Stopwatch.StartNew();
                if (stream != null && stream.Length != 0)
                {
                    MemoryStream Optimized = new();
                    PdfLoadedDocument loadedDocument = new PdfLoadedDocument(stream);

                    loadedDocument.Save(Optimized);
                    if (!Doc.IsTransformation)
                    {
                        switch (SourceType)
                        {
                            case e_SourceType.Item:
                                await AttachmentService._allMan.SalvaFileAsync(AttachId, Optimized);
                                break;

                            case e_SourceType.Attachment:
                                await AttachmentService._allMan.SalvaFileAsync(DocIdentifier, Optimized);
                                break;
                        }
                    }
                    Doc.Payload = Optimized;
                    Logger.LogInformation($"DocumentManager.Save[{LogMarkup}]: Saved in {SW.ElapsedMilliseconds} ms");
                    return Doc;
                }
                else Logger.LogError($"DocumentManager.Save[{LogMarkup}]: Empty stream!!");
            }
            catch (Exception ex)
            {
                Logger.LogError($"DocumentManager.Save[{LogMarkup}]: Unexpected error in {ex}");
            }
            return null;
        }

        /// <summary>
        /// Rotate the page to right
        /// </summary>
        /// <param name="Page">Page to rotate</param>
        /// <param name="stream">Document to edit, if different from last one</param>
        /// <returns>The current changed document</returns>
        public async Task<Document> RotateRight(int Page, MemoryStream stream=null)
        {
            if (Doc.DocType != e_DocType.PDF) return Doc;

            try
            {
                Stopwatch SW = Stopwatch.StartNew();
                if (stream == null) stream = Doc.Payload;

                if (stream != null && stream.Length != 0)
                {
                    MemoryStream M = new MemoryStream();
                    PdfLoadedDocument loadedDocument = new PdfLoadedDocument(stream);
                    PdfPageBase page = loadedDocument.Pages[Page - 1] as PdfPageBase;
                    switch (page.Rotation)
                    {
                        case PdfPageRotateAngle.RotateAngle0:
                            page.Rotation = PdfPageRotateAngle.RotateAngle90;
                            foreach (PdfLoadedAnnotation A in page.Annotations)
                            {
                                string t = A.Text;
                                A.Rotate = PdfAnnotationRotateAngle.RotateAngle270;
                                A.Text = t;
                            }
                            break;
                        case PdfPageRotateAngle.RotateAngle90:
                            page.Rotation = PdfPageRotateAngle.RotateAngle180;
                            foreach (PdfLoadedAnnotation A in page.Annotations)
                            {
                                string t = A.Text;
                                A.Rotate = PdfAnnotationRotateAngle.RotateAngle180;
                                A.Text = t;
                            }
                            break;
                        case PdfPageRotateAngle.RotateAngle180:
                            page.Rotation = PdfPageRotateAngle.RotateAngle270;
                            foreach (PdfLoadedAnnotation A in page.Annotations)
                            {
                                string t = A.Text;
                                A.Rotate = PdfAnnotationRotateAngle.RotateAngle90;
                                A.Text = t;
                            }
                            break;
                        case PdfPageRotateAngle.RotateAngle270:
                            page.Rotation = PdfPageRotateAngle.RotateAngle0;
                            foreach (PdfLoadedAnnotation A in page.Annotations)
                            {
                                string t = A.Text;
                                A.Rotate = PdfAnnotationRotateAngle.RotateAngle0;
                                A.Text = t;
                            }
                            break;
                    }



                    loadedDocument.Save(M);
                    Doc.Payload = M;
                    Logger.LogInformation($"DocumentManager.RotateRight[{LogMarkup}]: Done in {SW.ElapsedMilliseconds} ms");
                    return Doc;
                }
            }catch(Exception ex)
            {
                Logger.LogError($"DocumentManager.RotateRight[{LogMarkup}]: Unexpected error in {ex}");
            }
            return Doc;
        }

        /// <summary>
        /// Rotate the page to left
        /// </summary>
        /// <param name="Page">Page to rotate</param>
        /// <param name="stream">Document to edit, if different from last one</param>
        /// <returns>The current changed document</returns>
        public async Task<Document> RotateLeft(int Page, MemoryStream stream=null)
        {
            if (Doc.DocType != e_DocType.PDF) return Doc;

            try
            {
                Stopwatch SW = Stopwatch.StartNew();
                if (stream == null) stream = Doc.Payload;

                if (stream != null && stream.Length != 0)
                {
                    MemoryStream M = new MemoryStream();
                    PdfLoadedDocument loadedDocument = new PdfLoadedDocument(stream);
                    PdfPageBase page = loadedDocument.Pages[Page - 1] as PdfPageBase;

                    switch (page.Rotation)
                    {
                        case PdfPageRotateAngle.RotateAngle0:
                            page.Rotation = PdfPageRotateAngle.RotateAngle270;
                            foreach (PdfLoadedAnnotation A in page.Annotations)
                            {
                                string t = A.Text;
                                A.Rotate = PdfAnnotationRotateAngle.RotateAngle90;
                                A.Text = t;
                            }
                            break;
                        case PdfPageRotateAngle.RotateAngle90:
                            page.Rotation = PdfPageRotateAngle.RotateAngle0;
                            foreach (PdfLoadedAnnotation A in page.Annotations)
                            {
                                string t = A.Text;
                                A.Rotate = PdfAnnotationRotateAngle.RotateAngle0;
                                A.Text = t;
                            }
                            break;
                        case PdfPageRotateAngle.RotateAngle180:
                            page.Rotation = PdfPageRotateAngle.RotateAngle90;
                            foreach (PdfLoadedAnnotation A in page.Annotations)
                            {
                                string t = A.Text;
                                A.Rotate = PdfAnnotationRotateAngle.RotateAngle270;
                                A.Text = t;
                            }
                            break;
                        case PdfPageRotateAngle.RotateAngle270:
                            page.Rotation = PdfPageRotateAngle.RotateAngle180;
                            foreach (PdfLoadedAnnotation A in page.Annotations)
                            {
                                string t = A.Text;
                                A.Rotate = PdfAnnotationRotateAngle.RotateAngle180;
                                A.Text = t;
                            }

                            
                            break;
                    }
                    loadedDocument.Save(M);
                    Doc.Payload = M;
                    Logger.LogInformation($"DocumentManager.RotateLeft[{LogMarkup}]: Done in {SW.ElapsedMilliseconds} ms");
                    return Doc;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"DocumentManager.RotateLeft[{LogMarkup}]: Unexpected error in {ex}");
            }
            return Doc;
        }

        /// <summary>
        /// Remove the selected page
        /// </summary>
        /// <param name="Page">Page to delete</param>
        /// <param name="stream">Document to edit, if different from last one</param>
        /// <returns>The current changed document</returns>
        public async Task<Document> RemovePage(int Page, MemoryStream stream = null)
        {
            if (Doc.DocType != e_DocType.PDF) return Doc;

            try
            {
                Stopwatch SW = Stopwatch.StartNew();
                if (stream == null) stream = Doc.Payload;

                if (stream != null && stream.Length != 0)
                {
                    MemoryStream M = new MemoryStream();
                    PdfLoadedDocument loadedDocument = new PdfLoadedDocument(stream);
                    loadedDocument.Pages.RemoveAt(Page - 1);
                    loadedDocument.Save(M);
                    Doc.Payload = M;
                    Logger.LogInformation($"DocumentManager.RemovePage[{LogMarkup}]: Removed Page {Page} in {SW.ElapsedMilliseconds} ms");
                    return Doc;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"DocumentManager.RemovePage[{LogMarkup}]: Unexpected error in {ex}");
            }
            return Doc;
        }

    }
}
