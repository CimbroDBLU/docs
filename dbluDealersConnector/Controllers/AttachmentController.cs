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
using dbluDealersConnector.Model;

namespace dblu.Docs.Service.Controllers
{
    /// <summary>
    /// Controller for downloading attachments
    /// </summary>
    [Route("/Attachments")]
    [Authorize]
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
        /// Retreive an attachment using the Id
        /// </summary>
        /// <param name="Id">Id of the attachment</param>
        /// <returns>
        /// The Attachment found
        /// </returns>
        [Authorize]
        [HttpGet("{Id}")]
        public Attachment Get(Guid Id)
        {
            AllegatiManager AM = new AllegatiManager(conf["dbluDocs:db"], log);
            Allegati A=AM.Get(Id);
            if(A!=null)
                return new Attachment() { Id = Id, Filename = A.NomeFile, Description = A.Descrizione };
            return new Attachment() { Id = Guid.Empty, Filename = "", Description = "" };
        }
    
        /// <summary>
        /// Retreive the attachments included in this element
        /// </summary>
        /// <param name="Id">Id of the item</param>
        /// <returns>
        /// List of the Attachment linked to this item
        /// </returns>
        [Authorize]
        [HttpGet("_ByItemId/{Id}")]
        public List<Attachment> GetByItemId(Guid Id)
        {
            ElementiManager EM=new ElementiManager(conf["dbluDocs:db"], log);
            
            List<Allegati> lst=EM.GetAllegatiElemento(Id);
            List<Attachment> lstAttachment = new List<Attachment>();
            foreach (Allegati A in lst)
                lstAttachment.Add(new Attachment() { Id = A.Id, Filename = A.NomeFile, Description = A.Descrizione });
            return lstAttachment;
        }

        /// <summary>
        /// Return the specified attachment
        /// </summary>
        /// <param name="Id">Id of the attachment</param>
        /// <returns>
        /// The file downloaded
        /// </returns>
        [Authorize]
        [HttpGet("{Id}/Doc")]
        public async Task<IActionResult> Download(Guid Id)
        {
            AllegatiManager AM = new AllegatiManager(conf["dbluDocs:db"], log);
            Allegati A = AM.Get(Id);
            if (A != null)
            {              
                MemoryStream Zipped = await AM.GetFileAsync(A.Id.ToString());
                if(Zipped!=null)
                    return File(Zipped.ToArray(), AM.GetContentType(A.NomeFile), A.NomeFile);
            }
            return NotFound();
        }


    }
}
