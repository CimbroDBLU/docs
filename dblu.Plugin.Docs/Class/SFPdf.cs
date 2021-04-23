using Syncfusion.Pdf;
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
using System.Text.RegularExpressions;
using MimeKit.Tnef;
using Telerik.Windows.Documents.Flow.FormatProviders.Rtf;
using Telerik.Windows.Documents.Flow.Model;
using Telerik.Windows.Documents.Flow.FormatProviders.Pdf;
using Telerik.Windows.Documents.Flow.FormatProviders.Html;


namespace dblu.Portale.Plugin.Docs.Class
{
    class SFPdf
    {

        private readonly ILogger _logger;
        private readonly IWebHostEnvironment _appEnvironment;
        private IConfiguration _config { get; }
        private AllegatiManager _allMan;

        public SFPdf(IWebHostEnvironment appEnvironment,
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


        private string PulisciHtml(string htxt)
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

        public List<EmailAttachments> CreaTmpPdfCompletoSF(string NomePdf, MimeMessage Messaggio)
        {
            List<EmailAttachments> res = new List<EmailAttachments>();
            try
            {
                PdfUnitConverter convertor = new PdfUnitConverter();
                float mm = 10;
                try
                {
                    mm = float.Parse(_config["Docs:Margini"]);
                }
                catch
                {
                    mm = 10;
                }
                float MarginPoints = convertor.ConvertUnits(mm, PdfGraphicsUnit.Millimeter, PdfGraphicsUnit.Point);

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

                if (htxt == "") {
                    // controlla presenza testo rtf exchange/outlook (winmail.dat)
                    // estrae rtf e converte in pdf
                    var rtxt = "";
                    try
                    {
                        var tnef = Messaggio.BodyParts.OfType<TnefPart>().FirstOrDefault();
                        if (tnef != null)
                        {
                            foreach (var attachment in tnef.ExtractAttachments())
                            {
                                var mime_part = attachment as MimePart;
                                var text = attachment as TextPart;
                                if (text != null)
                                {
                                    rtxt += text.Text;
                                }
                            }
                        }
                    }
                    catch { 
                    }
                    if (rtxt != "") {
                        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                        htxt= RtfPipe.Rtf.ToHtml(rtxt);
                    }
                }

                if (htxt == "")
                    {
                        if (txt != null)
                        {
                            PdfPage page = document.Pages.Add();

                            PdfGraphics graphics = page.Graphics;
                            //PdfFont font = new PdfStandardFont(PdfFontFamily.Helvetica, 10);
                            //graphics.DrawString($"Oggetto: {oggetto} \n\n {txt} ", font, PdfBrushes.Black, new PointF(0, 0));
                            
                           //intestazione
                            PdfTextElement textElement = new PdfTextElement($"Da: {mittente} \nOggetto: {oggetto} \ndel: {Messaggio.Date.ToLocalTime().ToString("dd/MM/yyyy HH:mm")} \n\n {txt} ", new PdfStandardFont(PdfFontFamily.Helvetica, 10));

                            textElement.Draw(page, new Syncfusion.Drawing.RectangleF(0, 0, page.GetClientSize().Width, page.GetClientSize().Height));
                            document.Save(pdfstream);
                            //Close the document.
                            document.Close(true);
                        }
                    }
                    else   // conversione html in pdf
                    {
                        try
                        {
                        
                        //var htxt = Messaggio.HtmlBody.Replace("http://", "_http://").Replace("https://", "_https://");
                        //HtmlFormatProvider htmlFormatProvider = new HtmlFormatProvider();
                        //document = htmlFormatProvider.Import(htxt);

                        //intestazione
                        if (!htxt.Contains("<body>"))
                            {
                                htxt = $"<body>{htxt}</body>";
                            }
                            htxt = PulisciHtml(htxt);

                            htxt = htxt.Replace("<body>", $"<body><div><b>Da: </b>{mittente}<br><b>Oggetto: </b>{oggetto}<br><b>del: </b>{Messaggio.Date.ToLocalTime().ToString("dd/MM/yyyy HH:mm ")}<br></div>");

                            //File.WriteAllText("d:\\temp\\testo.html", htxt);
                            string baseUrl = Path.Combine(_appEnvironment.WebRootPath, "_tmp");

                            //Set WebKit path
                            settings.WebKitPath = _config["Docs:PercorsoWebKit"];
                            settings.EnableJavaScript = false;
                            settings.EnableHyperLink = false;
                            settings.EnableOfflineMode = true;
                            settings.SplitTextLines = true;
                            settings.SplitImages = true;
                            //settings.SinglePageLayout = Syncfusion.Pdf.HtmlToPdf.SinglePageLayout.FitHeight;
                            settings.Margin.Right = MarginPoints;
                            settings.Margin.Left = MarginPoints;
                            settings.Margin.Top = MarginPoints;
                            settings.Margin.Bottom = MarginPoints;
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
//                }
                 //pdfstream.Close();
                ListaPdf.Add(pdfstream);
                //}

                int i = 0;
                foreach (var attachment in Messaggio.Allegati())
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
                        part.Content.DecodeTo(m);
                    }
                    try
                    {
                        switch (System.IO.Path.GetExtension(fileName).ToLower())
                        {
                            case ".pdf":
                                MemoryStream m1 = ElaboraPdfStream(m, fileName, out avvisi );
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
                                    //graphics.DrawImage(image, 0, 0);

                                    //PdfImage image = new PdfBitmap(PathToImage);
                                    float shrinkFactor;
                                    float myWidth = image.Width;
                                    float myHeight = image.Height;

                                    if (myWidth > 100 && myHeight > 100)
                                    {
                                        document = new PdfDocument();
                                        //PdfSection section = document.Sections.Add();

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
                                    else
                                    {
                                        incluso = false;
                                    }

                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError($"CreaTmpPdfCompleto: impossibile includere il file {fileName}. {ex.Message}");
                                }
                                break;
                            default:
                                avvisi = "file non supportato.";
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"CreaTmpPdfCompleto: {ex.Message}");
                    }
                    var a = new EmailAttachments { Id = fileName, NomeFile = fileName, Valido = false, Incluso = incluso, Avvisi = avvisi };
                    res.Add(a);
                }

                if (ListaPdf.Count() == 0)
                {
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
        public List<EmailAttachments> CreaTmpPdfCompletoSF(string NomePdf, ZipArchive ZipFile, string Testo = "")
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

                if (Testo.Length >0 )
                {
                    PdfPage page = document.Pages.Add();

                    PdfGraphics graphics = page.Graphics;
                    //PdfFont font = new PdfStandardFont(PdfFontFamily.Helvetica, 10);
                    //graphics.DrawString($"Oggetto: {oggetto} \n\n {txt} ", font, PdfBrushes.Black, new PointF(0, 0));

                    PdfTextElement textElement = new PdfTextElement(Testo, new PdfStandardFont(PdfFontFamily.Helvetica, 10));
                    textElement.Draw(page, new Syncfusion.Drawing.RectangleF(0, 0, page.GetClientSize().Width, page.GetClientSize().Height));

                    document.Save(pdfstream);

                    //Close the document.

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
                                        MemoryStream m1 = ElaboraPdfStream(m, fileName, out avvisi);
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
                                    avvisi = "file non supportato.";
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"CreaTmpPdfCompleto: {ex.Message}");
                        }
                        var a = new EmailAttachments { Id = fileName, NomeFile = fileName, Valido = false, Incluso = incluso, Avvisi=avvisi };
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
                PdfUnitConverter convertor = new PdfUnitConverter();
                float mm = 10;
                try
                {
                    mm = float.Parse(_config["Docs:Margini"]);
                }
                catch
                {
                    mm = 10;
                }
                float MarginPoints = convertor.ConvertUnits(mm, PdfGraphicsUnit.Millimeter, PdfGraphicsUnit.Point);

                string rpt = _config["Docs:EtichettaProtocollo"];
                if (string.IsNullOrEmpty(rpt))
                    return true;
                
                string etichetta = Path.Combine(_appEnvironment.ContentRootPath, "Report", rpt);

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
                    document.PageSettings.SetMargins(MarginPoints);

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
                            _logger.LogError($"MarcaAllegato errore pagina {i + 1}: {ex.Message}");
                            document.ImportPageRange(pdftmp, i, i);
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

        
        private MemoryStream ElaboraPdfStream(MemoryStream m, string fileName, out string avvisi ) {
             avvisi = "";
            try
            {
                PdfUnitConverter convertor = new PdfUnitConverter();
                float mm = 10;
                try
                {
                    mm = float.Parse(_config["Docs:Margini"]);
                }
                catch
                {
                    mm = 10;
                }
                float MarginPoints = convertor.ConvertUnits(mm, PdfGraphicsUnit.Millimeter, PdfGraphicsUnit.Point);

                var flAnn = false;  //contiene annotazioni
                var flResize = false;  // richiede resize

                m.Position = 0;
                var A4Size = PdfPageSize.A4;
                PdfLoadedDocument ld = null;
                try
                {
                    ld = new PdfLoadedDocument(m);
                }
                catch 
                {
                    ld = new PdfLoadedDocument(m, true);
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
                           
                            if (nn is PdfLoadedFreeTextAnnotation )
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
                                        _logger.LogError($"ElaboraPdfStream: impossibile cancellare la nota. {ex.Message}");
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
                                        while (fontSize > 0f && (pdfFont.MeasureString(nn.Text, drawFormat).Height > bounds.Height || pdfFont.MeasureString(nn.Text, drawFormat).Width > bounds.Width))
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
                                //p.Annotations.RemoveAt(i);
                                nn.Flatten = true;
                                
                                flNoteManuali = true;
                                //PdfTextMarkupAnnotation xx1 = new PdfTextMarkupAnnotation(new RectangleF(nn.Location, nn.Size));
                                //xx1.TextMarkupAnnotationType = ((Syncfusion.Pdf.Interactive.PdfLoadedTextMarkupAnnotation)nn).TextMarkupAnnotationType;
                                //xx1.TextMarkupColor = ((Syncfusion.Pdf.Interactive.PdfLoadedTextMarkupAnnotation)nn).TextMarkupColor;
                                
                                //xx1.Flatten = true;
                                
                                //p.Annotations.Add(xx1);
                            }
                            else if (nn is PdfLoadedInkAnnotation)
                            {
                                //DrawPolygon(PdfPen pen, PdfBrush brush, PointF[] points);
                                flNoteManuali = true;
                                nn.Flatten = true;

                            }
                            else if (nn is PdfLoadedRubberStampAnnotation)
                            {
                                flNoteManuali = true;
                                //nn.Flatten = true;
                                
                            }
                            else if (nn is PdfLoadedLineAnnotation)
                            {
                                if (((PdfLoadedLineAnnotation)nn).EndLineStyle != PdfLineEndingStyle.None)
                                {
                                    nn.Flatten = true;
                                }
                                else { 
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
                                        _logger.LogError($"ElaboraPdfStream: impossibile cancellare la nota. {ex.Message}");
                                    }
                                    int[] xLine= ((PdfLoadedLineAnnotation)nn).LinePoints;
                                    PdfPen xPen = new PdfPen(nn.Color, ((PdfLoadedLineAnnotation)nn).Border.Width);

                                    if (p.Rotation == PdfPageRotateAngle.RotateAngle90)
                                    {
                                        p.Graphics.DrawLine(xPen, new PointF(xLine[1], xLine[0]), new PointF(xLine[3],xLine[2]));
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

                    if (flNoteManuali) {
                          ControllaPag.Add((pn).ToString());
                        _logger.LogWarning($"ElaboraPdfStream: presenza di note manuali pag {pn}  {fileName}. ");
                    }

                };

                if (ControllaPag.Count > 0) {
                    avvisi = $" Controllare le note manuali (pag. {string.Join(",",ControllaPag)})";
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
                        avvisi += " Impossibile importare le note. ";
                        _logger.LogError($"ElaboraPdfStream: impossibile importare le note {fileName}. {ex.Message}");
                    }
                }

                if (flResize )
                {
                    PdfDocument doc1 = new PdfDocument();
                    foreach (PdfPageBase p in ld.Pages)
                    {
                        
                        PdfPage page = doc1.Pages.Add();
                        

                        page.Section.PageSettings.Margins.All = MarginPoints;
                        
                        PdfGraphics g = page.Graphics;
                        PdfTemplate template = p.CreateTemplate();
                       
                        // g.DrawPdfTemplate(template, PointF.Empty, PdfPageSize.A4);
                        Syncfusion.Drawing.PointF posizione = new Syncfusion.Drawing.PointF() { X = 0, Y = 0 };
                        Syncfusion.Drawing.SizeF pDest = CalcolaProporzioni(p.Size.Width, p.Size.Height, page.Size.Width, page.Size.Height);
                        

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
                    if (flAnn) { 
                        m2.Close();
                    }
                    m2 = new MemoryStream();
                    doc1.Save(m2);
                }

                if (!flAnn && !flResize)
                {
                    m2 = new MemoryStream();
                    m.Position = 0;
                    m.CopyTo(m2);
                }
                m2.Position = 0;
                return m2;            }
            catch (Exception ex)
            {
                 avvisi = "Impossibile includere il file";
                _logger.LogError($"ElaboraPdfStream: impossibile includere il file {fileName}. {ex.Message}");
            }
            return null;
        }
    }
    
   
}

