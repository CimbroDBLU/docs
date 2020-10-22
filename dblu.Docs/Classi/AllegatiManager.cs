using Dapper;
using Dapper.Contrib.Extensions;
using dblu.Docs.Models;
using Microsoft.Data.SqlClient;
//using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace dblu.Docs.Classi
{
    public class AllegatiManager 
    {
        //private dbluDocsContext _context;
        private string StringaConnessione ;
        private readonly ILogger _logger;

        //public AllegatiManager(dbluDocsContext context, ILogger logger) {
        public AllegatiManager(string connessione, ILogger logger)
            {
            //   _context = context;
            StringaConnessione = connessione;
            _logger = logger;
        }

        public TipiAllegati GetTipo(string Codice)
        {
            TipiAllegati ta = null;
            try
            {
                //ta = _context.TipiAllegati
                //    .Where(t => t.Codice == Codice).FirstOrDefault();
                using (SqlConnection cn = new SqlConnection(StringaConnessione)) {
                    //ta = cn.QueryFirstOrDefault<TipiAllegati>(
                    //        "SELECT * FROM TipiAllegati WHERE Codice=@Codice ",
                    //        new { Codice = Codice });
                    ta = cn.Get<TipiAllegati>(Codice);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
            return ta;
        }


        public String Sottocartella(Allegati allegato) { 
        
            return Path.Combine(allegato.TipoNavigation.Cartella, allegato.DataC.Year.ToString(), allegato.DataC.ToString("MMdd"));
        }

        public Allegati Get(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }
            //var guid = Guid.Parse(id);
            //return Get(guid);            
            Allegati all = null;
            using (SqlConnection cn = new SqlConnection(StringaConnessione))
            {
                //all = cn.QueryFirstOrDefault<Allegati>(
                //        "SELECT * FROM Allegati WHERE ID=@ID ",
                //        new { ID = id });
                //all.TipoNavigation = cn.QueryFirstOrDefault<TipiAllegati>(
                //            "SELECT * FROM TipiAllegati WHERE Codice=@Codice ",
                //            new { Codice = all.Tipo});
                all = cn.Get<Allegati>(id);
                if (all != null) { 
                all.TipoNavigation =  cn.Get<TipiAllegati>(all.Tipo);
                all.elencoAttributi = all.TipoNavigation.Attributi;
                if (!string.IsNullOrEmpty(all.Attributi))
                {
                    all.elencoAttributi.SetValori(all.Attributi);
                }
            }
            }
            return all;
        }

        public Allegati Get(Guid? guid)
        {
            Allegati all = null;
            try
            {
                if (guid != null)
                {
                    //all = _context.Allegati
                    //.Include(a => a.TipoNavigation)
                    //.Where(a => a.Id == guid).FirstOrDefault();
                    //all.Attributi = all.TipoNavigation.Attributi;

                    //if (!string.IsNullOrEmpty(all._attributi))
                    //{
                    //    all.Attributi.SetValori(all._attributi);
                    //}
                    return Get(guid.ToString());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            } 
            return all;
        }

        public async Task<MemoryStream> GetFileAsync(string Nome)
        {
            MemoryStream m = null;
            try
            {//
                //var doc =  _context.VDocs
                //    .Where(d => d.Name == Nome).FirstOrDefault();
                ////.FirstOrDefaultAsync();
                using (SqlConnection cn = new SqlConnection(StringaConnessione))
                {
                    var doc = cn.QueryFirstOrDefault<VDocs>(
                            "SELECT File_Stream AS FileStream FROM VDocs WHERE Name=@Name ",
                            new { Name = Nome});
                if (doc != null)
                {
                    m = new MemoryStream(doc.FileStream);
                }
            }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
            return m;
        }

        public async Task<Allegati> SalvaAsync(Allegati allegato, MemoryStream file, bool isNew) {

            try {

                using (SqlConnection cn = new SqlConnection(StringaConnessione))
                {

                if (string.IsNullOrEmpty(allegato.Tipo))
                {
                    return null;
                }
                    if (allegato.TipoNavigation == null)
                    {
                        //allegato.TipoNavigation = _context.TipiAllegati.Single(t => t.Codice == allegato.Tipo);
                        allegato.TipoNavigation = cn.Get<TipiAllegati>(allegato.Tipo);
                        allegato.elencoAttributi = allegato.TipoNavigation.Attributi;
                    }
                    if (allegato.Attributi != null)
                    {
                        allegato.Attributi = allegato.elencoAttributi.GetValori();
                    }

                    //_context.Allegati.Add(allegato);
                    //int i = await _context.SaveChangesAsync();
                    bool b = false;
                    if (isNew)
                    {
                        var r = await cn.InsertAsync<Allegati>(allegato);
                        b = true;
                }
                    else { 
                         b = await cn.UpdateAsync<Allegati>(allegato);
                }
                

                    if (b)
                    {

                        //dbluDocsContext db = new dbluDocsContext(_context.Connessione);

                        string percorso = Sottocartella(allegato);

                    //    var parameters = new SqlParameter[] {
                    //    new SqlParameter() {
                    //        ParameterName = "@relativePath",
                    //        SqlDbType =  SqlDbType.VarChar,
                    //        Direction = ParameterDirection.Input,
                    //        Value = percorso
                    //    },
                    //    new SqlParameter() {
                    //            ParameterName = "@name",
                    //            SqlDbType =  SqlDbType.VarChar,
                    //            Direction = ParameterDirection.Input,
                    //            Value = allegato.Id.ToString()
                    //    },
                    //    new SqlParameter() {
                    //        ParameterName = "@stream",
                    //        SqlDbType =  SqlDbType.VarBinary,
                    //        Direction = ParameterDirection.Input,
                    //        Value = file.ToArray()
                    //    },
                    //        new SqlParameter() {vDocs
                    //        ParameterName = "@path_locator",
                    //        SqlDbType =  SqlDbType.VarChar,
                    //        Direction = ParameterDirection.InputOutput,
                    //        Value = ""
                    //    }

                    //};

                        var p = new DynamicParameters();

                        p.Add("@relativePath", percorso, dbType: DbType.String, direction: ParameterDirection.Input);
                        p.Add("@name", allegato.Id.ToString(), dbType: DbType.String, direction: ParameterDirection.Input);
                        file.Seek(0, SeekOrigin.Begin);
                        p.Add("@stream", file, dbType: DbType.Binary, direction: ParameterDirection.Input,-1);

                        //var fs = db.VDocs
                        //    .Where(f => f.Name == allegato.Id.ToString())
                        //    .FirstOrDefault();
                        var fs = cn.QueryFirstOrDefault<VDocs>(
                            "select Name, path_locator as PathLocator from vDocs where Name=@Name", 
                            new { Name = allegato.Id.ToString() });

                        if (fs == null ||  fs.PathLocator == null)
                        {

                            if (fs != null)
                            {   //PathLocator  non valido
                                cn.Execute("delete from Docs  where Name=@Name ", new { Name = allegato.Id.ToString() });
                        }

                            //i = await db.Database.ExecuteSqlRawAsync("[dbo].[sp_InsDoc] @relativePath, @name, @stream, @path_locator ", parameters);
                            p.Add("@path_locator", "", dbType: DbType.String, direction: ParameterDirection.InputOutput);
                            cn.Execute("[dbo].[sp_InsDoc] @relativePath, @name, @stream, @path_locator ", p);
                        }
                        else
                        {
                            //parameters[3].Value = fs.PathLocator;
                            //i = await db.Database.ExecuteSqlRawAsync("[dbo].[sp_AggDoc] @relativePath, @name, @stream, @path_locator ", parameters);

                            p.Add("@path_locator", fs.PathLocator, dbType: DbType.String, direction: ParameterDirection.InputOutput, 4000);
                            cn.Execute("[dbo].[sp_AggDoc] @relativePath, @name, @stream, @path_locator ", p);
                    }
                    }
                }

            }
            catch (Exception ex) {
                _logger.LogError(ex.Message);
                return null;
            }
            
            return allegato;
        }
        public async Task<bool> SalvaFileAsync(string IdAllegato, MemoryStream file)
        {
            bool res = false;
            try
            {
                using (SqlConnection cn = new SqlConnection(StringaConnessione))
                {
                    if (! string.IsNullOrEmpty(IdAllegato))
                    {
                        Allegati all = Get(IdAllegato);
                        if (all != null) { 
                            string percorso = Sottocartella(all);

                            var p = new DynamicParameters();

                            p.Add("@relativePath", percorso, dbType: DbType.String, direction: ParameterDirection.Input);
                            p.Add("@name", IdAllegato, dbType: DbType.String, direction: ParameterDirection.Input);
                            file.Seek(0, SeekOrigin.Begin);
                            p.Add("@stream", file, dbType: DbType.Binary, direction: ParameterDirection.Input, -1);

                            var fs = cn.QueryFirstOrDefault<VDocs>(
                                "select Name, path_locator as PathLocator from vDocs where Name=@Name",
                                new { Name = IdAllegato });

                            if (fs == null || fs.PathLocator == null)
                            {
                                if (fs != null)
                                {   //PathLocator  non valido
                                    await cn.ExecuteAsync("delete from Docs where Name=@Name ", new { Name = IdAllegato });
                                }

                                p.Add("@path_locator", "", dbType: DbType.String, direction: ParameterDirection.InputOutput);
                                await  cn.ExecuteAsync("[dbo].[sp_InsDoc] @relativePath, @name, @stream, @path_locator ", p);
                            }
                            else
                            {
                                p.Add("@path_locator", fs.PathLocator, dbType: DbType.String, direction: ParameterDirection.InputOutput, 4000);
                                await cn.ExecuteAsync("[dbo].[sp_AggDoc] @relativePath, @name, @stream, @path_locator ", p);
                            }
                            res = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"SalvaFileAsync: {ex.Message}");
            }
            return res;
        }



        public bool Cancella(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return false;
            }
            //var guid = Guid.Parse(id);
            //return Cancella(guid);
            var bres = false;
            try
            {
                using (SqlConnection cn = new SqlConnection(StringaConnessione))
                {
                    cn.Execute($"Delete from Allegati where Id='{id}'");
                    bres = true;
 
                    //var fs = db.VDocs
                    //    .Where(f => f.Name == allegato.Id.ToString())
                    //    .FirstOrDefault();
                    var fs = cn.QueryFirstOrDefault<VDocs>(
                        "select Name, path_locator as PathLocator from vDocs where Name=@Name",
                        new { Name = id });

                    if (fs!= null)
                    {
                        var p = new DynamicParameters();

                        p.Add("@relativePath", "", dbType: DbType.String, direction: ParameterDirection.Input,4000);
                        p.Add("@name", id, dbType: DbType.String, direction: ParameterDirection.Input,512);
                        p.Add("@path_locator", fs.PathLocator, dbType: DbType.String, direction: ParameterDirection.InputOutput, 4000);
                        
                        cn.Execute("[dbo].[sp_CancDoc] @relativePath, @name, @path_locator ", p);
                    }
                }
            }
            catch (Exception ex) {
                _logger.LogError($"Cancella allegato {ex.Message}");
            }
            return bres;
        }

        public bool Cancella(Guid? guid)
        {
            var bres = false;
            try
            {

                bres = this.Cancella(guid.ToString()); 
 
                //Allegati all = null;
                //all = _context.Allegati
                //    .Where(a => a.Id == guid).FirstOrDefault();

                //if (all != null)
                //{
                //    _context.Allegati.Remove(all);
                //}
                //int i = _context.SaveChanges();

                //if (i >= 0)
                //{
                //    VDocs fs = _context.VDocs
                //        .Where(f => f.Name == guid.ToString())
                //        .FirstOrDefault();
                //    if (fs != null)
                //    {

                //        var parameters = new SqlParameter[] {
                //            new SqlParameter()
                //            {
                //                ParameterName = "@relativePath",
                //                SqlDbType = SqlDbType.VarChar,
                //                Direction = ParameterDirection.Input,
                //                Value = ""
                //            },
                //            new SqlParameter()
                //            {
                //                ParameterName = "@name",
                //                SqlDbType = SqlDbType.VarChar,
                //                Direction = ParameterDirection.Input,
                //                Value = all.Id.ToString()
                //            },
                //            new SqlParameter()
                //            {
                //                ParameterName = "@path_locator",
                //                SqlDbType = SqlDbType.VarChar,
                //                Direction = ParameterDirection.InputOutput,
                //                Value = fs.PathLocator
                //            }
                //        };

                //        i = _context.Database.ExecuteSqlRaw("[dbo].[sp_CancDoc] @relativePath, @name, @path_locator ", parameters);
                //    };
                //    bres = true;
                //}
            }
            catch (Exception ex)
            {
                _logger.LogError($"Cancella: {ex.Message}");

            }
            return bres;
        }

        public string GetContentType(string path)
        {
            string t = "";
            try { 
            var types = GetMimeTypes();
            var ext = Path.GetExtension(path).ToLowerInvariant();
                t = types[ext];
            }
            catch {
            }
            return t;
        }

        private Dictionary<string, string> GetMimeTypes()
        {
            return new Dictionary<string, string>
                {
                    {".bmp", "image/bmp"},                   
                    {".txt", "text/plain"},
                    {".pdf", "application/pdf"},
                    {".doc", "application/ms-word"},
                    {".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"},
                    {".odt", "application/vnd.oasis.opendocument.text"},
                    {".xls", "application/vnd.ms-excel"},
                    {".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},  
                    {".png", "image/png"},
                    {".jpg", "image/jpeg"},
                    {".jpeg", "image/jpeg"},
                    {".gif", "image/gif"},
                    {".tiff", "image/tiff"},
                    {".tif", "image/tiff"},
                    {".csv", "text/csv"},
                    {".html", "text/html"},
                    {".htm", "text/html"},
                    {".dat", "application/dat"},
                    {".svg", "image/svg+xml"},
                    {".eml", "application/eml"},
                    {".zip", "application/zip"},
                    {".rar", "application/rar"},
                    {".7z", "application/7zip"},
                    {".", "text/html"},
                };
            /*
             
            .doc      application/msword
            .dot      application/msword

            .docx     application/vnd.openxmlformats-officedocument.wordprocessingml.document
            .dotx     application/vnd.openxmlformats-officedocument.wordprocessingml.template
            .docm     application/vnd.ms-word.document.macroEnabled.12
            .dotm     application/vnd.ms-word.template.macroEnabled.12

            .xls      application/vnd.ms-excel
            .xlt      application/vnd.ms-excel
            .xla      application/vnd.ms-excel

            .xlsx     application/vnd.openxmlformats-officedocument.spreadsheetml.sheet
            .xltx     application/vnd.openxmlformats-officedocument.spreadsheetml.template
            .xlsm     application/vnd.ms-excel.sheet.macroEnabled.12
            .xltm     application/vnd.ms-excel.template.macroEnabled.12
            .xlam     application/vnd.ms-excel.addin.macroEnabled.12
            .xlsb     application/vnd.ms-excel.sheet.binary.macroEnabled.12

            .ppt      application/vnd.ms-powerpoint
            .pot      application/vnd.ms-powerpoint
            .pps      application/vnd.ms-powerpoint
            .ppa      application/vnd.ms-powerpoint

            .pptx     application/vnd.openxmlformats-officedocument.presentationml.presentation
            .potx     application/vnd.openxmlformats-officedocument.presentationml.template
            .ppsx     application/vnd.openxmlformats-officedocument.presentationml.slideshow
            .ppam     application/vnd.ms-powerpoint.addin.macroEnabled.12
            .pptm     application/vnd.ms-powerpoint.presentation.macroEnabled.12
            .potm     application/vnd.ms-powerpoint.template.macroEnabled.12
            .ppsm     application/vnd.ms-powerpoint.slideshow.macroEnabled.12

            .mdb      application/vnd.ms-access
             
             */
        }

        public List<TipiAllegati> GetAllTipiAllegati()
        {
            List<TipiAllegati> l = new List<TipiAllegati>();
            try
            {
                //l = _context.TipiAllegati
                //    .ToList<TipiAllegati>();
                using (SqlConnection cn = new SqlConnection(StringaConnessione))
                {
                    l = cn.GetAll<TipiAllegati>().ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetAllTipiAllegati: {ex.Message}");
            }
            return l;
        }

        public TipiAllegati GetTipoAllegato(string Codice)
        {
            TipiAllegati c = null;
            try
            {
                //c = _context.TipiAllegati
                //   .FirstOrDefault(e => e.Codice == Codice);
                using (SqlConnection cn = new SqlConnection(StringaConnessione))
                {
                    c = cn.Get<TipiAllegati>(Codice);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetTipoAllegato: {ex.Message}");
            }
            return c;
        }

        public bool SalvaTipoAllegato(TipiAllegati obj)
        {
            var bres = false;
            try
            {
                using (SqlConnection cn = new SqlConnection(StringaConnessione))
                {
                //var c = _context.TipiAllegati
                //    .FirstOrDefault(e => e.Codice == obj.Codice);

                    var c = cn.Get<TipiAllegati>(obj.Codice);

                if (c == null)
                {
                        //_context.TipiAllegati.Add(obj);
                        bres = cn.Insert<TipiAllegati>(obj) > 0;
                }
                else
                {
                        cn.Update<TipiAllegati>(obj);

                    //c.Descrizione = obj.Descrizione;
                    //c.ViewAttributi = obj.ViewAttributi;
                    //c.Attributi = obj.Attributi;
                    //c.Cartella = obj.Cartella;
                }
                //_context.SaveChanges();
                bres = true;
            }
            }
            catch (Exception ex)
            {
                _logger.LogError($"SalvaTipoAllegato: {ex.Message}");

            }
            return bres;
        }

        public bool CancellaTipoAllegato(TipiAllegati obj)
        {
            var bres = false;
            try
            {

                using (SqlConnection cn = new SqlConnection(StringaConnessione))
                {
                    cn.Execute($"Delete from TipiAllegati where Codice=@Codice", new { Codice = obj.Codice});
                    bres = true;

                    //var c = _context.TipiAllegati
                    //    .FirstOrDefault(e => e.Codice == obj.Codice);

                    //if (c != null)
                    //{
                    //    _context.TipiAllegati.Remove(obj);

                    //}
                    //_context.SaveChanges();
                    bres = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"CancellaTipoAllegato: {ex.Message}");

            }
            return bres;
        }

        public List<Allegati> GetEmailInArrivo(string Tipo, string NomeServer)
        {
            if (NomeServer == null) NomeServer = "";
            List<Allegati> l = new List<Allegati>();
            try
            {
                //l = _context.Allegati
                //    .Where(a => (a.Tipo == Tipo && a.Stato < StatoAllegato.Chiuso))
                //    .ToList<Allegati>();
                using (SqlConnection cn = new SqlConnection(StringaConnessione))
                {
                    if (NomeServer.Length == 0)
                    {
                    l = cn.Query<Allegati>("Select *,(select top 1 Operazione from LogDoc where IdOggetto=A.ID Order by Data DESC) LastOp FROM Allegati A where Tipo=@Tipo and Stato < @Stato",
                        new { Tipo = Tipo, Stato = StatoAllegato.Chiuso }).ToList();
                    }
                    else
                    {
                        l = cn.Query<Allegati>("Select A.*,(select top 1 Operazione from LogDoc where IdOggetto=A.ID Order by Data DESC) LastOp FROM Allegati A WHERE Tipo=@Tipo and Stato < @Stato and Origine = @origine",
                        new { Tipo = Tipo, Stato = StatoAllegato.Chiuso, origine = NomeServer }).ToList();
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"GetEmailInArrivo: {ex.Message}");
            }
            return l;
        }

        public int CountEmailInArrivo(string Tipo)
        {
            int l = 0;
            try
            {
                //l = _context.Allegati
                //    .Where(a => (a.Tipo == Tipo && a.Stato < StatoAllegato.Chiuso))
                //    .Count();
                using (SqlConnection cn = new SqlConnection(StringaConnessione))
                {
                    l = cn.ExecuteScalar<int>("Select count(*) from Allegati where Tipo=@Tipo and Stato < @Stato",
                        new { Tipo = Tipo, Stato = StatoAllegato.Chiuso });

                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"CountEmailInArrivo: {ex.Message}");
            }
            return l;
        }

        public List<Allegati> GetEmailProcessate(string Tipo, string NomeServer)
        {
            if (NomeServer == null) NomeServer = "";
            List<Allegati> l = new List<Allegati>();
            try
            {
                //l = _context.Allegati
                //    .Where(a => (a.Tipo == Tipo && a.Stato < StatoAllegato.Chiuso))
                //    .ToList<Allegati>();
                using (SqlConnection cn = new SqlConnection(StringaConnessione))
                {
                    if (NomeServer.Length == 0)
                    {
                        l = cn.Query<Allegati>("Select *,(select top 1 Operazione from LogDoc where IdOggetto=A.ID Order by Data DESC) LastOp FROM Allegati A where Tipo=@Tipo and Stato in (@Chiuso, @Ann ) ",
                        new { Tipo = Tipo, Chiuso = StatoAllegato.Chiuso , Ann= StatoAllegato.Annullato}).ToList();
                    }
                    else
                    {
                        l = cn.Query<Allegati>("Select *,isnull((select top 1 Operazione from LogDoc where IdOggetto=A.ID Order by Data DESC),0) LastOp FROM Allegati A where Tipo=@Tipo and Stato in (@Chiuso, @Ann ) and Origine = @origine",
                        new { Tipo = Tipo, Chiuso = StatoAllegato.Chiuso, Ann = StatoAllegato.Annullato, origine = NomeServer }).ToList();
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"GetEmailInArrivo: {ex.Message}");
            }
            return l;
        }

        public bool Salva(Allegati all, bool isNew)
        {
            var bres = false;
            try
            {
                using (SqlConnection cn = new SqlConnection(StringaConnessione))
                {
                    if (all.TipoNavigation == null)
                    {
                        //elemento.TipoNavigation = _context.TipiElementi.Single(t => t.Codice == elemento.Tipo);
                        all.TipoNavigation = cn.Get<TipiAllegati>(all.Tipo);
                        all.elencoAttributi = all.TipoNavigation.Attributi;

                    }
                    //var c = _context.Elementi
                    //    .FirstOrDefault(e => e.Id == elemento.Id);

                    //if (c == null)
                    //{
                    //    _context.Elementi.Add(elemento);
                    //}
                    all.DataUM = DateTime.Now;
                    all.Attributi = all.elencoAttributi.GetValori();
                    all.Note = all.Note == null ? "" : all.Note;
                    if (all.Stato == StatoAllegato.Attivo) all.Stato = StatoAllegato.Elaborato;
                    // _context.SaveChanges();
                    if (isNew)
                    {
                        var r = cn.Insert<Allegati>(all);
                    }
                    else
                    {
                        var r = cn.Update<Allegati>(all);
                    }

                    bres = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Salva Allegato: {ex.Message}");

            }
            return bres;
        }
        public List<AllegatoEmail> GetEmailInviate(string Tipo, string NomeServer)
        {
            if (NomeServer == null) NomeServer = "";
            List<AllegatoEmail> l = new List<AllegatoEmail>();
            try
            {
                //l = _context.Allegati
                //    .Where(a => (a.Tipo == Tipo && a.Stato < StatoAllegato.Chiuso))
                //    .ToList<Allegati>();
                using (SqlConnection cn = new SqlConnection(StringaConnessione))
                {
                    if (NomeServer.Length == 0)
                    {
                        l = cn.Query<AllegatoEmail>("Select *,(select top 1 Operazione from LogDoc where IdOggetto=A.ID Order by Data DESC) LastOp, " +
                            " JSON_VALUE(Attributi,'$.Mittente') Mittente, JSON_VALUE(Attributi,'$.Destinatario') Destinatario " +
                            " FROM Allegati A where Tipo=@Tipo and Stato = @Stato",
                        new { Tipo = Tipo, Stato = StatoAllegato.Spedito }).ToList();
                    }
                    else
                    {
                        l = cn.Query<AllegatoEmail>("Select *,isnull((select top 1 Operazione from LogDoc where IdOggetto=A.ID Order by Data DESC),0) LastOp," +
                            "JSON_VALUE(Attributi, '$.Mittente') Mittente, JSON_VALUE(Attributi, '$.Destinatario') Destinatario " +
                            " FROM Allegati A where Tipo=@Tipo and Stato = @Stato and Origine = @origine",
                        new { Tipo = Tipo, Stato = StatoAllegato.Spedito, origine = NomeServer }).ToList();
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"GetEmailInviate: {ex.Message}");
            }
            return l;
        }

    }
}
