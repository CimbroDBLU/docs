using System;
using System.Collections.Generic;
using dWorkerInterface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MoonSharp.Interpreter;
using dblu.Docs.Classi;
using System.IO;
using System.Threading.Tasks;

namespace dWorker.Plugin.Docs
{
    /// <summary>
    /// Standar dWorker docs plug-In
    ///     Add method for print an attachment
    /// </summary>
    public class DocsPlugin : IWorkerPlugin
    {
        /// <summary>
        /// Logger Interface
        /// </summary>
        private ILogger _logger;

        /// <summary>
        /// Configuration interface
        /// </summary>
        private IConfiguration _conf;

        /// <summary>
        /// Dispose pluging
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// Method for executing the operation requested
        /// </summary>
        /// <param name="Operation">Operation requested</param>
        /// <param name="OperationKey">Key that identify the operation (ID of the DB, or script name or whatever, depending on source)</param>
        /// <returns>
        /// bool if operation has been done properly
        /// </returns>
        public bool Execute(batch_operation Operation, string OperationKey)
        {
            bool res = false;
            _logger.LogDebug($"operation: {Operation.operation_type} {OperationKey}");
            switch (Operation.operation_type)
            {
                case "print_attach":
                    string AttachID=Operation.parameters["AttachID"].ToString();
                    string Printer = Operation.printer_name;
                    _logger.LogInformation($"DocsPlugIn.Execute: Request print of attachment {AttachID}");
                    res = PrintAttach(AttachID, Printer);
                    break;
                default:
                    break;
            }
            return res;
        }

        /// <summary>
        /// Init the plugin
        /// </summary>
        /// <param name="worker">Worker that is using this plugin</param>
        /// <returns>
        /// True if plugin has beeninitialized properly
        /// </returns>
        public bool Init(IPluginOwner worker)
        {
            _logger = worker.logger;
            _conf = worker.conf;
            return true;
        }
        /// <summary>
        /// Supported operations for this plug-in
        /// </summary>
        /// <returns></returns>
        public List<string> OperationNames()
        {
            return new List<string>() { "print_attach" };
        }

        /// <summary>
        /// Register object for script interfacing
        /// </summary>
        /// <param name="script">Script Interpreter Object</param>
        /// <returns>
        /// True once all object has been registered
        /// </returns>
        public bool RegisterScriptObjects(Script script)
        {
            return true;
        }

        /// <summary>
        /// Print a specific attachment to a specific printer
        /// </summary>
        /// <param name="AttachID">Id of the attachment</param>
        /// <param name="Printer">Name of the printer</param>
        /// <returns>
        /// True if the printing has been done properly
        /// </returns>
        public bool PrintAttach(string AttachID,string Printer)
        {
            try
            {
                AllegatiManager AM = new(_conf["ConnectionStrings:dblu.Docs"], _logger);
                MemoryStream MS = AM.GetFileAsync(AttachID).Result;
                if (MS == null) return false;

                return (new PrintPDF(_logger)).Print(AttachID, MS, Printer);
            }catch(Exception ex)
            {
                _logger.LogInformation($"DocsPlugIn.PrintAttach: Printing of {AttachID}, failed : {ex}");
                return false;
            }
        }


    }
}
