using BPMClient;
using dblu.Docs.Classi;
using dblu.Docs.Interfacce;
using dblu.Docs.Models;
using dblu.Portale.Plugin.Docs.Data;
using dblu.Portale.Plugin.Docs.ViewModels;
using dblu.Portale.Services.Camunda;
using Microsoft.AspNetCore.Builder;
//using dblu.Portale.Plugin.Docs.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Data.SqlTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MimeKit;
using Newtonsoft.Json.Linq;
using NToastNotify;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Telerik.Documents.Primitives;
using Telerik.Windows.Documents.Common.FormatProviders;
using Telerik.Windows.Documents.Fixed.FormatProviders.Pdf.Export;
using Telerik.Windows.Documents.Fixed.FormatProviders.Pdf.Streaming;
using Telerik.Windows.Documents.Fixed.Model;
using Telerik.Windows.Documents.Fixed.Model.Resources;
using Telerik.Windows.Documents.Fixed.Model.Objects;
using Telerik.Windows.Documents.Flow.Model;
using Telerik.Windows.Documents.Flow.FormatProviders.Pdf;
using Telerik.Windows.Documents.Flow.Model.Editing;
using Telerik.Windows.Documents.Flow.FormatProviders.Html;
using Telerik.Windows.Documents.Flow.Model.Fields;
using dblu.Docs.Extensions;
using Telerik.Reporting;
using Telerik.Windows.Documents.Fixed.Model.Data;
using Telerik.Reporting.Processing;
using dblu.Portale.Plugin.Docs.Models;
using MailKit.Net.Smtp;
using Newtonsoft.Json;
using Dapper;
using System.Net;
using dblu.Portale.Core.Infrastructure.Identity.Services;
using System.Security.Claims;
using Syncfusion.EJ2.BarcodeGenerator;
using Telerik.Windows.Documents.Model;
using dblu.Portale.Plugin.Docs.Class;
using MimeKit.Utils;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using dblu.Portale.Core.Infrastructure.Identity.Class;
using dblu.CamundaClient;
using AutoMapper;

namespace dblu.Portale.Plugin.Docs.Services
{
    public class MailService 
    {
        public readonly dbluDocsContext _context;
        public readonly ILogger _logger;
        private readonly IWebHostEnvironment _appEnvironment;
        private readonly IToastNotification _toastNotification;
        private readonly CamundaService _bpm;
        public readonly AllegatiManager _allMan;
        public readonly FascicoliManager _fasMan;
        public readonly ElementiManager _elmMan;
        public readonly ServerEmailManager _serMan;
        public readonly LogDocManager _logMan;
        public readonly IApplicationUsersManager _usrManager;

        //        private readonly SoggettiManager _sggMan;
        public IConfiguration _config { get; }
        public ISoggettiService _soggetti;
        private MapperConfiguration _mapperConfig;

        public const string NOME_FILE_CONTENUTO_EMAIL = "email-contenuto.pdf";

        public MailService(CamundaService bpm, dbluDocsContext db,
            IWebHostEnvironment appEnvironment,
            ILoggerFactory loggerFactory,
            IToastNotification toastNotification,
            IConfiguration config,
            ISoggettiService sogg,
            IApplicationUsersManager usrManager
            )
        {
            _toastNotification = toastNotification;
            _context = db; // new dbluDocsContext(db.Connessione);
            _appEnvironment = appEnvironment;
            _logger = loggerFactory.CreateLogger("MailService");
            _bpm = bpm;
            _allMan = new AllegatiManager(_context.Connessione, _logger);
            _fasMan = new FascicoliManager(_context.Connessione, _logger);
            _elmMan = new ElementiManager(_context.Connessione, _logger);
            _serMan = new ServerEmailManager(_context.Connessione, _logger);
            _logMan = new LogDocManager(_context.Connessione, _logger);
//            _sggMan = new SoggettiManager(_context, _logger);
            _config = config;
            _usrManager = usrManager;

            _mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<BPMProcessInfo, BPMDocsProcessInfo>());
            try
            {
                _soggetti = sogg;
                //string sServizioSoggetti = _config["Docs:ServizioSoggetti"];

                //if (!string.IsNullOrEmpty(sServizioSoggetti)) {
//                    ServizioSoggetti = new dblu.sgGeaClient.Classi.GeaClientService(); // System.Reflection.Assembly.GetExecutingAssembly().CreateInstance(sServizioSoggetti);
//                    ServizioSoggetti.Init(_config, _logger);
                //}
            }
            catch
            {

            }

        }

        public List<String> getRuoli(IEnumerable<Claim> Roles, string NomeServer)
        {
            List<string> l = new List<string>();


            if (NomeServer != "")
            {

                string xRol = "'";
                foreach (Claim x in Roles)
                {
                    if (x.Type == ClaimTypes.Role) xRol = xRol + x.Value + "','";
                }
                xRol = xRol.Substring(0, xRol.Length - 2);
                try
                {

                    using (SqlConnection cn = new SqlConnection(_context.Connessione))
                    {

                        string sql = "Select RoleID FROM [ServersInRole] where [RoleID] IN (" + xRol + ") and [idServer]='" + NomeServer + "'";
                        l = cn.Query<string>(sql).ToList();

                    }
                }

                catch (Exception ex)
                {
                    _logger.LogError($"getRuoli: {ex.Message}");
                }

            }
            else
            {
                foreach (Claim x in Roles)
                {
                    if (x.Type == ClaimTypes.Role) l.Add(x.Value);
                }
            }

            return l;



            
        }


        public List<String> getRuoli(List<string> Ruoli, string NomeServer)
        {
            List<string> l = new List<string>();


            if (NomeServer != "")
            {

                //string xRol = "'";
                //foreach (Claim x in Roles)
                //{
                //    if (x.Type == ClaimTypes.Role) xRol = xRol + x.Value + "','";
                //}
                //xRol = xRol.Substring(0, xRol.Length - 2);
                try
                {

                    using (SqlConnection cn = new SqlConnection(_context.Connessione))
                    {

                        string sql = "Select RoleID FROM [ServersInRole] where [RoleID] IN ('" + string.Join("','", Ruoli) + "') and [idServer]='" + NomeServer + "'";
                        l = cn.Query<string>(sql).ToList();

                    }
                }

                catch (Exception ex)
                {
                    _logger.LogError($"getRuoli: {ex.Message}");
                }

            }
            else
            {
                //foreach (Claim x in Roles)
                //{
                //    if (x.Type == ClaimTypes.Role) l.Add(x.Value);
                //}
                l = Ruoli;
            }

            return l;




        }

        public List<String> getRuoli(string Modulo, string NomeServer)
        {
            List<string> l = new List<string>();
            IEnumerable<Role> Roles;
            Roles = _usrManager.GetAllRolesForModule(Modulo);

            if (NomeServer != "")
            {

                string xRol = "'";
                foreach (Role x in Roles)
                {
                      xRol = xRol + x.Code + "','";
                }
          
                xRol = xRol.Substring(0, xRol.Length - 2);
                try
                {

                    using (SqlConnection cn = new SqlConnection(_context.Connessione))
                    {

                        string sql = "Select RoleID FROM [ServersInRole] where [RoleID] IN (" + xRol + ") and [idServer]='" + NomeServer + "'";
                        l = cn.Query<string>(sql).ToList();

                    }
                }

                catch (Exception ex)
                {
                    _logger.LogError($"getRuoli: {ex.Message}");
                }

            }
            else
            {

                foreach (Role x in Roles)
                {
                    l.Add(x.Code);
                }
            }

            return l;




        }


        public int CountEmailInArrivo(string Tipo, IEnumerable<Claim> Roles, StatoAllegato stato = StatoAllegato.Elaborato)
        {

            int l = 0;
            try
            {
                List<EmailServer> ListaServer = _serMan.GetServersEmailinRoles(Roles,TipiRecordServer.CartellaMail);

                if (ListaServer != null && ListaServer.Count > 0)
                {
                    string xServer = "'";
                    foreach (EmailServer x in ListaServer)
                    {
                        xServer = xServer + x.Nome + "','";
                    }
                    xServer = xServer.Substring(0, xServer.Length - 2);
                    //xServer = "'" + xServer +"'";


                    using (SqlConnection cn = new SqlConnection(_context.Connessione))
                    {
                        l = cn.ExecuteScalar<int>("Select count(*) from Allegati where Tipo=@Tipo and Stato <= @Stato and Origine IN (" + xServer + ")",
                            new { Tipo = Tipo, Stato = stato });

                    }
                }
                else
                {
                    using (SqlConnection cn = new SqlConnection(_context.Connessione))
                    {
                        l = cn.ExecuteScalar<int>("Select count(*) from Allegati where Tipo=@Tipo and Stato <= @Stato",
                            new { Tipo = Tipo, Stato = stato });

                    }
                }
                

            }

            catch (Exception ex)
            {
                _logger.LogError($"CountEmailInArrivoSever: {ex.Message}");
            }

            return l;
        }
        
        public async Task<MailViewModel> GetMailViewModel(Guid t)
        {
            MailViewModel mv = new MailViewModel();
            try
            {

                mv.IdAllegato = t.ToString();

                mv.task = new BPMTaskDto();
                mv.task.id = "";
                //var allMan = new AllegatiManager(_context, _logger);
                mv.Allegato = _allMan.Get(mv.IdAllegato);

                mv.TaskVar = new Dictionary<string, VariableValue>();
                mv.TaskVar.Add("sMittente", new VariableValue { value = mv.Allegato.Chiave1 });
                mv.TaskVar.Add("dData", new VariableValue { value = mv.Allegato.Chiave2 });
                mv.TaskVar.Add("sOggetto", new VariableValue { value = mv.Allegato.Chiave4 });
                  


                //var FMan = new FascicoliManager(_context, _logger);
                mv.ListaCategorie = await _fasMan.GeAllCategorieAsync();

                mv.ListaTipiElementi = await _elmMan.GetAllTipiElementiAsync();

                mv.Fascicolo = _fasMan.Get(mv.IdFascicolo);
                mv.Elemento = _elmMan.Get(mv.IdElemento,0);
                if (string.IsNullOrEmpty(mv.CodiceSoggetto) && mv.Fascicolo != null)
                {
                    mv.Allegato.SetAttributo("CodiceSoggetto", mv.Fascicolo.CodiceSoggetto);
                }
                if (!string.IsNullOrEmpty(mv.CodiceSoggetto))
                mv.Soggetto = _soggetti.GetSoggetto(mv.CodiceSoggetto);
                
                if (mv.Soggetto == null)
                { mv.Soggetto = new Soggetti(); }

                var m = await _allMan.GetFileAsync(mv.IdAllegato);
                mv.Messaggio = MimeKit.MimeMessage.Load(m, new CancellationToken());

                //mv.FileAllegati = await GetTmpPdfCompletoAsync(mv.Allegato, mv.Messaggio, false);

            }
            catch (Exception ex)
            {
                _logger.LogError($" GetMailViewModel : {ex.Message}");
            }
            return mv;
        }

        public async Task<MailViewModel> GetMailViewModel(BPMTaskDto t)
        {
            MailViewModel mv = new MailViewModel();
            try
            {

                mv.task = t;
                BPMTask tsk = new BPMTask();
                mv.TaskVar = await tsk.GetTaskFormVariables(_bpm._eng, t.id);
                mv.IdAllegato = mv.TaskVar["sIdAllegato"].ToString();

                //var allMan = new AllegatiManager(_context, _logger);
                mv.Allegato = _allMan.Get(mv.IdAllegato);

                //var FMan = new FascicoliManager(_context, _logger);
                mv.ListaCategorie = await _fasMan.GeAllCategorieAsync();

                mv.ListaTipiElementi = await _elmMan.GetAllTipiElementiAsync();

                mv.Fascicolo = _fasMan.Get(mv.IdFascicolo);
                mv.Elemento = _elmMan.Get(mv.IdElemento, 0);
                mv.Soggetto = _soggetti.GetSoggetto(mv.CodiceSoggetto);
                
                var m = await _allMan.GetFileAsync(mv.IdAllegato);
                mv.Messaggio = MimeKit.MimeMessage.Load(m, new CancellationToken());

               // mv.FileAllegati = await GetTmpPdfCompletoAsync(mv.Allegato, mv.Messaggio, false);
                    }
            catch (Exception ex)
            {
                _logger.LogError($" GetMailViewModel : {ex.Message}");
            }
            return mv;
        }

        internal async Task<Elementi> CreaElementoAsync(ClaimsPrincipal User, Guid idFascicolo, Guid idAllegato, string categoria, string codiceCliente, string elencoFile, bool allegaEmail, string Des)
        {
            try
            {

                Elementi elemento = new Elementi()
                {
                    DataC = DateTime.Now,
                    IdFascicolo = idFascicolo,
                    Tipo = categoria,
                    UtenteC = User.Identity.Name,
                    UtenteUM = User.Identity.Name,
                    DataUM = DateTime.Now,
                    Revisione = 0,
                    Descrizione = Des

                };
               // _context.Elementi.Add(elemento);

                var cancel = new CancellationToken();

                var m = await _allMan.GetFileAsync(idAllegato.ToString());
                var Messaggio = MimeKit.MimeMessage.Load(m, cancel);

                var Allegato = _allMan.Get(idAllegato.ToString());

                //TipiAllegati tipoAll = _context.TipiAllegati
                //   .Where(t => t.Codice == "FILE")
                //   .FirstOrDefault();
                TipiAllegati tipoAll = _allMan.GetTipoAllegato("FILE");
                LogDoc log;
                int i = 0;
                foreach (var attachment in Messaggio.Allegati())
                {
                    i++;
                    var fileName = attachment.NomeAllegato(i);
                    m = new MemoryStream();
                    if (attachment is MessagePart)
                    {
                        //fileName = attachment.ContentDisposition?.FileName;
                        var rfc822 = (MessagePart)attachment;

                        if (string.IsNullOrEmpty(fileName))
                            fileName = "email-allegata.eml";

                        //using (var stream = File.Create(fileName))
                        rfc822.Message.WriteTo(m);
                    }
                    else
                    {
                        var part = (MimePart)attachment;
                        //fileName = part.FileName;

                        //using (var stream = File.Create(fileName))
                        part.Content.DecodeTo(m);
                    }
                    
                    if (elencoFile.Contains(fileName))
                    {
                        var all = new Allegati()
                        {
                            Descrizione = Allegato.Descrizione,
                            NomeFile = fileName,
                            Tipo = "FILE",
                            TipoNavigation = tipoAll,
                            Stato = StatoAllegato.Attivo,
                            IdFascicolo = Allegato.IdFascicolo,
                            IdElemento = Allegato.IdElemento
                        };
                        string emailmitt = Messaggio.From.Mailboxes.First().Address;

                        if (all.elencoAttributi == null) { all.elencoAttributi = tipoAll.Attributi; }
                        all.SetAttributo("Chiave1", emailmitt);
                        all.SetAttributo("Chiave2", Messaggio.Date.UtcDateTime);
                        all.SetAttributo("Chiave3", codiceCliente);
                        all.SetAttributo("Chiave4", Messaggio.MessageId);

                        all = await _allMan.SalvaAsync(all, m, true);

                        //-------- Memorizzo l'operazione----------------------
                        log = new LogDoc()
                        {
                            IdOggetto = all.Id,
                            TipoOggetto = TipiOggetto.ALLEGATO,
                            Operazione = TipoOperazione.Creato,
                            Utente = User.Identity.Name
                        };
                        _logMan.Salva(log, true);
                        //-------- Memorizzo l'operazione----------------------
                    }
                }

                //var i = await _context.SaveChangesAsync();
                _elmMan.Salva(elemento, true);

                //-------- Memorizzo l'operazione----------------------
                log = new LogDoc()
                {
                    IdOggetto = elemento.Id,
                    TipoOggetto = TipiOggetto.ELEMENTO,
                    Operazione = TipoOperazione.Creato,
                    Utente = User.Identity.Name
                };
                _logMan.Salva(log, true);
                //-------- Memorizzo l'operazione----------------------

                return elemento;
            }
            catch (Exception ex)
            {
                _logger.LogError($"CreaElementoAsync : {ex.Message}");
            }
            return null;
            
        }

        public async Task<MemoryStream> GetFileAsync(string IdAllegato, string NomeFile)
           
        {
            var cancel = new CancellationToken();
            var m = await _allMan.GetFileAsync(IdAllegato);
            var Messaggio = MimeKit.MimeMessage.Load(m, cancel);
            
            var mpdf = new MemoryStream();
            int i = 0;
            foreach (var attachment in Messaggio.Allegati())
            {
                i++;
                var NomeAllegato = attachment.NomeAllegato(i);
                var fileName = "";
                if (attachment is MessagePart)
                {
                    fileName = attachment.ContentDisposition?.FileName;
                    var rfc822 = (MessagePart)attachment;

                    if (string.IsNullOrEmpty(fileName))
                        fileName = "email-allegata.eml";

                    if (string.Compare(fileName, NomeFile, true) == 0 || string.Compare(NomeAllegato, NomeFile, true) == 0)
                    {
                        rfc822.Message.WriteTo(mpdf);
                        break;
                    }
                }
                else
                {
                    var part = (MimePart)attachment;
                    fileName = part.FileName;

                    if (string.Compare(fileName, NomeFile, true) == 0  || string.Compare(NomeAllegato, NomeFile, true) == 0)
                    {
                        part.Content.DecodeTo(mpdf);
                        break;
                    }
                }
            }
            mpdf.Position = 0;
            return mpdf;

        }

        public async Task<MemoryStream> GetPdfAsync(string IdAllegato, string NomeFile)

        {
            var cancel = new CancellationToken();
            var m = await _allMan.GetFileAsync(IdAllegato);
            var Messaggio = MimeKit.MimeMessage.Load(m, cancel);

            var mpdf = new MemoryStream();
            int i = 0;
            foreach (var attachment in Messaggio.Allegati())
            {
                i++;
                var NomeAllegato = attachment.NomeAllegato(i);
                var fileName = "";
                if (attachment is MessagePart)
                {
                    fileName = attachment.ContentDisposition?.FileName;
                    var rfc822 = (MessagePart)attachment;

                    if (string.IsNullOrEmpty(fileName))
                        fileName = "email-allegata.eml";

                    if (string.Compare(fileName, NomeFile, true) == 0 || string.Compare(NomeAllegato, NomeFile, true) == 0)
                    {
                        rfc822.Message.WriteTo(mpdf);
                        break;
                    }
                }
                else
                {
                    var part = (MimePart)attachment;
                    fileName = part.FileName;

                    if (string.Compare(fileName, NomeFile, true) == 0 || string.Compare(NomeAllegato, NomeFile, true) == 0)
                    {
                        part.Content.DecodeTo(mpdf);
                        break;
                    }
                }
            }

           
            switch (System.IO.Path.GetExtension(NomeFile).ToLower())
            {
                case ".pdf":

                    //using (PdfFileSource pdfToMerge = new PdfFileSource(m))
                    //{
                    //    foreach (PdfPageSource pageToMerge in pdfToMerge.Pages)
                    //    {
                    //        pdfWriter.WritePage(pageToMerge);
                    //    }
                    //}

                    break;
                case ".jpg":
                case ".jpeg":
                case ".png":
                    var fxdoc = new RadFixedDocument();

                    AddPageWithImage(fxdoc, NomeFile, new ImageSource(mpdf, ImageQuality.High));

                    IFormatProvider<RadFixedDocument> fxProvider = new Telerik.Windows.Documents.Fixed.FormatProviders.Pdf.PdfFormatProvider();

                    //var imgstream = new MemoryStream();
                    mpdf = new MemoryStream();
                    fxProvider.Export(fxdoc, mpdf);

                    //using (PdfFileSource pdfToMerge = new PdfFileSource(imgstream))
                    //{
                    //    foreach (PdfPageSource pageToMerge in pdfToMerge.Pages)
                    //    {
                    //        pdfWriter.WritePage(pageToMerge);
                    //    }
                    //}

                    break;
                default:
                    break;
            }
            mpdf.Position = 0;
            return mpdf;
        }


        public async Task<List<EmailAttachments>> GetTmpPdfCompletoAsync(Allegati Allegato, MimeMessage Messaggio, bool daEmail)
        {
            List<EmailAttachments> res = new List<EmailAttachments>();

            var m = await _allMan.GetFileAsync(Allegato.Id.ToString());
            if (Messaggio == null)
                Messaggio = MimeKit.MimeMessage.Load(m, new CancellationToken());

            string NomePdf = Path.Combine(_appEnvironment.WebRootPath, "_tmp");
            if (!Directory.Exists(NomePdf))
        {
                Directory.CreateDirectory(NomePdf);
            }
            NomePdf = Path.Combine(NomePdf, $"{Allegato.Id}.pdf");
            if (File.Exists(NomePdf))
                File.Delete(NomePdf);

            Allegati ll = null;
            if (daEmail==false && Allegato.IdElemento != null)
            {
                try
                {
                    //ll = _context.Allegati
                    //   .Where(a => a.IdElemento == Allegato.IdElemento && a.NomeFile == NOME_FILE_CONTENUTO_EMAIL)
                    //   .OrderByDescending(al => al.DataC)
                    //   .FirstOrDefault();
                    using (SqlConnection cn = new SqlConnection(_context.Connessione))
                    {
                        ll = cn.QueryFirstOrDefault<Allegati>(
                            "Select * from Allegati where IdElemento=@IdElemento AND NomeFile = @NomeFile",
                            new { IdElemento = Allegato.IdElemento.ToString(), NomeFile = $"{Allegato.Id.ToString()}.pdf" });
                        
                        if (ll == null) // per pregresso
                        { 
                            ll = cn.QueryFirstOrDefault<Allegati>(
                               "Select * from Allegati where IdElemento=@IdElemento AND NomeFile = @NomeFile",
                            new { IdElemento = Allegato.IdElemento.ToString(), NomeFile = NOME_FILE_CONTENUTO_EMAIL });
                    }

                    }
                 }
                catch (Exception ex)
                {
                    _logger.LogError($"GetTmpPdfCompletoAsync : elemento - {ex.Message}");
                }
            }
                    if (ll != null)
                    {
                var pdfcompleto = await _allMan.GetFileAsync(ll.Id.ToString());
                        pdfcompleto.Position=0;
                using (var fileStream = new FileStream(NomePdf, FileMode.Create, FileAccess.Write))
                {
                    pdfcompleto.CopyTo(fileStream);
                    }
                try
                {
                    if (Allegato.GetAttributo("jAllegati") != null)
                    {
                        JToken jAllegati = Allegato.GetAttributo("jAllegati");
                        res = jAllegati.ToObject<List<EmailAttachments>>();
                }

                }
                catch
                {
                    int i = 0;
                    foreach (var attachment in Messaggio.Allegati())
                    {
                         i++;
                        var fileName = attachment.NomeAllegato(i);
                        //if (attachment is MessagePart)
                        //{
                        //    fileName = attachment.ContentDisposition?.FileName;
                        //    var rfc822 = (MessagePart)attachment;

                        //    if (string.IsNullOrEmpty(fileName))
                        //        fileName = "email-allegata.eml";
                        //}
                        //else
                        //{
                        //    var part = (MimePart)attachment;
                        //    fileName = part.FileName;
                        //}
                        var incluso = false;
                        switch (System.IO.Path.GetExtension(fileName).ToLower())
                        {
                            case ".pdf":
                            case ".jpg":
                            case ".jpeg":
                            case ".png":
                                incluso = true;
                                break;
                        }
                        var a = new EmailAttachments { Id = fileName, NomeFile = fileName, Valido = false, Incluso=incluso };
                        res.Add(a);
                    }
                }
            }
            else
            {
                try
                {
                    //res = CreaTmpPdfCompleto(NomePdf, Messaggio);
                    var sfdpf = new SFPdf(_appEnvironment,_logger,_config, _allMan);
                    res = sfdpf.CreaTmpPdfCompletoSF(NomePdf, Messaggio);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"GetTmpPdfCompletoAsync : {ex.Message}");
                }

            }

            return res;
        }

        private List<EmailAttachments> CreaTmpPdfCompleto(string NomePdf, MimeMessage Messaggio)
        {
            List<EmailAttachments> res = new List<EmailAttachments>(); ;

            var testfile = NomePdf + ".tmp";
            if (File.Exists(NomePdf))
                File.Delete(NomePdf);

            //testo email
            RadFlowDocument document = null;
            IFormatProvider<RadFlowDocument> formatProvider = new PdfFormatProvider();

            var oggetto = Messaggio.Subject;
            var txt =  Messaggio.TextBody==null ? "": Messaggio.TextBody;
            var pdfstream = new MemoryStream();

            if (txt.Trim().Length > 5  || Messaggio.Allegati().Count() ==0 )
            {
            if (Messaggio.HtmlBody == null)
            {
                if (txt != null)
                {
                document = new RadFlowDocument();
                RadFlowDocumentEditor editor = new RadFlowDocumentEditor(document);
                        editor.InsertText($"Oggetto: {oggetto}");
                        editor.InsertBreak(BreakType.LineBreak);
                        editor.InsertText($"del: {Messaggio.Date.DateTime}");
                        editor.InsertBreak(BreakType.LineBreak);
                editor.InsertText(txt);
                }
            }
            else
            {
               
                try
                {
                
                        var htxt = Messaggio.HtmlBody.Replace("http://", "_http://").Replace("https://", "_https://");
                    HtmlFormatProvider htmlFormatProvider = new HtmlFormatProvider();
                        document = htmlFormatProvider.Import(htxt);
      
                } 
                catch (Exception ex)
                {
                    document = new RadFlowDocument();
                    RadFlowDocumentEditor editor = new RadFlowDocumentEditor(document);
                        editor.InsertText($"Oggetto: {oggetto}");
                        editor.InsertBreak(BreakType.LineBreak);
                        editor.InsertText($"del: {Messaggio.Date.DateTime}");
                        editor.InsertBreak(BreakType.LineBreak);
                    editor.InsertText(txt);
                }                
                
            }
            if (document != null)
            {
            Section section = document.Sections.First();
            Footer footer = section.Footers.Add(HeaderFooterType.Default);
            Paragraph paragraph = footer.Blocks.AddParagraph();
            FieldInfo field = new FieldInfo(document);
            paragraph.Inlines.Add(field.Start);
            paragraph.Inlines.AddRun("Page");
            paragraph.Inlines.Add(field.Separator);
            paragraph.Inlines.AddRun("0");
            paragraph.Inlines.Add(field.End);
            document.UpdateFields();

                try
                {
                formatProvider.Export(document, pdfstream);
            }
                catch (Exception ex)
                {
                document = new RadFlowDocument();
                RadFlowDocumentEditor editor = new RadFlowDocumentEditor(document);
                        //editor.InsertText(txt);
                        editor.InsertText($"Oggetto: {oggetto}");
                        editor.InsertBreak(BreakType.LineBreak);
                editor.InsertText(txt);
                formatProvider.Export(document, pdfstream);
            } 
            }
            }

            using (PdfStreamWriter pdfWriter = new PdfStreamWriter(File.OpenWrite(NomePdf)))
            {

                if (pdfstream.Length > 0)
                {
                    using (PdfFileSource pdfToMerge = new PdfFileSource(pdfstream))
                    {
                        foreach (PdfPageSource pageToMerge in pdfToMerge.Pages)
                        {
                            pdfWriter.WritePage(pageToMerge);
                        }
                    }
                }

                Size A4 = PaperTypeConverter.ToSize(PaperTypes.A4);
                int i = 0;
                foreach (var attachment in Messaggio.Allegati())
                {
                    i++;
                    var fileName = attachment.NomeAllegato(i);
                    var m = new MemoryStream();
                    var incluso = false;

                    if (attachment is MessagePart)
                    {
                        //fileName = attachment.ContentDisposition?.FileName;
                        //if (string.IsNullOrEmpty(fileName))
                        //    fileName = "email-allegata.eml";
                        var rfc822 = (MessagePart)attachment;
                        rfc822.Message.WriteTo(m);
                    }
                    else
                    {
                        var part = (MimePart)attachment;
                        //fileName = part.FileName;

                        part.Content.DecodeTo(m);
                    }
                    try
                    {
                        PdfFileSource pdfToMerge = null;
                    switch (System.IO.Path.GetExtension(fileName).ToLower())
                    {
                        case ".pdf":
                                try
                                {
                                    try
                                    {
                                        pdfToMerge = new PdfFileSource(m);
                                    }
                                    catch (Exception ex)
                                    {
                                        pdfToMerge = new PdfFileSource(m.RepairPdfWithSimpleCrossReferenceTable());
                                    }
                                    using (pdfToMerge)
                            {
                                        //TEST PDF
                                        var flok = true;
                                        try
                                        {
                                            using (PdfStreamWriter TestPdfWriter = new PdfStreamWriter(File.OpenWrite(testfile)))
                                            {
                                                foreach (PdfPageSource pageToMerge in pdfToMerge.Pages)
                                                {
                                                    TestPdfWriter.WritePage(pageToMerge);
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            _logger.LogError($"CreaTmpPdfCompleto: impossibile includere il file {fileName}. {ex.Message}");
                                            flok = false;
                                        }
                                        if (File.Exists(testfile))
                                            File.Delete(testfile);

                                        if (flok)
                                        {
                                foreach (PdfPageSource pageToMerge in pdfToMerge.Pages)
                                {
                                                //var scalaX = 1.0;
                                                //var scalaY = 1.0;
                                                //if (pageToMerge.Size.Height > pageToMerge.Size.Width) // verticale
                                                //{
                                                //    if (pageToMerge.Size.Height > A4.Height ) {
                                                //        scalaY = A4.Height / pageToMerge.Size.Height * .99;
                                                //    }
                                                //    if (pageToMerge.Size.Width > A4.Width)
                                                //    {
                                                //        scalaX = A4.Width / pageToMerge.Size.Width * .99;
                                                //    }
                                                //}
                                                //else {
                                                //    if (pageToMerge.Size.Height > A4.Width)
                                                //    {
                                                //        scalaY = A4.Width / pageToMerge.Size.Height * .99;
                                                //    }
                                                //    if (pageToMerge.Size.Width > A4.Height)
                                                //    {
                                                //        scalaX = A4.Height / pageToMerge.Size.Width * .99;
                                                //    }
                                                //// }
                                                //if (scalaX != 1 || scalaY != 1)
                                                //{
                                                //    PdfPageStreamWriter resultPage = pdfWriter.BeginPage(A4, pageToMerge.Rotation);
                                                //    using (resultPage.SaveContentPosition())
                                                //    {
                                                //        resultPage.ContentPosition.Scale(scalaX, scalaY);
                                                //        resultPage.WriteContent(pageToMerge);
                                                //        resultPage.Dispose();
                                                //    }
                                                //}
                                                //else
                                                //{
                                    pdfWriter.WritePage(pageToMerge);
                                                //}
                                                   
                                            }
                                            incluso = pdfToMerge.Pages.Length>0;
     
                                }
                            }
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
                            var fxdoc = new RadFixedDocument();
                            AddPageWithImage(fxdoc, fileName, new ImageSource(m, ImageQuality.High));
                            IFormatProvider<RadFixedDocument> fxProvider = new Telerik.Windows.Documents.Fixed.FormatProviders.Pdf.PdfFormatProvider();
                            var imgstream = new MemoryStream();
                            fxProvider.Export(fxdoc, imgstream);

                                    pdfToMerge = new PdfFileSource(imgstream);
                                    using (pdfToMerge)
                            {
                                foreach (PdfPageSource pageToMerge in pdfToMerge.Pages)
                                {
                                    pdfWriter.WritePage(pageToMerge);
                                }
                            }
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

            return res;
        }
                    

        public async Task<MemoryStream> GetPdfCompletoAsync(string IdAllegato, string IdElemento, bool daEmail )
        {
            string NomePdf = Path.Combine(_appEnvironment.WebRootPath, "_tmp");
            if (!Directory.Exists(NomePdf))
            {
                Directory.CreateDirectory(NomePdf);
                }
            NomePdf = Path.Combine(NomePdf, $"{IdAllegato}.pdf");
            if (!File.Exists(NomePdf))
            {
                Allegati all = _allMan.Get(IdAllegato);
                var l = await GetTmpPdfCompletoAsync(all, null, daEmail);
            }

            MemoryStream mpdf = new MemoryStream();
            using (FileStream fileStream = File.OpenRead(NomePdf))
            {
                mpdf.SetLength(fileStream.Length);
                //read file to MemoryStream
                fileStream.Read(mpdf.GetBuffer(), 0, (int)fileStream.Length);
            }
            if (File.Exists(NomePdf))
                File.Delete(NomePdf);


            mpdf.Position = 0;
            return mpdf;
        }

        //public async Task<MemoryStream> GetPdfCompletoAsync(string IdAllegato, string IdElemento)
        //{
        //    var cancel = new CancellationToken();

        //    if (!string.IsNullOrEmpty(IdElemento))
        //    {
        //        try
        //        {
        //            var db = new dbluDocsContext(_context.Connessione);


        //            var ll = db.Allegati
        //               .Where(a => a.IdElemento == Guid.Parse(IdElemento) && a.NomeFile == NOME_FILE_CONTENUTO_EMAIL)
        //               .OrderByDescending(al => al.DataC)
        //               .FirstOrDefault();
        //            if (ll != null)
        //            {
        //                AllegatiManager alm = new AllegatiManager(db, _logger);
        //                var pdfcompleto = await alm.GetFileAsync(ll.Id.ToString());
        //                pdfcompleto.Position = 0;
        //                return pdfcompleto;
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.LogError($"GetPdfCompletoAsync : elemento - {ex.Message}");
        //        }

        //    }


        //    var m = await _allMan.GetFileAsync(IdAllegato);
        //    var Messaggio = MimeKit.MimeMessage.Load(m, cancel);

        //    string NomePdf = Path.Combine(_appEnvironment.WebRootPath, "_tmp");
        //    if (!Directory.Exists(NomePdf))
        //    {
        //        Directory.CreateDirectory(NomePdf);
        //    }
        //    NomePdf = Path.Combine(NomePdf, $"{IdAllegato}.pdf");
        //    if (File.Exists(NomePdf))
        //        File.Delete(NomePdf);

        //    //testo email
        //    RadFlowDocument document = null;
        //    IFormatProvider<RadFlowDocument> formatProvider = new PdfFormatProvider();

        //    var txt = Messaggio.TextBody;
        //    if (Messaggio.HtmlBody == null)
        //    {
        //        if (txt != null)
        //        {
        //            document = new RadFlowDocument();
        //            RadFlowDocumentEditor editor = new RadFlowDocumentEditor(document);
        //            editor.InsertText(txt);
        //        }
        //    }
        //    else
        //    {

        //        try
        //        {

        //            txt = Messaggio.HtmlBody.Replace("http://", "_http://").Replace("https://", "_https://");
        //            HtmlFormatProvider htmlFormatProvider = new HtmlFormatProvider();
        //            document = htmlFormatProvider.Import(txt);

        //        }
        //        catch (Exception ex)
        //        {
        //            document = new RadFlowDocument();
        //            RadFlowDocumentEditor editor = new RadFlowDocumentEditor(document);
        //            editor.InsertText(txt);
        //        }

        //    }
        //    var pdfstream = new MemoryStream();
        //    if (document != null)
        //    {
        //        Section section = document.Sections.First();
        //        Footer footer = section.Footers.Add(HeaderFooterType.Default);
        //        Paragraph paragraph = footer.Blocks.AddParagraph();
        //        FieldInfo field = new FieldInfo(document);
        //        paragraph.Inlines.Add(field.Start);
        //        paragraph.Inlines.AddRun("Page");
        //        paragraph.Inlines.Add(field.Separator);
        //        paragraph.Inlines.AddRun("0");
        //        paragraph.Inlines.Add(field.End);
        //        document.UpdateFields();

        //        try
        //        {

        //            formatProvider.Export(document, pdfstream);

        //        }
        //        catch (Exception ex)
        //        {
        //            document = new RadFlowDocument();
        //            RadFlowDocumentEditor editor = new RadFlowDocumentEditor(document);
        //            editor.InsertText(txt);
        //            formatProvider.Export(document, pdfstream);
        //        }
        //    }

        //    var l = new List<EmailAttachments>();
        //    using (PdfStreamWriter pdfWriter = new PdfStreamWriter(File.OpenWrite(NomePdf)))
        //    {

        //        if (pdfstream.Length > 0)
        //        {
        //            using (PdfFileSource pdfToMerge = new PdfFileSource(pdfstream))
        //            {
        //                foreach (PdfPageSource pageToMerge in pdfToMerge.Pages)
        //                {
        //                    pdfWriter.WritePage(pageToMerge);
        //                }
        //            }
        //        }


        //        foreach (var attachment in Messaggio.Allegati())
        //        {
        //            var fileName = "";
        //            m = new MemoryStream();
        //            if (attachment is MessagePart)
        //            {
        //                fileName = attachment.ContentDisposition?.FileName;
        //                var rfc822 = (MessagePart)attachment;

        //                if (string.IsNullOrEmpty(fileName))
        //                    fileName = "email-allegata.eml";
        //                rfc822.Message.WriteTo(m);
        //            }
        //            else
        //            {
        //                var part = (MimePart)attachment;
        //                fileName = part.FileName;

        //                part.Content.DecodeTo(m);
        //            }
        //            try
        //            {
        //                switch (System.IO.Path.GetExtension(fileName).ToLower())
        //                {
        //                    case ".pdf":
        //                        using (PdfFileSource pdfToMerge = new PdfFileSource(m))
        //                        {
        //                            foreach (PdfPageSource pageToMerge in pdfToMerge.Pages)
        //                            {
        //                                pdfWriter.WritePage(pageToMerge);
        //                            }
        //                        }
        //                        break;
        //                    case ".jpg":
        //                        var fxdoc = new RadFixedDocument();
        //                        AddPageWithImage(fxdoc, fileName, new ImageSource(m, ImageQuality.High));
        //                        IFormatProvider<RadFixedDocument> fxProvider = new Telerik.Windows.Documents.Fixed.FormatProviders.Pdf.PdfFormatProvider();
        //                        var imgstream = new MemoryStream();
        //                        fxProvider.Export(fxdoc, imgstream);

        //                        using (PdfFileSource pdfToMerge = new PdfFileSource(imgstream))
        //                        {
        //                            foreach (PdfPageSource pageToMerge in pdfToMerge.Pages)
        //                            {
        //                                pdfWriter.WritePage(pageToMerge);
        //                            }
        //                        }

        //                        break;
        //                    default:
        //                        break;
        //                }
        //            }

        //            catch (Exception ex)
        //            {
        //                _logger.LogError($"GetPdfCompletoAsync: {ex.Message}");

        //            }

        //        }
        //    }

        //    MemoryStream mpdf = new MemoryStream();
        //    using (FileStream fileStream = File.OpenRead(NomePdf))
        //    {
        //        mpdf.SetLength(fileStream.Length);
        //        //read file to MemoryStream
        //        fileStream.Read(mpdf.GetBuffer(), 0, (int)fileStream.Length);
        //    }
        //    if (File.Exists(NomePdf))
        //        File.Delete(NomePdf);


        //    mpdf.Position = 0;
        //    return mpdf;

        //}


        private async Task<Allegati> EstraiAllegatiEmail( 
            Allegati Mail, 
            string ElencoFile, 
            bool AllegaEmail, 
            string Descrizione, 
            TipiAllegati tipoAll, 
            bool daEmail ,
            CancellationToken cancel)
        {
            Allegati all = null;
            try
            {
                using (SqlConnection cn = new SqlConnection(_context.Connessione))
                {
                if (!string.IsNullOrEmpty(ElencoFile) || AllegaEmail)
                {
                        //var fileName = NOME_FILE_CONTENUTO_EMAIL ;
                        var fileName = $"{Mail.Id.ToString()}.pdf";
                        var m = await _allMan.GetFileAsync(Mail.Id.ToString());
                        var Messaggio = MimeKit.MimeMessage.Load(m, cancel);
                    string emailmitt = Messaggio.From.Mailboxes.First().Address;
                   
                    if (AllegaEmail )
                    {  //file pdf completo
                            MemoryStream mpdf = new MemoryStream();
                            PdfEditAction pdfed = new PdfEditAction();
                            pdfed.IdAllegato = Mail.Id.ToString();
                            pdfed.TempFolder = Path.Combine(_appEnvironment.WebRootPath, "_tmp");
                            
                            if (File.Exists(pdfed.FilePdfModificato)) {
                                using (FileStream fileStream = File.OpenRead(pdfed.FilePdfModificato))
                                {
                                    mpdf.SetLength(fileStream.Length);
                                    //read file to MemoryStream
                                    fileStream.Read(mpdf.GetBuffer(), 0, (int)fileStream.Length);
                                }
                                _allMan.Salva(Mail, false);
                            }
                            else { 
                        var l =  await GetTmpPdfCompletoAsync(Mail, null, daEmail); 

                        Mail.SetAttributo("jAllegati", JToken.FromObject(l));
                        //_context.SaveChanges();
                        _allMan.Salva(Mail, false);

                        //prende il file tmp appena creato
                                mpdf = await GetPdfCompletoAsync(Mail.Id.ToString(), null, daEmail);
                            }
                            //var all = _context.Allegati
                            //    .Where(a => (a.Tipo == "FILE" && a.IdElemento == Mail.IdElemento && a.NomeFile == fileName))
                            //    .FirstOrDefault();
                            if (daEmail == false && Mail.IdElemento != null)
                            {
                            all = cn.QueryFirstOrDefault<Allegati>(
                                "Select * from Allegati WHERE tipo ='FILE' and IdElemento= @IdElemento and NomeFile=@NomeFile",
                                new { IdElemento = Mail.IdElemento.ToString(), NomeFile = fileName });
                            }
                            var isNewAll = false;
                        if (all == null)
                        {
                            all = new Allegati()
                            {
                                Descrizione = Descrizione,
                                NomeFile = fileName,
                                Tipo = "FILE",
                                TipoNavigation = tipoAll,
                                Stato = StatoAllegato.Attivo,
                                IdFascicolo = Mail.IdFascicolo,
                                    IdElemento = Mail.IdElemento,
                                    jNote = Mail.jNote
                            };
                                //_context.Add(all);
                                isNewAll = true;
                        }
                        else
                        {
                            all.Descrizione = Descrizione;
                        };

                        if (all.elencoAttributi == null) { all.elencoAttributi = tipoAll.Attributi; }

                        all.SetAttributo("Mittente", emailmitt);
                        all.SetAttributo("Data", Messaggio.Date.UtcDateTime);
                        all.SetAttributo("CodiceSoggetto", Mail.GetAttributo("CodiceSoggetto"));
                        all.SetAttributo("Oggetto", Messaggio.Subject);
                        all.SetAttributo("MessageId", Messaggio.MessageId);

                            
                            if (File.Exists(pdfed.FileAnnotazioni))
                            {
                                var note = File.ReadAllText(pdfed.FileAnnotazioni);
                                all.jNote = JObject.Parse(note);
                            }
                            
                        all = await _allMan.SalvaAsync(all, mpdf, isNewAll);
                    }
                    //else   // file separati
                    //{ 

                    //    var m = await _allMan.GetFileAsync(Mail.Id.ToString());
                    //    var Messaggio = MimeKit.MimeMessage.Load(m, cancel);

                    //    string emailmitt = Messaggio.From.Mailboxes.First().Address;  
                    
                    //    if (AllegaEmail)
                    //    {
                    //        var fileName = "email-contenuto";
                    //        var txt = Messaggio.TextBody;
                    //        if (Messaggio.HtmlBody == null)
                    //        {
                    //            fileName += ".txt";
                    //        }
                    //        else
                    //        {
                    //            fileName += ".html";
                    //            txt = Messaggio.HtmlBody;
                    //        }

                    //        byte[] byteArray = Encoding.UTF8.GetBytes(txt);
                    //        var mb = new MemoryStream(byteArray);

                    //        var all = _context.Allegati
                    //            .Where(a => (a.Tipo == "FILE" && a.IdElemento == Mail.IdElemento && a.NomeFile== fileName))
                    //            .FirstOrDefault();

                    //        if (all == null)
                    //        {
                    //            all = new Allegati()
                    //            {
                    //                Descrizione = Descrizione,
                    //                NomeFile = fileName,
                    //                Tipo = "FILE",
                    //                TipoNavigation = tipoAll,
                    //                Stato = StatoAllegato.Attivo,
                    //                IdFascicolo = Mail.IdFascicolo,
                    //                IdElemento = Mail.IdElemento
                    //            };
                    //            _context.Add(all);
                    //        } else
                    //        {
                    //            all.Descrizione = Descrizione;
                    //        };

                    //        if (all.Attributi == null) { all.Attributi = tipoAll.Attributi; }

                    //        all.SetAttributo("Mittente", emailmitt);
                    //        all.SetAttributo("Data", Messaggio.Date.UtcDateTime);
                    //        all.SetAttributo("CodiceSoggetto", Mail.GetAttributo("CodiceSoggetto"));
                    //        all.SetAttributo("Oggetto", Messaggio.Subject);
                    //        all.SetAttributo("MessageId", Messaggio.MessageId);

                    //        all = await _allMan.SalvaAsync(all, mb);
                    //    }


                    //file da allegare singolarmente
                    if (!string.IsNullOrEmpty(ElencoFile))
                    {
                var listafile = ElencoFile.Split(";").ToList();
                            int i = 0;
                            foreach (var attachment in Messaggio.Allegati())
                {
                                i++;
                                fileName = attachment.NomeAllegato(i);
                    m = new MemoryStream();
                    if (attachment is MessagePart)
                    {
                                    //fileName = attachment.ContentDisposition?.FileName;
                                    //if (string.IsNullOrEmpty(fileName))
                                    //  fileName = "email-allegata.eml";                                    

                        var rfc822 = (MessagePart)attachment;

                        //using (var stream = File.Create(fileName))
                        rfc822.Message.WriteTo(m);
                    }
                    else
                    {
                        var part = (MimePart)attachment;
                                    //fileName = part.FileName;

                        //using (var stream = File.Create(fileName))
                        part.Content.DecodeTo(m);
                    }
                    if (listafile.Contains(fileName))
                    {
                                    //var all = _context.Allegati
                                    //    .Where(a => (a.Tipo == "FILE" && a.IdElemento == Mail.IdElemento && a.NomeFile == fileName))
                                    //    .FirstOrDefault();
                                    var all2 = cn.QueryFirstOrDefault<Allegati>(
                                        "Select * from Allegati WHERE tipo ='FILE' and IdElemento= @IdElemento and NomeFile=@NomeFile",
                                        new { IdElemento = Mail.IdElemento.ToString(), NomeFile = fileName });

                                    var isNewAll = false;

                                    if (all2 == null)
                                {
                                        all2 = new Allegati()
                        {
                                        Descrizione = Mail.Descrizione,
                            NomeFile = fileName,
                            Tipo = "FILE",
                            TipoNavigation = tipoAll,
                            Stato = StatoAllegato.Attivo,
                                        IdFascicolo = Mail.IdFascicolo,
                                        IdElemento = Mail.IdElemento
                        };
                                        //_context.Add(all);
                                        isNewAll = true;
                                }

                                    if (all2.elencoAttributi == null) { all2.elencoAttributi = tipoAll.Attributi; }
                                    all2.Descrizione = Descrizione;
                                    all2.SetAttributo("Mittente", emailmitt);
                                    all2.SetAttributo("Data", Messaggio.Date.UtcDateTime);
                                    all2.SetAttributo("CodiceSoggetto", Mail.GetAttributo("CodiceSoggetto"));
                                    all2.SetAttributo("Oggetto", Messaggio.Subject);
                                    all2.SetAttributo("MessageId", Messaggio.MessageId);
                               
                                    all2 = await _allMan.SalvaAsync(all2, m, isNewAll);

                    }
                }
                }
                }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"EstraiAllegatiEmail : {ex.Message}");
                return null;
            }
            return all;
        }

        public async Task<Elementi> CreaElementoFascicoloAsync(string IdAllegato, 
            string IdFascicolo,
            string IdElemento, 
            string Categoria, 
            string TipoElemento, 
            string CodiceSoggetto, 
            string NomeSoggetto,
            string ElencoFile, 
            bool AllegaEmail, 
            string Descrizione,
            ClaimsPrincipal User)
        {
            try
            {
                Fascicoli f = null;
                var cancel = new CancellationToken();

                var Allegato = _allMan.Get(IdAllegato);
                if (Descrizione == null)
                    Descrizione = Allegato.Descrizione;

                //TipiAllegati tipoAll = _context.TipiAllegati
                //        .Where(t => t.Codice == "FILE")
                //        .FirstOrDefault();
                TipiAllegati tipoAll = _allMan.GetTipoAllegato("FILE");

                Allegato.SetAttributo("CodiceSoggetto", CodiceSoggetto);
                Allegato.SetAttributo("NomeSoggetto", NomeSoggetto);
                //var attr = Allegato.Attributi;
                //attr["CodiceCliente"] = CodiceCliente;
                //Allegato.Attributi = attr;
                //Allegato.UtenteUM = User.Identity.Name;
                Allegato.DataUM = DateTime.Now;
                if (Allegato.IdFascicolo == null && !string.IsNullOrEmpty(IdFascicolo))
                {
                    Allegato.IdFascicolo = Guid.Parse(IdFascicolo);
                }

                var isNew = false;
                if (Allegato.IdFascicolo == null)
                {
                    //CreaFascicolo nuovo fascicolo e assegna alla mail
                    f = new Fascicoli();
                    f.Categoria = Categoria;
                    f.CategoriaNavigation = _fasMan.GetCategoria(Categoria);
                    f.elencoAttributi = f.CategoriaNavigation.Attributi;

                    //f.UtenteC = User.Identity.Name;
                    //_context.Add(f);
                    isNew = true;
                    Allegato.IdFascicolo = f.Id;
                    f.Descrizione = Descrizione;
                }
                else
                {
                    
                    //f = _context.Fascicoli
                    //    .Where(x => x.Id == Allegato.IdFascicolo)
                    //    .Include(s => s.CategoriaNavigation)
                    //    .FirstOrDefault();
                    f = _fasMan.Get(IdFascicolo);
                    if (f.elencoAttributi == null) { 
                        f.elencoAttributi = f.CategoriaNavigation.Attributi;
                    }
                }
                f.CodiceSoggetto = CodiceSoggetto;
                f.SetAttributo("CodiceSoggetto", CodiceSoggetto);
                f.SetAttributo("NomeSoggetto", NomeSoggetto);
                if (_fasMan.Salva(f, isNew) == false) return null ;


                //-------- Memorizzo l'operazione----------------------
                LogDoc log = new LogDoc()
                {
                    Data = DateTime.Now,
                    IdOggetto = f.Id,
                    TipoOggetto = TipiOggetto.FASCICOLO,
                    Utente = User.Identity.Name
                };
                if (isNew) log.Operazione = TipoOperazione.Creato; else log.Operazione = TipoOperazione.Modificato;
                _logMan.Salva(log, true);
                //-------- Memorizzo l'operazione----------------------

                //if (Allegato.IdElemento == null)
                //{
                    //crea nuovo elemento e e assegna alla mail ?
                    var e = new Elementi();
                    e.Tipo = TipoElemento;
                    e.IdFascicolo = f.Id;
                    e.Descrizione = Descrizione;
                //_context.Add(e);
                isNew = true;
                    e.IdFascicoloNavigation = f;
                //TipiElementi tipoEl = _context.TipiElementi
                //        .Where(t => t.Codice == TipoElemento)
                //        .FirstOrDefault();
                TipiElementi tipoEl =  _elmMan.GetTipoElemento(TipoElemento);
                e.TipoNavigation = tipoEl;
                e.elencoAttributi = tipoEl.Attributi;

                    Allegato.IdElemento = e.Id;
                //if (!string.IsNullOrEmpty(IdElemento)) {
                //    var te = _elmMan.GetTipoElemento(TipoElemento);
                //    var el = _elmMan.Get(IdElemento,0);
                //    el.TipoNavigation = te;
                //    e.elencoAttributi = te.Attributi;
                //    e.elencoAttributi.SetValori(el.elencoAttributi.GetValori());
                //}
                e.SetAttributo("CodiceSoggetto", CodiceSoggetto);
                e.SetAttributo("NomeSoggetto", NomeSoggetto);

                if (_elmMan.Salva(e,isNew) == false) return null;

                //-------- Memorizzo l'operazione----------------------
                log = new LogDoc()
                {
                    Data = DateTime.Now,
                    IdOggetto = e.Id,
                    TipoOggetto = TipiOggetto.ELEMENTO ,
                    Operazione = TipoOperazione.Creato,
                    Utente = User.Identity.Name
                };
                 _logMan.Salva(log, true);
                //-------- Memorizzo l'operazione----------------------


                //var i = await _context.SaveChangesAsync(cancel);
                Allegato.Stato = StatoAllegato.Elaborato;
                if (_allMan.Salva(Allegato, false)== false) return null;

                //-------- Memorizzo l'operazione----------------------
                log = new LogDoc()
                {
                    Data = DateTime.Now,
                    IdOggetto = Allegato.Id,
                    TipoOggetto = TipiOggetto.ALLEGATO ,
                    Operazione = TipoOperazione.Elaborato,
                    Utente = User.Identity.Name
                };
                _logMan.Salva(log, true);
                //-------- Memorizzo l'operazione----------------------

                //estrae i file dalla mail presenti in lista e li assegna all'elemento
                var estrai = await EstraiAllegatiEmail(Allegato, ElencoFile, AllegaEmail, Descrizione, tipoAll, false, cancel);

                

                return e;
                    }
            catch (Exception ex)
                    {
                _logger.LogError($"CreaFascicoloAsync : {ex.Message}");
            }
            return null;
        }

        public async Task<Elementi> CreaElementoAsync(string IdFascicolo, string IdAllegato, string TipoElemento,
            string CodiceSoggetto, string ElencoFile, bool AllegaEmail, string Descrizione, ClaimsPrincipal User)
            {
            try
                {
                //Fascicoli f = null;
                Elementi e = null;
                var cancel = new CancellationToken();

                var Allegato = _allMan.Get(IdAllegato);

                //TipiAllegati tipoAll = _context.TipiAllegati
                //        .Where(t => t.Codice == "FILE")
                //        .FirstOrDefault();
                TipiAllegati tipoAll = _allMan.GetTipoAllegato("FILE");

                Allegato.SetAttributo("CodiceSoggetto", CodiceSoggetto);
                //var attr = Allegato.Attributi;
                //attr["CodiceCliente"] = CodiceCliente;
                //Allegato.Attributi = attr;
                //Allegato.UtenteUM = User.Identity.Name;
                Allegato.DataUM = DateTime.Now;

                //f = _context.Fascicoli
                //        .Where(x => x.Id == Allegato.IdFascicolo)
                //        .FirstOrDefault();


                //if (Allegato.IdElemento == null)
                //{
                    //crea nuovo elemento e e assegna alla mail ?
                    e = new Elementi();
                    e.Tipo = TipoElemento;
                    e.IdFascicolo = (Guid)Allegato.IdFascicolo;
                    e.Descrizione = Descrizione;
                    //_context.Add(e);

                    Allegato.IdElemento = e.Id;
                //}else
                //{
                //    e = _context.Elementi
                 //           .Where(x => x.Id == Allegato.IdElemento)
                 //           .FirstOrDefault();
                 //}

                //var i = await _context.SaveChangesAsync(cancel);
                var i = _elmMan.Salva(e, true);


                //-------- Memorizzo l'operazione----------------------
                LogDoc log = new LogDoc()
                {
                    Data = DateTime.Now,
                    IdOggetto = e.Id,
                    TipoOggetto = TipiOggetto.ELEMENTO,
                    Operazione = TipoOperazione.Creato,
                    Utente = User.Identity.Name
                };
                _logMan.Salva(log, true);
                //-------- Memorizzo l'operazione----------------------


                var estrai = await EstraiAllegatiEmail(Allegato, ElencoFile, AllegaEmail, Descrizione, tipoAll, false, cancel);


                return e;
            }
            catch (Exception ex)
            {
                _logger.LogError($"CreaFascicoloAsync : {ex.Message}");
            }
            return null;
        }

        //public async Task<List<ISoggetti>> GetSoggetti()
        //{
        //    if (! (ServizioSoggetti == null)) {
        //        return ServizioSoggetti.GetSoggetti();
        //    }

        //    return await _context.Soggetti.ToListAsync<ISoggetti>();
        //}

        //public ISoggetti GetSoggetto(string Codice)
        //{
        //    if (!(ServizioSoggetti == null))
        //    {
        //        return ServizioSoggetti.GetSoggetto(Codice);
        //    }
        //    else
        //    {
        //        return _sggMan.Get(Codice);
        //    }
           
        //}

        public async Task<bool> AllegaAElementoFascicolo(string IdAllegato,
                string IdFascicolo,
                string IdElemento,
                string ElencoFile,
                bool AllegaEmail,
                string Descrizione,
                ClaimsPrincipal User,
                BPMDocsProcessInfo Info,
                Dictionary<string, VariableValue> variabili)
         {
            try
            {
                var cancel = new CancellationToken();

                var Allegato = _allMan.Get(IdAllegato);
                
                if (Descrizione == null)
                    Descrizione = Allegato.Descrizione;

                TipiAllegati tipoAll = _allMan.GetTipoAllegato("FILE");
                Fascicoli f = _fasMan.Get(IdFascicolo);
                Elementi e = _elmMan.Get(IdElemento, 0);
                if (tipoAll != null && f != null & e != null)
                {
                    //if (Allegato.IdFascicolo == null)
                    Allegato.IdFascicolo = f.Id;
                    //if (Allegato.IdElemento == null)
                    Allegato.IdElemento = e.Id; 
                    //var i = await _context.SaveChangesAsync(cancel);
                    Allegato.Stato = StatoAllegato.Elaborato;
                    var i = _allMan.Salva(Allegato, false);

                    //-------- Memorizzo l'operazione----------------------
                    LogDoc log = new LogDoc()
                    {
                        Data = DateTime.Now,
                        IdOggetto = Allegato.Id,
                        TipoOggetto = TipiOggetto.ALLEGATO,
                        Operazione = TipoOperazione.Elaborato,
                        Utente = User.Identity.Name
                    };
                    _logMan.Salva(log, true);
                    //-------- Memorizzo l'operazione----------------------

                    //estrae i file dalla mail presenti in lista e li assegna all'elemento
                    Allegati all = await EstraiAllegatiEmail(Allegato, ElencoFile, AllegaEmail, Descrizione, tipoAll,true, cancel);

                    var sfdpf = new SFPdf(_appEnvironment, _logger, _config, _allMan);
                    var estrai = all != null;
                    estrai = estrai && await sfdpf.MarcaAllegatoSF(all, e.elencoAttributi);

                    Info.StatoPrec = (int)e.Stato;  
                    Info.Stato = (int)e.Stato;
                    estrai = estrai && AvviaProcesso(Info, e , variabili);

                    return estrai;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"AllegaAElementoFascicolo : {ex.Message}");
            }
            return false;
        }


        private void AddPageWithImage(RadFixedDocument document, string description, ImageSource imageSource)
        {
            RadFixedPage page = document.Pages.AddPage();

            //page.Size = new Size(Telerik.Windows.Documents.Media.Unit.MmToDip(210), Telerik.Windows.Documents.Media.Unit.MmToDip(297));
            page.Size = Telerik.Windows.Documents.Model.PaperTypeConverter.ToSize(Telerik.Windows.Documents.Model.PaperTypes.A4);
            if (imageSource.Height < imageSource.Width)
                page.Size = new Size(Telerik.Windows.Documents.Media.Unit.MmToDip(297), Telerik.Windows.Documents.Media.Unit.MmToDip(210));


            Size size = CalculateFitImageToPage(page.Size, imageSource);
            Image image = new Image();
            image.ImageSource = imageSource;
            image.Width = size.Width;
            image.Height = size.Height;

            page.Content.Add(image);

            //FixedContentEditor editor = new FixedContentEditor(page);
            //editor.GraphicProperties.StrokeThickness = 0;
            //editor.GraphicProperties.IsStroked = false;
            //editor.GraphicProperties.FillColor = new RgbColor(200, 200, 200);
            //editor.DrawRectangle(new Rect(0, 0, PageSize.Width, PageSize.Height));
            //editor.Position.Translate(Margins.Left, Margins.Top);

            //Block block = new Block();
            //block.HorizontalAlignment = Telerik.Windows.Documents.Fixed.Model.Editing.Flow.HorizontalAlignment.Center;
            //block.TextProperties.FontSize = 22;
            //block.InsertText(description);
            //Size blockSize = block.Measure(RemainingPageSize);
            //editor.DrawBlock(block, RemainingPageSize);

            //editor.Position.Translate(Margins.Left, blockSize.Height + Margins.Top + 20);

            //Block imageBlock = new Block();
            //imageBlock.HorizontalAlignment = Telerik.Windows.Documents.Fixed.Model.Editing.Flow.HorizontalAlignment.Center;
            //imageBlock.InsertImage(imageSource);
            //editor.DrawBlock(imageBlock, RemainingPageSize);
        }

        private static Size CalculateFitImageToPage(Size fitSize, ImageSource source)
        {
            double k1 = fitSize.Width / source.Width;
            double k2 = fitSize.Height / source.Height;

            double k = Math.Min(k1, k2);

            return new Size(source.Width * k, source.Height * k);
        }


        public async Task<bool> MarcaAllegati(Elementi el)
        {
            bool res = true;
            try
            {
                using (SqlConnection cn = new SqlConnection(_context.Connessione))
                {
                    // IList<Allegati> ll = _context.Allegati
                    //.Where(l => l.Tipo == "FILE" && l.IdElemento == el.Id).ToList();
                    List<Allegati> ll = _elmMan.GetAllegatiElemento(el.Id);

                    foreach (Allegati a in ll)
                    {

                        if (a.NomeFile.ToLower().EndsWith(".pdf"))
                        {
                            var sfdpf = new SFPdf(_appEnvironment, _logger, _config, _allMan);
                            bool r = await sfdpf.MarcaAllegatoSF(a, el.elencoAttributi);
                        res = res && r;                    
                    }
                }

            }
            }
            catch (Exception ex)
            {
                res=false;
                _logger.LogError($"MarcaAllegati : {ex.Message}");
            }
            return res;
        }


        public async Task<bool> MarcaAllegato(Allegati all, ElencoAttributi att)
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
                    if (a.Valore!=null)
                    {
                    reportSource.Parameters.Add(a.Nome,  a.Valore == null ? "" : a.Valore ) ;
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
                Size A4 = PaperTypeConverter.ToSize(PaperTypes.A4);

                if (pdfstream != null)
                { 
                    using (PdfStreamWriter pdfWriter = new PdfStreamWriter(File.OpenWrite(NomePdf)))
                {
                    PdfFileSource pdfToMerge = null;
                    try
                    {
                            pdfstream.Position = 0;
                        pdfToMerge = new PdfFileSource(pdfstream);
                    }
                    catch (Exception ex)
                    {
                        pdfToMerge = new PdfFileSource(pdfstream.RepairPdfWithSimpleCrossReferenceTable());
                    }

                    using (pdfToMerge)
                    {
                        PdfPageStreamWriter resultPage = null;

                        var p = 0;
                        var pt = pdfToMerge.Pages.Length;
                            double scalaX = 1.0;
                            double scalaY = 1.0;

                        foreach (PdfPageSource pageToMerge in pdfToMerge.Pages)
                        {
                            p++;
                            var rot = pageToMerge.Rotation;
                            double newWidth = pageToMerge.Size.Width * .95;
                            double newHeight = pageToMerge.Size.Height * .95;
                            double MarginX = 0;
                            double MarginY = pageToMerge.Size.Height - newHeight;
                            double rotaz = 0;

                            //switch (pageToMerge.Rotation)
                            //{
                            //    case Rotation.Rotate90:
                            //        rotaz = 90;
                            //        break;
                            //    case Rotation.Rotate180:
                            //        rotaz = 180;
                            //        break;
                            //    case Rotation.Rotate270:
                            //        rotaz = 270;
                            //        break;
                            //}
                               if (pageToMerge.Size.Height > pageToMerge.Size.Width) // verticale
                                {
                                    if (pageToMerge.Size.Height > A4.Height)
                                    {
                                        scalaY = A4.Height / pageToMerge.Size.Height * .99;
                                    }
                                    if (pageToMerge.Size.Width > A4.Width)
                                    {
                                        scalaX = A4.Width / pageToMerge.Size.Width * .99;
                                    }
                                }
                                else
                                {
                                    if (pageToMerge.Size.Height > A4.Width)
                                    {
                                        scalaY = A4.Width / pageToMerge.Size.Height * .99;
                                    }
                                    if (pageToMerge.Size.Width > A4.Height)
                                    {
                                        scalaX = A4.Height / pageToMerge.Size.Width * .99;
                                    }
                                }

                            if ((pageToMerge.Rotation == Rotation.Rotate0 ||
                                  pageToMerge.Rotation == Rotation.Rotate180)
                                && pageToMerge.Size.Width > pageToMerge.Size.Height)
                            {
                                if (pageToMerge.Rotation == Rotation.Rotate0)
                                {
                                    rot = Rotation.Rotate90;
                                    rotaz = 90;
                                }
                                else
                                {
                                    rot = Rotation.Rotate270;
                                    rotaz = 270;
                                }
                                    newWidth = pageToMerge.Size.Height * .95 * scalaY;
                                    newHeight = pageToMerge.Size.Width * .95 * scalaX;
                                MarginX = pageToMerge.Size.Width - newHeight;
                                MarginY = 0;
                            }
                            resultPage = pdfWriter.BeginPage(pageToMerge.Size, rot);

                            using (resultPage.SaveContentPosition())
                            {

                                //reportSource.Parameters.Add("NumeroProtocollo", "124578/2020");
                                //reportSource.Parameters.Add("DataConsegna", DateTime.Today.AddDays(20));
                                //reportSource.Parameters.Add("DataConsRich", DateTime.Today.AddDays(15));
                                //reportSource.Parameters.Add("CodiceSoggetto", "25468");
                                reportSource.Parameters.Add("NPag", p);
                                reportSource.Parameters.Add("TPag", pt);

                                RenderingResult curEti = reportProcessor.RenderReport("PDF", reportSource, null);

                                PdfFileSource pdfeti = new PdfFileSource(new MemoryStream(curEti.DocumentBytes));
                                //if (rot != pageToMerge.Rotation)
                                //{
                                //    resultPage.ContentPosition.Rotate(-90);
                                //    resultPage.ContentPosition.Translate(0, pageToMerge.Size.Height);
                                //}
                                //else
                                //{
                                //    resultPage.ContentPosition.Translate(0, 0);
                                //}
                                switch (rot)
                                {
                                    case Rotation.Rotate90:
                                        resultPage.ContentPosition.Rotate(-90);
                                        resultPage.ContentPosition.Translate(0, pageToMerge.Size.Height);
                                        break;
                                    case Rotation.Rotate180:
                                        resultPage.ContentPosition.Rotate(-180);
                                        resultPage.ContentPosition.Translate(pageToMerge.Size.Width, pageToMerge.Size.Height);
                                        break;
                                    case Rotation.Rotate270:
                                        resultPage.ContentPosition.Rotate(90);
                                        resultPage.ContentPosition.Translate(pageToMerge.Size.Width, 0);
                                        break;

                                    default:
                                        resultPage.ContentPosition.Translate(0, 0);
                                        break;
                                }
                                resultPage.WriteContent(pdfeti.Pages[0]);

                                //aggiungo la pagina ridotta
                                resultPage.ContentPosition.Rotate(0);
                                    resultPage.ContentPosition.Scale(0.95 * scalaX, 0.95 * scalaY);
                                resultPage.ContentPosition.Translate(MarginX, MarginY);
                                resultPage.WriteContent(pageToMerge);

                                resultPage.Dispose();
                            }

                        }

                    }
                }
                MemoryStream mpdf = new MemoryStream();
                using (FileStream fileStream = File.OpenRead(NomePdf))
                {
                    mpdf.SetLength(fileStream.Length);
                    //read file to MemoryStream
                    fileStream.Read(mpdf.GetBuffer(), 0, (int)fileStream.Length);
                }
               //mpdf.Position = 0;
               var al = await _allMan.SalvaAsync(all, mpdf, false);
               if (File.Exists(NomePdf))
                    File.Delete(NomePdf);
                }
            }
            catch (Exception ex)
            {
                res = false;
                _logger.LogError($"MarcaAllegato : {ex.Message}");
            }
            return res;
        }

        public bool AvviaProcesso(BPMDocsProcessInfo Info , Elementi el, Dictionary<string, VariableValue> variabili)
        {
            bool res = true;
            try
            {
                if (!string.IsNullOrEmpty(el.TipoNavigation.Processo))
                {

                    var pd = new BPMProcessDefinition(_bpm._eng);
                    if (variabili== null) 
                        variabili = new Dictionary<string, VariableValue>();

                    VariableValue v = VariableValue.FromObject(el.Id.ToString());
                    variabili.Add("IdElemento", v);
                    v = VariableValue.FromObject(JsonConvert.SerializeObject(el));
                    variabili.Add("jElemento", v);

                    v = VariableValue.FromObject(JsonConvert.SerializeObject(Info));
                    variabili.Add("_ProcessInfo", v);

                    var pi = pd.Start("", el.TipoNavigation.Processo, el.Id.ToString(), variabili);

                    res = (pi != null && pi.Result != null );

                }
            }
            catch (Exception ex)
            {
                res = false;
                _logger.LogError($"AvviaProcesso : {ex.Message}");
            }
            return res;
        }
    
        public bool AvviaProcesso(BPMDocsProcessInfo Info, Elementi el)
        {
            return AvviaProcesso(Info, el, null);
        }

        public async Task<RisultatoAzione> InoltraEmail(string IdAllegato, string Indirizzi, bool chiudi, ClaimsPrincipal User)
        {
            RisultatoAzione res = new RisultatoAzione();
            try
            {
                Allegati al = _allMan.Get(IdAllegato);
                MemoryStream eml = new MemoryStream();
                eml = await _allMan.GetFileAsync(al.Id.ToString());
                if (eml == null)
                {
                    res.Successo = false;
                    res.Messaggio = "Messaggio non valido.";
                    return res;
                }
                CancellationToken c = new CancellationToken();
                MimeKit.MimeMessage message = MimeKit.MimeMessage.Load(eml, c);

                using (var client = new SmtpClient())
                {
                    EmailServer srv = null;
                    //srv = _context.EmailServer.Where(s => s.Attivo == true && s.InUscita == true).FirstOrDefault();
                    using (SqlConnection cn = new SqlConnection(_context.Connessione))
                    {
                        srv = cn.QueryFirstOrDefault<EmailServer>("select Top 1 * from EmailServer where Attivo=1 and InUscita=1");
                    }
#if (DEBUG)

    srv.Email = "jobaid@dblu.it";
    srv.Server = "mail.dblu.it";
    srv.Porta = 465;
    srv.Ssl = true;
    srv.Utente = "jobaid@dblu.it";
    srv.Password = "j0b41d!";

#endif
                    
                    if (srv == null)
                    {
                        res.Successo = false;
                        res.Messaggio = "server di posta in uscita non definito.";
                        return res;
                    }
                    await client.ConnectAsync(srv.Server, srv.Porta, srv.Ssl, c);
                    var flOk = true;
                    if (client.IsConnected)
                    {
                        if (srv.Utente != "")
                        {
                        await client.AuthenticateAsync(srv.Utente, srv.Password, c);
                            flOk = client.IsAuthenticated;
                        }

                        if (flOk)
                        {
                            //message.To.Clear();
                            InternetAddressList listind = new InternetAddressList();
                            foreach (string ind in Indirizzi.Replace(";", ",").Split(","))
                            {
                                listind.Add(new MailboxAddress(ind));
                            }
                            //message.To.AddRange(listind);
                            //if (al.Stato >= StatoAllegato.Chiuso)
                            //{
                            //    var sender = new MailboxAddress(srv.Nome, srv.Email);
                            //    await client.SendAsync(message, sender, listind.Mailboxes, c);
                            //}
                            //else
                            //{
                            //    await client.SendAsync(message, c);
                            //}

                            //// clear the Resent-* headers in case this message has already been Resent...
                            //message.ResentSender = null;
                            //message.ResentFrom.Clear();
                            //message.ResentReplyTo.Clear();
                            //message.ResentTo.Clear();
                            //message.ResentCc.Clear();
                            //message.ResentBcc.Clear();

                            //// now add our own Resent-* headers...
                            //message.ResentFrom.Add(new MailboxAddress(srv.Nome, srv.Email));
                            //message.ResentReplyTo.Add(new MailboxAddress(srv.Nome, srv.Email));
                            ////message.ResentTo.AddRange(listind);
                            //message.ResentMessageId = MimeUtils.GenerateMessageId();
                            //message.ResentDate = DateTimeOffset.Now;
                            var newmessage = new MimeMessage();
                            newmessage.From.Add(new MailboxAddress(srv.Nome, srv.Email));
                            newmessage.ReplyTo.Add(new MailboxAddress(srv.Nome, srv.Email));
                            newmessage.To.AddRange(listind);
                            newmessage.Subject = "FWD: " + message.Subject;

                            // now to create our body...
                            var builder = new BodyBuilder();
                            //  builder.TextBody = message.TextBody;


                            if(message.HtmlBody == null)
                            {
                                builder.TextBody = "Da : " + message.From + "\nA : " + message.To + "\nInviato : " + message.Date.DateTime + "\nOggetto : " + message.Subject + "\n" + message.TextBody;
                            }
                            else
                            {
                                 //builder.TextBody = $"<body><div><b>Da : </b>{newmessage.From}<br><b>A: </b>{newmessage.ReplyTo}<br><b>Inviato : </b>{newmessage.Date.UtcDateTime}<br><b>Oggetto : </b>{newmessage.Subject}<br><br> {message.TextBody} </div></body>";
                                 var htxt = message.HtmlBody;
                                if (! htxt.Contains("<body>") ) {
                                    htxt = $"<body>{htxt}</body>";
                                }
                                string sfrom = System.Web.HttpUtility.HtmlEncode(message.From);
                                string sTo = System.Web.HttpUtility.HtmlEncode(message.To);
                                builder.HtmlBody = htxt.Replace("<body>", $"<body><div><b>Da : </b>{sfrom}<br><b>A: </b>{sTo}<br><b>Inviato : </b>{message.Date.DateTime}<br><b>Oggetto : </b>{message.Subject}<br><br></div><br>");

                            }

                            foreach (MimeEntity att in message.Allegati()) { 
                                builder.Attachments.Add(att);
                            }
                            newmessage.Body = builder.ToMessageBody();
                            await client.SendAsync(newmessage, c);
                            //-------- Memorizzo l'operazione----------------------
                            LogDoc log = new LogDoc()
                            {
                                Data = DateTime.Now,
                                IdOggetto = al.Id,
                                TipoOggetto = TipiOggetto.ALLEGATO,
                                Operazione = TipoOperazione.Inoltrato,
                                Utente = User.Identity.Name
                            };
                            _logMan.Salva(log, true);
                            //-------- Memorizzo l'operazione----------------------

                            if (chiudi) { 
                                al.Stato = StatoAllegato.Chiuso;
                                //await _context.SaveChangesAsync();
                                _allMan.Salva(al,false);
                                //-------- Memorizzo l'operazione----------------------
                                log = new LogDoc()
                                {
                                    Data = DateTime.Now,
                                    IdOggetto = al.Id,
                                    TipoOggetto = TipiOggetto.ALLEGATO,
                                    Operazione = TipoOperazione.Chiuso,
                                    Utente = User.Identity.Name
                                };
                                _logMan.Salva(log, true);
                                //-------- Memorizzo l'operazione----------------------

                            }
                        }
                        else
                        {
                            res.Successo = false;
                            res.Messaggio = "autenticazione al server di posta fallita.";
                        }
                        await client.DisconnectAsync(true);
                    }
                    else
                    {
                        res.Successo = false;
                        res.Messaggio = "connessione al server di posta fallita.";
                    }
                }                
            }
            catch (Exception ex)
            {
                res.Successo = false;
                res.Messaggio = ex.Message;
                _logger.LogError($"InoltraEmail : {ex.Message}");
            }
            return res;
        }



        public async Task<RisultatoAzione> RispondiEmail(
            string IdAllegato, 
            string NomeServer, 
            string to,
            string cc,
            string Oggetto, 
            string Testo, 
            bool allegaEmail, 
            bool chiudiEmail,
            ClaimsPrincipal User)
         
        {
            RisultatoAzione res = new RisultatoAzione();
            try
            {
                Allegati al = _allMan.Get(IdAllegato);
                MemoryStream eml = new MemoryStream();
                eml = await _allMan.GetFileAsync(al.Id.ToString());
                if (eml == null)
                {
                    res.Successo = false;
                    res.Messaggio = "Messaggio non valido.";
                    return res;
                }
                CancellationToken c = new CancellationToken();
               
                MimeKit.MimeMessage message = MimeKit.MimeMessage.Load(eml, c);

                using (var client = new SmtpClient())
                {
                    EmailServer srv = null;
                    //srv = _context.EmailServer.Where(s => s.Attivo == true && s.InUscita == true).FirstOrDefault();
                    using (SqlConnection cn = new SqlConnection(_context.Connessione))
                    {
                        srv = cn.QueryFirstOrDefault<EmailServer>($"Select * from EmailServer where Nome in (Select NomeServerInUscita from  EmailServer where Nome = '{NomeServer}')");
                        if (srv == null) srv  = cn.QueryFirstOrDefault<EmailServer>("select Top 1 * from EmailServer where Attivo=1 and InUscita=1");
                    }
                    #if (DEBUG)

                        srv.Email = "jobaid@dblu.it";
                        srv.Server = "mail.dblu.it";
                        srv.Porta = 465;
                        srv.Ssl = true;
                        srv.Utente = "jobaid@dblu.it";
                        srv.Password = "j0b41d!";

                    #endif

                    if (srv == null)
                    {
                        res.Successo = false;
                        res.Messaggio = "server di posta in uscita non definito.";
                        return res;
                    }
                    await client.ConnectAsync(srv.Server, srv.Porta, srv.Ssl, c);
                    var flOk = true;
                    if (client.IsConnected)
                    {
                        if (srv.Utente != "")
                        {
                            await client.AuthenticateAsync(srv.Utente, srv.Password, c);
                            flOk = client.IsAuthenticated;
                        }

                        if (flOk)
                        {
                            var newmessage = new MimeMessage();
                            var utente = _usrManager.GetUser(User.Identity.Name);
                            if (utente != null && !string.IsNullOrEmpty(utente.Email))
                            {
                               newmessage.From.Add(new MailboxAddress($"{utente.Name} {utente.LastName}", utente.Email));
                            }
                            else { 
                            newmessage.From.Add(new MailboxAddress(srv.Nome, srv.Email));
                            }
                            //newmessage.ReplyTo.Add(new MailboxAddress(srv.Nome, srv.Email));

                            InternetAddressList listind = new InternetAddressList();
                            foreach (string ind in to.Replace(";", ",").Split(","))
                            {
                                listind.Add(new MailboxAddress("", ind));
                            }
                            newmessage.To.AddRange(listind);
                            listind.Clear();
                            if(!string.IsNullOrEmpty(cc))
                                foreach (string ind in cc.Replace(";", ",").Split(","))
                                {
                                    listind.Add(new MailboxAddress("", ind));
                                }
                            if (listind.Count > 0) {
                                newmessage.Cc.AddRange(listind);
                            }
                            newmessage.Subject = Oggetto;  //"FWD: " + message.Subject;

                            // now to create our body...
                            var builder = new BodyBuilder();
                            

                            //testo
                            if (string.IsNullOrEmpty(Testo))
                            {
                                Testo = "Da : " + message.From + "\nA : " + message.To + "\nInviato : " + message.Date.DateTime + "\nOggetto : " + message.Subject + "\n" + message.TextBody;
                            }


                            if (allegaEmail) {
                                Testo = $"{Testo} \n\n{message.TextBody}";
                                }

                                    builder.TextBody = Testo;

                            //if (message.HtmlBody == null || !allegaMail )
                            //{
                            //    if (Testo == null) { 
                            //        builder.TextBody = "Da : " + message.From + "\nA : " + message.To + "\nInviato : " + message.Date.DateTime + "\nOggetto : " + message.Subject + "\n" + message.TextBody;
                            //    }
                            //    else
                            //    {
                            //        builder.TextBody = Testo;
                            //    }
                            //}
                            //else
                            //{
                            //    var htxt = message.HtmlBody;
                            //    if (!htxt.Contains("<body>"))
                            //    {
                            //        htxt = $"<body>{htxt}</body>";
                            //    }

                            //    if (Testo == null) { 
                            //        string sfrom = System.Web.HttpUtility.HtmlEncode(message.From);
                            //        string sTo = System.Web.HttpUtility.HtmlEncode(message.To);
                            //        builder.HtmlBody = htxt.Replace("<body>", $"<body><div><b>Da : </b>{sfrom}<br><b>A: </b>{sTo}<br><b>Inviato : </b>{message.Date.DateTime}<br><b>Oggetto : </b>{message.Subject}<br><br></div><br>");
                            //        }
                            //    else
                            //    {
                            //        builder.HtmlBody = htxt.Replace("<body>", $"<body><div>{Testo}<br><br></div><br>");
                            //    }
                            //}

                            //string NomePdf = Path.Combine(_appEnvironment.WebRootPath, "_tmp");
                            //NomePdf = Path.Combine(NomePdf, $"riepilogo_{IdAllegato}.pdf.sav");
                            //if (File.Exists(NomePdf))
                            //{
                            //    builder.Attachments.Add(NomePdf);
                            //}
                            
                            newmessage.Body = builder.ToMessageBody();
                            await client.SendAsync(newmessage, c);
                            //-------- Memorizzo l'operazione----------------------
                            LogDoc log = new LogDoc()
                            {
                                Data = DateTime.Now,
                                IdOggetto = al.Id,
                                TipoOggetto = TipiOggetto.ALLEGATO,
                                Operazione = TipoOperazione.Risposto,
                                Utente = User.Identity.Name
                            };
                            _logMan.Salva(log, true);

                            ////-------- Memorizzo l'operazione----------------------

                            //if (chiudi)
                            //{
                            //    al.Stato = StatoAllegato.Chiuso;
                            //    //await _context.SaveChangesAsync();
                            //    _allMan.Salva(al, false);
                            //    //-------- Memorizzo l'operazione----------------------
                            //    log = new LogDoc()
                            //    {
                            //        Data = DateTime.Now,
                            //        IdOggetto = al.Id,
                            //        TipoOggetto = TipiOggetto.ALLEGATO,
                            //        Operazione = TipoOperazione.Chiuso,
                            //        Utente = User.Identity.Name
                            //    };
                            //    _logMan.Salva(log, true);
                            //    //-------- Memorizzo l'operazione----------------------

                            //}
                              Allegati newall = new Allegati()
                                {
                                    Descrizione = Oggetto,
                                    NomeFile = "email",
                                    Tipo = al.Tipo,
                                    TipoNavigation = al.TipoNavigation,
                                    Stato = StatoAllegato.Spedito,
                                    Origine = NomeServer
                              };
                              newall.elencoAttributi = al.TipoNavigation.Attributi;
                            string emailmitt = newmessage.From.Mailboxes.First().Address;
                            newall.SetAttributo("Mittente", emailmitt);
                            newall.SetAttributo("Destinatario", to);
                            newall.SetAttributo("Data", newmessage.Date.UtcDateTime);
                            newall.SetAttributo("Oggetto", Oggetto);
                            newall.SetAttributo("MessageId", newmessage.MessageId);
                            newall.SetAttributo("CodiceSoggetto", al.GetAttributo("CodiceSoggetto"));
                            newall.SetAttributo("NomeSoggetto", al.GetAttributo("NomeSoggetto"));

                            
                            MemoryStream file = new MemoryStream();
                            await newmessage.WriteToAsync(file);
                            newall = await _allMan.SalvaAsync(newall, file, true);

                            if (chiudiEmail) {
                                al.Stato = StatoAllegato.Chiuso;
                                if (_allMan.Salva(al, false)) {
                                    LogDoc logC = new LogDoc()
                                    {
                                        Data = DateTime.Now,
                                        IdOggetto = al.Id,
                                        TipoOggetto = TipiOggetto.ALLEGATO,
                                        Operazione = TipoOperazione.Chiuso,
                                        Utente = User.Identity.Name
                                    };
                                    _logMan.Salva(logC, true);
                                }

                            }
                            
                        }
                        else
                        {
                            res.Successo = false;
                            res.Messaggio = "autenticazione al server di posta fallita.";
                        }
                        await client.DisconnectAsync(true);
                    }
                    else
                    {
                        res.Successo = false;
                        res.Messaggio = "connessione al server di posta fallita.";
                    }
                }
            }
            catch (Exception ex)
            {
                res.Successo = false;
                res.Messaggio = ex.Message;
                _logger.LogError($"InoltraEmail : {ex.Message}");
            }
            return res;
        }


        public RisultatoAzione SpostaEmail(string IdAllegato, string NomeServer, ClaimsPrincipal User)
        {
            RisultatoAzione res = new RisultatoAzione();
            try
            {

                using (SqlConnection cn = new SqlConnection(_context.Connessione))
                {
                    var n = cn.Execute("UPDATE allegati SET Origine=@Origine WHERE Id=@Id ",
                        new { Origine = NomeServer, Id = IdAllegato });
                    if (n > 0)
                    {
                        res.Successo = true;
                        res.Messaggio = $"Email spostata in {NomeServer}.";
                    }
                    else
                    {
                        res.Successo = false;
                        res.Messaggio = $"parametri non validi";
                    }
                }

                //-------- Memorizzo l'operazione----------------------
                LogDoc log = new LogDoc()
                {
                    IdOggetto = Guid.Parse(IdAllegato),
                    TipoOggetto = TipiOggetto.ALLEGATO,
                    Operazione = TipoOperazione.Spostato,
                    Utente = User.Identity.Name
                };
                _logMan.Salva(log, true);
                //-------- Memorizzo l'operazione----------------------
            }
            catch (Exception ex)
            {
                res.Successo = false;
                res.Messaggio = ex.Message;
                _logger.LogError($"SpostaEmail : {ex.Message}");
            }
            return res;
        }

        public List<EmailElementi> ListaElementiEmail(string IdFascicolo)
        {

            var res = new List<EmailElementi>();
            try
            {

                if (!string.IsNullOrEmpty(IdFascicolo))
                {
                using (SqlConnection cn = new SqlConnection(_context.Connessione))
                {
                        //var sqlEl = " SELECT distinct  e.Id, e.Revisione, e.Tipo, e.Descrizione, e.Chiave1, e.Chiave2, e.Chiave3, e.Chiave4, e.Chiave5, te.Descrizione AS DescrizioneTipo, e.Stato, e.IdFascicolo, isnull(am.stato, 0) as Ultimo "
                        //    + " FROM Elementi AS e INNER JOIN TipiElementi AS te ON te.Codice = e.Tipo "
                        //    + " LEFT JOIN Allegati AM on am.idfascicolo = e.idfascicolo and am.idelemento = e.id and am.tipo = 'EMAIL' "
                        //    + " WHERE (e.IdFascicolo = @IdFascicolo)";

                         //var sqlEl = " SELECT distinct  e.IdElemento Id, e.Revisione, e.TipoElemento Tipo, e.DscElemento Descrizione, e.Campo1 Chiave1, e.Campo2 Chiave2, e.Campo3 Chiave3, e.Campo4 Chiave4, e.Campo5 Chiave5, e.DscTipoElemento AS DescrizioneTipo, e.Stato, e.IdFascicolo, isnull(am.stato, 0) as Ultimo, "
                         //    + "(select top 1 Operazione from LogDoc where IdOggetto=e.IdElemento Order by Data DESC) LastOp "
                         //   + " FROM vListaElementi AS e "
                         //   + " LEFT JOIN Allegati AM on am.idfascicolo = e.idfascicolo and am.idelemento = e.IdElemento and am.tipo = 'EMAIL' "
                         //   + " WHERE (e.IdFascicolo = @IdFascicolo)";
                        var sqlEl = " SELECT distinct  e.IdElemento, e.Revisione, e.TipoElemento, e.DscElemento, e.Campo1, e.Campo2, e.Campo3 , e.Campo4 , e.Campo5, e.DscTipoElemento , e.Stato, e.IdFascicolo, isnull(am.stato, 0) as Ultimo, e.DataC, "
                             + "(select top 1 Operazione from LogDoc where IdOggetto=e.IdElemento Order by Data DESC) LastOp "
                            + " FROM vListaElementi AS e "
                            + " LEFT JOIN Allegati AM on am.idfascicolo = e.idfascicolo and am.idelemento = e.IdElemento and am.tipo = 'EMAIL' "
                          + " WHERE (e.IdFascicolo = @IdFascicolo) ORDER BY Ultimo DESC, e.DataC DESC ";

                    res = cn.Query<EmailElementi>(sqlEl, new { IdFascicolo = IdFascicolo }).ToList();
                }
            }
            }
            catch (Exception ex)
            {
                _logger.LogError($"ListaElementiEmail : {ex.Message}");
            }

         
            return res;

        }


        public async Task<MemoryStream> GetPdfRiepilogo(string IdEmail)
        {
            MemoryStream mpdf = null;

            string NomePdf = Path.Combine(_appEnvironment.WebRootPath, "_tmp");
            NomePdf = Path.Combine(NomePdf, $"riepilogo_{IdEmail}.pdf");
            if (File.Exists(NomePdf))
                File.Delete(NomePdf);

            try
            {
            if (!File.Exists(NomePdf))
            {
                string etichetta = Path.Combine(_appEnvironment.ContentRootPath, "Report", _config["Docs:EtichettaProtocollo"]);
                if (!File.Exists(etichetta))
                {
                    _logger.LogError("etichetta inesistente");
                    return null;
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


                    // cambiato in left join per elementi senza allegato

                    var sql = "SELECT e.* FROM allegati m INNER JOIN elementi e ON e.idfascicolo = m.idfascicolo " +
                    " LEFT JOIN allegati f ON f.idfascicolo = m.idfascicolo and f.idelemento = e.id and f.Tipo = 'FILE' and f.NomeFile = cast(m.id as varchar(50)) + '.pdf' " +
                    " WHERE m.id = @IdEmail Order by e.datac";
                    using (SqlConnection cn = new SqlConnection(_context.Connessione))
                    {
                        List<Elementi> el = cn.QueryAsync<Elementi>(sql, new { IdEmail = IdEmail }).Result.ToList();

                        //Create a new PDF document
                        Syncfusion.Pdf.PdfDocument document = new Syncfusion.Pdf.PdfDocument();
                        document.PageSettings.SetMargins(0);
                        //Add a page to the document
                        Syncfusion.Pdf.PdfPage page = document.Pages.Add();

                        //Create PDF graphics for the page
                        Syncfusion.Pdf.Graphics.PdfGraphics graphics = page.Graphics;

                        var all = _allMan.Get(IdEmail);
                        var l = await GetTmpPdfCompletoAsync(all, null, true);
                        string NomePdftmp = Path.Combine(_appEnvironment.WebRootPath, "_tmp", $"{IdEmail}.pdf");
                        MemoryStream mpdftmp = new MemoryStream();
                        using (FileStream fileStream = File.OpenRead(NomePdftmp))
                        {
                            mpdftmp.SetLength(fileStream.Length);
                            //read file to MemoryStream
                            fileStream.Read(mpdftmp.GetBuffer(), 0, (int)fileStream.Length);
                        }
                        if (File.Exists(NomePdftmp))
                            File.Delete(NomePdftmp);
                        Syncfusion.Pdf.Parsing.PdfLoadedDocument pdftmp = new Syncfusion.Pdf.Parsing.PdfLoadedDocument(mpdftmp);


                        float curpos = 20;
                        float etiHeight = page.Size.Height * (float) .05;
                        int i = 0;
                        //foreach (Elementi e in el)
                        int nrpag = (int)Math.Round(el.Count() / 13.0+0.5,0,MidpointRounding.AwayFromZero) ;

                        for(int p=0; p< nrpag; p++) {
                            if (p > 0) {
                                page = document.Pages.Add();
                                graphics = page.Graphics;
                                curpos = 20;
                            }
                            for (i=0; i< 13;i++)
                        {
                                int j = p * 13 + i;
                                if (j >= el.Count())
                                    break;
                                
                                Elementi e = el[j];

                            e.TipoNavigation = _elmMan.GetTipoElemento(e.Tipo);
                            e.elencoAttributi = e.TipoNavigation.Attributi;
                            e.elencoAttributi.SetValori(e.Attributi);
                        
                        foreach (Attributo a in e.elencoAttributi.ToList())
                        {
                            if (a.Valore != null)
                            {
                                reportSource.Parameters.Add(a.Nome, a.Valore == null ? "" : a.Valore);
                            }
                        }
                                //i++;
                                reportSource.Parameters.Add("NPag", p+1);
                                reportSource.Parameters.Add("TPag", pdftmp.Pages.Count + nrpag );

                            RenderingResult curEti = reportProcessor.RenderReport("PDF", reportSource, null);

                            Syncfusion.Pdf.Parsing.PdfLoadedDocument pdfEti = new Syncfusion.Pdf.Parsing.PdfLoadedDocument(new MemoryStream(curEti.DocumentBytes));

                            //Load the page

                            //using (FileStream fileRiepilogo = new FileStream($"{NomePdf}_{curpos}.pdf", FileMode.CreateNew, FileAccess.ReadWrite))
                            //{
                            //    //salvataggio e chiusura
                            //    pdfEti.Save(fileRiepilogo);
                            //}

                            Syncfusion.Pdf.PdfLoadedPage loadedPage = pdfEti.Pages[0] as Syncfusion.Pdf.PdfLoadedPage;

                            //Create the template from the page.
                            Syncfusion.Pdf.Graphics.PdfTemplate template = loadedPage.CreateTemplate();

                            //Draw the template
                            Syncfusion.Drawing.PointF posizione = new Syncfusion.Drawing.PointF() { X = 0, Y = curpos  };

                            graphics.DrawPdfTemplate(template, posizione,
                                new Syncfusion.Drawing.SizeF(loadedPage.Size.Width, loadedPage.Size.Height));
                            curpos += etiHeight + 20;
                            pdfEti.Close();

                    }
                        }
               
                        i = 1;
                        foreach (Syncfusion.Pdf.PdfLoadedPage lptmp in pdftmp.Pages) {
                            

                            try
                            {

                                Syncfusion.Pdf.Graphics.PdfTemplate template = lptmp.CreateTemplate();
                            page = document.Pages.Add();
 
                            graphics = page.Graphics;
                            etiHeight = page.Size.Height * .05F + 5;

                            Syncfusion.Drawing.PointF posizione = new Syncfusion.Drawing.PointF() { X = 0, Y = etiHeight + 1 };

                            Syncfusion.Drawing.SizeF pDest = SFPdf.CalcolaProporzioni(lptmp.Size.Width , lptmp.Size.Height , page.Size.Width * 0.95F, page.Size.Height - etiHeight);

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
                                        posizione = new Syncfusion.Drawing.PointF() { X = 0, Y =  0};
                                    }
                            
                                    graphics.DrawPdfTemplate(template, posizione, pDest);
                                    if (pDest.Height < pDest.Width)
                                    {
                                     graphics.RotateTransform(90);
                                     graphics.TranslateTransform(0, -page.Size.Height);
                                    }
                                    break;
                                default:

                                    
                                    if (pDest.Height  < pDest.Width)
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
                            reportSource.Parameters.Add("TPag", pdftmp.Pages.Count+1);
                            reportSource.Parameters.Add("flRiepilogo", true);

                            RenderingResult curEti = reportProcessor.RenderReport("PDF", reportSource, null);
                            Syncfusion.Pdf.Parsing.PdfLoadedDocument pdfEti = new Syncfusion.Pdf.Parsing.PdfLoadedDocument(new MemoryStream(curEti.DocumentBytes));
                            posizione = new Syncfusion.Drawing.PointF() { X = 0, Y = 5 };
                            Syncfusion.Pdf.PdfLoadedPage loadedPage = pdfEti.Pages[0] as Syncfusion.Pdf.PdfLoadedPage;
                            template = loadedPage.CreateTemplate();
                            
                            
                            graphics.DrawPdfTemplate(template, posizione,
                                new Syncfusion.Drawing.SizeF(loadedPage.Size.Width , loadedPage.Size.Height ));
                            pdfEti.Close();
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError($"GetPdfRiepilogo (errore pagina { i } ) : {ex.Message}");

                                document.ImportPageRange(pdftmp, i-1, i-1);
                                i++;

                            }

                        }
                        pdftmp.Close();

                        using (FileStream fileRiepilogo = new FileStream(NomePdf, FileMode.CreateNew, FileAccess.ReadWrite))
                        {
                            //salvataggio e chiusura

                            document.Save(fileRiepilogo);
                        }

                        document.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetPdfRiepilogo : {ex.Message}");

            }
            mpdf = new MemoryStream();
            using (FileStream fileStream = File.OpenRead(NomePdf))
            {
                mpdf.SetLength(fileStream.Length);
                //read file to MemoryStream
                fileStream.Read(mpdf.GetBuffer(), 0, (int)fileStream.Length);
            }
            if (File.Exists(NomePdf))
                File.Delete(NomePdf);

            mpdf.Position = 0;
            return mpdf;

        }


        public  RisultatoAzione RiapriEmail(string IdAllegato, ClaimsPrincipal User)
        {
            RisultatoAzione res = new RisultatoAzione();
            try
            {
                Allegati al = _allMan.Get(IdAllegato);
                if (al.IdElemento == null)
                {
                    al.Stato = StatoAllegato.Attivo;
                }
                else {
                    al.Stato = StatoAllegato.Elaborato;
                }
                res.Successo = _allMan.Salva(al, false);
                
                //-------- Memorizzo l'operazione----------------------
                LogDoc log = new LogDoc()
                {
                    IdOggetto = Guid.Parse(IdAllegato),
                    TipoOggetto = TipiOggetto.ALLEGATO,
                    Utente = User.Identity.Name
                };
                 log.Operazione = TipoOperazione.Riaperto; 
                _logMan.Salva(log, true);
                //-------- Memorizzo l'operazione----------------------

            }
            catch (Exception ex)
            {
                res.Successo = false;
                res.Messaggio = ex.Message;
                _logger.LogError($"RiapriEmail : {ex.Message}");
            }
            return res;
        }


        public async Task<RisultatoAzione> CancellaEmail(string IdAllegato, bool EliminaDaServer, ClaimsPrincipal User)
        {
            RisultatoAzione res = new RisultatoAzione();
            try
            {
                
                if (EliminaDaServer)
                {
                    Allegati al = _allMan.Get(IdAllegato);
                    
                    MemoryStream eml = new MemoryStream();
                    eml = await _allMan.GetFileAsync(al.Id.ToString());
                    if (eml == null)
                    {
                        res.Successo = false;
                        res.Messaggio = "Messaggio non valido.";
                        return res;
                    }
                    CancellationToken c = new CancellationToken();
                    MimeKit.MimeMessage message = MimeKit.MimeMessage.Load(eml, c);

                    using (var client = new ImapClient())
                    {

                        EmailServer srv = _serMan.GetServer(al.Origine);

                        if (srv == null) {
                            res.Successo = false;
                            res.Messaggio = $"Server di posta non valido: {al.Origine}.";
                            return res;
                        }

                        await client.ConnectAsync(srv.Server, srv.Porta, srv.Ssl, c);
                        var flOk = true;
                        if (client.IsConnected)
                        {
                            
                            if (srv.Utente != "")
                            {
                                await client.AuthenticateAsync(srv.Utente, srv.Password, c);
                                flOk = client.IsAuthenticated;
                            }

                            if (flOk)
                            {

                                IMailFolder inbox = null;
                                if (!string.IsNullOrEmpty(srv.Cartella))
                                {
                                    inbox = client.GetFolder(srv.Cartella, c);
                                }
                                else
                                {
                                    inbox = client.Inbox;
                                }
                                await inbox.OpenAsync(FolderAccess.ReadWrite, c);
                                
                                var uids = await inbox.SearchAsync(SearchQuery.HeaderContains("Message-Id", message.MessageId));
                                inbox.AddFlags(uids, MessageFlags.Deleted, silent: true);
                                inbox.Close();
                            }
                        }
                        else
                        {
                            res.Successo = false;
                            res.Messaggio = "autenticazione al server di posta fallita.";
                        }
                        await client.DisconnectAsync(true);
                    }
                    
                }

                
                if (_allMan.Cancella(IdAllegato))
                {
                    res.Successo = true;
                    //-------- Memorizzo l'operazione----------------------
                    _logMan.Salva( new LogDoc { IdOggetto = Guid.Parse(IdAllegato) ,
                        TipoOggetto = TipiOggetto.ALLEGATO,
                        Operazione = TipoOperazione.Cancellato,
                        Utente = User.Identity.Name
                    } ,  true);
                    //-------- Memorizzo l'operazione----------------------
                }

            }
            catch (Exception ex)
            {
                res.Successo = false;
                res.Messaggio = ex.Message;
                _logger.LogError($"CancellaEmail : {ex.Message}");
            }
            return res;
        }

        public RisultatoAzione CancellaElemento(string IdElemento, short Revisione, ClaimsPrincipal User)
        {
            RisultatoAzione res = new RisultatoAzione();
            try
            {
                res.Successo = true;
                using (SqlConnection cn = new SqlConnection(_context.Connessione))
                {

                    if (_elmMan.Cancella(IdElemento, Revisione)) { 
                        res.Successo = true;

                        _logMan.Salva(new LogDoc
                        {
                            IdOggetto = Guid.Parse(IdElemento),
                            TipoOggetto = TipiOggetto.ELEMENTO,
                            Operazione = TipoOperazione.Cancellato,
                            Utente = User.Identity.Name
                        }, true);
                    }
                }
            }
            catch (Exception ex)
            {
                res.Successo = false;
                res.Messaggio = ex.Message;
                _logger.LogError($"Cancella Elemento : {ex.Message}");
            }
            return res;
        }


        public async Task<PdfEditAction> GetFilePdfCompletoAsync(PdfEditAction pdf, bool daEmail)
        {
            pdf.FilePdf = "";
           string NomePdf = Path.Combine(_appEnvironment.WebRootPath, "_tmp");
            if (!Directory.Exists(NomePdf))
            {
                Directory.CreateDirectory(NomePdf);
            }
            NomePdf = Path.Combine(NomePdf, $"{pdf.IdAllegato}.pdf");
            if (!File.Exists(NomePdf))
            {
                Allegati all = _allMan.Get(pdf.IdAllegato);
                pdf.FileAllegati = await GetTmpPdfCompletoAsync(all, null, daEmail);
            }
            if (File.Exists(NomePdf))
                pdf.FilePdf = NomePdf;

            return pdf;
        }

        public bool PulisciFileTemp(string IdAllegato) {

            try
            {
                PdfEditAction pdf = new PdfEditAction();

                pdf.IdAllegato = IdAllegato;
                pdf.TempFolder = Path.Combine(_appEnvironment.WebRootPath, "_tmp");
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
                if (System.IO.File.Exists(pdf.FilePdf))
                {
                    System.IO.File.Delete(pdf.FilePdf);
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Pulisci File Temp: {ex.Message}");
            }
            return false;
        }

        public BPMDocsProcessInfo GetProcessInfo(
            TipiOggetto Tipo,
            AzioneOggetto Azione 
            )
        {
            BPMProcessInfo baseinfo = _bpm.GetProcessInfo();
            var mapper = _mapperConfig.CreateMapper();
            BPMDocsProcessInfo info = mapper.Map<BPMDocsProcessInfo>(baseinfo);
            info.TipoOggetto = Tipo;
            info.Azione = Azione;
            //info.StatoPrec = StatoPrec;
            //info.Stato = StatoAttuale;
            return info;
        }
    }
}
