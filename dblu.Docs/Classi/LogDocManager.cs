#if Framework48

using Dapper;
using Dapper.Contrib.Extensions;

#else

using Microsoft.EntityFrameworkCore;

#endif
using dblu.Docs.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using dbluTools.Extensions;

namespace dblu.Docs.Classi
{
    public class LogDocManager 
    {

        private readonly ILogger _logger;

#if Framework48 
        private string StringaConnessione ;

        public LogDocManager(string connessione, ILogger logger)
        {
            StringaConnessione = connessione;
            _logger = logger;
        }

#else
        private dbluDocsContext _context;
        public LogDocManager(dbluDocsContext context, ILogger logger)
            {
            _context = context;
            //StringaConnessione = connessione;
            _logger = logger;
        }

#endif        




        //public LogDoc Get(string id)
        //{
        //    if (string.IsNullOrEmpty(id))
        //    {
        //        return null;
        //    }
        //    LogDoc all = null;
        //    using (SqlConnection cn = new SqlConnection(StringaConnessione))
        //    {
        //        all = cn.Get<LogDoc>(id);
        //    }
        //    return all;
        //}


        public List<LogDoc> GetAll()
        {
            List<LogDoc> l = new List<LogDoc>();

            try
            {

#if Framework48 
                using (SqlConnection cn = new SqlConnection(StringaConnessione))
                {
                    l = cn.GetAll<LogDoc>().ToList();
                }
#else
                l = _context.LogDoc.ToList();
#endif
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetAllLogDoc: {ex.Message}");
            }
            return l;

        }
                    
        //public LogDoc Get(Guid? guid)
        //{
        //    LogDoc all = null;
        //    try
        //    {
        //        if (guid != null)
        //        {
        //            return Get(guid.ToString());
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex.Message);
        //        return null;
        //    } 
        //    return all;
        //}

        public void  PostLog(Guid nId, TipiOggetto nType, TipoOperazione nOperation,string nUser,string nDescription,ExtAttributes Att=null)
        {
            LogDoc log = new()
            {
                Data = DateTime.Now,
                IdOggetto = nId,
                TipoOggetto = nType,
                Operazione = nOperation,
                Utente = nUser,
                Descrizione=nDescription,
                JAttributi= null,

            };
            Task.Run(() =>this.Salva(log, true));
        }

        public void PostLog(string nId, TipiOggetto nType, TipoOperazione nOperation, string nUser, string nDescription, ExtAttributes Att = null)
        {
            LogDoc log = new()
            {
                Data = DateTime.Now,
                IdOggetto = Guid.Parse(nId),
                TipoOggetto = nType,
                Operazione = nOperation,
                Utente = nUser,
                Descrizione = nDescription,
                JAttributi = null,

            };
            Task.Run(() => this.Salva(log, true));
        }

        public bool Salva(LogDoc log, bool isNew)
        {
            var bres = false;
            try
            {
#if Framework48
            using (SqlConnection cn = new SqlConnection(StringaConnessione))
            {
                if (isNew)
                {
                    var r = cn.Insert<LogDoc>(log);
                }
                else
                {
                    var r = cn.Update<LogDoc>(log);
                }
            }
            bres = true;

#else
                _context.LogDoc.Add(log);
                _context.SaveChanges();
#endif

            }
            catch (Exception ex)
            {
                _logger.LogError($"LogDocManager.Salva: Unexpected exception {ex.Message}");

            }
            return bres;
        }
      
        public List<LogDoc> GetLogOggetto(Guid IdOggetto, TipiOggetto Tipo)
        {
            List<LogDoc > log = null;
            try
            {
 #if Framework48                
                using (SqlConnection cn = new SqlConnection(StringaConnessione))
                {
                    log = cn.Query<LogDoc>("SELECT * FROM LogDoc WHERE IdOggetto = @Id and TipoOggetto = @Tipo ORDER BY data DESC ",
                        new { Id = IdOggetto.ToString() , Tipo = Tipo }).ToList();
                }
#else
               log = _context.LogDoc.Where(l => l.IdOggetto == IdOggetto).OrderByDescending(l => l.Data) .ToList();
#endif
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetLogElemento: {ex.Message}");

            }
            return log;
        }

        public bool IsStato(string IdOggetto, TipiOggetto Tipo, TipoOperazione Stato)
        {
            try
            {
#if Framework48
                using (SqlConnection cn = new SqlConnection(StringaConnessione))
                {
                    if (cn.QueryFirstOrDefault<LogDoc>("SELECT * FROM LogDoc WHERE IdOggetto = @Id and TipoOggetto = @Tipo and Operazione =@Stato",
                        new { Id = IdOggetto, Tipo = Tipo, Stato = Stato }) != null)
                        return true;
                }

#else
                return _context.LogDoc.Any(l => l.IdOggetto == Guid.Parse(IdOggetto) && l.TipoOggetto==Tipo &&  l.Operazione == Stato);
#endif

            }
            catch (Exception ex)
            {
                _logger.LogError($"IsStato: {ex.Message}");
            }
            return false;

        }
    }
}
