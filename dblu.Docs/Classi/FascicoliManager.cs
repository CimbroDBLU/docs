using Dapper;
using Dapper.Contrib.Extensions;
using dblu.Docs.Models;
using Microsoft.Data.SqlClient;
//using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
    public class FascicoliManager
    {

        //private dbluDocsContext _context;
        private string StringaConnessione;
        private readonly ILogger _logger;

        //public FascicoliManager(dbluDocsContext context, ILogger logger)
        public FascicoliManager(string connessione, ILogger logger)
        {
            //_context = context;
            StringaConnessione = connessione;
            _logger = logger;
        }

        public Fascicoli Get(string id)
        {
            if (string.IsNullOrEmpty(id)) {
                return null;
            }
            //var guid = Guid.Parse(id);
            //return Get(guid);
            Fascicoli fsc = null;
            using (SqlConnection cn = new SqlConnection(StringaConnessione))
            {
                fsc = cn.Get<Fascicoli>(id);
                if (fsc != null)
                {
                    fsc.CategoriaNavigation = cn.Get<Categorie>(fsc.Categoria);
                    fsc.elencoAttributi = fsc.CategoriaNavigation.Attributi;
                    if (!string.IsNullOrEmpty(fsc.Attributi))
                    {
                        fsc.elencoAttributi.SetValori(fsc.Attributi);
                    }
                }
            }
            return fsc;
        }

        public viewFascicoli GetV(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }
            //var guid = Guid.Parse(id);
            //return Get(guid);
            viewFascicoli fsc = null;
            using (SqlConnection cn = new SqlConnection(StringaConnessione))
            {
                fsc = cn.Get<viewFascicoli>(id);
            
                //fsc.elencoAttributi = fsc.CategoriaNavigation.Attributi;
                //if (!string.IsNullOrEmpty(fsc.Attributi))
                //{
                //    fsc.elencoAttributi.SetValori(fsc.Attributi);
                //}
            }
            return fsc;
        }

        public viewFascicoli GetV(Guid? guid)
        {
            viewFascicoli fsc = null;
            try
            {
                if (guid!= null) { 
                    //fsc = _context.Fascicoli
                    //.Include(a => a.CategoriaNavigation)
                    //.Single(a => a.Id == guid);

                    //fsc.Attributi = fsc.CategoriaNavigation.Attributi;

                    //if (!string.IsNullOrEmpty(fsc._attributi))
                    //{
                    //    fsc.Attributi.SetValori(fsc._attributi);
                    //}
                    return this.GetV(guid.ToString());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
            return fsc;
        }

         public Fascicoli Get(Guid? guid)
        {
            Fascicoli fsc = null;
            try
            {
                if (guid!= null) { 
                    //fsc = _context.Fascicoli
                    //.Include(a => a.CategoriaNavigation)
                    //.Single(a => a.Id == guid);

                    //fsc.Attributi = fsc.CategoriaNavigation.Attributi;

                    //if (!string.IsNullOrEmpty(fsc._attributi))
                    //{
                    //    fsc.Attributi.SetValori(fsc._attributi);
                    //}
                    return this.Get(guid.ToString());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
            return fsc;
        }

    


        public bool Salva(Fascicoli fascicolo, bool isNew)
        {
            var bres = false;
            try
            {
                //if (fascicolo.Categoria == null  || fascicolo.CategoriaNavigation == null)
                //{
                //    fascicolo.CategoriaNavigation = _context.Categorie.Single(t => t.Codice == fascicolo.Categoria);
                //    fascicolo.Attributi = fascicolo.CategoriaNavigation.Attributi;
                //}
                ////var c = _context.Elementi
                ////    .FirstOrDefault(e => e.Id == elemento.Id);

                ////if (c == null)
                ////{
                ////    _context.Elementi.Add(elemento);
                ////}
                //fascicolo.DataUM = DateTime.Now;
                //fascicolo._attributi = fascicolo.Attributi.GetValori();
                //_context.SaveChanges();
                if (fascicolo.UtenteC == null)
                    fascicolo.UtenteC = "";
                if (fascicolo.UtenteUM == null)
                    fascicolo.UtenteUM = "";

                using (SqlConnection cn = new SqlConnection(StringaConnessione))
                {
                    if (string.IsNullOrEmpty(fascicolo.Categoria))
                    {
                        return false;
                    }
                    if (fascicolo.CategoriaNavigation == null)
                    {
                        fascicolo.CategoriaNavigation = cn.Get<Categorie>(fascicolo.Categoria);
                        fascicolo.elencoAttributi = fascicolo.CategoriaNavigation.Attributi;
                    }
                    if (fascicolo.Attributi != null)
                    {
                        fascicolo.Attributi = fascicolo.elencoAttributi.GetValori();
                    }

                    if (isNew)
                    {
                        var r = cn.Insert<Fascicoli>(fascicolo);
                    }
                    else
                    {
                        var r = cn.Update<Fascicoli>(fascicolo);
                    }
                }
                    bres = true;

            }
            catch (Exception ex)
            {
                _logger.LogError($"Salva Fascicolo: {ex.Message}");

            }
            return bres;
        }

        public async Task<List<Categorie>> GeAllCategorieAsync()
        {
            List<Categorie> l = new List<Categorie>();
            try
            {
                //l =  await _context.Categorie
                //    .ToListAsync<Categorie>();
                using (SqlConnection cn = new SqlConnection(StringaConnessione))
                {
                    IEnumerable<Categorie> e = await cn.GetAllAsync<Categorie>();
                    l = e.ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"GeAllCategorieAsync: {ex.Message}");
            }
            return l;
        }

        public  List<Categorie> GetAllCategorie()
        {
            List<Categorie> l = new List<Categorie>();
            try
            {
                //l =  _context.Categorie
                //    .ToList<Categorie>();
                using (SqlConnection cn = new SqlConnection(StringaConnessione))
                {
                    l = cn.GetAll<Categorie>().ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetAllCategorie: {ex.Message}");
            }
            return l;
        }

        public Categorie GetCategoria(string Codice)
        {
            Categorie c = null;
            try
            {
                //c = _context.Categorie
                //   .FirstOrDefault(e => e.Codice == Codice);
                using (SqlConnection cn = new SqlConnection(StringaConnessione))
                {
                    c = cn.Get<Categorie>(Codice);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetCategoria: {ex.Message}");
            }
            return c;
        }

        public bool SalvaCategoria(Categorie cat) {
            var bres = false;
            try
            {

                //var c = _context.Categorie
                //    .FirstOrDefault(e => e.Codice == cat.Codice);

                //if (c == null)
                //{
                //    _context.Categorie.Add(cat);
                //}
                //else
                //{
                //    c.Descrizione = cat.Descrizione;
                //    c.ViewAttributi = cat.ViewAttributi;
                //    c.Attributi = cat.Attributi;
                //}
                //_context.SaveChanges();
                using (SqlConnection cn = new SqlConnection(StringaConnessione))
                {
                    var c = cn.Get<Categorie>(cat.Codice);
                if (c == null)
                {
                        bres = cn.Insert<Categorie>(cat) > 0;
                }
                else
                {
                     
                        cn.Update<Categorie>(cat);
                }
                }
                bres = true;
            }
            catch { 
                
            }
            return bres;
        }
        /// <summary>
        /// check if a Categoria has Fascicoli associated
        /// </summary>
        /// <param name="Cod"></param>
        /// <returns></returns>
        public bool CheckIfDeletable(string Cod)
        {
            bool Res = false;
            List<Fascicoli> e = new();
            try
            {
                using (SqlConnection cn = new SqlConnection(StringaConnessione))
                {
                    e = cn.Query<Fascicoli>("select * from Fascicoli where Categoria = @cod", new { cod = Cod }).ToList();
                    if (e.Count == 0) Res = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"FascicoliManager.CheckIfDeletable : {ex}");
            }
            return Res;
        }
        public bool CancellaCategoria(Categorie cat)
        {
            var bres = false;
            try
            {
                using (SqlConnection cn = new SqlConnection(StringaConnessione))
                {
                    cn.Execute($"Delete from Categorie where Codice=@Codice", new { Codice = cat.Codice });
               }
                bres = true;
            } 
            catch
            {

            }
            return bres;
        }
        public List<Elementi> GetElemetiFascicolo(Guid fascicolo)
        {

            //var doc = _context.Allegati.Where(x => x.IdElemento == elemento && x.Tipo == "FILE");
            List<Elementi> doc = null;
            try
            {
                using (SqlConnection cn = new SqlConnection(StringaConnessione))
                {
                    doc = cn.Query<Elementi>("Select * from Elementi where IdFascicolo=@IdFascicolo",
                        new { IdFascicolo = fascicolo.ToString() }).ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetElemetiFascicolo: {ex.Message}");

            }
            return doc;


        }
        public List<viewElementi > GetvElemetiFascicolo(Guid fascicolo)
        {

            //var doc = _context.Allegati.Where(x => x.IdElemento == elemento && x.Tipo == "FILE");
            List<viewElementi> doc = null;
            try
            {
                using (SqlConnection cn = new SqlConnection(StringaConnessione))
                {
                    doc = cn.Query<viewElementi>("Select * from vListaElementi where IdFascicolo=@IdFascicolo",
                        new { IdFascicolo = fascicolo.ToString() }).ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetvElemetiFascicolo: {ex.Message}");

            }
            return doc;


        }

        public List<Fascicoli> CercaFascicoli(Fascicoli queryObj)
        {
            List<Fascicoli> l = new List<Fascicoli>();
            try
            {

                if (queryObj.Id != null && queryObj.Id != Guid.Empty)
                {
                    Fascicoli a = Get(queryObj.Id);
                    if (a != null)
                    {
                        l.Add(a);
                    }
                }
                else
                {
                    using (SqlConnection cn = new SqlConnection(StringaConnessione))
                    {
                        StringBuilder sb = new StringBuilder("SELECT * FROM Fascicoli WHERE 1=1 ");
                        DynamicParameters pp = new DynamicParameters();
                        if (!string.IsNullOrEmpty(queryObj.Categoria))
                        {
                            sb.Append(" AND Tipo = @Tipo ");
                            pp.Add("@Tipo", queryObj.Categoria);
                        }
                        if (!string.IsNullOrEmpty(queryObj.Descrizione))
                        {
                            sb.Append(" AND Descrizione like @Descrizione ");
                            pp.Add("@Descrizione", queryObj.Descrizione);
                        }
                        if (!string.IsNullOrEmpty(queryObj.CodiceSoggetto))
                        {
                            sb.Append(" AND CodiceSoggetto like @CodiceSoggetto ");
                            pp.Add("@CodiceSoggetto", queryObj.CodiceSoggetto);
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

                        if (!string.IsNullOrEmpty(queryObj.Attributi))
                        {
                            var values = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(queryObj.Attributi);
                            foreach (KeyValuePair<string, dynamic> d in values)
                            {
                                sb.Append($" and JSON_VALUE(attributi,'$.{d.Key}') = @attr_{d.Key}");
                                pp.Add($"@attr_{d.Key}", d.Value);
                            }
                        }
                        l = cn.Query<Fascicoli>(sb.ToString(), pp).ToList();
                    }

                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"CercaFascicoli: {ex.Message}");
            }
            return l;
        }

    }
}
