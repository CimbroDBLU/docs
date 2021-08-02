using Microsoft.AspNet.OData;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dblu.Docs.Classi;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNet.OData.Query;
using System.Diagnostics;
using dblu.Docs.Models;

namespace dblu.Portale.Plugin.Docs.Controllers.ODATA
{

    /// <summary>
    /// Controller for ODATA integration of History
    /// </summary>
    public class ODATA_HistoryController : ODataController
    {
        /// <summary>
        /// History Manager class for reading History Content
        /// </summary>
        private readonly HistoryManager History;

        /// <summary>
        /// Log interface
        /// </summary>
        public readonly ILogger _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="loggerFactory">factory for log creation</param>
        public ODATA_HistoryController( ILoggerFactory loggerFactory, IConfiguration nConfiguration)
        {           
            _logger = loggerFactory.CreateLogger("ODATA");
            History= new HistoryManager(nConfiguration["ConnectionStrings:dblu.Docs"], _logger);
        }

        /// <summary>
        /// Function that return the data (filtered already)
        /// NB. Return a simply IQueryable with [EnableQuery] decorator is better for performance BUT
        /// since Synchfusion support "SEARCH" and Kestrel not yet, we replace "Search" with a manual, case insensitive, filter
        /// </summary>
        /// <param name="opts">Query params</param>
        /// <returns>
        /// Result set
        /// </returns>
        public async Task<PageResult<Processi>> Get(ODataQueryOptions opts)
        {
            Stopwatch sw = Stopwatch.StartNew();


            var results = (await History.GetAll()).AsQueryable();
            var count = results.Count();
            if (opts.OrderBy != null)
                results = opts.OrderBy.ApplyTo(results);
            if (opts.Filter != null)
            {
                results = opts.Filter.ApplyTo(results, new ODataQuerySettings()).Cast<Processi>();
            }
            var queryString = opts.Request.Query;
            string search = queryString["$search"];
            if (search != null)
            {
                string key = search.Split(" OR ")[search.Split(" OR ").Length - 1].ToLowerInvariant();
                results = results.Where(
                                        fil => fil.Id.ToString().ToLowerInvariant().Contains(key)
                                            || ( fil.Nome != null && fil.Nome.ToLowerInvariant().Contains(key))
                                            || (fil.Descrizione != null && fil.Descrizione.ToLowerInvariant().Contains(key))
                                            || (fil.Diagramma != null && fil.Diagramma.ToLowerInvariant().Contains(key))
                                            || (fil.UtenteAvvio != null && fil.UtenteAvvio.ToLowerInvariant().Contains(key))
                                            || (fil.Versione != null && fil.Versione.ToLowerInvariant().Contains(key))
                                       );
            }
            if (opts.Count != null)
                count = results.Count();
            if (opts.Skip != null)
                results = opts.Skip.ApplyTo(results, new ODataQuerySettings());
            if (opts.Top != null)
                results = opts.Top.ApplyTo(results, new ODataQuerySettings());

            _logger.LogInformation($"ODATA_HistoryController.Get: ODATA Read in {sw.ElapsedMilliseconds} ms");
            return new PageResult<Processi>(results, null, count);
        }
    }

}
