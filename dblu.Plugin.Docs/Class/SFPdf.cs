﻿using Syncfusion.Pdf;
using Syncfusion.HtmlConverter;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Drawing;
using System.Collections.Generic;
using MimeKit;
using System.IO;
using dblu.Docs.Extensions;
using System.Linq;
using System;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Telerik.Reporting;
using Syncfusion.Pdf.Parsing;
using System.Text;
using dblu.Portale.Plugin.Docs.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using System.IO.Compression;
using Telerik.Reporting.Processing;
using dblu.Docs.Models;
using dblu.Docs.Classi;
using Telerik.Windows.Documents.Model;
using Syncfusion.Pdf.Interactive;

namespace dblu.Portale.Plugin.Docs.Class
{
    class SFPdf
    {

        private readonly ILogger _logger;
        private readonly IWebHostEnvironment _appEnvironment;
        private IConfiguration _config { get; }
        private AllegatiManager _allMan;

        public SFPdf( IWebHostEnvironment appEnvironment,
            ILogger logger,
            IConfiguration config,
            AllegatiManager am
            )
        {
            _appEnvironment = appEnvironment;
            _logger = logger;
            _config = config;
            _allMan = am;
        }


        public List<EmailAttachments> CreaTmpPdfCompletoSF(string NomePdf, MimeMessage Messaggio)
        {
            List<EmailAttachments> res = new List<EmailAttachments>();

            try
            {


                var testfile = NomePdf + ".tmp";
                if (File.Exists(NomePdf))
                    File.Delete(NomePdf);

                var mittente = $"{Messaggio.From.Mailboxes.First().Name} ({Messaggio.From.Mailboxes.First().Address})";
                var oggetto = Messaggio.Subject;
                var txt = Messaggio.TextBody == null ? "" : Messaggio.TextBody;
                var htxt = Messaggio.HtmlBody == null ? "" : Messaggio.HtmlBody;
                var pdfstream = new MemoryStream();
                var ListaPdf = new List<MemoryStream>();
                //FileStream pdfstream = new FileStream(NomePdf, FileMode.CreateNew, FileAccess.ReadWrite);

                PdfDocument document = new PdfDocument();
                HtmlToPdfConverter htmlConverter = new HtmlToPdfConverter(HtmlRenderingEngine.WebKit);
                WebKitConverterSettings settings = new WebKitConverterSettings();

                if (txt.Trim().Length > 5 || htxt.Trim().Length > 10 || Messaggio.Allegati().Count() == 0)
                {
                    if (Messaggio.HtmlBody == null)
                    {
                        if (txt != null)
                        {
                            //document = new RadFlowDocument();
                            //RadFlowDocumentEditor editor = new RadFlowDocumentEditor(document);
                            //editor.InsertText($"Oggetto: {oggetto}");
                            //editor.InsertBreak(BreakType.LineBreak);
                            //editor.InsertText(txt);Messaggio.From
                            PdfPage page = document.Pages.Add();

                            PdfGraphics graphics = page.Graphics;
                            //PdfFont font = new PdfStandardFont(PdfFontFamily.Helvetica, 10);
                            //graphics.DrawString($"Oggetto: {oggetto} \n\n {txt} ", font, PdfBrushes.Black, new PointF(0, 0));

                            PdfTextElement textElement = new PdfTextElement($"Da: {mittente} \nOggetto: {oggetto} \ndel: {Messaggio.Date.DateTime} \n\n {txt} ", new PdfStandardFont(PdfFontFamily.Helvetica, 10));
                            textElement.Draw(page, new Syncfusion.Drawing.RectangleF(0, 0, page.GetClientSize().Width, page.GetClientSize().Height));

                            document.Save(pdfstream);

                            //Close the document.

                            document.Close(true);
                        }
                    }
                    else
                    {

                        try
                        {


                            //var htxt = Messaggio.HtmlBody.Replace("http://", "_http://").Replace("https://", "_https://");
                            //HtmlFormatProvider htmlFormatProvider = new HtmlFormatProvider();
                            //document = htmlFormatProvider.Import(htxt);


                            if (!htxt.Contains("<body>"))
                            {
                                htxt = $"<body>{htxt}</body>";
                            }
                            htxt = htxt.Replace("<body>", $"<body><div><b>Da: </b>{mittente}<br><b>Oggetto: </b>{oggetto}<br><b>del: </b>{Messaggio.Date.DateTime}<br></div>");

                            //string baseUrl = @"D:/temp/pdf/test";
                            string baseUrl = Path.Combine(_appEnvironment.WebRootPath, "_tmp");

                            //Set WebKit path
                            //settings.WebKitPath = @"C:/d.blu/SF/HTMLConverter/18.1.0.42/QtBinariesDotNetCore/";
                            settings.WebKitPath = _config["Docs:PercorsoWebKit"];
                            settings.EnableJavaScript = false;
                            settings.EnableHyperLink = false;
                            settings.EnableOfflineMode = true;
                            settings.Margin.Right = 20;
                            settings.Margin.Left = 20;
                            settings.Margin.Top = 20;
                            //Assign WebKit settings to HTML converter
                            htmlConverter.ConverterSettings = settings;

                            //Convert HTML string to PDF
                            document = htmlConverter.Convert(htxt, baseUrl);

                            //Save and close the PDF document 
                            document.Save(pdfstream);
                            document.Close(true);

                        }
                        catch (Exception ex)
                        {
                            //document = new RadFlowDocument();
                            //RadFlowDocumentEditor editor = new RadFlowDocumentEditor(document);
                            //editor.InsertText($"Oggetto: {oggetto}");
                            //editor.InsertBreak(BreakType.LineBreak);
                            //editor.InsertText(txt);
                            _logger.LogError($"CreaTmpPdfCompleto: impossibile includere il testo. {ex.Message}");
                        }

                    }
                    //pdfstream.Close();
                    ListaPdf.Add(pdfstream);
                }

                foreach (var attachment in Messaggio.Allegati())
                {
                    var fileName = "";
                    var m = new MemoryStream();
                    var incluso = false;


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
                        fileName = part.NomeAllegato();
                        part.Content.DecodeTo(m);
                    }
                    try
                    {
                        switch (System.IO.Path.GetExtension(fileName).ToLower())
                        {
                            case ".pdf":
                                try
                                {
                                    MemoryStream m1 = new MemoryStream();
                                    m.Position = 0;
                                    m.CopyTo(m1);
                                    PdfDocumentAnalyzer analyzer = new PdfDocumentAnalyzer(m1);
                                    //Get the syntax errors
                                    SyntaxAnalyzerResult anRes = analyzer.AnalyzeSyntax();
                                    //Check whether the document is corrupted or not
                                    if (anRes.IsCorrupted)
                                    {
                                        //Get syntax error details from results.error
                                        StringBuilder builder = new StringBuilder();
                                        //builder.AppendLine("The PDF document is corrupted.");
                                        int count = 1;
                                        foreach (PdfException exception in anRes.Errors)
                                        {
                                            builder.AppendLine(count++.ToString() + ": " + exception.Message);
                                        }
                                        _logger.LogError($"CreaTmpPdfCompleto: impossibile includere il file {fileName}. {builder.ToString()}");
                                    }
                                    else
                                    {
                                        //No syntax error found in the provided PDF document
                                        m.Position = 0;
                                        //  appiattisco le note 

                                        PdfLoadedDocument ld = new PdfLoadedDocument(m);
                                        var flAnn = false; 
                                        foreach (PdfPageBase p in ld.Pages) { 
                                            if (p.Annotations.Count > 0) { 
                                                foreach (PdfAnnotation nn in p.Annotations) {
                                                    if (!string.IsNullOrEmpty(nn.Text)){
                                                        nn.Flatten = true;
                                                flAnn = true;
                                            }
                                                }
                                                //p.Annotations.Flatten = true;
                                                //flAnn = true;
                                            }
                                        };
                                        if (flAnn)
                                        {
                                            MemoryStream m2 = new MemoryStream();
                                            ld.Save(m2);
                                            m2.Position = 0;
                                            ListaPdf.Add(m2);
                                            m.Close();
                                            m.Dispose();
                                        }
                                        else { 
                                            m.Position = 0;
                                        ListaPdf.Add(m);
                                        }
                                        incluso = true;
                                    }
                                    m1.Close();
                                    m1.Dispose();
                                    analyzer.Close();

                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError($"CreaTmpPdfCompleto: impossibile includere il file {fileName}. {ex.Message}");
                                }
                                break;
                            case ".jpg":
                            case ".jpeg":
                            case ".png":
                                try
                                {
                                    document = new PdfDocument();
                                    //PdfSection section = document.Sections.Add();
                                    
                                    PdfImage image = new PdfBitmap(m);
                                    //graphics.DrawImage(image, 0, 0);

                                    //PdfImage image = new PdfBitmap(PathToImage);
                                    float shrinkFactor;
                                    float myWidth = image.Width;
                                    float myHeight = image.Height;

                                    if (image.Width > image.Height)
                                    {
                                        document.PageSettings.Orientation = PdfPageOrientation.Landscape;
                                        //page.Rotation = PdfPageRotateAngle.RotateAngle90;
                                        //section.PageSettings.Rotate = PdfPageRotateAngle.RotateAngle90;
                                        //PageWidth = page.Graphics.ClientSize.Height;
                                        //PageHeight = page.Graphics.ClientSize.Width;
                                    }
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
                                    _logger.LogError($"CreaTmpPdfCompleto: impossibile includere il file {fileName}. {ex.Message}");
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"CreaTmpPdfCompleto: {ex.Message}");
                    }
                    var a = new EmailAttachments { Id = fileName, NomeFile = fileName, Valido = false, Incluso = incluso };
                    res.Add(a);
                }

                if (ListaPdf.Count() == 0) { 
                    // aggiungo almeno l'oggetto
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
                    //PdfDocument finalDoc = new PdfDocument();
                    using (PdfDocument finalDoc = new PdfDocument())
                    {
                    //funzione merge
                    try
                    {

                        PdfDocumentBase.Merge(finalDoc, ListaPdf.ToArray());
                        //creazione stram per il file finale
                            //FileStream fileStreamMerge = new FileStream(NomePdf, FileMode.CreateNew, FileAccess.ReadWrite);
                            using (FileStream fileStreamMerge = new FileStream(NomePdf, FileMode.CreateNew, FileAccess.ReadWrite))
                            {

                        //salvataggio e chiusura
                        finalDoc.Save(fileStreamMerge);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"CreaTmpPdfCompleto: {ex.Message}");
                        }
                        foreach (var ms in ListaPdf)
                        {
                            ms.Close();
                        }
                        finalDoc.Close(true);
                        //finalDoc.Dispose();
                    }


                    }

            }
                    catch (Exception ex)
                    {
                        _logger.LogError($"CreaTmpPdfCompleto: {ex.Message}");

                    }

            return res;
        }
        public List<EmailAttachments> CreaTmpPdfCompletoSF(string NomePdf, ZipArchive ZipFile)
        {
            List<EmailAttachments> res = new List<EmailAttachments>();

            try
            {
                var testfile = NomePdf + ".tmp";
                if (File.Exists(NomePdf))
                    File.Delete(NomePdf);

                var pdfstream = new MemoryStream();
                var ListaPdf = new List<MemoryStream>();
                //FileStream pdfstream = new FileStream(NomePdf, FileMode.CreateNew, FileAccess.ReadWrite);

                PdfDocument document = new PdfDocument();
                //HtmlToPdfConverter htmlConverter = new HtmlToPdfConverter(HtmlRenderingEngine.WebKit);
                //WebKitConverterSettings settings = new WebKitConverterSettings();

                foreach (ZipArchiveEntry entry in ZipFile.Entries)
                {
                    {
                        var fileName = entry.Name;
                        var m = new MemoryStream();
                        var incluso = false;

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
                                        MemoryStream m1 = new MemoryStream();
                                        m.Position = 0;
                                        m.CopyTo(m1);
                                        PdfDocumentAnalyzer analyzer = new PdfDocumentAnalyzer(m1);
                                        //Get the syntax errors
                                        SyntaxAnalyzerResult anRes = analyzer.AnalyzeSyntax();
                                        //Check whether the document is corrupted or not
                                        if (anRes.IsCorrupted)
                                        {
                                            //Get syntax error details from results.error
                                            StringBuilder builder = new StringBuilder();
                                            //builder.AppendLine("The PDF document is corrupted.");
                                            int count = 1;
                                            foreach (PdfException exception in anRes.Errors)
                                            {
                                                builder.AppendLine(count++.ToString() + ": " + exception.Message);
                                            }
                                            _logger.LogError($"CreaTmpPdfCompleto: impossibile includere il file {fileName}. {builder.ToString()}");
                                        }
                                        else
                                        {
                                            //No syntax error found in the provided PDF document
                                            m.Position = 0;
                                            //  appiattisco le note 
                                            PdfLoadedDocument ld = new PdfLoadedDocument(m);
                                            var flAnn = false;
                                            foreach (PdfPageBase p in ld.Pages)
                                            {
                                                if (p.Annotations.Count > 0)
                                                {
                                                    p.Annotations.Flatten = true;
                                                    flAnn = true;
                                                }
                                            };
                                            if (flAnn)
                                            {
                                                MemoryStream m2 = new MemoryStream();
                                                ld.Save(m2);
                                                m2.Position = 0;
                                                ListaPdf.Add(m2);
                                                m.Close();
                                                m.Dispose();
                                            }
                                            else
                                            {
                                                m.Position = 0;
                                                ListaPdf.Add(m);
                                            }
                                            incluso = true;
                                        }
                                        m1.Close();
                                        m1.Dispose();
                                        analyzer.Close();
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogError($"CreaTmpPdfCompleto: impossibile includere il file {fileName}. {ex.Message}");
                                    }
                                    break;
                                case ".jpg":
                                case ".jpeg":
                                case ".png":
                                    try
                                    {
                                        using (var unzippedEntryStream = entry.Open())
                                        {
                                            unzippedEntryStream.CopyTo(m);
                                        }
                                        document = new PdfDocument();

                                        PdfImage image = new PdfBitmap(m);

                                        float shrinkFactor;
                                        float myWidth = image.Width;
                                        float myHeight = image.Height;

                                        if (image.Width > image.Height)
                                        {
                                            document.PageSettings.Orientation = PdfPageOrientation.Landscape;
                                        }
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
                                        _logger.LogError($"CreaTmpPdfCompleto: impossibile includere il file {fileName}. {ex.Message}");
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"CreaTmpPdfCompleto: {ex.Message}");
                        }
                        var a = new EmailAttachments { Id = fileName, NomeFile = fileName, Valido = false, Incluso = incluso };
                        res.Add(a);
                    }
                }
                    if (ListaPdf.Count() == 0)
                    {
                        // aggiungo almeno l'oggetto
                        PdfPage page = document.Pages.Add();
                        PdfGraphics graphics = page.Graphics;
                        PdfTextElement textElement = new PdfTextElement($"nomefile: {NomePdf} ", new PdfStandardFont(PdfFontFamily.Helvetica, 10));
                        textElement.Draw(page, new Syncfusion.Drawing.RectangleF(0, 0, page.GetClientSize().Width, page.GetClientSize().Height));
                        document.Save(pdfstream);
                        document.Close(true);
                        ListaPdf.Add(pdfstream);
                    }

                    if (ListaPdf.Count() > 0)
                    {
                        //PdfDocument finalDoc = new PdfDocument();
                        using (PdfDocument finalDoc = new PdfDocument())
                        {
                            //funzione merge
                            try
                            {

                                PdfDocumentBase.Merge(finalDoc, ListaPdf.ToArray());
                                //creazione stram per il file finale
                                //FileStream fileStreamMerge = new FileStream(NomePdf, FileMode.CreateNew, FileAccess.ReadWrite);
                                using (FileStream fileStreamMerge = new FileStream(NomePdf, FileMode.CreateNew, FileAccess.ReadWrite))
                                {

                                    //salvataggio e chiusura
                                    finalDoc.Save(fileStreamMerge);
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError($"CreaTmpPdfCompleto: {ex.Message}");
                            }
                    foreach (var ms in ListaPdf)
                    {
                        ms.Close();
                    }
                    finalDoc.Close(true);
                        //finalDoc.Dispose();
                    }
                    }


                }
            catch (Exception ex)
            {
                _logger.LogError($"CreaTmpPdfCompleto: {ex.Message}");

            }

            return res;
        }



        public async Task<bool> MarcaAllegatoSF(Allegati all, ElencoAttributi att)
        {
            bool res = true;
            try
            {
                string etichetta = Path.Combine(_appEnvironment.ContentRootPath, "Report", _config["Docs:EtichettaProtocollo"]);

                if (!File.Exists(etichetta))
                {
                    _logger.LogError("etichetta inesistente");
                    return false;
                }
                //MemoryStream msOutputStream = new MemoryStream();

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

                string NomePdf = Path.Combine(_appEnvironment.WebRootPath, "_tmp");
                if (!Directory.Exists(NomePdf))
                {
                    Directory.CreateDirectory(NomePdf);
                }
                NomePdf = Path.Combine(NomePdf, $"{all.Id.ToString()}.pdf");
                if (File.Exists(NomePdf))
                    File.Delete(NomePdf);

                var pdfstream = await _allMan.GetFileAsync(all.Id.ToString());
                Telerik.Documents.Primitives.Size A4 = PaperTypeConverter.ToSize(PaperTypes.A4);

                if (pdfstream != null)
                {

                    Syncfusion.Pdf.Parsing.PdfLoadedDocument pdftmp = new Syncfusion.Pdf.Parsing.PdfLoadedDocument(pdfstream);

                    Syncfusion.Pdf.PdfDocument document = new Syncfusion.Pdf.PdfDocument();
                    document.PageSettings.SetMargins(0);

                    int i = 0;
                    foreach (Syncfusion.Pdf.PdfLoadedPage lptmp in pdftmp.Pages)
                    {

                        //Syncfusion.Pdf.PdfSection section = document.Sections.Add();
                        //section.PageSettings.Rotate = lptmp.Rotation;
                        try
                        {

                            // .CreateTemplate() va in errore con formati documento molto particolari
                            Syncfusion.Pdf.Graphics.PdfTemplate template = lptmp.CreateTemplate();
                        Syncfusion.Pdf.PdfPage page = document.Pages.Add();

                        Syncfusion.Pdf.Graphics.PdfGraphics graphics = page.Graphics;
                        float etiHeight = page.Size.Height * .05F + 5;


                        Syncfusion.Drawing.PointF posizione = new Syncfusion.Drawing.PointF() { X = 0, Y = etiHeight + 1 };

                        Syncfusion.Drawing.SizeF pDest = CalcolaProporzioni(lptmp.Size.Width, lptmp.Size.Height, page.Size.Width * 0.95F, page.Size.Height - etiHeight);

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
                            _logger.LogError($"MarcaAllegato errore pagina {i+1}: {ex.Message}");
                            document.ImportPageRange(pdftmp, i , i );
                            i++;
                        }

                    }
                    pdftmp.Close();

                    //using (FileStream fileMarchiato = new FileStream(NomePdf, FileMode.CreateNew, FileAccess.ReadWrite))
                    //{
                    //    //salvataggio e chiusura

                    //    document.Save(fileMarchiato);
                    //}

                    //MemoryStream mpdf = new MemoryStream();
                    //using (FileStream fileStream = File.OpenRead(NomePdf))
                    //{
                    //    mpdf.SetLength(fileStream.Length);
                    //    //read file to MemoryStream
                    //    fileStream.Read(mpdf.GetBuffer(), 0, (int)fileStream.Length);
                    //}
                    //mpdf.Position = 0;
                    using (MemoryStream mpdf = new MemoryStream())
                    {

                        document.Save(mpdf);
                        mpdf.Position = 0;
                        var al = await _allMan.SalvaAsync(all, mpdf, false);
                        if (File.Exists(NomePdf))
                            File.Delete(NomePdf);

                    }
                    document.Close();

                }
            }
            catch (Exception ex)
            {
                res = false;
                _logger.LogError($"MarcaAllegato : {ex.Message}");
            }
            return res;
        }

        public static Syncfusion.Drawing.SizeF CalcolaProporzioni(
            float OrigWidth, float OrigHeight,
            float DestWidth, float DestHeight)
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
