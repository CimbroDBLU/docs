#if Framework48

using Dapper;
using Dapper.Contrib.Extensions;
using dblu.Docs.Extensions;

#else

using Microsoft.EntityFrameworkCore;
using dbluTools.Extensions;

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
using Microsoft.Extensions.DependencyInjection;

namespace dblu.Docs.Classi
{
    public class LogDocManager 
    {

        private readonly ILogger _logger;
        private readonly IConfiguration _conf;

#if Framework48 
        private string StringaConnessione ;

        public LogDocManager(string connessione, ILogger logger)
        {
            StringaConnessione = connessione;
            _logger = logger;
        }

#else
        private dbluDocsContext _context;
        public LogDocManager(dbluDocsContext context, ILogger logger,IConfiguration conf)
            {
            _context = context;
            _logger = logger;
            _conf = conf;
        }

#endif        

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


#if !Framework48
        /// <summary>
        /// Write the log in an async way
        /// </summary>
        /// <param name="nId">Id of the object</param>
        /// <param name="nType">Type of the object</param>
        /// <param name="nOperation">Description of the operation done</param>
        /// <param name="nUser">User that do that operation</param>
        /// <param name="nDescription">Text description </param>
        /// <param name="Att">Other generic properties to better detail the operation</param>
        public void  PostLog(Guid nId, TipiOggetto nType, TipoOperazione nOperation,string nUser,string nDescription, ExtAttributes Att=null)
        {
            LogDoc log = new LogDoc()
            {
                Data = DateTime.Now,
                IdOggetto = nId,
                TipoOggetto = nType,
                Operazione = nOperation,
                Utente = nUser,
                Descrizione=nDescription,
                JAttributi= null,

            };
            Task.Run(
                () =>
                {
                    try
                    {
                        using var _context = new dbluDocsContext(_conf);
                        _context.LogDoc.Add(log);
                        _context.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"LogDocManager.PostLog: Unexpected exception {ex.Message}");
                    }
                }
                );
        }


        /// <summary>
        ///  Write the log in an async way
        /// </summary>
        /// <param name="nId">Id of the object</param>
        /// <param name="nType">Type of the object</param>
        /// <param name="nOperation">Description of the operation done</param>
        /// <param name="nUser">User that do that operation</param>
        /// <param name="nDescription">Text description </param>
        /// <param name="Att">Other generic properties to better detail the operation</param>
        public void PostLog(string nId, TipiOggetto nType, TipoOperazione nOperation, string nUser, string nDescription, ExtAttributes Att = null)
        {
            LogDoc log = new LogDoc()
            {
                Data = DateTime.Now,
                IdOggetto = Guid.Parse(nId),
                TipoOggetto = nType,
                Operazione = nOperation,
                Utente = nUser,
                Descrizione = nDescription,
                JAttributi = null,

            };
            Task.Run(
                () =>
                {
                    try
                    {
                        using var _context = new dbluDocsContext(_conf);
                        _context.LogDoc.Add(log);
                        _context.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"LogDocManager.PostLog: Unexpected exception {ex.Message}");
                    }
                }
                );
        }
#endif

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
