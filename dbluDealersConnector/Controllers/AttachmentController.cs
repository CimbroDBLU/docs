using dblu.Docs.Classi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using dbluDealersConnector.Classes;
using dblu.Docs.Models;
using System.IO;

namespace dblu.Docs.Service.Controllers
{
    /// <summary>
    /// Controller for downloading attachments
    /// </summary>
    [Route("/Attachments")]
    [ApiController]
    public class AttachmentController : Controller
    {
        /// <summary>
        /// Injected configuration
        /// </summary>
        private readonly IConfiguration conf;
        /// <summary>
        /// Injected logger
        /// </summary>
        private readonly ILogger<AttachmentController> log;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="nLog">Injected configuration</param>
        /// <param name="nConf"> List of accepted users</param>
        public AttachmentController(ILogger<AttachmentController> nLog, IConfiguration nConf)
        {
            log = nLog;
            conf = nConf;
        }

        /// <summary>
        /// Return the specidief attachment
        /// </summary>
        /// <param name="Id">Id of the attachment</param>
        /// <returns>
        /// The file downloaded
        /// </returns>
        [Authorize]
        [HttpGet("{Id}/Docs")]
        public async Task<IActionResult> Download(Guid Id)
        {
            return NotFound();
        }


    }
}
