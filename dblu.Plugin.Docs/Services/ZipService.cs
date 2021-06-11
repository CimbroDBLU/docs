using AutoMapper;
using BPMClient;
using Dapper;
using dblu.CamundaClient;
using dblu.Docs.Classi;
using dblu.Docs.Interfacce;
using dblu.Docs.Models;
using dblu.Portale.Core.Infrastructure;
using dblu.Portale.Core.Infrastructure.Identity.Classes;
using dblu.Portale.Core.Infrastructure.Identity.Services;
using dblu.Portale.Plugin.Docs.Class;
using dblu.Portale.Plugin.Docs.Models;
using dblu.Portale.Plugin.Docs.ViewModels;
using dblu.Portale.Services.Camunda;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NToastNotify;
using Syncfusion.Pdf.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telerik.Reporting;
using Telerik.Reporting.Processing;

namespace dblu.Portale.Plugin.Docs.Services
{
    public class ZipService
    {

        public readonly dbluDocsContext _context;
        public readonly ILogger _logger;
        private readonly IWebHostEnvironment _appEnvironment;
        private readonly IToastNotification _toastNotification;
        public readonly CamundaService _bpm;
        public readonly AllegatiManager _allMan;
        public readonly FascicoliManager _fasMan;
        public readonly ElementiManager _elmMan;
        public readonly ServerEmailManager _serMan;

        public readonly LogDocManager _logMan;
        //        private readonly SoggettiManager _sggMan;
        public IConfiguration _config { get; }
        public ISoggettiService _soggetti;
        
        private IApplicationUsersManager _usrManager;
        private MapperConfiguration _mapperConfig;

        public ZipService(CamundaService bpm, dbluDocsContext db,
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
            _logger = loggerFactory.CreateLogger("ZipService");
            _bpm = bpm;
            _allMan = new AllegatiManager(_context.Connessione, _logger);
            _fasMan = new FascicoliManager(_context.Connessione, _logger);
            _elmMan = new ElementiManager(_context.Connessione, _logger);
            _serMan = new ServerEmailManager(_context.Connessione, _logger);
            _logMan = new LogDocManager(_context.Connessione, _logger);
            _config = config;
            _usrManager = usrManager;
            _mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<BPMProcessInfo, BPMDocsProcessInfo>());
            try
            {
                _soggetti = sogg;
            }
            catch
            {

            }

        }

        //public List<TipiAllegati> GetTipiZip()
            //{

        //    var res = new List<TipiAllegati>();
            //    try
            //    {

            //        using (SqlConnection cn = new SqlConnection(_context.Connessione))
            //        {
        //            var sqlZip = " SELECT A.* "
        //                + " FROM TipiAllegati A "
        //                + " WHERE Cartella='ZIP' ";

        //            res = cn.Query<TipiAllegati>(sqlZip).ToList();
            //        }
            //    }

            //    catch (Exception ex)
            //    {
        //        _logger.LogError($"ListaElementiEmail : {ex.Message}");

            //    }
        //    return res;
        //}
        public List<SelectListItem> GetRuoliZip(IEnumerable<Claim> UserRoles)
        {

            var res = new List<SelectListItem>();
            try
            {
                List<string> rr= new List<string>();
                    if (!string.IsNullOrEmpty(_config["Docs:RuoliZip"]))
                {
                    foreach (string r in _config["Docs:RuoliZip"].Split(","))
                    {
                        Claim c = UserRoles.SingleOrDefault(c => c.Value == r);
                        if (c != null)
                        {
                            rr.Add($"'{r}'");
                        }
                    }
                }   
                    
                if (rr.Count > 0) {

                    IEnumerable<Role> rl = _usrManager.GetRoles().Where(d => rr.Contains(d.Code));

                    foreach (Role ur in rl) 
                        res.Add(new SelectListItem() { Value = ur.RoleId, Text = ur.Name});
                    
            }

            }
            catch (Exception ex)
            {
                _logger.LogError($"GetRuoliZip : {ex.Message}");
            }
            return res;
        }

        public List<string> GetRuoliTipoAll(IEnumerable<Claim> Roles, string TipoAll)
        {

            List<string> l = new List<string>();


            if (TipoAll != "")
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

                        string sql = "Select RoleId FROM [AllegatiInRoles] where [RoleId] IN (" + xRol + ") and [Tipo]='" + TipoAll + "'";
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

        public List<string> GetRuoli(IEnumerable<Claim> Roles, string NomeServer)
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

        public async Task<IList<EmailAttachments>> GetZipFilesAsync(string IdAllegato)
        {
            IList<EmailAttachments> l = new List<EmailAttachments>();
            try
            {

                MemoryStream m = await _allMan.GetFileAsync(IdAllegato);
                m.Position = 0;
                using (ZipArchive za = new ZipArchive(m, ZipArchiveMode.Read))
                {
                    foreach (ZipArchiveEntry entry in za.Entries)
                    {
                        l.Add(new EmailAttachments() { Id = entry.FullName, Incluso = false, NomeFile = entry.Name, Valido = true });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($" GetZipFiles : {ex.Message}");
            }

            return l;
        }

        public async Task<MemoryStream> GetFileFromZipAsync(string IdAllegato, string NomeFile)
        {

            MemoryStream mfile = new MemoryStream();
            try
            {

                MemoryStream m = await _allMan.GetFileAsync(IdAllegato);
                using (ZipArchive za = new ZipArchive(m, ZipArchiveMode.Read))
                {
                    foreach (ZipArchiveEntry entry in za.Entries)
                    {
                        if (string.Compare(entry.Name, NomeFile, true) == 0) {
                            using (var unzippedEntryStream = entry.Open())
                            {
                                unzippedEntryStream.CopyTo(mfile);
                                break;
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($" GetZipFiles : {ex.Message}");
            }
            mfile.Position = 0;
            return mfile;
        }

        public async Task<bool> AddFileToZipAsync(string IdAllegato, string NomeFile, MemoryStream mfile)
        {
            bool res = false;
            try
            {
                mfile.Position = 0;
                using (var resultms = new MemoryStream())
                {
                    using (var resultZip = new ZipArchive(resultms, ZipArchiveMode.Create, true))
                    {
                    MemoryStream m = await _allMan.GetFileAsync(IdAllegato);
                    if (m != null)
                        {
                            m.Position = 0;
                            using (ZipArchive za = new ZipArchive(m, ZipArchiveMode.Read))
                            {
                                foreach (ZipArchiveEntry entry in za.Entries)
                                {
                                    if (string.Compare(entry.Name, NomeFile, true) != 0)
                                    {
                                        //entry.Delete();
                                        //break;
                                        using (var entryS = entry.Open())
                                        {
                                            ZipArchiveEntry e = resultZip.CreateEntry(entry.Name, CompressionLevel.Fastest);
                                            using (var eS = e.Open())
                                            {
                                                entryS.CopyTo(eS);
                                            }
                                        }

                                    }
                                }
                            }
                        }
                    ZipArchiveEntry enew = resultZip.CreateEntry(NomeFile, CompressionLevel.Fastest);
                     using (var eS = enew.Open()) 
                        {
                            mfile.CopyTo(eS);
                        }
                    }
                    resultms.Position = 0;
                    res = await _allMan.SalvaFileAsync(IdAllegato, resultms);                
                }

             }
            catch (Exception ex)
            {
                _logger.LogError($" AddFileToZipAsync : {ex.Message}");
                res = false;
            }

            return res;
        }

        public int CountFileTask(string Tipo, IEnumerable<Claim> Roles)
        {

            int l = 0;
            try
            {
                List<SelectListItem> ListaRuoli = GetRuoliZip(Roles);

                if (ListaRuoli != null && ListaRuoli.Count > 0)
                {
                    BPMQueryTaskDto qp = new BPMQueryTaskDto();

                    List<string> sb = new List<string>();
                    foreach (var li in ListaRuoli){
                        sb.Add(li.Value); 
                    }
                    qp.candidateGroups = sb;
                    
                    l = _bpm._task.GetListCount(_bpm._eng, qp);
                }
            }

            catch (Exception ex)
            {
                _logger.LogError($"CountFileInArrivo: {ex.Message}");
            }

            return l;
        }

        public int CountZipInArrivo(string Tipo, IEnumerable<Claim> Roles)
        {

            int l = 0;
            try
            {
                List<EmailServer> ListaServer = _serMan.GetServersEmailinRoles(Roles, TipiRecordServer.CartellaFile);

                if (ListaServer != null && ListaServer.Count > 0)
                {
                    string xServer = "'";
                    foreach (EmailServer x in ListaServer)
                    {
                        xServer = xServer + x.Cartella + "','";
                    }
                    xServer = xServer.Substring(0, xServer.Length - 2);
                    //xServer = "'" + xServer +"'";


                    using (SqlConnection cn = new SqlConnection(_context.Connessione))
                    {
                        l = cn.ExecuteScalar<int>("Select count(*) from Allegati where Tipo=@Tipo and Stato IN (1,2,3 ) and Origine IN (" + xServer + ")",
                            new { Tipo = Tipo });
                    }
                }
                else
                {
                    using (SqlConnection cn = new SqlConnection(_context.Connessione))
                    {
                        l = cn.ExecuteScalar<int>("Select count(*) from Allegati where Tipo=@Tipo and Stato IN (1,2,3 ) ",
                            new { Tipo = Tipo });

                    }
                }


            }

            catch (Exception ex)
            {
                _logger.LogError($"CountZipInArrivo: {ex.Message}");
            }

            return l;
        }

        public List<ZipViewModel> GetFileTask(string Ruolo, int firstResult, int maxResults)
        {

            List<ZipViewModel> lz = new List<ZipViewModel>();
            try
            {
                BPMQueryTaskDto qp = new BPMQueryTaskDto();
                qp.candidateGroup = Ruolo;
                List<BPMTaskDto> lt = _bpm._task.GetList(_bpm._eng, qp, firstResult, maxResults);
                lz = JsonConvert.DeserializeObject<List<ZipViewModel>>(JsonConvert.SerializeObject(lt));
                //foreach (var t in lt) {
                //    var z = new ZipViewModel() { task = t };
                //}
            }

            catch (Exception ex)
            {
                _logger.LogError($"GetFileTask: {ex.Message}");
            }
            return lz;
        }


        
        public async Task<ZipViewModel> GetZipViewModel(string IdTask, string IdAllegato = "")
        {
            ZipViewModel zm = new ZipViewModel();
            try
            {
                zm.id = IdTask;

                //da task
                if (string.IsNullOrEmpty(IdAllegato))
                {
                BPMClient.BPMVariable var = new BPMClient.BPMVariable();
                Dictionary<string, BPMClient.VariableValue> variables = var.GetAll(_bpm._eng, IdTask).Result;
                    if (variables.ContainsKey("_IdAllegato"))
                    {
                        IdAllegato = variables["_IdAllegato"].GetValue<string>();
                    }
                }


                if (!string.IsNullOrEmpty(IdAllegato))
                {
                    zm.IdAllegato = IdAllegato;
                    Allegati all = _allMan.Get(zm.IdAllegato);
                    if (all != null && all.Tipo == "ZIP") { 
                        zm.CodiceSoggetto = all.GetAttributo("CodiceSoggetto","");
                        zm.NomeSoggetto = all.GetAttributo("NomeSoggetto","");
                        zm.IdFascicolo = all.IdFascicolo.ToString();
                        zm.IdElemento = all.IdElemento.ToString();
                        zm.DescrizioneElemento = all.Descrizione;
                        zm.FileAllegati = await this.GetListaFileZippatiAsync(all,null);
                        zm.StatoZip = all.Stato;

                        Elementi el = null;
                        if (zm.IdElemento != null)
                        {
                            el = _elmMan.Get(zm.IdElemento, 0); 
                        }
                        if (el == null)
                        {
                            zm.Stato = StatoElemento.Attivo;
                        }
                        else
                        {
                            zm.DescrizioneElemento = el.Descrizione;
                            zm.Stato = el.Stato;
                        }
                    }
                    else if (all != null && all.Tipo == "REQ"){
                        zm.CodiceSoggetto = all.GetAttributo("CodiceSoggetto", "");
                        zm.NomeSoggetto = all.GetAttributo("NomeSoggetto", "");
                        zm.IdFascicolo = all.IdFascicolo.ToString();
                        zm.IdElemento = all.IdElemento.ToString();
                        zm.DescrizioneElemento = all.Descrizione;
                        zm.FileAllegati = await this.GetListaFileZippatiAsync(all, null);
                        zm.StatoZip = all.Stato;

                        Elementi el = null;
                        if (zm.IdElemento != null)
                        {
                            el = _elmMan.Get(zm.IdElemento, 0);
                        }
                        if (el == null)
                        {
                            zm.Stato = StatoElemento.Attivo;
                        }
                        else
                        {
                            zm.DescrizioneElemento = el.Descrizione;
                            zm.Stato = el.Stato;
                        }


                    }

                }
            }
            catch (Exception ex)
            {
                _logger.LogError($" GetZipViewModel : {ex.Message}");
            }
            return zm;
        }


        public List<EmailElementi> ListaElementiZip(string IdFascicolo, string IdAllegato)
        {
            var res = new List<EmailElementi>();
            try
            {

                if (!string.IsNullOrEmpty(IdFascicolo))
                {
                    using (SqlConnection cn = new SqlConnection(_context.Connessione))
                    {
                        //var sqlEl = " SELECT distinct e.Id, e.Revisione, e.Tipo, e.Descrizione, e.Chiave1, e.Chiave2, e.Chiave3, e.Chiave4, e.Chiave5, te.Descrizione AS DescrizioneTipo, e.Stato, e.IdFascicolo, isnull(am.stato, 0) as Ultimo "
                        //    + " FROM Elementi AS e INNER JOIN TipiElementi AS te ON te.Codice = e.Tipo "
                        //    + " LEFT JOIN Allegati AM on am.idfascicolo = e.idfascicolo and am.idelemento = e.id and am.tipo = 'ZIP' "
                        //    + " WHERE (e.IdFascicolo = @IdFascicolo)";

                        //var sqlEl = " SELECT distinct  e.IdElemento Id, e.Revisione, e.TipoElemento Tipo, e.DscElemento Descrizione, e.Campo1 Chiave1, e.Campo2 Chiave2, e.Campo3 Chiave3, e.Campo4 Chiave4, e.Campo5 Chiave5, e.DscTipoElemento AS DescrizioneTipo, e.Stato, e.IdFascicolo, isnull(am.stato, 0) as Ultimo "
                        //   + " FROM vListaElementi AS e "
                        //   + " LEFT JOIN Allegati AM on am.idfascicolo = e.idfascicolo and am.idelemento = e.IdElemento and am.tipo = 'ZIP' "
                        //   + " WHERE (e.IdFascicolo = @IdFascicolo)";

                        var sqlall = "";
                        if (!string.IsNullOrEmpty(IdAllegato)) {
                            sqlall = " and AM.Id=@IdAllegato ";
                        }

                        var sqlEl = @$" SELECT distinct  e.IdElemento, e.Revisione, e.TipoElemento, e.DscElemento, e.Campo1, e.Campo2, e.Campo3 , e.Campo4 , e.Campo5, e.DscTipoElemento , e.Stato, e.IdFascicolo, isnull(am.stato, 0) as Ultimo, e.DataC, 
                            (select top 1 Operazione from LogDoc where IdOggetto=e.IdElemento Order by Data DESC) LastOp 
                             FROM vListaElementi AS e 
                            LEFT JOIN Allegati AM on AM.idfascicolo = e.idfascicolo and AM.idelemento = e.IdElemento and AM.tipo = 'ZIP'  {sqlall}
                              WHERE (e.IdFascicolo = @IdFascicolo  ) ORDER BY Ultimo DESC, e.DataC DESC ";

                        res = cn.Query<EmailElementi>(sqlEl, new { IdFascicolo = IdFascicolo, IdAllegato = IdAllegato }).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"ListaElementiZip : {ex.Message}");
            }


            return res;
        }


        public async Task<MemoryStream> GetPdfCompletoAsync(string IdAllegato, string IdElemento, bool daZip)
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
                var l = await GetTmpPdfCompletoAsync(all, null, daZip);
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

        public async Task<PdfEditAction> GetFilePdfCompletoAsync(PdfEditAction pdf, bool daZip)
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
                pdf.FileAllegati = await GetTmpPdfCompletoAsync(all, null, daZip);
            }
            if (File.Exists(NomePdf))
                pdf.FilePdf = NomePdf;
            return pdf;
        }


        public async Task<List<EmailAttachments>> GetTmpPdfCompletoAsync(Allegati Allegato, ZipArchive FileZip, bool daZip)
        {
            List<EmailAttachments> res = new List<EmailAttachments>();
            using MemoryStream Ms = new MemoryStream();
            string NomePdf = Path.Combine(_appEnvironment.WebRootPath, "_tmp");
            if (!Directory.Exists(NomePdf))
            {
                Directory.CreateDirectory(NomePdf);
            }
            NomePdf = Path.Combine(NomePdf, $"{Allegato.Id}.pdf");
            if (File.Exists(NomePdf))
                File.Delete(NomePdf);

            var m = await _allMan.GetFileAsync(Allegato.Id.ToString());
            if (m==null|| m.Length!=0)
            {
                if (FileZip == null)
                    FileZip = new ZipArchive(m, ZipArchiveMode.Read);
            }
            else
            {
                if (FileZip == null)
                {
                    FileZip = new ZipArchive(Ms, ZipArchiveMode.Create,true);
                    FileZip.Dispose();
                    FileZip = new ZipArchive(Ms, ZipArchiveMode.Read, true);
                }
            }
            Allegati ll = null;
            if (daZip == false && Allegato.IdElemento != null)
            {
                try
                {
                    using (SqlConnection cn = new SqlConnection(_context.Connessione))
                    {
                        ll = cn.QueryFirstOrDefault<Allegati>(
                            "Select * from Allegati where IdElemento=@IdElemento AND NomeFile = @NomeFile",
                            new { IdElemento = Allegato.IdElemento.ToString(), NomeFile = $"{Allegato.Id.ToString()}.pdf" });
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
                pdfcompleto.Position = 0;
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
                    foreach (ZipArchiveEntry entry in FileZip.Entries)
                    {
                        string fileName = entry.Name;
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
                        var a = new EmailAttachments { Id = fileName, NomeFile = fileName, Valido = false, Incluso = incluso };
                        res.Add(a);
                    }
                }
            }
            else
            {
                try
                {
                    var sfdpf = new SFPdf(_appEnvironment, _logger, _config, _allMan);
                    string Testo;
                    var mittente = $"{Allegato.elencoAttributi.Get("NomeSoggetto")}";
                    if (Allegato.Tipo == "REQ")
                        mittente = $"{Allegato.elencoAttributi.Get("Email")} - {Allegato.elencoAttributi.Get("NomeSoggetto")}";
                   
                    var oggetto = Allegato.Descrizione;       //Chiave4;
                    var txt = Allegato.Testo;
                    var DataZip = Allegato.elencoAttributi.Get("Data") == null ? Allegato.DataC.ToString() : Allegato.elencoAttributi.Get("Data");

                    Testo = $"Da: {mittente} \nOggetto: {oggetto} \ndel: {DataZip}\n\n {txt} ";
                    if (Allegato.Tipo == "REQ")
                        Testo = $"Da: {mittente} \nOggetto: {oggetto} del: {DataZip}\nRiferimento: {Allegato.elencoAttributi.Get("Riferimento")}\nTipo: {Allegato.elencoAttributi.Get("Tipo")} \n\n {txt} ";
                    
                    res = sfdpf.CreaTmpPdfCompletoSF(NomePdf, FileZip, Testo);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"GetTmpPdfCompletoAsync : {ex.Message}");
                }

            }

            return res;
        }
        
        public async Task<List<EmailAttachments>> GetListaFileZippatiAsync(Allegati Allegato, ZipArchive FileZip)
        {
            List<EmailAttachments> res = new List<EmailAttachments>();

            var m = await _allMan.GetFileAsync(Allegato.Id.ToString());
            if (FileZip == null)
                FileZip = new ZipArchive(m, ZipArchiveMode.Read);

            foreach (ZipArchiveEntry entry in FileZip.Entries)
            {
                string fileName = entry.Name;
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
                var a = new EmailAttachments { Id = fileName, NomeFile = fileName, Valido = false, Incluso = incluso };
                res.Add(a);
            }

            return res;
        }


                public async Task<bool> RemoveFileFromZipAsync(string IdAllegato, string NomeFile)
        {
            bool res = false;
            try
            {
                //MemoryStream m = await _allMan.GetFileAsync(IdAllegato);
                //m.Position = 0;
                //using (ZipArchive za = new ZipArchive(m, ZipArchiveMode.Update))
                //{
                //    foreach (ZipArchiveEntry entry in za.Entries)
                //    {
                //        if (string.Compare(entry.Name, NomeFile, true) == 0)
                //        {
                //            entry.Delete();
                //            break;
                //        }
                //    }
                //    //ZipArchiveEntry zentry = za.GetEntry(NomeFile);
                //    //if (zentry != null) {
                //    //    zentry.Delete();
                //    //}
                //    za.
                //    m.Position = 0;
                //    res = await _allMan.SalvaFileAsync(IdAllegato, m);
                //}
                using (var resultms = new MemoryStream())
                {
                    using (var resultZip = new ZipArchive(resultms, ZipArchiveMode.Create, true))
                    {
                MemoryStream m = await _allMan.GetFileAsync(IdAllegato);
                m.Position = 0;
                        using (ZipArchive za = new ZipArchive(m, ZipArchiveMode.Read))
                {
                    foreach (ZipArchiveEntry entry in za.Entries)
                    {
                                if (string.Compare(entry.Name, NomeFile, true) != 0)

                        {
                                    using (var entryS = entry.Open())
                                    {
                                        ZipArchiveEntry e = resultZip.CreateEntry(entry.Name, CompressionLevel.Fastest);
                                        using (var eS = e.Open())
                                        {
                                            entryS.CopyTo(eS);
                        }
                    }
                   
                                }
                            }
                        }
                    }
                    resultms.Position = 0;
                    res = await _allMan.SalvaFileAsync(IdAllegato, resultms);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($" RemoveFileFromZipAsync : {ex.Message}");
                res = false;
            }

            return res;
        }

        public async Task<bool> AllegaAElementoFascicolo(string IdAllegato,
            string IdFascicolo,
            string IdElemento,
            string ElencoFile,
            bool AllegaZip,
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
                    Allegato.IdFascicolo = f.Id;
                    Allegato.IdElemento = e.Id;
                    Allegato.Stato = StatoAllegato.Elaborato;
                    var i = _allMan.Salva(Allegato, false);

                    _logMan.Salva(new LogDoc()
                    {
                        Data = DateTime.Now,
                        IdOggetto = Allegato.Id,
                        TipoOggetto = TipiOggetto.ALLEGATO,
                        Utente = User.Identity.Name,
                        Operazione = TipoOperazione.Elaborato
                    }, true);

                    //estrae i file dallo zip presenti in lista e li assegna all'elemento
                    Allegati all = await EstraiAllegatiZip(Allegato, ElencoFile, AllegaZip, Descrizione, tipoAll, true, User.Identity.Name, cancel);

                    var estrai = all != null;
                    var sfdpf = new SFPdf(_appEnvironment, _logger, _config, _allMan);
                    estrai = estrai && await sfdpf.MarcaAllegatoSF(all, e.elencoAttributi);

                    Info.StatoPrec = (int)e.Stato;
                    Info.Stato = (int)e.Stato;
                    if (variabili == null)
                        variabili = new Dictionary<string, VariableValue>();
                    if (!variabili.ContainsKey("IdAllegato"))
                        variabili.Add("IdAllegato", VariableValue.FromObject(IdAllegato));
                    
                    estrai = estrai && AvviaProcesso(Info,e, variabili);
                    return estrai;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"AllegaAElementoFascicolo : {ex.Message}");
            }
            return false;
        }

        private async Task<Allegati> EstraiAllegatiZip(
                Allegati Zip,
                string ElencoFile,
                bool AllegaZip,
                string Descrizione,
                TipiAllegati tipoAll,
                bool daZip,
                string Utente,
                CancellationToken cancel)
        {
            Allegati all = null;
            try
            {
                using (SqlConnection cn = new SqlConnection(_context.Connessione))
                {
                    if (!string.IsNullOrEmpty(ElencoFile) || AllegaZip)
                    {
                        //var fileName = NOME_FILE_CONTENUTO_EMAIL ;
                        var fileName = $"{Zip.Id.ToString()}.pdf";
                        var m = await _allMan.GetFileAsync(Zip.Id.ToString());

                        var FileZip = new ZipArchive(m, ZipArchiveMode.Read);
                        if (AllegaZip)
                        {  //file pdf completo
                            MemoryStream mpdf = new MemoryStream();
                            PdfEditAction pdfed = new PdfEditAction();
                            pdfed.IdAllegato = Zip.Id.ToString();
                            pdfed.TempFolder = Path.Combine(_appEnvironment.WebRootPath, "_tmp");

                            if (File.Exists(pdfed.FilePdfModificato))
                            {
                                using (FileStream fileStream = File.OpenRead(pdfed.FilePdfModificato))
                                {
                                    mpdf.SetLength(fileStream.Length);
                                    //read file to MemoryStream
                                    fileStream.Read(mpdf.GetBuffer(), 0, (int)fileStream.Length);
                                }
                                _allMan.Salva(Zip, false);
                            }
                            else
                            {
                                var l = await GetTmpPdfCompletoAsync(Zip, null, daZip);

                                Zip.SetAttributo("jAllegati", JToken.FromObject(l));
                                _allMan.Salva(Zip, false);

                                //prende il file tmp appena creato
                                mpdf = await GetPdfCompletoAsync(Zip.Id.ToString(), null, daZip);
                            }
                            if (daZip == false && Zip.IdElemento != null)
                            {
                                all = cn.QueryFirstOrDefault<Allegati>(
                                "Select * from Allegati WHERE tipo ='FILE' and IdElemento= @IdElemento and NomeFile=@NomeFile",
                                new { IdElemento = Zip.IdElemento.ToString(), NomeFile = fileName });
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
                                    IdFascicolo = Zip.IdFascicolo,
                                    IdElemento = Zip.IdElemento,
                                    jNote = Zip.jNote,
                                    UtenteC = Utente,
                                    UtenteUM=Utente
                                };
                                isNewAll = true;
                            }
                            else
                            {
                                all.Descrizione = Descrizione;
                            };

                            if (all.elencoAttributi == null) { all.elencoAttributi = tipoAll.Attributi; }

                            all.SetAttributo("Data", Zip.DataC);
                            all.SetAttributo("CodiceSoggetto", Zip.GetAttributo("CodiceSoggetto"));
                            all.SetAttributo("NomeSoggetto", Zip.GetAttributo("NomeSoggetto"));
                           
                            if (File.Exists(pdfed.FileAnnotazioni))
                            {
                                var note = File.ReadAllText(pdfed.FileAnnotazioni);
                                all.jNote = JObject.Parse(note);
                            }
                            all = await _allMan.SalvaAsync(all, mpdf, isNewAll);
                        }
                        //file da allegare singolarmente
                        if (!string.IsNullOrEmpty(ElencoFile))
                        {
                            var listafile = ElencoFile.Split(";").ToList();

                            foreach (ZipArchiveEntry entry in FileZip.Entries)
                            {
                                fileName = entry.Name;
                                
                                if (listafile.Contains(fileName))
                                {
                                    m = new MemoryStream();

                                    var all2 = cn.QueryFirstOrDefault<Allegati>(
                                        "Select * from Allegati WHERE tipo ='FILE' and IdElemento= @IdElemento and NomeFile=@NomeFile",
                                        new { IdElemento = Zip.IdElemento.ToString(), NomeFile = fileName });

                                    var isNewAll = false;

                                    if (all2 == null)
                                    {
                                        all2 = new Allegati()
                                        {
                                            Descrizione = Zip.Descrizione,
                                            NomeFile = fileName,
                                            Tipo = "FILE",
                                            TipoNavigation = tipoAll,
                                            Stato = StatoAllegato.Attivo,
                                            IdFascicolo = Zip.IdFascicolo,
                                            IdElemento = Zip.IdElemento,
                                            UtenteC =Utente,
                                            UtenteUM = Utente
                                        };
                                        //_context.Add(all);
                                        isNewAll = true;
                                    }

                                    if (all2.elencoAttributi == null) { all2.elencoAttributi = tipoAll.Attributi; }
                                    all2.Descrizione = Descrizione;
                                    all2.SetAttributo("Data", Zip.DataC);
                                    all2.SetAttributo("CodiceSoggetto", Zip.GetAttributo("CodiceSoggetto"));
                                    all2.SetAttributo("NomeSoggetto", Zip.GetAttributo("NomeSoggetto"));

                                    all2 = await _allMan.SalvaAsync(all2, m, isNewAll);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"EstraiAllegatiZip : {ex.Message}");
                return null;
            }
            return all;
        }

        public bool AvviaProcesso(BPMDocsProcessInfo Info, Elementi el, Dictionary<string, VariableValue> variabili)
        {
            bool res = true;
            try
            {
                if (!string.IsNullOrEmpty(el.TipoNavigation.Processo))
                {

                    var pd = new BPMProcessDefinition(_bpm._eng);
                    if (variabili == null)
                        variabili = new Dictionary<string, VariableValue>();

                    VariableValue v = VariableValue.FromObject(el.Id.ToString());
                    variabili.Add("IdElemento", v);
                    v = VariableValue.FromObject(el);
                    variabili.Add("jElemento", v);

                    v = VariableValue.FromObject(JsonConvert.SerializeObject(Info));
                    variabili.Add("_ProcessInfo", v);

                    var pi = pd.Start("", el.TipoNavigation.Processo, el.Id.ToString(), variabili);

                    res = (pi != null);

                }
            }
            catch (Exception ex)
            {
                res = false;
                _logger.LogError($"AvviaProcesso : {ex.Message}");
            }
            return res;
        }


        public async Task<Elementi> CreaElementoFascicoloAsync(string IdAllegato,
            string IdFascicolo,
            string IdElemento,
            string Categoria,
            string TipoElemento,
            string CodiceSoggetto,
            string NomeSoggetto,
            string ElencoFile,
            bool AllegaZip,
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

                TipiAllegati tipoAll = _allMan.GetTipoAllegato("FILE");

                Allegato.SetAttributo("CodiceSoggetto", CodiceSoggetto);
                Allegato.SetAttributo("NomeSoggetto", NomeSoggetto);
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

                    isNew = true;
                    Allegato.IdFascicolo = f.Id;
                    f.Descrizione = Descrizione;
                }
                else
                {

                    f = _fasMan.Get(IdFascicolo);
                    f.elencoAttributi = f.CategoriaNavigation.Attributi;
                }
                f.CodiceSoggetto = CodiceSoggetto;
                f.SetAttributo("CodiceSoggetto", CodiceSoggetto);
                f.SetAttributo("NomeSoggetto", NomeSoggetto);

                if (_fasMan.Salva(f, isNew) == false) return null;

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

                //crea nuovo elemento e e assegna alla mail ?
                var e = new Elementi();
                e.Tipo = TipoElemento;
                e.IdFascicolo = f.Id;
                e.Descrizione = Descrizione;
                isNew = true;
                e.IdFascicoloNavigation = f;
                TipiElementi tipoEl = _elmMan.GetTipoElemento(TipoElemento);
                e.TipoNavigation = tipoEl;
                e.elencoAttributi = tipoEl.Attributi;

                Allegato.IdElemento = e.Id;
                //assegna attributi dell'allegato come default
                foreach (string na in Allegato.elencoAttributi.Nomi() ) {
                    e.SetAttributo(na, Allegato.GetAttributo(na));
                } 
                e.SetAttributo("CodiceSoggetto", CodiceSoggetto);
                e.SetAttributo("NomeSoggetto", NomeSoggetto);
                e.SetAttributo("DataRichiesta", e.GetAttributo("Data"));
                if (_elmMan.Salva(e, isNew) == false) return null;

                //-------- Memorizzo l'operazione----------------------
                log = new LogDoc()
                {
                    Data = DateTime.Now,
                    IdOggetto = e.Id,
                    TipoOggetto = TipiOggetto.ELEMENTO,
                    Operazione = TipoOperazione.Creato,
                    Utente = User.Identity.Name
                };
                _logMan.Salva(log, true);
                //-------- Memorizzo l'operazione----------------------


                 Allegato.Stato = StatoAllegato.Elaborato;
                if (_allMan.Salva(Allegato, false) == false) return null;

                //-------- Memorizzo l'operazione----------------------
                log = new LogDoc()
                {
                    Data = DateTime.Now,
                    IdOggetto = Allegato.Id,
                    TipoOggetto = TipiOggetto.ALLEGATO,
                    Operazione = TipoOperazione.Elaborato,
                    Utente = User.Identity.Name
                };
                _logMan.Salva(log, true);
                //-------- Memorizzo l'operazione----------------------

                 var estrai = await EstraiAllegatiZip(Allegato, ElencoFile, AllegaZip, Descrizione, tipoAll, false, User.Identity.Name, cancel);

                return e;
            }
            catch (Exception ex)
            {
                _logger.LogError($"CreaFascicoloAsync : {ex.Message}");
            }
            return null;
        }

        public async Task<MemoryStream> GetPdfRiepilogo(string IdZip)
        {
            MemoryStream mpdf = null;

            string NomePdf = Path.Combine(_appEnvironment.WebRootPath, "_tmp");
            NomePdf = Path.Combine(NomePdf, $"riepilogo_{IdZip}.pdf");
            if (File.Exists(NomePdf))
                File.Delete(NomePdf);

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
                    " WHERE m.id = @IdZip Order by e.datac";
                    using (SqlConnection cn = new SqlConnection(_context.Connessione))
                    {
                        List<Elementi> el = cn.QueryAsync<Elementi>(sql, new { IdZip = IdZip }).Result.ToList();

                        //Create a new PDF document
                        Syncfusion.Pdf.PdfDocument document = new Syncfusion.Pdf.PdfDocument();
                        document.PageSettings.SetMargins(MarginPoints);
                        //Add a page to the document
                        Syncfusion.Pdf.PdfPage page = document.Pages.Add();

                        //Create PDF graphics for the page
                        Syncfusion.Pdf.Graphics.PdfGraphics graphics = page.Graphics;

                        var all = _allMan.Get(IdZip);
                        var l = await GetTmpPdfCompletoAsync(all, null, true);
                        string NomePdftmp = Path.Combine(_appEnvironment.WebRootPath, "_tmp", $"{IdZip}.pdf");
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
                        float etiHeight = page.Size.Height * (float).05;
                        int i = 0;
                        foreach (Elementi e in el)
                        {
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
                            i++;
                            reportSource.Parameters.Add("NPag", 1);
                            reportSource.Parameters.Add("TPag", pdftmp.Pages.Count + 1);

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
                            Syncfusion.Drawing.PointF posizione = new Syncfusion.Drawing.PointF() { X = 0, Y = curpos };

                            graphics.DrawPdfTemplate(template, posizione,
                                new Syncfusion.Drawing.SizeF(loadedPage.Size.Width, loadedPage.Size.Height));
                            curpos += etiHeight + 20;
                            pdfEti.Close();

                        }

                        i = 1;
                        foreach (Syncfusion.Pdf.PdfLoadedPage lptmp in pdftmp.Pages)
                        {


                            page = document.Pages.Add();

                            graphics = page.Graphics;
                            etiHeight = page.Size.Height * .05F + 5;
                            Syncfusion.Pdf.Graphics.PdfTemplate template = lptmp.CreateTemplate();
                            Syncfusion.Drawing.PointF posizione = new Syncfusion.Drawing.PointF() { X = 0, Y = etiHeight + 1 };

                            Syncfusion.Drawing.SizeF pDest = SFPdf.CalcolaProporzioni(lptmp.Size.Width, lptmp.Size.Height, page.Size.Width * 0.95F, page.Size.Height - etiHeight);

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
                            reportSource.Parameters.Add("TPag", pdftmp.Pages.Count + 1);
                            reportSource.Parameters.Add("flRiepilogo", true);

                            RenderingResult curEti = reportProcessor.RenderReport("PDF", reportSource, null);
                            Syncfusion.Pdf.Parsing.PdfLoadedDocument pdfEti = new Syncfusion.Pdf.Parsing.PdfLoadedDocument(new MemoryStream(curEti.DocumentBytes));
                            posizione = new Syncfusion.Drawing.PointF() { X = 0, Y = 5 };
                            Syncfusion.Pdf.PdfLoadedPage loadedPage = pdfEti.Pages[0] as Syncfusion.Pdf.PdfLoadedPage;
                            template = loadedPage.CreateTemplate();


                            graphics.DrawPdfTemplate(template, posizione,
                                new Syncfusion.Drawing.SizeF(loadedPage.Size.Width, loadedPage.Size.Height));
                            pdfEti.Close();

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


        public RisultatoAzione CancellaElemento(string IdElemento, short Revisione, ClaimsPrincipal User)
        {
            RisultatoAzione res = new RisultatoAzione();
            try
            {
                res.Successo = true;
                using (SqlConnection cn = new SqlConnection(_context.Connessione))
                {

                    if (_elmMan.Cancella(IdElemento, Revisione))
                    {
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

        public RisultatoAzione SpostaZip(string IdAllegato, string Cartella, ClaimsPrincipal User)
        {
            RisultatoAzione res = new RisultatoAzione();
            try
            {

                using (SqlConnection cn = new SqlConnection(_context.Connessione))
                {
                    var n = cn.Execute("UPDATE allegati SET Origine=@Origine WHERE Id=@Id ",
                        new { Origine = Cartella, Id = IdAllegato });
                    if (n > 0)
                    {
                        res.Successo = true;
                        res.Messaggio = $"Documento spostato in {Cartella}.";
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
                _logger.LogError($"SpostaZip : {ex.Message}");
            }
            return res;
        }


        public RisultatoAzione RiapriZip(string IdAllegato, ClaimsPrincipal User)
        {
            RisultatoAzione res = new RisultatoAzione();
            try
            {
                Allegati al = _allMan.Get(IdAllegato);
                if (al.IdElemento == null)
                {
                    al.Stato = StatoAllegato.Attivo;
                }
                else
                {
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
                _logger.LogError($"RiapriZip : {ex.Message}");
            }
            return res;
        }


        public async Task<RisultatoAzione> CancellaZip(string IdAllegato, ClaimsPrincipal User)
        {
            RisultatoAzione res = new RisultatoAzione();
            try
            {

                if (_allMan.Cancella(IdAllegato))
                {
                    res.Successo = true;
                    //-------- Memorizzo l'operazione----------------------
                    _logMan.Salva(new LogDoc
                    {
                        IdOggetto = Guid.Parse(IdAllegato),
                        TipoOggetto = TipiOggetto.ALLEGATO,
                        Operazione = TipoOperazione.Cancellato,
                        Utente = User.Identity.Name
                    }, true);
                    //-------- Memorizzo l'operazione----------------------
                }

            }
            catch (Exception ex)
            {
                res.Successo = false;
                res.Messaggio = ex.Message;
                _logger.LogError($"CancellaZip : {ex.Message}");
            }
            return await Task.FromResult(res);
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
