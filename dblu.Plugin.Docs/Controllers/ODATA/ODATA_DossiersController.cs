using dblu.Docs.Models;
using dblu.Portale.Plugin.Docs.Services;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Extensions;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Syncfusion.Blazor.Data;
using Syncfusion.Blazor;

namespace dblu.Portale.Plugin.Docs.Controllers.ODATA
{

    /// <summary>
    /// Controller for ODATA integration of Dossiers
    /// </summary>
    public class ODATA_DossiersController : ODataController
    {
        /// <summary>
        /// Attachment Service injected
        /// </summary>
        private AllegatiService _doc;

        /// <summary>
        /// Log interface
        /// </summary>
        public readonly ILogger _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="doc">Attachment Service injected</param>
        /// <param name="loggerFactory">factory for log creation</param>
        public ODATA_DossiersController(AllegatiService doc, ILoggerFactory loggerFactory)
        {
            _doc = doc;
            _logger = loggerFactory.CreateLogger("ODATA");
        }

        // OPTIMIZED VERSION, SEE COMMENT IN ACTUAL GET
        //[EnableQuery]
        //private IQueryable Get()
        //{
        //    Stopwatch sw = Stopwatch.StartNew();
        //    var results = _doc.GetFascicoliV().AsQueryable();
        //    _logger.LogInformation($"ODATA_DossiersController.Get: ODATA Read in {sw.ElapsedMilliseconds} ms");
        //    return results;
        //}
        
        /// <summary>
        /// Function that return the data (filtered already)
        /// NB. Return a simply IQueryable with [EnableQuery] decorator is better for performance BUT
        /// since Synchfusion support "SEARCH" and Kestrel not yet, we replace "Search" with a manual, case insensitive, filter
        /// </summary>
        /// <param name="opts">Query params</param>
        /// <returns>
        /// Result set
        /// </returns>
        public PageResult<viewFascicoli> Get(ODataQueryOptions opts)
        {
            Stopwatch sw = Stopwatch.StartNew();


            var results = _doc.GetFascicoliV().AsQueryable();
            var count = results.Count();
            if (opts.OrderBy != null)
                results = opts.OrderBy.ApplyTo(results);
            if (opts.Filter != null)
            {
                results = opts.Filter.ApplyTo(results, new ODataQuerySettings()).Cast<viewFascicoli>();
            }
            var queryString = opts.Request.Query;
            string search = queryString["$search"];
            if (search != null)
            {
                string key = search.Split(" OR ")[search.Split(" OR ").Length - 1].ToLowerInvariant();
                results = results.Where(
                                        fil => fil.IdFascicolo.ToString().ToLowerInvariant().Contains(key)
                                            || fil.DscCategoria.ToLowerInvariant().Contains(key)
                                            || fil.DscFascicolo.ToLowerInvariant().Contains(key)
                                            || fil.Campo1.ToLowerInvariant().Contains(key)
                                            || fil.Campo2.ToLowerInvariant().Contains(key)
                                            || fil.Campo3.ToLowerInvariant().Contains(key)
                                            || fil.Campo4.ToLowerInvariant().Contains(key)
                                            || fil.Campo5.ToLowerInvariant().Contains(key)
                                            || fil.Campo6.ToLowerInvariant().Contains(key)
                                            || fil.Campo7.ToLowerInvariant().Contains(key)
                                            || fil.Campo8.ToLowerInvariant().Contains(key)
                                            || fil.Campo9.ToLowerInvariant().Contains(key)
                                            || fil.Campo10.ToLowerInvariant().Contains(key)
                                       );
            }
            if (opts.Count != null)
                count = results.Count();
            if (opts.Skip != null)
                results = opts.Skip.ApplyTo(results, new ODataQuerySettings());
            if (opts.Top != null)
                results = opts.Top.ApplyTo(results, new ODataQuerySettings());

            _logger.LogInformation($"ODATA_DossiersController.Get: ODATA Read in {sw.ElapsedMilliseconds} ms");
            return new PageResult<viewFascicoli>(results, null, count);
        }
    }

}
