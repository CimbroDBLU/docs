using dblu.Docs.Classi;
using dblu.Docs.Interfacce;
using dblu.Docs.Models;
using dblu.Portale.Plugin.Docs.Models;
using dblu.Portale.Plugin.Docs.ViewModels;
using Kendo.Mvc.Extensions;
//using dblu.Portale.Plugin.Docs.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Data.SqlTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NToastNotify;
using System;
using System.Collections.Generic;
using System.Data;

using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Dapper;
using Newtonsoft.Json;


/*
 * Metodi per l'interazione con la FILETABLE
 * e la Tabella logica di salvataggio degli Allegati
 * ad un Fascicolo. 
 */

namespace dblu.Portale.Plugin.Docs.Services
{
    public class AllegatiService
    {
        private readonly /*FileContext*/ dbluDocsContext _context;
        private readonly IWebHostEnvironment _appEnvironment;
        private readonly ILogger _logger;
        public IConfiguration _config { get; }
        public readonly AllegatiManager _allMan;
        public readonly FascicoliManager _fasMan;
        public readonly ElementiManager _elmMan;
        public readonly HistoryManager _hiMan;

        //public ISoggettiService ServizioSoggetti = null;

        public AllegatiService(/*FileContext db,*/dbluDocsContext db,
            IWebHostEnvironment appEnvironment,
            ILoggerFactory loggerFactory,
            IConfiguration config
            )
        {
            _context = db;
            _appEnvironment = appEnvironment;
            _logger = loggerFactory.CreateLogger("FileRepository");
            _allMan = new AllegatiManager(_context.Connessione, _logger);
            _fasMan = new FascicoliManager(_context.Connessione, _logger);
            _elmMan = new ElementiManager(_context.Connessione, _logger);
            _hiMan = new HistoryManager(_context.Connessione, _logger);
            this._config = config;

            //try
            //{
            //    string sServizioSoggetti = _config["Docs:ServizioSoggetti"];

            //    if (!string.IsNullOrEmpty(sServizioSoggetti))
            //    {
//                    ServizioSoggetti = new dblu.sgGeaClient.Classi.GeaClientService(); // System.Reflection.Assembly.GetExecutingAssembly().CreateInstance(sServizioSoggetti);
//                    ServizioSoggetti.Init(_config, _logger);
            //    }
            //}
            //catch { }

        }

        //internal List<ISoggetti> GetSoggetti()
        //{
        //    if (!(ServizioSoggetti == null))
        //    {
        //        return ServizioSoggetti.GetSoggetti();
        //    }
        //    var soggetti = _context.Soggetti;
        //    return soggetti.ToList<ISoggetti>();
        //}

        //public async Task<List<ISoggetti>> GetSoggettiAsync()
        //{
        //    if (!(ServizioSoggetti == null))
        //    {
        //        return ServizioSoggetti.GetSoggetti();
        //    }

        //    return await _context.Soggetti.ToListAsync<ISoggetti>();
        //}




        internal List<Allegati> GetAllegatiElemento(Guid elemento)
        {
        
            //var doc = _context.Allegati.Where(x => x.IdElemento == elemento && x.Tipo=="FILE");
            List<Allegati> doc = null;
            using (SqlConnection cn = new SqlConnection(_context.Connessione))
            {
                doc = cn.Query<Allegati>("select * from Allegati where IdElemento=@IdElemento and Tipo='FILE' ",
                    new { IdElemento = elemento.ToString() }).ToList();
            }

            return doc;
        }

        public List<viewAllegati> GetvAllegatiElemento(Guid IdElemento)
        {

            //var doc = _context.Allegati.Where(x => x.IdElemento == elemento && x.Tipo=="FILE");
            List<viewAllegati> doc = null;
            using (SqlConnection cn = new SqlConnection(_context.Connessione))
            {
                doc = cn.Query<viewAllegati>($"select * from [{GetNomeVista("vALLEGATO")}] where IdElemento=@IdElemento ",
                    new { IdElemento = IdElemento.ToString() }).ToList();
            }

            return doc;
        }


        internal List<Allegati> GetFilesZip(Guid elemento)
        {

            //var doc = _context.Allegati.Where(x => x.IdElemento == elemento && x.Tipo=="FILE");
            List<Allegati> doc = null;
            using (SqlConnection cn = new SqlConnection(_context.Connessione))
            {
                doc = cn.Query<Allegati>("select * from Allegati where IdElemento=@IdElemento and Tipo='FILE' ",
                    new { IdElemento = elemento.ToString() }).ToList();
            }

            return doc;
        }

        public bool SaveAllegato(Allegati  allegato)
        {
            bool b = false;

            
            b = _allMan.Salva(allegato, false);
            return b;
        }


        public TipiElementi GetTipoElemento(string tipo)
        {
            // return _context.TipiElementi.Where(x => x.Codice == tipo).FirstOrDefault();
            //// return _context.TipiElementi.Find(tipo);
            return _elmMan.GetTipoElemento(tipo);
        }
        public Categorie GetCategoria(string categoria)
        {
            //return _context.Categorie.Find(categoria);
            return _fasMan.GetCategoria(categoria);
        }
        internal Allegati GetDocumento(Guid elemento)
        {
            //return _context.Allegati.Find(elemento);
            return _elmMan.GetAllegatiElemento(elemento).FirstOrDefault();
        }


        //get documenti allegati al fascicolo 
        public List<Allegati> GetDocuments(string nomeUtente, Guid fascicolo)
        {

            //return null;
            //var doc = from x in _context.Allegati
            //          where x.IdFascicolo == fascicolo
            //          select x;
            List<Allegati> doc = null;
            using (SqlConnection cn = new SqlConnection(_context.Connessione))
            {
                doc = cn.Query<Allegati>("select * from Allegati where IdFascicolo=@IdFascicolo and Tipo='FILE' ",
                    new { IdFascicolo = fascicolo.ToString() }).ToList();
            }

            return doc;
        }

        public List<Elementi> GetElementi(Guid fascicolo)
        {
            //var elemento = _context.Elementi.Where(x => x.IdFascicolo == fascicolo);
            //return elemento.ToList();
            List<Elementi> doc = null;
            using (SqlConnection cn = new SqlConnection(_context.Connessione))
            {
                doc = cn.Query<Elementi>("select * from Elementi where IdFascicolo=@IdFascicolo ",
                    new { IdFascicolo = fascicolo.ToString() }).ToList();
            }
            return doc;

        }

        public List<Elementi> GetElementsByAttribute(string AttrName, dynamic AttrValue )
        {
            //var elemento = _context.Elementi.Where(x => x.IdFascicolo == fascicolo);
            //return elemento.ToList();
            List<Elementi> doc = new List<Elementi>();
            using (SqlConnection cn = new SqlConnection(_context.Connessione))
            {
                doc = cn.Query<Elementi>($"select * from Elementi where JSON_VALUE(Attributi,'$.{AttrName}') = @AttrValue",
                    new { AttrValue }).ToList();
            }
            return doc;
        }

        public Elementi GetElemento(Guid elementoGuid, short rev)
        {
            //var elemento = _context.Elementi.Find(elementoGuid, rev);
            //return elemento;
            return _elmMan.Get(elementoGuid.ToString(), rev);
        }

        internal IEnumerable<ElementiViewModel> GetElementiSoggetto(string soggetto)
        {
            using (SqlConnection cn  = new SqlConnection(_context.Connessione)) {
                var sql = "SELECT IdFascicolo, CodiceSoggetto, DscFascicolo, DscCategoria, Chiave1F, Chiave2F, Chiave3F, Chiave4F, Chiave5F, IdElemento, Revisione, Tipo, DscElemento, DscTipo, Stato, Chiave1E, Chiave2E, Chiave3E, Chiave4E, Chiave5E from vElementi WHERE CodiceSoggetto = @Codice";
                return cn.Query<ElementiViewModel>(sql , new { Codice = soggetto }).ToList();

            }
        }

        public Fascicoli GetFascicolo(Guid fascicoloGuid)
        {

            ////var x = _context.Fascicoli.Where(x => x.Id == fascicoloGuid);
            ////if (x is null)
            ////    return null;


            //var fascicolo = _context.Fascicoli.AsNoTracking()
            //.Include(x => x.Elementi)
            //.Include(x => x.CategoriaNavigation)
            ////.Include(x => x.Allegati)
            //.Where(x => x.Id == fascicoloGuid).FirstOrDefault();

            //return fascicolo;
            var fascicolo = _fasMan.Get(fascicoloGuid);
            fascicolo.vElementi = _fasMan.GetvElemetiFascicolo(fascicoloGuid);
            
            return fascicolo;
        }

        public viewFascicoli GetFascicoloV(Guid fascicoloGuid)
        {

            ////var x = _context.Fascicoli.Where(x => x.Id == fascicoloGuid);
            ////if (x is null)
            ////    return null;


            //var fascicolo = _context.Fascicoli.AsNoTracking()
            //.Include(x => x.Elementi)
            //.Include(x => x.CategoriaNavigation)
            ////.Include(x => x.Allegati)
            //.Where(x => x.Id == fascicoloGuid).FirstOrDefault();

            //return fascicolo;
            var fascicolo = _fasMan.GetV(fascicoloGuid);
            fascicolo.Elementi = _fasMan.GetvElemetiFascicolo(fascicoloGuid);
            return fascicolo;
        }
        //Upload documents
        public bool DocumentUpload(IEnumerable<IFormFile> files, string elemento, string fascicolo, string descrizione,string utente)
        {

            foreach (IFormFile file in files)
            {
                string extention = Path.GetExtension(file.FileName);

                if (extention == ".pdf" || extention == ".PDF")
                {
                    try
                    {
                        Allegati a = new Allegati()
                        {
                            NomeFile = file.FileName,
                            elencoAttributi = new ElencoAttributi(),
                            UtenteC = utente,
                            UtenteUM  = utente,
                            Stato = 0,
                            Descrizione = descrizione,
                            jNote = JObject.Parse("{}"),
                            Tipo = "FILE",
                            IdElemento = Guid.Parse(elemento),
                            IdFascicolo = Guid.Parse(fascicolo)
                        };

                        MemoryStream memoryStream = file as MemoryStream;
                        if (memoryStream == null)
                        {
                            memoryStream = new MemoryStream();
                            file.CopyTo(memoryStream);
                        }


                        var allMan = new AllegatiManager(_context.Connessione, _logger);
                        var task = Task.Run(async () => await allMan.SalvaAsync(a, memoryStream, true));
                        var x = task.Result;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"DocumentUpload {ex.Message}");
                         var exp = ex;
                        return false;
                    }
                }
                else
                {
                     return false;
                }
            }
            return true;
        }

        public bool DocumentUploadX(IFormFile file, string elemento, string fascicolo, string descrizione, string utente)
        {

                string extention = Path.GetExtension(file.FileName);

               
                    try
                    {
                        Allegati a = new Allegati()
                        {
                            NomeFile = file.FileName,
                            elencoAttributi = new ElencoAttributi(),
                            UtenteC = utente,
                            UtenteUM = utente,
                            Stato = 0,
                            Descrizione = descrizione,
                            jNote = JObject.Parse("{}"),
                            Tipo = "FILE",
                            IdElemento = Guid.Parse(elemento),
                            IdFascicolo = Guid.Parse(fascicolo)
                        };

                        MemoryStream memoryStream = file as MemoryStream;
                        if (memoryStream == null)
                        {
                            memoryStream = new MemoryStream();
                            file.CopyTo(memoryStream);
                        }


                        var allMan = new AllegatiManager(_context.Connessione, _logger);
                        var task = Task.Run(async () => await allMan.SalvaAsync(a, memoryStream, true));
                        var x = task.Result;
                    }
                    catch (Exception ex)
                    {
                        var exp = ex;
                        return false;
                    }
                
            return true;
        }
        internal IEnumerable<Fascicoli> GetFascicoli(string soggetto)
        {
            //return _context.Fascicoli.Where(x => x.CodiceSoggetto == soggetto);
            using (SqlConnection cn = new SqlConnection(_context.Connessione))
            {
                var sql = "SELECT * from Fascicoli WHERE CodiceSoggetto = @Codice";
                return cn.Query<Fascicoli>(sql, new { Codice = soggetto });
            }


        }
        public string CountFascicoli()
        {
            //return _context.Fascicoli;
            string res;
            using (SqlConnection cn = new SqlConnection(_context.Connessione))
            {
                var sql = "select count(ID) from Fascicoli";
                res = cn.QueryFirstOrDefault<string>(sql).ToString();
               
                return res;
            }
        }

        public string CountElementi()
        {
            //return _context.Fascicoli;
            string res;
            using (SqlConnection cn = new SqlConnection(_context.Connessione))
            {
                var sql = "select count(ID) from Elementi";
                res = cn.QueryFirstOrDefault<string>(sql).ToString();

                return res;
            }
        }

        internal IEnumerable<Fascicoli> GetFascicoli()
        {
            //return _context.Fascicoli;
            using (SqlConnection cn = new SqlConnection(_context.Connessione))
            {
                var sql = "SELECT * from Fascicoli";
                return cn.Query<Fascicoli>(sql);
            }
        }


        internal IEnumerable<viewFascicoli> GetFascicoliV()
        {
            //return _context.Fascicoli;
            using (SqlConnection cn = new SqlConnection(_context.Connessione))
            {
                var sql = "SELECT * from vListaFascicoli";
                return cn.Query<viewFascicoli>(sql);
            }
        }

        internal IEnumerable<viewElementi> GetElementiV()
        {
            //return _context.Fascicoli;
            using (SqlConnection cn = new SqlConnection(_context.Connessione))
            {
                var sql = "SELECT * from vListaElementi";
                return cn.Query<viewElementi>(sql);
            }

        }
        private string GetNomeVista(string Configurazione) { 
            string NomeVista = "";
            switch (Configurazione) {
                case "vFASCICOLO":
                    NomeVista = "vListaFascicoli";
                    break;
                case "vELEMENTO":
                    NomeVista = "vListaElementi";
                    break;
                case "vALLEGATO":
                    NomeVista = "vListaAllegati";
                    break;
                default:
                    break;
            }
            return NomeVista;
      
        } 

        public List<Colonna> GetColonne(string Configurazione)
        {
            //return _context.Fascicoli;
            List<Colonna> Colonne = new List<Colonna>();
            try {
            using (SqlConnection cn = new SqlConnection(_context.Connessione))
            {
                var sql = "SELECT value from Configurazione where nome='" + Configurazione + "'";
                    string config = cn.QueryFirstOrDefault<string>(sql);

                    if (config != null)
                    {
                        Colonne = JsonConvert.DeserializeObject<List<Colonna>>(config);
                    }
                    else {
                        for (int i=1; i<11; i++) 
                        {
                           Colonne.Add(new Colonna() { Des = $"Campo {i}", Field = $"Campo {i}", Visible = false });
                        }
                    }

                }
            }
            catch (Exception ex) {
                _logger.LogError($"GetColonne {ex.Message}");
                }
                return Colonne;
            }

        public bool SalvaColonne(string Configurazione, List<Colonna> Colonne)
        {
            var bres = false;
            int r = 0;
            try
            {
                using (SqlConnection cn = new SqlConnection(_context.Connessione))
                {

                    string sql = "SELECT value from Configurazione where nome='" + Configurazione + "'";
                    string xColonne = cn.QueryFirstOrDefault<string>(sql);
                    if (xColonne == null)
                    {
                        sql = "INSERT INTO Configurazione (Nome,[Value]) VALUES (@Nome,@Value)";
                        r = cn.Execute(sql, new { Nome = Configurazione, Value = JsonConvert.SerializeObject(Colonne) });

                    }
                    else
                    {
                        sql = "UPDATE Configurazione SET [Value] = @Value WHERE  Nome = @Nome ";
                        r = cn.Execute(sql, new { Nome = Configurazione, Value = JsonConvert.SerializeObject(Colonne) });
                    }

                    if (r > 0) bres = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Salva Elementi: {ex.Message}");

            }
            return bres;
        }


        internal JToken GetNote(Dictionary<string, string> jsonObject)
        {
            var guid = Guid.Parse(jsonObject["document"]);
            //var allegato = _context.Allegati.Find(guid);
            var allegato = _allMan.Get(guid); 
            if (allegato != null)
            {
                return allegato.jNote;
            }

            return null;
        }

        internal string GetNote2(Dictionary<string, string> jsonObject)
        {
            try {
                var guid = Guid.Parse(jsonObject["document"].Replace(".pdf","").Split(';')[0]);
            //var allegato = _context.Allegati.Find(guid);
            var allegato = _allMan.Get(guid);
            if (allegato != null)
            {
                if (allegato.jNote!=null)
                    return allegato.jNote.ToString();
            }

            }
            catch (Exception ex) {
            
            }
            

            return null;
        }

        internal Allegati GetPdfAllegatoAElemento(PdfEditAction pdf) {
            Allegati all = null;
            if (!string.IsNullOrEmpty(pdf.IdElemento))
            {
                using (SqlConnection cn = new SqlConnection(_context.Connessione))
                {
                    var sql = "SELECT * FROM allegati f WHERE f.idelemento = @idelemento and f.Tipo = 'FILE' and f.NomeFile =  @idallegato + '.pdf' ";
                    all = cn.QueryFirstOrDefault<Allegati>(sql, new { idelemento = pdf.IdElemento, idallegato = pdf.IdAllegato });
                }
            }
            else
            {
                all = _allMan.Get(pdf.IdAllegato);
            }

            return all;
        }

        internal string GetNote2(PdfEditAction pdf)
        {
            string id = pdf.IdAllegato;
            Allegati allegato  = GetPdfAllegatoAElemento(pdf);
            if (allegato != null && allegato.jNote != null)
            {
                return allegato.jNote.ToString();
            }
            return null;
        }




        //get document name
        public string getName(Guid id)
        {
            //var item = _context.Allegati.Where(x => x.Id == id).FirstOrDefault();
            var item = _allMan.Get(id);
            return item.NomeFile;
        }



        public IEnumerable<Allegati> GetAllDocument()
        {
            //return _context.Allegati.Select(
            //        t => new Allegati { NomeFile = t.NomeFile, Id = t.Id, Descrizione = t.Descrizione });
            using (SqlConnection cn = new SqlConnection(_context.Connessione))
            {
                var sql = "SELECT NomeFile,Id,Descrizione from Allegati ";
                return cn.Query<Allegati>(sql);
            }

        }

        public JToken GetNoteGrid(Guid id)
        {
            //var allegato = _context.Allegati.Find(id);
            var allegato = _allMan.Get(id);
            return allegato.jNote;
        }

        public void SaveFile(Guid guid, IFormFile file)
        {
            try
            {

                MemoryStream memoryStream = file as MemoryStream;
                if (memoryStream == null)
                {
                    memoryStream = new MemoryStream();
                    file.CopyTo(memoryStream);
                }

                ////ALTER PROC[dbo].[sp_InsDoc]
                ////(@relativePath varchar(max)=null
                ////,@name varchar(512)=null
                ////,@stream VARBINARY(MAX)=NULL
                ////,@path_locator VARCHAR(4000)=null OUTPUT
                ////)


                //var parameters = new SqlParameter[] {

                //     new SqlParameter() {
                //            ParameterName = "@relativePath",
                //            SqlDbType =  SqlDbType.VarChar,
                //            Direction = ParameterDirection.Output,
                //            Value = "test"
                //        },
                //    new SqlParameter() {
                //            ParameterName = "@name",
                //            SqlDbType =  SqlDbType.UniqueIdentifier,
                //            Direction = ParameterDirection.Input,
                //            Value = guid
                //        },
                //        new SqlParameter() {
                //            ParameterName = "@stream",
                //            SqlDbType =  SqlDbType.VarBinary,
                //            Direction = ParameterDirection.Input,
                //            Value = memoryStream.ToArray()
                //        },
                //         new SqlParameter() {
                //            ParameterName = "@path_locator",
                //            SqlDbType =  SqlDbType.VarChar,
                //            Direction = ParameterDirection.Output,
                //            Value = ""
                //        }

                //};

                //_context.Database.ExecuteSqlRaw("[dbo].[sp_InsDoc] @relativePath, @name, @stream, @path_locator ", parameters);
                using (SqlConnection cn = new SqlConnection(_context.Connessione))
                {
                    var p = new DynamicParameters();

                    p.Add("@relativePath", "test", dbType: DbType.String, direction: ParameterDirection.Input);
                    p.Add("@name", guid.ToString(), dbType: DbType.String, direction: ParameterDirection.Input);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    p.Add("@stream", memoryStream, dbType: DbType.Binary, direction: ParameterDirection.Input, -1);
                    p.Add("@path_locator", "", dbType: DbType.String, direction: ParameterDirection.InputOutput);
                    
                    cn.Execute("[dbo].[sp_InsDoc] @relativePath, @name, @stream, @path_locator ", p);
                        }
                         

            }
            catch (Exception ex)
            {


            }

        }

        public List<Note> GetNoteList(Guid allegato)
        {

            var retnote = new List<Note>();
            JToken note = this.GetNoteGrid(allegato);
            try
            {
                var jnote = note["note"] as JArray;
               

                dynamic nnote = new JObject();
                nnote.note = new JArray() as dynamic;
                foreach (var item in jnote)
                {
                    retnote.Add(new Note { contenuto = item["contenuto"].ToString(), utentemodifica = item["utentemodifica"].ToString() 
                    , date = Convert.ToDateTime(item["data"]),utente = item["utente"].ToString(),
                        datemodifica = Convert.ToDateTime(item["datemodifica"])

                    });
                }
            }
            catch (Exception)
            {
                
            }

            return retnote;

        }
        public void SaveNoteJ(Guid allegato , dynamic note)
        {
            //var a = _context.Allegati.Find(allegato);
            //a.Note = note;
            //_context.SaveChanges();
            var a = _allMan.Get(allegato);
            if (a != null) { 
                 a.jNote = note;
                _allMan.Salva(a,false);
            }

        }

        internal void SaveNote(Dictionary<string, string> jsonObject)
        {
            var guid = Guid.Parse(jsonObject["document"]);

            //var allegato = _context.Allegati.Find(guid);
            var allegato = _allMan.Get(guid);
            if (allegato != null)
            {
            
                allegato.jNote = JObject.Parse(jsonObject["pdfAnnotation"]);
                //_context.SaveChanges();
                _allMan.Salva(allegato, false);
            }

        }
        internal void SaveNoteString( PdfEditAction pdf, string json)
        {
            var allegato = GetPdfAllegatoAElemento(pdf);
            if (allegato != null)
            {

                allegato.jNote = JObject.Parse(json);
                _allMan.Salva(allegato, false);
            }

        }


        internal void SaveNoteString(Dictionary<string, string> jsonObject, string json)
        {
            try
            {

                var guid = Guid.Parse(jsonObject["document"].Replace(".pdf", "").Split(';')[0]);

            //var allegato = _context.Allegati.Find(guid);
            var allegato = _allMan.Get(guid);
            if (allegato != null)
            {
                allegato.jNote = JObject.Parse(json);
                // _context.SaveChanges();
                _allMan.Salva(allegato, false);
            }


            }
            catch (Exception ex){ }

        }


        public Allegati GetFile(Guid id)
        {

        //    string connectionString = "";// get your connection string from encrypted config

        //    // assumes your FILESTREAM data column is called Img in a table called ImageTable
        //    const string sql = @"
        //                        SELECT            
        //                        Img.PathName(),
        //                        GET_FILESTREAM_TRANSACTION_CONTEXT()
        //                        FROM ImageTagble
        //                        WHERE ImageId = @id";

        //    string serverPath;
        //    byte[] txnToken;
        //    string base64ImageData = null;
        //    using (var ts = new TransactionScope())
        //    {
        //        using (var conn = new SqlConnection(connectionString))
        //        {
        //            conn.Open();
        //            using (SqlCommand cmd = new SqlCommand(sql, conn))
        //            {
        //                cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
        //                using (SqlDataReader rdr = cmd.ExecuteReader())
        //                {
        //                    rdr.Read();
        //                    serverPath = rdr.GetSqlString(0).Value;
        //                    txnToken = rdr.GetSqlBinary(1).Value;
        //                }
        //            }
        //            using (var sfs = new SqlFileStream(serverPath, txnToken, FileAccess.Read))
        //            {
        //                // sfs will now work basically like a FileStream.  You can either copy it locally or return it as a base64 encoded string
        //                using (var ms = new MemoryStream())
        //                {
        //                    sfs.CopyTo(ms);
        //                    base64ImageData = Convert.ToBase64String(ms.ToArray());
        //                }
        //            }
        //        }
        //        ts.Complete();

               return new Allegati()
                {
                    //ContentType = "data:img/png;base64",
                    //Content = base64ImageData
                };

        //    }
        }


    }
}
