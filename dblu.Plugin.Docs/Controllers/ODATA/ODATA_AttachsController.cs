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
using Microsoft.AspNet.OData.Routing;

namespace dblu.Portale.Plugin.Docs.Controllers.ODATA
{

    /// <summary>
    /// Controller for ODATA integration of Attachments (Mails and files)
    /// </summary>
    public class ODATA_AttachsController : ODataController
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
        public ODATA_AttachsController(AllegatiService doc, ILoggerFactory loggerFactory)
        {
            _doc = doc;
            _logger = loggerFactory.CreateLogger("ODATA");
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
        [ODataRoute("GetMails(Type={Type},Mailbox={Mailbox})")]
        public PageResult<AllegatoEmail> GetMails([FromODataUri] string Type, [FromODataUri] string Mailbox, ODataQueryOptions opts)
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
                _logger.LogInformation($"ODATA_AttachsController.GetMails: Loaded {count} {Type} emails in {sw.ElapsedMilliseconds} ms");

                return new PageResult<AllegatoEmail>(results, null, count);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ODATA_AttachsController.GetMails: Unexpected exception {ex}");
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
        [ODataRoute("GetFiles(Doc={Doc},Type={Type},Folder={Folder})")]
        public PageResult<Allegati> GetFiles([FromODataUri] string Doc, [FromODataUri] string Type, [FromODataUri] string Folder, ODataQueryOptions opts)
        {
            try
            {
                Stopwatch sw = Stopwatch.StartNew();
                var results = new List<Allegati>().AsQueryable();
                switch (Type.ToUpper())
                {
                    case "PROCESSED":
                        results = _doc._allMan.GetZipInArrivo(Doc, Folder).AsQueryable();
                        break;
                    case "INBOX":
                    default:
                        results = _doc._allMan.GetEmailProcessate(Doc, Folder).AsQueryable();
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
                _logger.LogInformation($"ODATA_AttachsController.GetFiles: Loaded {count} {Type} {Doc} files in {sw.ElapsedMilliseconds} ms");
                return new PageResult<Allegati>(results, null, count);
            }
            catch(Exception ex)
            {
                _logger.LogError($"ODATA_AttachsController.GetFile: Unexpected exception {ex}");
                return new PageResult<Allegati>(null,null,0);
            }
           
        }

    }

}
