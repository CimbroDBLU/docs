using BPMClient;
using dblu.Docs.Models;
using dbluDealersConnector.DealersAPI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dbluDealersConnector.Workers
{
    /// <summary>
    /// Helper class for running a Process against Camunda
    /// </summary>
    public class RunWorkflow
    {
        /// <summary>
        /// Injected configuration
        /// </summary>
        public IConfiguration _config { get; set; }

        /// <summary>
        /// Injected logger
        /// </summary>
        private readonly ILogger _log;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="nConfig">Injected configuration</param>
        /// <param name="nLogger">Injected logger</param>
        public RunWorkflow(IConfiguration nConfig,ILogger nLogger)
        {
            _config = nConfig;
            _log = nLogger;
        }

        /// <summary>
        /// Start a process
        /// </summary>
        /// <param name="Name">Name of the process to start</param>
        /// <param name="REQ">Request to send to CAMUNDA</param>
        /// <param name="A">Attachment to send to CAMUNDA</param>
        /// <returns>
        /// true if process is started
        /// </returns>
        public async Task<bool> Start(string Name, DealersRequest REQ, Allegati A)
        {
            string CamundaUrl = _config["Camunda:Ip"];
            if (string.IsNullOrEmpty(CamundaUrl))
            {
                _log.LogError($"RunWorkflow.Start: Camunda url is empty!");
                return false;
            }

            if (string.IsNullOrEmpty(Name))
            {
                _log.LogError($"RunWorkflow.Start: Process name is empty");
                return false;
            }

            BPMClient.BPMEngine eng = new BPMClient.BPMEngine();
            eng.Imposta(_config["Camunda:Ip"], "", "", "");
            var pd = new BPMProcessDefinition(eng);

            var pdi = await pd.Get("", Name);
            if (pdi == null || pdi.Id == null)
            {
                _log.LogError($"RunWorkflow.Start: Unabled to find process {Name}.");
                return false;
            }
            else
            {
                SubmitStartForm ssf = new SubmitStartForm();

                ssf.BusinessKey = REQ.Id.ToString();
                ssf.SetVariable("sMittente", REQ.Mail);
                ssf.SetVariable("dData", REQ.LastModificationTime?.ToString("dd/MM/yyyy hh:mm")??"");
                ssf.SetVariable("sOggetto",REQ.Descrizione);
                ssf.SetVariable("sIdAllegato", A.Id.ToString());
           
                BPMProcessInstanceInfo pi = await pd.SubmitForm(pdi.Id, Name, ssf);
                if (pi == null)
                {
                    _log.LogError($"RunWorkflow.Start: Unable to start the process {Name}.");
                    return false;
                }
                else
                {
                    _log.LogDebug($"RunWorkflow.Start: Process {Name} started.");
                    return true;
   
                }


           
            }

        }
    }
}
