using Syncfusion.Pdf;
using Syncfusion.Pdf.Parsing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using dblu.Portale.Plugin.Docs.Models;

namespace dblu.Portale.Plugin.Docs.Services
{
    public class PdfEditService
    {

        public bool Modifica(PdfEditAction azione)
        {
            bool res = false;
            try
            {
                if (System.IO.File.Exists(azione.FilePdf))
                {
                    PdfLoadedDocument loadedDocument = null;
                    using (FileStream fsSource = new FileStream(azione.FilePdf, FileMode.Open, FileAccess.ReadWrite))
                    {
                        loadedDocument = new PdfLoadedDocument(fsSource);
                   
                        switch (azione.Azione)
                        {
                            case Azioni.RuotaPagina90:
                                PdfPageBase p90 = loadedDocument.Pages[azione.Pagina-1] as PdfPageBase;
                                switch (p90.Rotation) {
                                    case PdfPageRotateAngle.RotateAngle0:
                                        p90.Rotation = PdfPageRotateAngle.RotateAngle90;
                                        break;
                                    case PdfPageRotateAngle.RotateAngle90:
                                        p90.Rotation = PdfPageRotateAngle.RotateAngle180;
                                        break;
                                    case PdfPageRotateAngle.RotateAngle180:
                                        p90.Rotation = PdfPageRotateAngle.RotateAngle270;
                                        break;
                                    case PdfPageRotateAngle.RotateAngle270:
                                        p90.Rotation = PdfPageRotateAngle.RotateAngle0;
                                        break;
                                }
                                break;
                            case Azioni.RuotaPagina270:
                                PdfPageBase p270 = loadedDocument.Pages[azione.Pagina-1] as PdfPageBase;
                                switch (p270.Rotation)
                                {
                                    case PdfPageRotateAngle.RotateAngle0:
                                        p270.Rotation = PdfPageRotateAngle.RotateAngle270;
                                        break;
                                    case PdfPageRotateAngle.RotateAngle90:
                                        p270.Rotation = PdfPageRotateAngle.RotateAngle0;
                                        break;
                                    case PdfPageRotateAngle.RotateAngle180:
                                        p270.Rotation = PdfPageRotateAngle.RotateAngle90;
                                        break;
                                    case PdfPageRotateAngle.RotateAngle270:
                                        p270.Rotation = PdfPageRotateAngle.RotateAngle180;
                                        break;
                                }
                                break;
                            case Azioni.CancellaPagina:
                                loadedDocument.Pages.RemoveAt(azione.Pagina-1);
                                break;
                            default:
                                break;
                        }
                        loadedDocument.Save(fsSource);

                    }
                    loadedDocument.Close(true);
                    res = true;
                }
            }
            catch (Exception ex)
            {
                // log 
               res=false;
            }

            return res;
        }


    }
}
