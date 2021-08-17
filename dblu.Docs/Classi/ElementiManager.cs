using Dapper;
using Dapper.Contrib.Extensions;
using dblu.Docs.Models;
using Microsoft.Data.SqlClient;
//using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace dblu.Docs.Classi
{
    public class ElementiManager
    {

        //private dbluDocsContext _context;
        private string StringaConnessione;
        private readonly ILogger _logger;

        //public ElementiManager(dbluDocsContext context, ILogger logger)
        public ElementiManager(string connessione, ILogger logger)
        {
            //_context = context;
            StringaConnessione = connessione;
            _logger = logger;
        }



        public Elementi Get(string id, short rev)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }
            //var guid = Guid.Parse(id);
            //return Get(guid,rev);
            Elementi elm = null;
            using (SqlConnection cn = new SqlConnection(StringaConnessione))
            {
                elm = cn.QueryFirstOrDefault<Elementi>(
                        "SELECT * FROM Elementi WHERE ID=@ID and Revisione=@Revisione ",
                        new { ID = id , Revisione  = rev});
                if (elm != null) { 
                elm.TipoNavigation = cn.Get<TipiElementi>(elm.Tipo);
                elm.elencoAttributi = elm.TipoNavigation.Attributi;
                if (!string.IsNullOrEmpty(elm.Attributi))
                {
                    elm.elencoAttributi.SetValori(elm.Attributi);
                }
            }
            }
            return elm;
        }

        public Elementi Get(Guid? guid, short rev)
        {
            Elementi elm = null;
            try
            {
                if (guid != null)
                {
                    //elm = _context.Elementi
                    //.Include(a => a.TipoNavigation)
                    //.Where(a => (a.Id == guid && a.Revisione==rev) ).FirstOrDefault();
                    //elm.Attributi = elm.TipoNavigation.Attributi;

                    //if (!string.IsNullOrEmpty(elm._attributi))
                    //{
                    //    elm.Attributi.SetValori(elm._attributi);
                    //}

                    elm = this.Get(guid.ToString(), rev);
                }
             }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
            return elm;
        }


        public List<Elementi> GetElementi(string Tipo, int Stato)
        {
            List<Elementi> l = new List<Elementi>();
            try
            {
                using (SqlConnection cn = new SqlConnection(StringaConnessione))
                {
                    l = cn.Query<Elementi>($"Select * FROM Elementi E where Tipo=@Tipo And Stato=@Stato", new { Tipo = Tipo, Stato = Stato }).ToList();
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"GetElementi: {ex.Message}");
            }
            return l;
        }

        public  bool Salva(Elementi elemento, bool isNew)
        {
            var bres = false;
            try
            {
                using (SqlConnection cn = new SqlConnection(StringaConnessione))
                {
                if (elemento.TipoNavigation == null)
                {
                        //elemento.TipoNavigation = _context.TipiElementi.Single(t => t.Codice == elemento.Tipo);
                        elemento.TipoNavigation = cn.Get<TipiElementi>(elemento.Tipo);
                        elemento.elencoAttributi = elemento.TipoNavigation.Attributi;

                }
                //var c = _context.Elementi
                //    .FirstOrDefault(e => e.Id == elemento.Id);

                //if (c == null)
                //{
                //    _context.Elementi.Add(elemento);
                //}
                elemento.DataUM = DateTime.Now;
                    if (elemento.UtenteC == null)
                        elemento.UtenteC = "";
                    if (elemento.UtenteUM == null)
                        elemento.UtenteUM = "";
                 elemento.Attributi = elemento.elencoAttributi.GetValori();
                    // _context.SaveChanges();
                    if (isNew)
                    {
                        var sql = "INSERT INTO Elementi (ID,Revisione,Tipo,Descrizione,Stato,IDFascicolo,Attributi,DataC,UtenteC,DataUM,UtenteUM,Chiave1,Chiave2,Chiave3,Chiave4 ,Chiave5)" +
                            " VALUES (@ID,@Revisione,@Tipo,@Descrizione,@Stato,@IDFascicolo,@Attributi,@DataC,@UtenteC,@DataUM,@UtenteUM,@Chiave1,@Chiave2,@Chiave3,@Chiave4 ,@Chiave5)";
                        var r = cn.Execute(sql,elemento);
                    }
                    else
                    {
                        var sql = "UPDATE Elementi SET Tipo = @Tipo, Descrizione = @Descrizione,Stato=@Stato,IDFascicolo=@IDFascicolo,Attributi=@Attributi," +
                            "DataUM=@DataUM,UtenteUM=@UtenteUM,Chiave1=@Chiave1,Chiave2=@Chiave2,Chiave3=@Chiave3,Chiave4=@Chiave4 ,Chiave5=@Chiave5 " +
                            " WHERE  ID = @ID  AND Revisione=@Revisione ";
                        var r = cn.Execute(sql, elemento);
                    }

                bres = true;
            }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Salva Elementi: {ex.Message}");

            }
            return bres;
        }

        public Elementi Nuovo(string Tipo)
        {
            Elementi elemento = new Elementi()
            {
                //DataC = DateTime.Now,
                Tipo = Tipo,
                //UtenteC = User.Identity.Name,
                //UtenteUM = User.Identity.Name,
                //DataUM = DateTime.Now,
                //Revisione = 0,
            };
            
            try
            {
                using (SqlConnection cn = new SqlConnection(StringaConnessione))
                {
                    if (elemento.TipoNavigation == null)
                    {
                        //elemento.TipoNavigation = _context.TipiElementi.Single(t => t.Codice == elemento.Tipo);
                        elemento.TipoNavigation = cn.Get<TipiElementi>(elemento.Tipo);
                        elemento.elencoAttributi = elemento.TipoNavigation.Attributi;

                    }
                    
                    //  elemento.DataUM = DateTime.Now;
                    elemento.Attributi = elemento.elencoAttributi.GetValori();
                    // _context.SaveChanges();
                    
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Salva Elementi: {ex.Message}");

            }
            return elemento;
        }

        public async Task<List<TipiElementi>> GetAllTipiElementiAsync()
        {
            List<TipiElementi> l = new List<TipiElementi>();
            try
            {
                //l = await _context.TipiElementi
                //    .ToListAsync<TipiElementi>();
                using (SqlConnection cn = new SqlConnection(StringaConnessione))
                {
                    IEnumerable<TipiElementi>  e = await cn.GetAllAsync<TipiElementi>();
                    l = e.ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetAllTipiElementiAsync: {ex.Message}");
            }
            return l;
        }

        public List<TipiElementi> GetAllTipiElementi(List<string> Ruoli)
        {
            List<TipiElementi> l = new List<TipiElementi>();
            try
            {
                //l = await _context.TipiElementi
                //    .ToListAsync<TipiElementi>();
                using (SqlConnection cn = new SqlConnection(StringaConnessione))
                {
                    string sql = "Select * FROM [TipiElementi] where [Codice] IN (Select Tipo from [ElementiInRoles] where [RoleId] IN ('" + string.Join("','", Ruoli) + "') Group by Tipo)";
                    l = cn.Query<TipiElementi>(sql).ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetAllTipiElementi: {ex.Message}");
            }
            return l;
        }

        public List<TipiElementi> GetAllTipiElementi(string NomeServer = "")
        {
            List<TipiElementi> l = new List<TipiElementi>();
            try
            {
                //l = _context.TipiElementi
                //    .ToList<TipiElementi>();
                using (SqlConnection cn = new SqlConnection(StringaConnessione))
                {
                    l = cn.GetAll<TipiElementi>().ToList();
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"GetAllTipiElementi: {ex.Message}");
            }
            return l;
        }

        public TipiElementi GetTipoElemento(string Codice)
        {
            TipiElementi c = null;
            try
            {
                //c = _context.TipiElementi
                //   .FirstOrDefault(e => e.Codice == Codice);
                using (SqlConnection cn = new SqlConnection(StringaConnessione))
                {
                    c = cn.Get<TipiElementi>(Codice);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"GetTipoElemento: {ex.Message}");
            }
            return c;
        }

        public bool SalvaTipoElemento(TipiElementi obj)
        {
            var bres = false;
            try
            {

                //var c = _context.TipiElementi
                //    .FirstOrDefault(e => e.Codice == cat.Codice);

                //if (c == null)
                //{
                //    _context.TipiElementi.Add(cat);
                //}
                //else
                //{
                //    c.Descrizione = cat.Descrizione;
                //    c.Processo = cat.Processo;
                //    c.ViewAttributi = cat.ViewAttributi;
                //    c.Categoria = cat.Categoria;
                //    c.Attributi = cat.Attributi;
                //}
                //_context.SaveChanges();
                using (SqlConnection cn = new SqlConnection(StringaConnessione))
                {
                    var c = cn.Get<TipiElementi>(obj.Codice);
                if (c == null)
                {
                        bres = cn.Insert<TipiElementi>(obj) > 0;
                }
                else
                {
                        cn.Update<TipiElementi>(obj);
                }
                }
                bres = true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"SalvaTipoElemento: {ex.Message}");

            }
            return bres;
        }

        public bool CancellaTipoElemento(TipiElementi obj)
        {
            var bres = false;
            try
            {
                using (SqlConnection cn = new SqlConnection(StringaConnessione))
                {
                        cn.Execute($"Delete from TipiElementi where Codice=@Codice", new { Codice = obj.Codice });
                        bres = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"CancellaTipoElemento: {ex.Message}");

            }
            return bres;
        }
       

        /// <summary>
        /// check if a TipoElemento has Elementi associated
        /// </summary>
        /// <param name="Cod"></param>
        /// <returns></returns>
        public bool CheckIfDeletable(string Cod)
        {
            bool Res = false;
            List<Elementi> e = new List<Elementi>();
            try
            {
            using (SqlConnection cn = new SqlConnection(StringaConnessione))
            {
                e = cn.Query<Elementi>("select * from Elementi where Tipo = @cod", new { cod = Cod }).ToList();
                if (e.Count == 0) Res = true;
            }
            }catch(Exception ex)
            {
                _logger.LogError($"ElementiManager.CheckIfDeletable : {ex}");
            }
            return Res;
        }

        public List<Allegati> GetAllAllegatiElemento(Guid elemento)
        {

            //var doc = _context.Allegati.Where(x => x.IdElemento == elemento && x.Tipo == "FILE");
            List<Allegati> doc = null;
            try
            {
                using (SqlConnection cn = new SqlConnection(StringaConnessione))
                {
                    doc = cn.Query<Allegati>("Select * from Allegati where IdElemento=@IdElemento ",
                        new { IdElemento = elemento.ToString() }).ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetAllAllegatiElemento: {ex.Message}");

            }
            return doc;

        }

        public List<Allegati> GetAllegatiElemento(Guid elemento)
        {

            //var doc = _context.Allegati.Where(x => x.IdElemento == elemento && x.Tipo == "FILE");
            List<Allegati> doc = null;
            try
            {
                using (SqlConnection cn = new SqlConnection(StringaConnessione))
                {
                    doc = cn.Query<Allegati>("Select * from Allegati where IdElemento=@IdElemento and Tipo='FILE' ",
                        new { IdElemento = elemento.ToString()}).ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetAllegatiElemento: {ex.Message}");

            }
            return doc;
        
        }

        public bool Cancella(string Id, short Revisione)
        {
            if (string.IsNullOrEmpty(Id))
            {
                return false;
            }
            var bres = false;
            try
            {
                using (SqlConnection cn = new SqlConnection(StringaConnessione))
                {


                    var nr = cn.Execute($"DELETE FROM Elementi WHERE Id='{Id}' and Revisione={Revisione} ");

                    var nrRev = cn.ExecuteScalar<int>($"SELECT COUNT(Revisione) FROM Elementi where Id ='{Id}' ");
                    if (nrRev == 0)
                    {
                        var sqlAl = " SELECT ID FROM Allegati WHERE IdElemento = @IdElemento and Tipo = 'FILE' ";
                        List<Guid> la = cn.Query<Guid>(sqlAl, new { IdElemento = Id }).ToList();

                        if (la.Count > 0)
                        {
                            var _allMan = new AllegatiManager(StringaConnessione, _logger);
                            foreach (Guid idAll in la)
                            {
                                _allMan.Cancella(idAll);
                            }
                        }
                        nr = cn.Execute($"UPDATE Allegati SET IdElemento = NULL WHERE IdElemento='{Id}' ");
                    }
                    bres = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Cancella Elemento {ex.Message}");
            }
            return bres;
        }

        public bool Cancella(Guid? guid, short Revisione)
        {
            var bres = false;
            try
            {

                bres = this.Cancella(guid.ToString(), Revisione);

            }
            catch (Exception ex)
            {
                _logger.LogError($"Cancella Elemento : {ex.Message}");

            }
            return bres;
        }

        public List<Elementi> CercaElementi(Elementi queryObj)
        {
            List<Elementi> l = new List<Elementi>();
            try
            {

                if (queryObj.Id != null && queryObj.Id != Guid.Empty)
                {
                    Elementi a = Get(queryObj.Id, queryObj.Revisione);
                    if (a != null)
                    {
                        l.Add(a);
                    }
                }
                else
                {
                    using (SqlConnection cn = new SqlConnection(StringaConnessione))
                    {
                        StringBuilder sb = new StringBuilder("SELECT * FROM Elementi WHERE 1=1 ");
                        DynamicParameters pp = new DynamicParameters();
                        if (!string.IsNullOrEmpty(queryObj.Tipo))
                        {
                            sb.Append(" AND Tipo = @Tipo ");
                            pp.Add("@Tipo", queryObj.Tipo);
                        }
                        if (!string.IsNullOrEmpty(queryObj.Descrizione))
                        {
                            sb.Append(" AND Descrizione like @Descrizione ");
                            pp.Add("@Descrizione", queryObj.Descrizione);
                        }
                        if (!string.IsNullOrEmpty(queryObj.UtenteC))
                        {
                            sb.Append(" AND UtenteC = @UtenteC ");
                            pp.Add("@UtenteC", queryObj.UtenteC);
                        }
                        if (!string.IsNullOrEmpty(queryObj.UtenteUM))
                        {
                            sb.Append(" AND UtenteUM = @UtenteUM ");
                            pp.Add("@UtenteUM", queryObj.UtenteUM);
                        }
                        if (queryObj.IdFascicolo != null && queryObj.IdFascicolo != Guid.Empty)
                        {
                            sb.Append(" AND IdFascicolo = @IdFascicolo ");
                            pp.Add("@IdFascicolo", queryObj.IdFascicolo);
                        }

                        if (!string.IsNullOrEmpty(queryObj.Attributi))
                        {
                            var values = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(queryObj.Attributi);
                            foreach (KeyValuePair<string, dynamic> d in values)
                            {
                                sb.Append($" and JSON_VALUE(attributi,'$.{d.Key}') = @attr_{d.Key}");
                                pp.Add($"@attr_{d.Key}", d.Value);
                            }
                        }
                        l = cn.Query<Elementi>(sb.ToString(), pp).ToList();
                    }

                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"CercaElementi: {ex.Message}");
            }
            return l;
        }
        public List<Elementi> GetElementiDaAllegato(Guid IdAllegato)
        {

            //var doc = _context.Allegati.Where(x => x.IdElemento == elemento && x.Tipo == "FILE");
            List<Elementi> doc = null;
            try
            {
                using (SqlConnection cn = new SqlConnection(StringaConnessione))
                {
                    doc = cn.Query<Elementi>("SELECT e.* FROM allegati m INNER JOIN elementi e ON e.idfascicolo = m.idfascicolo " +
                    " LEFT JOIN allegati f ON f.idfascicolo = m.idfascicolo and f.idelemento = e.id and f.Tipo = 'FILE' and f.NomeFile = cast(m.id as varchar(50)) + '.pdf' " +
                    " WHERE m.id = @IdAllegato Order by e.datac ",
                        new { IdAllegato = IdAllegato.ToString() }).ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetElementiDaAllegato: {ex.Message}");

            }
            return doc;

        }

    }
}
