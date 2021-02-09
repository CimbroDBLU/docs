using dblu.Docs.Classi;
using dbluDealersConnector.Models;
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
    [Route("/Attachments")]
    [ApiController]
    public class AttachmentController : Controller
    {
        private IConfiguration Configuration;
        private ILogger Logger;

        public AttachmentController(IConfiguration configuration,ILogger logger)
        {
            Configuration = configuration;
            Logger= logger;
        }


        [Authorize]
        [HttpGet("{Id}/Docs")]
        public async Task<IActionResult> Download(Guid Id)
        {
            return NotFound();
        }


    }
}
