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
        Item
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
        /// WORD_PROCESSOR (DOC, RTF)
        /// </summary>
        WORD_PROCESSOR,
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
        /// Type of the document
        /// </summary>
        public e_DocType DocType { get;set; }

        /// <summary>
        /// The document payload as MemoryStream
        /// </summary>
        public MemoryStream Payload { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="nFileName">Name of the file</param>
        /// <param name="nPayload">Document payload as MemoryStream</param>
        public Document(string nFileName,MemoryStream nPayload )
        {
            FileName = nFileName;
            Payload = nPayload;
            if (nPayload is not null)
                nPayload.Position = 0;
            DocType = GetDocType(FileName);
        }

        /// <summary>
        /// Processing the file name (extension) determinates the docuemtn type
        /// </summary>
        /// <param name="nFileName"></param>
        /// <returns></returns>
        private  e_DocType GetDocType(string nFileName)
        {
            string s = nFileName?.ToLowerInvariant()??"";
            if (s.EndsWith(".pdf")) return e_DocType.PDF;
            if (s.EndsWith(".png") || s.EndsWith(".gif") || s.EndsWith(".jpg")) return e_DocType.IMAGE;
            //if (s.EndsWith(".doc") || s.EndsWith(".rtf")) return e_DocType.WORD_PROCESSOR;
            if(s.EndsWith(".eml")) return e_DocType.EMAIL;
            return e_DocType.UNDEFINED;
        }

        /// <summary>
        /// Get the memory stream as an HTML string (in case of an email)
        /// </summary>
        /// <returns>
        /// HTML string 
        /// </returns>
        public string ToHtml()
        {
            if (this.DocType == e_DocType.EMAIL)
            {
                if (this.Payload != null)
                    this.Payload.Position = 0;

                var message = MimeMessage.Load(this.Payload);
                var html = message.ToHtml();
                if (string.IsNullOrEmpty(html))
                {
                    html = message.TextBody;
                }
                return html;
            }
            return "";
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

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="nAttachmentService">Attachment service for IO operations</param>
        /// <param name="nLog">Logger interface</param>
        public DocumentManager(AllegatiService nAttachmentService, ILogger nLog)
        {
            AttachmentService = nAttachmentService;
            Logger = nLog;
        }

        /// <summary>
        /// Load a specified document from Database
        /// </summary>
        /// <param name="nType">Type of the content to load</param>
        /// <param name="nContent">Identifier of the content</param>
        /// <returns>
        /// A document loaded
        /// </returns>
        public async Task<Document> Load(e_SourceType nType, string nContent)
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
                        Allegati A = AttachmentService.GetAllegatiElemento(Guid.Parse(DocIdentifier)).FirstOrDefault(x => x.Tipo == "FILE");
                        if (A is not null)
                        {
                            AttachId = A.Id.ToString();
                            LogMarkup = $"ELEMENT:{DocIdentifier} ATTACH:{AttachId}"; FileName = A?.NomeFile;
                            stream = await AttachmentService._allMan.GetFileAsync(A.Id.ToString());
                            
                        }
                        break;
                    case e_SourceType.Attachment:
                        Allegati A1 = AttachmentService._allMan.Get(Guid.Parse(DocIdentifier));
                        LogMarkup = $"ATTACH:{nContent}"; FileName = A1?.NomeFile;
                        stream = await AttachmentService._allMan.GetFileAsync(nContent);
                        break;
                }
                Logger.LogInformation($"DocumentManager.Load[{LogMarkup}]: Loaded in {SW.ElapsedMilliseconds} ms");
                Doc= new Document(FileName, stream);
                return Doc;
            }
            catch (Exception ex)
            {
                Logger.LogError($"DocumentManager.Load[{LogMarkup}]: Unecpected error in {ex}");
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
                    foreach (PdfLoadedPage P in loadedDocument.Pages)
                        foreach (PdfLoadedAnnotation A in P.Annotations)
                            A.Flatten = true;

                    loadedDocument.Save(Optimized);

                    switch (SourceType)
                    {
                        case e_SourceType.Item:
                            await AttachmentService._allMan.SalvaFileAsync(AttachId, Optimized);
                            break;

                        case e_SourceType.Attachment:
                            await AttachmentService._allMan.SalvaFileAsync(DocIdentifier, Optimized);
                            break;
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
                            break;
                        case PdfPageRotateAngle.RotateAngle90:
                            page.Rotation = PdfPageRotateAngle.RotateAngle180;
                            break;
                        case PdfPageRotateAngle.RotateAngle180:
                            page.Rotation = PdfPageRotateAngle.RotateAngle270;
                            break;
                        case PdfPageRotateAngle.RotateAngle270:
                            page.Rotation = PdfPageRotateAngle.RotateAngle0;
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
                            break;
                        case PdfPageRotateAngle.RotateAngle90:
                            page.Rotation = PdfPageRotateAngle.RotateAngle0;
                            break;
                        case PdfPageRotateAngle.RotateAngle180:
                            page.Rotation = PdfPageRotateAngle.RotateAngle90;
                            break;
                        case PdfPageRotateAngle.RotateAngle270:
                            page.Rotation = PdfPageRotateAngle.RotateAngle180;
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
