
using dblu.Docs.Classi;
using dblu.Docs.Models;
using dblu.Portale.Plugin.Docs.Services;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dblu.Portale.Plugin.Docs.Controllers.ODATA
{
    public class ODATAController : ODataController
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
        public ODATAController(AllegatiService doc, IConfiguration nConfiguration, ILoggerFactory loggerFactory)
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
        [HttpGet]
        [ODataRoute("Dossiers")]
        public PageResult<viewFascicoli> Dossiers(ODataQueryOptions opts)
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

            _logger.LogInformation($"ODATAController.Dossiers: ODATA Read in {sw.ElapsedMilliseconds} ms");
            return new PageResult<viewFascicoli>(results, null, count);
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
        [HttpGet]
        [ODataRoute("Items")]
        public PageResult<viewElementi> Items(ODataQueryOptions opts)
        {
            Stopwatch sw = Stopwatch.StartNew();


            var results = _doc.GetElementiV().AsQueryable();
            var count = results.Count();
            if (opts.OrderBy != null)
                results = opts.OrderBy.ApplyTo(results);
            if (opts.Filter != null)
            {
                results = opts.Filter.ApplyTo(results, new ODataQuerySettings()).Cast<viewElementi>();
            }
            var queryString = opts.Request.Query;
            string search = queryString["$search"];
            if (search != null)
            {
                string key = search.Split(" OR ")[search.Split(" OR ").Length - 1].ToLowerInvariant();
                results = results.Where(
                                        fil => fil.IdElemento.ToString().ToLowerInvariant().Contains(key)
                                            || fil.DscElemento.ToLowerInvariant().Contains(key)
                                            || fil.DscTipoElemento.ToLowerInvariant().Contains(key)
                                            || fil.Stato.ToString().ToLowerInvariant().Contains(key)
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

            _logger.LogInformation($"ODATAController.Items: ODATA Read in {sw.ElapsedMilliseconds} ms");
            return new PageResult<viewElementi>(results, null, count);
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
        [HttpGet]
        [ODataRoute("History")]
        public async Task<PageResult<Processi>> History(ODataQueryOptions opts)
        {
            Stopwatch sw = Stopwatch.StartNew();


            var results = (await _doc._hiMan.GetAll()).AsQueryable();
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
                                            || (fil.Nome != null && fil.Nome.ToLowerInvariant().Contains(key))
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

            _logger.LogInformation($"ODATAController.History: ODATA Read in {sw.ElapsedMilliseconds} ms");
            return new PageResult<Processi>(results, null, count);
        }

        /// <summary>
        /// Function that return the data (filtered already)
        /// NB. Return a simply IQueryable with [EnableQuery] decorator is better for performance BUT
        /// since Synchfusion support "SEARCH" and Kestrel not yet, we replace "Search" with a manual, case insensitive, filter
        /// </summary>
        /// <param name="Type">Type of attacments (inbox, processed, sent, etc)</param>
        /// <param name="Mailbox">Name of the filtering mailbox</param>
        /// <param name="opts">Other filter done by grid itself</param>
        /// <returns>
        /// List of selected emails
        /// </returns>
        [HttpGet]
        [ODataRoute("Mails(Type={Type},Mailbox={Mailbox})")]
        public PageResult<AllegatoEmail> Mails([FromODataUri] string Type, [FromODataUri] string Mailbox, ODataQueryOptions opts)
        {
            try
            {
                Stopwatch sw = Stopwatch.StartNew();
                var results = new List<AllegatoEmail>().AsQueryable();
                switch (Type.ToUpper())
                {
                    case "PROCESSED":
                        results = _doc._allMan.GetEmailProcessate("EMAIL", Mailbox).AsQueryable();
                        break;
                    case "SENT":
                        results = _doc._allMan.GetEmailInviate("EMAIL", Mailbox).AsQueryable();
                        break;
                    case "SORTING":
                        results = _doc._allMan.GetEmailDaSmistare("EMAIL", Mailbox).AsQueryable();
                        break;
                    case "INBOX":
                    default:
                        results = _doc._allMan.GetEmailInArrivo("EMAIL", Mailbox).AsQueryable();
                        break;
                }

                var count = results.Count();
                if (opts.OrderBy != null)
                    results = opts.OrderBy.ApplyTo(results);
                else
                    results = results.OrderByDescending(c => c.DataC);

                if (opts.Filter != null)
                {
                    results = opts.Filter.ApplyTo(results, new ODataQuerySettings()).Cast<AllegatoEmail>();
                }
                var queryString = opts.Request.Query;
                string search = queryString["$search"];
                if (search != null)
                {
                    string key = search.Split(" OR ")[search.Split(" OR ").Length - 1].ToLowerInvariant();
                    results = results.Where(
                                            fil =>
                                                (!string.IsNullOrEmpty(fil.Oggetto) && (fil.Oggetto.ToLowerInvariant().Contains(key)))
                                                || (!string.IsNullOrEmpty(fil.Mittente) && (fil.Mittente.ToLowerInvariant().Contains(key)))
                                                || (!string.IsNullOrEmpty(fil.Destinatario) && (fil.Destinatario.ToLowerInvariant().Contains(key)))
                                                || (!string.IsNullOrEmpty(fil.Chiave1) && (fil.Chiave1.ToLowerInvariant().Contains(key)))
                                                || (!string.IsNullOrEmpty(fil.Chiave2) && (fil.Chiave2.ToLowerInvariant().Contains(key)))
                                                || (!string.IsNullOrEmpty(fil.Chiave3) && (fil.Chiave3.ToLowerInvariant().Contains(key)))
                                                || (!string.IsNullOrEmpty(fil.Chiave4) && (fil.Chiave4.ToLowerInvariant().Contains(key)))
                                                || (!string.IsNullOrEmpty(fil.Chiave5) && (fil.Chiave5.ToLowerInvariant().Contains(key)))
                                           );

                }
                if (opts.Count != null)
                    count = results.Count();
                if (opts.Skip != null)
                    results = opts.Skip.ApplyTo(results, new ODataQuerySettings());
                if (opts.Top != null)
                    results = opts.Top.ApplyTo(results, new ODataQuerySettings());
                foreach (var a in results)
                {
                    a.jAttributi = null;
                    a.jNote = null;
                }
                _logger.LogInformation($"ODATAController.Mails: Loaded {count} {Type} emails in {sw.ElapsedMilliseconds} ms");

                return new PageResult<AllegatoEmail>(results, null, count);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ODATAController.Mails: Unexpected exception {ex}");
                return new PageResult<AllegatoEmail>(null, null, 0);
            }

        }

        /// <summary>
        /// Function that return the data (filtered already)
        /// NB. Return a simply IQueryable with [EnableQuery] decorator is better for performance BUT
        /// since Synchfusion support "SEARCH" and Kestrel not yet, we replace "Search" with a manual, case insensitive, filter
        /// </summary>
        /// <param name="Doc">Type od files (ZIP ,REQ)</param>
        /// <param name="Type">Type of attacments (inbox, processed, sent, etc)</param>
        /// <param name="Folder">Folder in witch look</param>
        /// <param name="opts">Query params</param>
        /// <returns>
        /// List of selected files
        /// </returns>
        [HttpGet]
        [ODataRoute("Files(Doc={Doc},Type={Type},Folder={Folder})")]
        public PageResult<Allegati> Files([FromODataUri] string Doc, [FromODataUri] string Type, [FromODataUri] string Folder, ODataQueryOptions opts)
        {
            try
            {
                Stopwatch sw = Stopwatch.StartNew();
                var results = new List<Allegati>().AsQueryable();
                switch (Type.ToUpper())
                {
                    case "PROCESSED":
                        results = _doc._allMan.GetEmailProcessate(Doc, Folder).AsQueryable();
                        break;
                    case "INBOX":
                    default:
                        results = _doc._allMan.GetZipInArrivo(Doc, Folder).AsQueryable();
                        break;
                }

                var count = results.Count();
                if (opts.OrderBy != null)
                    results = opts.OrderBy.ApplyTo(results);
                else
                    results = results.OrderByDescending(c => c.DataC);

                if (opts.Filter != null)
                {
                    results = opts.Filter.ApplyTo(results, new ODataQuerySettings()).Cast<Allegati>();
                }
                var queryString = opts.Request.Query;
                string search = queryString["$search"];
                if (search != null)
                {
                    string key = search.Split(" OR ")[search.Split(" OR ").Length - 1].ToLowerInvariant();
                    results = results.Where(
                                            fil =>
                                                (!string.IsNullOrEmpty(fil.Chiave1) && (fil.Chiave1.ToLowerInvariant().Contains(key)))
                                                || (!string.IsNullOrEmpty(fil.Chiave2) && (fil.Chiave2.ToLowerInvariant().Contains(key)))
                                                || (!string.IsNullOrEmpty(fil.Chiave3) && (fil.Chiave3.ToLowerInvariant().Contains(key)))
                                                || (!string.IsNullOrEmpty(fil.Chiave4) && (fil.Chiave4.ToLowerInvariant().Contains(key)))
                                                || (!string.IsNullOrEmpty(fil.Chiave5) && (fil.Chiave5.ToLowerInvariant().Contains(key)))
                                           );

                }
                if (opts.Count != null)
                    count = results.Count();
                if (opts.Skip != null)
                    results = opts.Skip.ApplyTo(results, new ODataQuerySettings());
                if (opts.Top != null)
                    results = opts.Top.ApplyTo(results, new ODataQuerySettings());
                foreach (var a in results)
                {
                    a.jAttributi = null;
                    a.jNote = null;
                }
                _logger.LogInformation($"ODATAController.Files: Loaded {count} {Type} {Doc} files in {sw.ElapsedMilliseconds} ms");
                return new PageResult<Allegati>(results, null, count);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ODATAController.Files: Unexpected exception {ex}");
                return new PageResult<Allegati>(null, null, 0);
            }

        }

    }
}
