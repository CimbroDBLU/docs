using Dapper;
using Dapper.Contrib.Extensions;
using dblu.Docs.Interfacce;
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
    public class SoggettiManager
    {

        //private dbluDocsContext _context;
        private string StringaConnessione;
        private readonly ILogger _logger;

        //public SoggettiManager(dbluDocsContext context, ILogger logger)
        public SoggettiManager(string connessione, ILogger logger)
        {
            //  _context = context;
            StringaConnessione = connessione;
            _logger = logger;
        }

        public Soggetti Get(string Codice)
        { 
            Soggetti sog = null;
            try
            {
                using (SqlConnection cn = new SqlConnection(StringaConnessione))
                {
                if (!string.IsNullOrEmpty(Codice))
                {
                        //sog = _context.Soggetti
                        //.Single(a => a.Codice == Codice);
                        sog = cn.Get<Soggetti>(Codice);

                }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
            return sog;
        }

        public async Task<List<ISoggetti>> GeAllAsync()
        {
            List<ISoggetti> l = new List<ISoggetti>();
            try
            {
                //l = await _context.Soggetti
                //.ToListAsync<ISoggetti>();
                using (SqlConnection cn = new SqlConnection(StringaConnessione))
                {
                    l = await Task.FromResult(cn.GetAll<Soggetti>().ToList<ISoggetti>()); 
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"GeAllAsync: {ex.Message}");
            }
            return l;
        }

        public List<ISoggetti> GetbyMail(string Mail)
        {
            List<ISoggetti> l = new List<ISoggetti>();
            try
            {
                //l = _context.Soggetti
                //    .ToList<ISoggetti>();
                using (SqlConnection cn = new SqlConnection(StringaConnessione))
                {
                    l = cn.Query<Soggetti>("select Soggetti.* from EmailSoggetti left join Soggetti on soggetti.Codice =EmailSoggetti.CodiceSoggetto  where  EmailSoggetti.email= '" + Mail + "'").ToList<ISoggetti>();
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"GeAll: {ex.Message}");
            }
            return l;
        }

        public List<ISoggetti> GetAll()
        {
            List<ISoggetti> l = new List<ISoggetti>();
            try
            {
                //l = _context.Soggetti
                //    .ToList<ISoggetti>();
                using (SqlConnection cn = new SqlConnection(StringaConnessione))
                {
                    l = cn.GetAll<Soggetti>().ToList<ISoggetti>();
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"GeAll: {ex.Message}");
            }
            return l;
        }

      
        public bool Salva(Soggetti soggetto, bool isNew)
        {
            var bres = false;
            try
            {
                using (SqlConnection cn = new SqlConnection(StringaConnessione))
                {
 
                    if (isNew)
                    {
                        var r = cn.Insert<Soggetti>(soggetto);
                    }
                    else
                    {
                        var r = cn.Update<Soggetti>(soggetto);
                    }
                }
                bres = true;

            }
            catch (Exception ex)
            {
                _logger.LogError($"Salva Soggetto: {ex.Message}");

            }
            return bres;
        }

    }
}
