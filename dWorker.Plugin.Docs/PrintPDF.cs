using Microsoft.Extensions.Logging;
using Syncfusion.Blazor.PdfViewer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dWorker.Plugin.Docs
{
    /// <summary>
    /// Class that print a PDF in silent mode, serverside
    /// </summary>
    public class PrintPDF
    {
        /// <summary>
        /// Log interface
        /// </summary>
        private ILogger _logger;

        /// <summary>
        /// Current selected printer
        /// </summary>
        public string Printer { get; set; } = "";
      

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="nLogger">Log interface</param>
        /// <param name="nPrinter">Current selected printer</param>
        public PrintPDF(ILogger nLogger, string nPrinter = null)
        {
            Printer = nPrinter;
            _logger = nLogger;
        }

        /// <summary>
        /// Print the Document
        /// </summary>
        /// <param name="Name">The document name (for log porpuses)</param>
        /// <param name="DocumentStream">Stream of the document to print</param>
        /// <param name="nPrinter">Name of the printer to use (opz)</param>
        /// <returns>
        /// True if printing has been done properly
        /// </returns>
        public bool Print(string Name,Stream DocumentStream,string nPrinter=null)
        {
            try
            {
                PdfRenderer loadedDocument = new();
                loadedDocument.Load(DocumentStream);

                if (loadedDocument.PageCount == 0) return true;

                using PrintDocument printDocument = new();
                //Preparo il doc settanto nome e stampante
                printDocument.DocumentName = Name;
                printDocument.PrinterSettings.PrinterName = Printer;
                if (!string.IsNullOrEmpty(nPrinter))
                {
                    printDocument.PrinterSettings.PrinterName = nPrinter;


                }
                    
                PrinterSettings ps = new PrinterSettings();
                var  Resolution = ps.PrinterResolutions.OfType<PrinterResolution>()
                                                         .OrderByDescending(r => r.X)
                                                         .ThenByDescending(r => r.Y)
                                                         .First();

                ///Carico il documento su un oggetto per tracciarne l'avanzamento di stampa
                PrintPDFStatus s = new(loadedDocument, Resolution);
                printDocument.PrintPage += (_, e) => PrintDocumentOnPrintPage(e, s);

                //if (printDocument.PrinterSettings.SupportsColor)
                //    {
                //        // Set the page default's to not print in color.
                //        printDocument.DefaultPageSettings.Color = false;
                //    }
                printDocument.Print();
                
                _logger.LogInformation($"PrintPDF.Print: Printing {Name} on {nPrinter}");
                return true;
            }catch(Exception ex)
            {
                _logger.LogError($"PrintPDF.Print: Unexpected error {ex}");
                return false;
            }
        }

        /// <summary>
        /// Print a page on the printer
        /// </summary>
        /// <param name="e">The print event</param>
        /// <param name="state">The current printing state</param>
        private static void PrintDocumentOnPrintPage(PrintPageEventArgs e, PrintPDFStatus state)
        {
            var destinationRect = new RectangleF(x: e.Graphics.VisibleClipBounds.X,y: e.Graphics.VisibleClipBounds.Y,width: e.Graphics.VisibleClipBounds.Width, height: e.Graphics.VisibleClipBounds.Height);
            e.Graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            using (Bitmap bitmapimage = state.Document.ExportAsImage(state.CurrentPageIndex ,600, 600))
                 e.Graphics.DrawImage(bitmapimage, destinationRect);
                //e.Graphics.DrawImage(bitmapimage, new Point(0, 0));
            e.HasMorePages = state.AdvanceToNextPage();
           // e.HasMorePages = false;
        }

        private static Bitmap MakeGrayscale3(Bitmap original)
        {
            //create a blank bitmap the same size as original
            Bitmap newBitmap = new Bitmap(original.Width, original.Height);

            //get a graphics object from the new image
            Graphics g = Graphics.FromImage(newBitmap);

            //create the grayscale ColorMatrix
            ColorMatrix colorMatrix = new ColorMatrix(
                new float[][]
                {
                        new float[] {.3f, .3f, .3f, 0, 0},
                        new float[] {.59f, .59f, .59f, 0, 0},
                        new float[] {.11f, .11f, .11f, 0, 0},
                        new float[] {0, 0, 0, 1, 0},
                        new float[] {0, 0, 0, 0, 1}
                });

            //create some image attributes
            ImageAttributes attributes = new ImageAttributes();

            //set the color matrix attribute
            attributes.SetColorMatrix(colorMatrix);

            //draw the original image on the new image
            //using the grayscale color matrix
            g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height),
               0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);

            //dispose the Graphics object
            g.Dispose();
            return newBitmap;
        }

    }
}
