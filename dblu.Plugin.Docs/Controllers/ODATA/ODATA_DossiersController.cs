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
using dblu.Docs.Classi;

namespace dblu.Portale.Plugin.Docs.Controllers.ODATA
{

    /// <summary>
    /// Controller for ODATA integration of Dossiers
    /// </summary>
    [Route("ODATA")]
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
        /// History Manager class for reading History Content
        /// </summary>
        private readonly HistoryManager History;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="doc">Attachment Service injected</param>
        /// <param name="loggerFactory">factory for log creation</param>
        public ODATA_DossiersController(AllegatiService doc, ILoggerFactory loggerFactory)
        {
            _doc = doc;
            _logger = loggerFactory.CreateLogger("ODATA");
            History = new HistoryManager(nConfiguration["ConnectionStrings:dblu.Docs"], _logger);
        }


    }

}
