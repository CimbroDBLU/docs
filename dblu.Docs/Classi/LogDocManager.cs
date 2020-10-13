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
    public class LogDocManager 
    {
        //private dbluDocsContext _context;
        private string StringaConnessione ;
        private readonly ILogger _logger;

        //public AllegatiManager(dbluDocsContext context, ILogger logger) {
        public LogDocManager(string connessione, ILogger logger)
            {
            //   _context = context;
            StringaConnessione = connessione;
            _logger = logger;
        }


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
                //l = _context.EmailServer
                //    .ToList<EmailServer>();
                using (SqlConnection cn = new SqlConnection(StringaConnessione))
                {
                    l = cn.GetAll<LogDoc>().ToList();
                }
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

        public bool Salva(LogDoc log, bool isNew)
        {
            var bres = false;
            try
            {
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

            }
            catch (Exception ex)
            {
                _logger.LogError($"Salva log: {ex.Message}");

            }
            return bres;
        }


      
        public List<LogDoc> GetLogOggetto(Guid IdOggetto, TipiOggetto Tipo)
        {

            List<LogDoc > log = null;
            try
            {
                using (SqlConnection cn = new SqlConnection(StringaConnessione))
                {
                    log = cn.Query<LogDoc>("SELECT * FROM LogDoc WHERE IdOggetto = @Id and TipoOggetto = @Tipo ORDER BY data DESC ",
                        new { Id = IdOggetto.ToString() , Tipo = Tipo }).ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetLogElemento: {ex.Message}");

            }
            return log;


        }
       
    }
}
