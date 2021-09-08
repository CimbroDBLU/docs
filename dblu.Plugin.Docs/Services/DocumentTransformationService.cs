using dblu.Docs.Classi;
using dblu.Docs.Extensions;
using dblu.Portale.Plugin.Docs.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using Syncfusion.Drawing;
using Syncfusion.HtmlConverter;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Interactive;
using Syncfusion.Pdf.Parsing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Reporting;
using Telerik.Reporting.Processing;
using Telerik.Windows.Documents.Model;

namespace dblu.Portale.Plugin.Docs.Services
{
    /// <summary>
    /// Attachments inside documents and how they have been processed
    /// </summary>
    public class OriginalAttachments
    {
        /// <summary>
        /// Id of the attach
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Name of the attach
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Idnicate if it has been considere valid
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Indicates if it has been included
        /// </summary>
        public bool IsIncluded { get; set; }

        /// <summary>
        /// Warning generated during process
        /// </summary>
        public string Warning { get; set; }
    }

    /// <summary>
    /// Processed document
    /// </summary>
    public class ProcessedDocument
    {
        /// <summary>
        /// The result document
        /// </summary>
        public MemoryStream Payload { get; set; } = new();
        /// <summary>
        /// The list of the processed attacdhments, with operation done on them
        /// </summary>
        public List<OriginalAttachments> Attachments { get; set; } = new();

    }

    /// <summary>
    /// Service for transform documents in PDF
    /// </summary>
    public class DocumentTransformationService
    {
        /// <summary>
        /// Injected app enviroinment
        /// </summary>
        private IWebHostEnvironment _appEnvironment { get; set; }

        /// <summary>
        /// Injected configuration
        /// </summary>
        private IConfiguration _config { get; set; }

        /// <summary>
        /// Injected logger
        /// </summary>
        private ILogger _logger { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="config">Injected configuration</param>
        /// <param name="loggerFactory">Injected logger factory</param>
        /// <param name="appEnvironment">Injected app enviroinmentn</param>
        public DocumentTransformationService(IConfiguration config, ILoggerFactory loggerFactory, IWebHostEnvironment appEnvironment)
        {
            _config = config;
            _appEnvironment = appEnvironment;
            _logger = loggerFactory.CreateLogger("DocumentService");
        }

        /// <summary>
        /// Trnasofr a Mail in a PFD
        /// </summary>
        /// <param name="Message">Mime Mesasge to transorm</param>
        /// <returns>
        /// The transofrmed document
        /// </returns>
        public ProcessedDocument PDF_From_EMail(MimeMessage Message)
        {
            ProcessedDocument ret= new ProcessedDocument();
            Stopwatch Sw1 = Stopwatch.StartNew();

            int.TryParse(_config["Docs:CompressioneImmagini"], out int Quality);
            if (Quality < 0 || Quality > 100) Quality = 50;
            try
            {         
                var mittente = $"{Message.From.Mailboxes.First().Name} ({Message.From.Mailboxes.First().Address})";
                var oggetto = Message.Subject;
                var txt = Message.TextBody == null ? "" : Message.TextBody;
                var htxt = Message.ToHtml();
                var pdfstream = new MemoryStream();
                var ListaPdf = new List<MemoryStream>();

                PdfDocument document = new PdfDocument();
                HtmlToPdfConverter htmlConverter = new HtmlToPdfConverter(HtmlRenderingEngine.WebKit);
                WebKitConverterSettings settings = new WebKitConverterSettings();

                if (htxt == "")
                {
                    if (txt != null)
                    {
                        PdfPage page = document.Pages.Add();
                        PdfGraphics graphics = page.Graphics;
                        PdfTextElement textElement = new PdfTextElement($"Da: {mittente} \nOggetto: {oggetto} \ndel: {Message.Date.ToLocalTime().ToString("dd/MM/yyyy HH:mm")} \n\n {txt} ", new PdfStandardFont(PdfFontFamily.Helvetica, 10));
                        textElement.Draw(page, new Syncfusion.Drawing.RectangleF(0, 0, page.GetClientSize().Width, page.GetClientSize().Height));
                        document.Save(pdfstream);
                        document.Close(true);
                    }
                }
                else   // conversione html in pdf
                {
                    try
                    {

                        if (!htxt.Contains("<body"))
                        {
                            htxt = $"<body>{htxt}</body>";
                        }
                        htxt = AUX_CleanHTML(htxt);
                        int bodys = htxt.IndexOf("<body");
                        if (bodys >= 0)
                        {
                            int bodye = htxt.IndexOf(">", bodys);
                            htxt = htxt.Substring(0, bodye + 1) +
                                $"<div><b>Da: </b>{mittente}<br><b>Oggetto: </b>{oggetto}<br><b>del: </b>{Message.Date.ToLocalTime().ToString("dd/MM/yyyy HH:mm ")}</div><br>"
                                + htxt.Substring(bodye + 1);
                        }

                        string baseUrl = Path.Combine(_appEnvironment.WebRootPath, "_tmp");

                        settings.WebKitPath = _config["Docs:PercorsoWebKit"];
                        settings.EnableJavaScript = false;
                        settings.EnableHyperLink = false;
                        settings.EnableOfflineMode = true;
                        settings.SplitTextLines = true;
                        settings.SplitImages = true;
                        settings.Margin.Right = 0;
                        settings.Margin.Left = 0;
                        settings.Margin.Top = 0;
                        settings.Margin.Bottom = 0;
                        htmlConverter.ConverterSettings = settings;
                        document = htmlConverter.Convert(htxt, baseUrl);
                        document.Save(pdfstream);
                        document.Close(true);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"DocumentTransformationService.PDF_From_EMail: Unable to include text {ex.Message}");
                    }

                }

                ListaPdf.Add(pdfstream);

                int i = 0;
                foreach (var attachment in Message.Allegati())
                {
                    var fileName = "";
                    var m = new MemoryStream();
                    var incluso = false;
                    string avvisi = "";
                    i++;
                    if (attachment is MessagePart)
                    {
                        fileName = attachment.ContentDisposition?.FileName;
                        var rfc822 = (MessagePart)attachment;

                        if (string.IsNullOrEmpty(fileName))
                            fileName = "email-allegata.eml";
                        rfc822.Message.WriteTo(m);
                    }
                    else
                    {
                        var part = (MimePart)attachment;
                        fileName = part.NomeAllegato(i);

                        if (part.FileName is null && part is TextPart)
                        {
                            var tp = (TextPart)part;
                            if (tp.IsHtml && tp.IsAttachment)
                            {
                                continue;
                            }
                        }
                        part.Content.DecodeTo(m);
                    }
                    try
                    {
                        switch (System.IO.Path.GetExtension(fileName).ToLower())
                        {
                            case ".pdf":
                                MemoryStream m1 = AUX_AdjustPDFStream(m, fileName, out avvisi);
                                if (m1 != null)
                                {
                                    m1.Position = 0;
                                    ListaPdf.Add(m1);
                                    incluso = true;
                                }
                                m.Close();
                                m.Dispose();
                                break;
                            case ".jpg":
                            case ".jpeg":
                            case ".png":
                                try
                                {

                                    PdfImage image = new PdfBitmap(m);
                                    float shrinkFactor;
                                    float myWidth = image.Width;
                                    float myHeight = image.Height;

                                    if (myWidth > 100 && myHeight > 100)
                                    {
                                        document = new PdfDocument();

                                        if (image.Width > image.Height)
                                            document.PageSettings.Orientation = PdfPageOrientation.Landscape;

                                        PdfPage page = document.Pages.Add();
                                        float PageWidth = page.Graphics.ClientSize.Width;
                                        float PageHeight = page.Graphics.ClientSize.Height;
                                        PdfGraphics graphics = page.Graphics;

                                        if (myWidth > PageWidth)
                                        {
                                            shrinkFactor = myWidth / PageWidth;
                                            myWidth = PageWidth;
                                            myHeight = myHeight / shrinkFactor;
                                        }

                                        if (myHeight > PageHeight)
                                        {
                                            shrinkFactor = myHeight / PageHeight;
                                            myHeight = PageHeight;
                                            myWidth = myWidth / shrinkFactor;
                                        }

                                        float XPosition = (PageWidth - myWidth) / 2;
                                        float YPosition = (PageHeight - myHeight) / 2;
                                        graphics.DrawImage(image, XPosition, YPosition, myWidth, myHeight);

                                        pdfstream = new MemoryStream();
                                        document.Save(pdfstream);
                                        document.Close(true);


                                        ListaPdf.Add(pdfstream);
                                        incluso = true;
                                    }
                                    else
                                    {
                                        incluso = false;
                                    }

                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError($"DocumentTransformationService.PDF_From_EMail: Unable to include file {fileName}. {ex.Message}");
                                }
                                break;
                            default:
                                avvisi = "file non supportato.";
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"DocumentTransformationService.PDF_From_EMail: Unexpected error on attachs {ex.Message}");
                    }                    
                    ret.Attachments.Add(new OriginalAttachments() {Id=fileName,Name=fileName,IsValid=false,IsIncluded=incluso, Warning=avvisi});
                }

                if (ListaPdf.Count() == 0)
                {
                    // Aggiungo pagina riepilogo
                    PdfPage page = document.Pages.Add();
                    PdfGraphics graphics = page.Graphics;
                    PdfTextElement textElement = new PdfTextElement($"Da: {mittente} \nOggetto: {oggetto} \n\n {txt} ", new PdfStandardFont(PdfFontFamily.Helvetica, 10));
                    textElement.Draw(page, new Syncfusion.Drawing.RectangleF(0, 0, page.GetClientSize().Width, page.GetClientSize().Height));
                    document.Save(pdfstream);
                    document.Close(true);
                    ListaPdf.Add(pdfstream);
                }

                if (ListaPdf.Count() > 0)
                {

                    using (PdfDocument finalDoc = new PdfDocument())
                    {
                        //Fondo gli altri allegati
                        try
                        {
                            foreach (Stream s in ListaPdf)
                            {
                                s.Position = 0;
                                PdfLoadedDocument l = new PdfLoadedDocument(s);
                                for (int q = 0; q < l.PageCount; q++)
                                {
                                    try
                                    {
                                        finalDoc.ImportPage(l, q);
                                    }
                                    catch (Exception)
                                    {
                                        _logger.LogError($"DocumentTransformationService.PDF_From_EMail: Unable to import page {q}, skipping it...");
                                    }
                                }
                            }

                            ///Comprimo il tutto
                            ret.Payload = new();
                            finalDoc.Save(ret.Payload);
                            if (Quality != 0)
                            {
                                Stopwatch SW = Stopwatch.StartNew();

                                finalDoc.Save(ret.Payload);
                                long unc = ret.Payload?.Length ?? 0;
                                ret.Payload = AUX_CompressPDF(ret.Payload, Quality);
                                long com = ret.Payload?.Length ?? 0;

                                _logger.LogInformation($"DocumentTransformationService.PDF_From_EMail: ONFLY Compression {unc}->{com} = {((unc - com) * 100.0) / unc:f}% in {SW.ElapsedMilliseconds} ms");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"DocumentTransformationService.PDF_From_EMail: Unexpected error on merging {ex.Message}");
                        }
                    foreach (var ms in ListaPdf)
                         ms.Close();
                    finalDoc.Close(true);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"DocumentTransformationService.PDF_From_EMail: General Unexptected error {ex.Message}");

            }
            _logger.LogInformation($"DocumentTransformationService.PDF_From_EMail: Document [{Message.MessageId}] processed in {Sw1.ElapsedMilliseconds} ms");
            return ret;
        }

        /// <summary>
        /// Trnasofr a ZipArchive in a PFD
        /// </summary>
        /// <param name="Message">Mime Mesasge to transorm</param>
        /// <returns>
        /// The transofrmed document
        /// </returns>
        public ProcessedDocument PDF_From_ZIP(string Filename,ZipArchive ZipFile, string Text = "")
        {
            ProcessedDocument ret = new ProcessedDocument();
            Stopwatch Sw1 = Stopwatch.StartNew();

            int.TryParse(_config["Docs:CompressioneImmagini"], out int Quality);
            if (Quality < 0 || Quality > 100) Quality = 50;

            try
            {
                var pdfstream = new MemoryStream();
                var ListaPdf = new List<MemoryStream>();
                PdfDocument document = new PdfDocument();


                if (Text.Length > 0)
                {
                    PdfPage page = document.Pages.Add();

                    PdfGraphics graphics = page.Graphics;
                    PdfTextElement textElement = new PdfTextElement(Text, new PdfStandardFont(PdfFontFamily.Helvetica, 10));
                    textElement.Draw(page, new Syncfusion.Drawing.RectangleF(0, 0, page.GetClientSize().Width, page.GetClientSize().Height));
                    document.Save(pdfstream);
                    document.Close(true);
                    ListaPdf.Add(pdfstream);

                }
                foreach (ZipArchiveEntry entry in ZipFile.Entries)
                {
                    {
                        var fileName = entry.Name;
                        var m = new MemoryStream();
                        var incluso = false;
                        string avvisi = "";
                        try
                        {
                            switch (System.IO.Path.GetExtension(fileName).ToLower())
                            {
                                case ".pdf":
                                    try
                                    {
                                        using (var unzippedEntryStream = entry.Open())
                                        {
                                            unzippedEntryStream.CopyTo(m);
                                        }
                                        MemoryStream m1 = AUX_AdjustPDFStream(m, fileName, out avvisi);
                                        if (m1 != null)
                                        {
                                            m1.Position = 0;
                                            ListaPdf.Add(m1);
                                            incluso = true;
                                        }
                                        m.Close();
                                        m.Dispose();
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogError($"DocumentTransformationService.PDF_From_ZIP: Unable to include file {fileName} {ex.Message}");
                                    }
                                    break;
                                case ".jpg":
                                case ".jpeg":
                                case ".png":
                                    try
                                    {
                                        using (var unzippedEntryStream = entry.Open())
                                            unzippedEntryStream.CopyTo(m);

                                        document = new PdfDocument();

                                        PdfImage image = new PdfBitmap(m);

                                        float shrinkFactor;
                                        float myWidth = image.Width;
                                        float myHeight = image.Height;

                                        if (image.Width > image.Height)
                                            document.PageSettings.Orientation = PdfPageOrientation.Landscape;

                                        PdfPage page = document.Pages.Add();
                                        float PageWidth = page.Graphics.ClientSize.Width;
                                        float PageHeight = page.Graphics.ClientSize.Height;
                                        PdfGraphics graphics = page.Graphics;

                                        if (myWidth > PageWidth)
                                        {
                                            shrinkFactor = myWidth / PageWidth;
                                            myWidth = PageWidth;
                                            myHeight = myHeight / shrinkFactor;
                                        }

                                        if (myHeight > PageHeight)
                                        {
                                            shrinkFactor = myHeight / PageHeight;
                                            myHeight = PageHeight;
                                            myWidth = myWidth / shrinkFactor;
                                        }

                                        float XPosition = (PageWidth - myWidth) / 2;
                                        float YPosition = (PageHeight - myHeight) / 2;
                                        graphics.DrawImage(image, XPosition, YPosition, myWidth, myHeight);

                                        pdfstream = new MemoryStream();
                                        document.Save(pdfstream);
                                        document.Close(true);

                                        //Close the document
                                        ListaPdf.Add(pdfstream);
                                        incluso = true;
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogError($"DocumentTransformationService.PDF_From_ZIP: Unable to include file {fileName} {ex.Message}");                                        
                                    }
                                    break;
                                default:
                                    avvisi = "file non supportato.";
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"DocumentTransformationService.PDF_From_ZIP: Unexpected error {ex.Message}");
                        }
                        ret.Attachments.Add(new OriginalAttachments() { Id = fileName, Name = fileName, IsValid = false, IsIncluded = incluso, Warning = avvisi });
                    }
                }
                if (ListaPdf.Count() == 0)
                {
                    // Aggiungo la pagina di intestazione
                    PdfPage page = document.Pages.Add();
                    PdfGraphics graphics = page.Graphics;
                    PdfTextElement textElement = new PdfTextElement($"Nome del file: {Filename} ", new PdfStandardFont(PdfFontFamily.Helvetica, 10));
                    textElement.Draw(page, new Syncfusion.Drawing.RectangleF(0, 0, page.GetClientSize().Width, page.GetClientSize().Height));
                    document.Save(pdfstream);
                    document.Close(true);
                    ListaPdf.Add(pdfstream);
                }

                if (ListaPdf.Count() > 0)
                {

                    using (PdfDocument finalDoc = new PdfDocument())
                    {
                        //Aggiungo gli altri pdf
                        try
                        {
                            PdfDocumentBase.Merge(finalDoc, ListaPdf.ToArray());

                            ///Comprimo il tutto                           
                            finalDoc.Save(ret.Payload);
                            if (Quality != 0)
                            {
                                Stopwatch SW = Stopwatch.StartNew();
                                ret.Payload = new();
                                finalDoc.Save(ret.Payload);
                                long unc = ret.Payload?.Length ?? 0;
                                ret.Payload = AUX_CompressPDF(ret.Payload, Quality);
                                long com = ret.Payload?.Length ?? 0;

                                _logger.LogInformation($"DocumentTransformationService.PDF_From_ZIP: ONFLY Compression {unc}->{com} = {((unc - com) * 100.0) / unc:f}% in {SW.ElapsedMilliseconds} ms");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"CreaTmpPdfCompleto: {ex.Message}");
                        }
                        foreach (var ms in ListaPdf)
                            ms.Close();
                        finalDoc.Close(true);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"CreaTmpPdfCompleto: {ex.Message}");

            }

            _logger.LogInformation($"DocumentTransformationService.PDF_From_ZIP: ZIP {Filename} processed in {Sw1.ElapsedMilliseconds} ms");
            return ret;
        }
        
        
        /// <summary>
        /// Sign a PDF Memory stream, adding required labels
        /// </summary>
        /// <param name="Doc">Document PDF to process (as MemoryStream)</param>
        /// <param name="att">Attributes od the document</param>
        /// <returns>
        /// A tupla with 2 values:
        /// 1) A bool value to indicate if the operation ends properly
        /// 2) A memorystream properly signed 
        /// </returns>
        public (bool,MemoryStream) SignPDF(MemoryStream Doc, ElencoAttributi att)
        {
            try
            {
                string rpt = _config["Docs:EtichettaProtocollo"];
                if (string.IsNullOrEmpty(rpt))
                    return (true, Doc);
                string etichetta = Path.Combine(_appEnvironment.ContentRootPath, "Report", rpt);

                if (!File.Exists(etichetta))
                {
                    _logger.LogError($"DocumentTransformationService.SignPDF: Label {etichetta} not found!");
                    return (false, new MemoryStream());
                }

                Telerik.Reporting.Report eti;
                var reportPackager = new ReportPackager();

                using (var sourceStream = System.IO.File.OpenRead(etichetta))
                {
                    eti = (Telerik.Reporting.Report)reportPackager.UnpackageDocument(sourceStream);
                }
                var reportProcessor = new Telerik.Reporting.Processing.ReportProcessor();
                var reportSource = new Telerik.Reporting.InstanceReportSource();
                reportSource.ReportDocument = eti;

                foreach (Attributo a in att.ToList())
                {
                    if (a.Valore != null)
                    {
                        reportSource.Parameters.Add(a.Nome, a.Valore == null ? "" : a.Valore);
                    }
                }

                Telerik.Documents.Primitives.Size A4 = PaperTypeConverter.ToSize(PaperTypes.A4);

                if (Doc != null)
                {

                    Syncfusion.Pdf.Parsing.PdfLoadedDocument pdftmp = new Syncfusion.Pdf.Parsing.PdfLoadedDocument(Doc);

                    Syncfusion.Pdf.PdfDocument document = new Syncfusion.Pdf.PdfDocument();
                    document.PageSettings.SetMargins(0);

                    int i = 0;
                    foreach (Syncfusion.Pdf.PdfLoadedPage lptmp in pdftmp.Pages)
                    {
                        try
                        {
                            Syncfusion.Pdf.Graphics.PdfTemplate template = lptmp.CreateTemplate();
                            Syncfusion.Pdf.PdfPage page = document.Pages.Add();

                            Syncfusion.Pdf.Graphics.PdfGraphics graphics = page.Graphics;
                            float etiHeight = page.Size.Height * .05F + 5;


                            Syncfusion.Drawing.PointF posizione = new Syncfusion.Drawing.PointF() { X = 0, Y = etiHeight + 1 };

                            Syncfusion.Drawing.SizeF pDest = AUX_ResizeKeepingRatio(lptmp.Size.Width, lptmp.Size.Height, page.Size.Width * 0.95F, page.Size.Height - etiHeight);

                            switch (lptmp.Rotation)
                            {
                                case Syncfusion.Pdf.PdfPageRotateAngle.RotateAngle90:
                                    if (pDest.Height < pDest.Width)
                                    {
                                        graphics.TranslateTransform(page.Size.Width, etiHeight);
                                        graphics.RotateTransform(90);
                                        posizione = new Syncfusion.Drawing.PointF() { X = 0, Y = 0 };
                                    }

                                    graphics.DrawPdfTemplate(template, posizione, pDest);

                                    if (pDest.Height < pDest.Width)
                                    {
                                        graphics.RotateTransform(-90);
                                        graphics.TranslateTransform(-page.Size.Width, -etiHeight);
                                    }

                                    break;
                                case Syncfusion.Pdf.PdfPageRotateAngle.RotateAngle270:
                                    if (pDest.Height < pDest.Width)
                                    {
                                        graphics.TranslateTransform(0, page.Size.Height);
                                        graphics.RotateTransform(-90);
                                        posizione = new Syncfusion.Drawing.PointF() { X = 0, Y = 0 };
                                    }
                                    graphics.DrawPdfTemplate(template, posizione, pDest);

                                    if (pDest.Height < pDest.Width)
                                    {
                                        graphics.RotateTransform(90);
                                        graphics.TranslateTransform(0, -page.Size.Height);
                                    }
                                    break;
                                default:
                                    if (pDest.Height < pDest.Width)
                                    {
                                        graphics.TranslateTransform(0, page.Size.Height);
                                        graphics.RotateTransform(-90);
                                        posizione = new Syncfusion.Drawing.PointF() { X = 0, Y = 0 };
                                    }
                                    graphics.DrawPdfTemplate(template, posizione, pDest);

                                    if (pDest.Height < pDest.Width)
                                    {
                                        graphics.RotateTransform(90);
                                        graphics.TranslateTransform(0, -page.Size.Height);
                                    }
                                    break;
                            }

                            i++;
                            reportSource.Parameters.Add("NPag", i);
                            reportSource.Parameters.Add("TPag", pdftmp.Pages.Count);

                            RenderingResult curEti = reportProcessor.RenderReport("PDF", reportSource, null);
                            Syncfusion.Pdf.Parsing.PdfLoadedDocument pdfEti = new Syncfusion.Pdf.Parsing.PdfLoadedDocument(new MemoryStream(curEti.DocumentBytes));
                            posizione = new Syncfusion.Drawing.PointF() { X = 0, Y = 5 };
                            Syncfusion.Pdf.PdfLoadedPage loadedPage = pdfEti.Pages[0] as Syncfusion.Pdf.PdfLoadedPage;
                            template = loadedPage.CreateTemplate();

                            graphics.DrawPdfTemplate(template, posizione,
                                new Syncfusion.Drawing.SizeF(loadedPage.Size.Width, loadedPage.Size.Height));
                            pdfEti.Close();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"DocumentTransformationService.SignPDF: Error at page {i + 1}: {ex.Message}");
                            document.ImportPageRange(pdftmp, i, i);
                            i++;
                        }

                    }
                    pdftmp.Close();


                    MemoryStream mpdf = new MemoryStream();
                    document.Save(mpdf);
                    document.Close();
                    mpdf.Position = 0;
                    return (true, mpdf);
                }
                return (false, new MemoryStream());
            }
            catch(Exception ex)
            {
                _logger.LogError($"DocumentTransformationService.SignPDF: Unexpected error {ex}");
                return (false, new MemoryStream());
            }
        }

        /// <summary>
        /// Clean an HTML string , removing annoyng tags for PDF rendering
        /// </summary>
        /// <param name="htxt">HTML text to process</param>
        /// <returns>A Clean html string</returns>
        private string AUX_CleanHTML(string htxt)
        {

            htxt = htxt.Replace("<img", "<img hidden");
            htxt = htxt.Replace("<pre", "<p").Replace("</pre>", "</p>");
            htxt = htxt.Replace("MS Sans Serif", "sans-serif");

            int i = htxt.IndexOf("face=\"MS");
            int f = i > 0 ? htxt.IndexOf("Serif\"", i) : -1;
            while (f > i)
            {
                htxt = htxt.Substring(0, i + 6) + "sans-serif" + htxt.Substring(f + 5);
                i = htxt.IndexOf("face=\"MS", i + 6);
                f = i > 0 ? htxt.IndexOf("Serif\"", i) : -1;
            }

            return htxt;
        }

        /// <summary>
        /// Compress a PDf stram 
        /// </summary>
        /// <param name="MS">Source Memory Stream</param>
        /// <param name="CompressRatio">Compress Ratio</param>
        /// <returns>
        /// Memory stream compressed
        /// </returns>
        private  MemoryStream AUX_CompressPDF(MemoryStream MS, int CompressRatio = 50)
        {
            try
            {
                PdfLoadedDocument loadedDocument = new(MS);
                loadedDocument.Compression = PdfCompressionLevel.Best;
                PdfCompressionOptions options = new();
                options.CompressImages = true;
                options.ImageQuality = CompressRatio;
                loadedDocument.Compress(options);
                MemoryStream MSOut = new();
                loadedDocument.Save(MSOut);
                return MSOut;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Adjust PDF stream (Ex elaborastreamPDF)
        /// Apply optimization to the PDF
        /// </summary>
        /// <param name="Stream">Srteam to process</param>
        /// <param name="FileName">Original file name</param>
        /// <param name="Notifications">Issues detected</param>
        /// <returns>
        /// A Elaborated memory stream
        /// </returns>
        private MemoryStream AUX_AdjustPDFStream(MemoryStream Stream, string FileName, out string Notifications)
        {
            Notifications = "";
            try
            {


                var flAnn = false;  //contiene annotazioni
                var flResize = false;  // richiede resize

                Stream.Position = 0;
                var A4Size = PdfPageSize.A4;
                PdfLoadedDocument ld = null;
                try
                {
                    ld = new PdfLoadedDocument(Stream);
                    if (ld.PageCount == 0) throw new Exception();
                }
                catch
                {
                    ld = new PdfLoadedDocument(Stream, true);
                    flAnn = true;  // forzo la copia dello stream corretto
                }
                MemoryStream m2 = null;
                List<string> ControllaPag = new List<string>();
                var flNoteManuali = false;
                var pn = 0;
                foreach (PdfLoadedPage p in ld.Pages)
                {
                    pn++;
                    flNoteManuali = false;
                    //controllo se le dimensioni superano a4
                    if (p.Size.Width > p.Size.Height)
                    {
                        if (p.Size.Height > (A4Size.Width + 1f) || p.Size.Width > (A4Size.Height + 1f))
                        {
                            flResize = true;
                        }
                    }
                    else
                    {
                        if (p.Size.Width > (A4Size.Width + 1f) || p.Size.Height > (A4Size.Height + 1f))
                        {
                            flResize = true;
                        }
                    }

                    //  appiattisco le note 


                    if (p.Annotations.Count > 0)
                    {
                        flAnn = true;
                        PdfAnnotationCollection xx = new PdfAnnotationCollection();
                        for (int i = p.Annotations.Count - 1; i >= 0; i--)
                        {
                            PdfAnnotation nn = (PdfAnnotation)p.Annotations[i];

                            if (nn is PdfLoadedFreeTextAnnotation)
                            {


                                if (((Syncfusion.Pdf.Interactive.PdfLoadedFreeTextAnnotation)nn).AnnotationIntent == PdfAnnotationIntent.FreeTextTypeWriter || ((Syncfusion.Pdf.Interactive.PdfLoadedFreeTextAnnotation)nn).BorderColor.IsEmpty == true)
                                {


                                    flNoteManuali = true;
                                    try
                                    {

                                        p.Annotations.Remove(nn);

                                    }
                                    catch (Exception ex)
                                    {
                                        nn.Flatten = true;
                                        nn.AnnotationFlags = PdfAnnotationFlags.Hidden;
                                        _logger.LogError($"DocumentTransformationService.AUX_AdjustPDFStream: Unable to remove notes. {ex.Message}");
                                    }



                                    PdfStringFormat drawFormat = new PdfStringFormat();

                                    drawFormat.WordWrap = PdfWordWrapType.Word;
                                    PdfFont font = (Syncfusion.Pdf.Graphics.PdfStandardFont)((Syncfusion.Pdf.Interactive.PdfLoadedFreeTextAnnotation)nn).Font;
                                    if (font.Size == 0)
                                    {
                                        PdfFont pdfFont = font;
                                        var fontSize = pdfFont.Size;

                                        fontSize = 10f;
                                        if (font is PdfCjkStandardFont)
                                            pdfFont = new PdfCjkStandardFont((PdfCjkStandardFont)font, fontSize, font.Style);
                                        else if (font is PdfStandardFont)
                                            pdfFont = new PdfStandardFont((PdfStandardFont)font, fontSize, font.Style);
                                        else if (font is PdfTrueTypeFont)
                                            pdfFont = new PdfTrueTypeFont((PdfTrueTypeFont)font, fontSize);
                                        font = pdfFont;
                                    }
                                    PdfBrush brush = new PdfSolidBrush(((Syncfusion.Pdf.Interactive.PdfLoadedFreeTextAnnotation)nn).TextMarkupColor);
                                    RectangleF bounds;

                                    if (p.Rotation == PdfPageRotateAngle.RotateAngle90)
                                    {
                                        bounds = new RectangleF(p.Graphics.ClientSize.Width - nn.Bounds.Y - nn.Bounds.Height, nn.Bounds.X, nn.Bounds.Height, nn.Bounds.Width);
                                    }
                                    else
                                    {
                                        bounds = new RectangleF(nn.Bounds.X, nn.Bounds.Y, nn.Bounds.Width, nn.Bounds.Height);
                                    }

                                    PdfStringLayouter layouter = new PdfStringLayouter();
                                    PdfStringLayoutResult result = layouter.Layout(nn.Text, font, drawFormat, new SizeF(bounds.Width, bounds.Height));
                                    if (result.Remainder != null && result.Remainder.Length != 0)
                                    {
                                        PdfFont pdfFont = font;
                                        var fontSize = pdfFont.Size;
                                        while (fontSize > 0f && (pdfFont.MeasureString(nn.Text, drawFormat).Height + 0.5 > bounds.Height || pdfFont.MeasureString(nn.Text, drawFormat).Width + 0.5 > bounds.Width))
                                        {
                                            fontSize -= 0.1f;
                                            if (font is PdfCjkStandardFont)
                                                pdfFont = new PdfCjkStandardFont((PdfCjkStandardFont)font, fontSize, font.Style);
                                            else if (font is PdfStandardFont)
                                                pdfFont = new PdfStandardFont((PdfStandardFont)font, fontSize, font.Style);
                                            else if (font is PdfTrueTypeFont)
                                                pdfFont = new PdfTrueTypeFont((PdfTrueTypeFont)font, fontSize);
                                        }
                                        font = pdfFont;
                                    }


                                    p.Graphics.DrawString(nn.Text, font, brush, bounds, drawFormat);
                                }
                                else
                                {
                                    nn.Flatten = true;
                                    flNoteManuali = true;
                                }
                            }

                            else if (nn is PdfLoadedTextMarkupAnnotation)
                            {
                                nn.Flatten = true;
                                flNoteManuali = true;
                            }
                            else if (nn is PdfLoadedInkAnnotation)
                            {
                                flNoteManuali = true;
                                nn.Flatten = true;
                            }
                            else if (nn is PdfLoadedRubberStampAnnotation)
                            {
                                flNoteManuali = true;
                            }
                            else if (nn is PdfLoadedLineAnnotation)
                            {
                                if (((PdfLoadedLineAnnotation)nn).EndLineStyle != PdfLineEndingStyle.None)
                                {
                                    nn.Flatten = true;
                                }
                                else
                                {
                                    flNoteManuali = true;
                                    try
                                    {

                                        p.Annotations.Remove(nn);
                                        PdfLoadedLineAnnotation yy = (PdfLoadedLineAnnotation)nn;

                                    }
                                    catch (Exception ex)
                                    {
                                        nn.Flatten = true;
                                        nn.AnnotationFlags = PdfAnnotationFlags.Hidden;
                                        _logger.LogError($"DocumentTransformationService.AUX_AdjustPDFStream: Unable to remove note. {ex.Message}");
                                    }
                                    int[] xLine = ((PdfLoadedLineAnnotation)nn).LinePoints;
                                    PdfPen xPen = new PdfPen(nn.Color, ((PdfLoadedLineAnnotation)nn).Border.Width);

                                    if (p.Rotation == PdfPageRotateAngle.RotateAngle90)
                                    {
                                        p.Graphics.DrawLine(xPen, new PointF(xLine[1], xLine[0]), new PointF(xLine[3], xLine[2]));
                                    }
                                    else
                                    {
                                        p.Graphics.DrawLine(xPen, new PointF(xLine[0], p.Graphics.ClientSize.Height - xLine[1]), new PointF(xLine[2], p.Graphics.ClientSize.Height - xLine[3]));
                                    }
                                }
                            }
                            else
                            {

                                nn.Flatten = true;

                            }
                        }
                        ////p.Annotations.Flatten = true;
                        //foreach (PdfAnnotation nn in p.Annotations)
                        //{
                        //    flAnn = true;
                        //    nn.Flatten = true;
                        //    if (nn is PdfLoadedInkAnnotation || nn is PdfLoadedFreeTextAnnotation)    // 18.4.39 non funziona flatten per le note manuali
                        //    {
                        //        flNoteManuali = true;
                        //    }
                        //}
                    }

                    if (flNoteManuali)
                    {
                        ControllaPag.Add((pn).ToString());
                        _logger.LogWarning($"DocumentTransformationService.AUX_AdjustPDFStream: Notes at page {pn} in [{FileName}]");
                    }

                };

                if (ControllaPag.Count > 0)
                {
                    Notifications = $" Controllare le note manuali (pag. {string.Join(",", ControllaPag)})";
                }

                if (flAnn)
                {
                    // ricarico il documento
                    try
                    {
                        m2 = new MemoryStream();
                        ld.Save(m2);
                        ld.Close();
                        ld = new PdfLoadedDocument(m2);
                    }
                    catch (Exception ex)
                    {
                        flAnn = false;
                        Notifications += " Impossibile importare le note. ";
                        _logger.LogError($"DocumentTransformationService.AUX_AdjustPDFStream:: Unable to import notes {FileName}. {ex.Message}");
                    }
                }

                if (flResize)
                {
                    PdfDocument doc1 = new PdfDocument();
                    foreach (PdfPageBase p in ld.Pages)
                    {

                        PdfPage page = doc1.Pages.Add();


                        page.Section.PageSettings.Margins.All = 0;

                        PdfGraphics g = page.Graphics;
                        PdfTemplate template = p.CreateTemplate();

                        // g.DrawPdfTemplate(template, PointF.Empty, PdfPageSize.A4);
                        Syncfusion.Drawing.PointF posizione = new Syncfusion.Drawing.PointF() { X = 0, Y = 0 };
                        Syncfusion.Drawing.SizeF pDest = AUX_ResizeKeepingRatio(p.Size.Width, p.Size.Height, page.Size.Width, page.Size.Height);


                        switch (p.Rotation)
                        {
                            case Syncfusion.Pdf.PdfPageRotateAngle.RotateAngle90:
                                if (pDest.Height < pDest.Width)
                                {
                                    g.TranslateTransform(page.Size.Width, 0);
                                    g.RotateTransform(90);
                                }
                                g.DrawPdfTemplate(template, posizione, pDest);

                                if (pDest.Height < pDest.Width)
                                {

                                    g.RotateTransform(-90);
                                    g.TranslateTransform(-page.Size.Width, 0);
                                }

                                break;
                            case Syncfusion.Pdf.PdfPageRotateAngle.RotateAngle270:
                                if (pDest.Height < pDest.Width)
                                {
                                    g.TranslateTransform(0, page.Size.Height);
                                    g.RotateTransform(-90);
                                }
                                g.DrawPdfTemplate(template, posizione, pDest);

                                if (pDest.Height < pDest.Width)
                                {
                                    g.RotateTransform(90);
                                    g.TranslateTransform(0, -page.Size.Height);
                                }
                                break;
                            default:
                                if (pDest.Height < pDest.Width)
                                {
                                    g.TranslateTransform(0, page.Size.Height);
                                    g.RotateTransform(-90);
                                    //page.Section.PageSettings.Orientation = PdfPageOrientation.Landscape;
                                }
                                g.DrawPdfTemplate(template, posizione, pDest);



                                if (pDest.Height < pDest.Width)
                                {
                                    //page.Section.PageSettings.Orientation = PdfPageOrientation.Landscape;
                                    g.RotateTransform(90);
                                    g.TranslateTransform(0, -page.Size.Height);
                                }

                                break;
                        }


                    }
                    if (flAnn)
                    {
                        m2.Close();
                    }
                    m2 = new MemoryStream();
                    doc1.Save(m2);
                }

                if (!flAnn && !flResize)
                {
                    m2 = new MemoryStream();
                    Stream.Position = 0;
                    Stream.CopyTo(m2);
                }
                m2.Position = 0;
                return m2;
            }
            catch (Exception ex)
            {
                Notifications = "Impossibile includere il file";
                _logger.LogError($"DocumentTransformationService.AUX_AdjustPDFStream:: Unable to include file {FileName}. {ex.Message}");
            }
            return null;
        }

        /// <summary>
        /// Rezied an area keeping the razio
        /// </summary>
        /// <param name="OrigWidth">Original Width</param>
        /// <param name="OrigHeight">Original Height</param>
        /// <param name="DestWidth">Destination Width</param>
        /// <param name="DestHeight">Destination Height</param>
        /// <returns></returns>
        public SizeF AUX_ResizeKeepingRatio(float OrigWidth, float OrigHeight, float DestWidth, float DestHeight)
        {

            Syncfusion.Drawing.SizeF r = new Syncfusion.Drawing.SizeF(OrigWidth, OrigHeight);
            float shrinkFactor = 1;
            if (OrigHeight > OrigWidth)
            {
                if (OrigHeight > DestHeight)
                {
                    shrinkFactor = OrigHeight / DestHeight;
                    r.Height = DestHeight;
                    r.Width = r.Width / shrinkFactor;
                }
                if (r.Width > DestWidth)
                {
                    shrinkFactor = r.Width / DestWidth;
                    r.Width = DestWidth;
                    r.Height = r.Height / shrinkFactor;
                }
            }
            else
            { //inverto le dimensioni di destinazione
                if (OrigHeight > DestWidth)
                {
                    shrinkFactor = OrigHeight / DestWidth;
                    r.Height = DestWidth;
                    r.Width = r.Width / shrinkFactor;
                }
                if (r.Width > DestHeight)
                {
                    shrinkFactor = r.Width / DestHeight;
                    r.Width = DestHeight;
                    r.Height = r.Height / shrinkFactor;
                }
            }
            return r;
        }
    }
}
