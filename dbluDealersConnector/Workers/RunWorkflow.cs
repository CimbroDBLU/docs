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
        /// <param name="Name">Name of the process</param>
        /// <param name="ssf">Parameters for the process</param>
        /// <returns>
        /// true if process is started
        /// </returns>
        public async Task<bool> Start(string Name, SubmitStartForm ssf)
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
