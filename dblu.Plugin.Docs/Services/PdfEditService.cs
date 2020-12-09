using Syncfusion.Pdf;
using Syncfusion.Pdf.Parsing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using dblu.Portale.Plugin.Docs.Models;
using dblu.Docs.Models;

namespace dblu.Portale.Plugin.Docs.Services
{
    public class PdfEditService
    {

        private AllegatiService _allegatiService;
        private MailService _mailService;
        private ZipService _zipSvc;

        public PdfEditService (
            AllegatiService allegatiService,
            MailService mailService,
             ZipService zipService
        )
        {
            _allegatiService = allegatiService;
            _mailService = mailService;
            _zipSvc = zipService;
        }

        public async Task<MemoryStream> GetPdf(PdfEditAction pdf)
        {
            MemoryStream stream = new MemoryStream();

            var flLoadMail = true;
            if (!string.IsNullOrEmpty(pdf.IdElemento))
            {
                Allegati ae = _allegatiService.GetPdfAllegatoAElemento(pdf);
                if (ae != null) {
                    stream = await _allegatiService._allMan.GetFileAsync(ae.Id.ToString());
                    if (System.IO.File.Exists(pdf.FilePdfModificato))
                    {
                        System.IO.File.Delete(pdf.FilePdfModificato);
                    }
                    flLoadMail = false;
                }
            }
            if (flLoadMail) 
            {
                byte[] bytes = null;
                if (System.IO.File.Exists(pdf.FilePdfModificato))
                {
                    bytes = System.IO.File.ReadAllBytes(pdf.FilePdfModificato);
                    stream = new MemoryStream(bytes);
                }
                else
                {
                    if (System.IO.File.Exists(pdf.FilePdf))
                    {
                        System.IO.File.Delete(pdf.FilePdf);
                    }
                    if (pdf.TipoAllegato == "FILE")
                    {
                        stream = await _allegatiService._allMan.GetFileAsync(pdf.IdAllegato.ToString());
                    }
                    else if(pdf.TipoAllegato == "ZIP")
                    {
                        pdf = await _zipSvc.GetFilePdfCompletoAsync(pdf, true);
                        bytes = System.IO.File.ReadAllBytes(pdf.FilePdf);
                        stream = new MemoryStream(bytes);
                    }
                    else
                    {
                        pdf = await _mailService.GetFilePdfCompletoAsync(pdf, true);
                        bytes = System.IO.File.ReadAllBytes(pdf.FilePdf);
                        stream = new MemoryStream(bytes);
                    }
                    //System.IO.File.Delete(pdf.FilePdf);
                }
                if (System.IO.File.Exists(pdf.FilePdf))
                {
                    System.IO.File.Delete(pdf.FilePdf);
                }
            }
            return stream;
        }
        public bool Modifica(PdfEditAction azione)
        {
            bool res = false;
            try
            {
                if (System.IO.File.Exists(azione.FilePdfInModifica))
                {
                    PdfLoadedDocument loadedDocument = null;
                    using (FileStream fsSource = new FileStream(azione.FilePdfInModifica, FileMode.Open, FileAccess.ReadWrite))
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
