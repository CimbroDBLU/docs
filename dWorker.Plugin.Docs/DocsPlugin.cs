using System;
using System.Collections.Generic;
using dWorkerInterface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MoonSharp.Interpreter;
using dblu.Docs.Classi;
using System.IO;
using System.Threading.Tasks;
using dblu.Docs.Models;
using Syncfusion.Licensing;
using dWorkerDatabase;
using Syncfusion.Blazor.Data;
using System.Linq;
using System.Diagnostics;

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

        protected IPluginOwner _worker;

        private DocsManager _docsManager { get; set; }
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
            _logger.LogDebug($"operation: {Operation.operation_type} {Operation.operation_name} {OperationKey}");
            switch (Operation.operation_type)
            {
                case "print_attach":
                    string AttachID = Operation.parameters["AttachID"].ToString();
                    string Printer = Operation.printer_name;
                    _logger.LogInformation($"DocsPlugIn.Execute: Request print of attachment {AttachID}");
                    res = PrintAttach(AttachID, Printer);
                    break;
                case "resume_suspended":
                    using (dWorkerContext dwDB = new(_conf[$"ConnectionStrings:{Operation.connection_name}"]))
                    {
                        Stopwatch SW = Stopwatch.StartNew();
                       List<dWorkerTask> Tsks= dwDB.Tasks.Where(x=>x.Status== e_dWorkerStatus.Suspended).ToList();
                        _logger.LogInformation($"DocsPlugIn.Execute: Request activation of {Tsks.Count} suspended tasks");
                        foreach (dWorkerTask Tsk in Tsks)
                            Tsk.Status = e_dWorkerStatus.Inserted;
                        dwDB.SaveChanges();
                        _logger.LogInformation($"DocsPlugIn.Execute: Activated {Tsks.Count} in {SW.ElapsedMilliseconds} ms");
                    }
                    break;
                case "email_capture":
                    ProcessaPostaInArrivo(Operation, OperationKey);
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
            SyncfusionLicenseProvider.RegisterLicense("NTU2MjQ2QDMxMzkyZTM0MmUzMEJEcjN2ZVhqR0dXWm8vOUlRdTljbTF0S1B5SzRlYnBpc05aMDh1WStOMEE9; NTU2MjQ3QDMxMzkyZTM0MmUzMGY1TER4VGlCSFdBWEdEemwreHRYQno3cFJsd1lVdSsybzduZUpYNGQreTQ9; NTU2MjQ4QDMxMzkyZTM0MmUzMG04UkVBZXlPK014d2ROT3FDTkJ5TlNOM0U1bWU2MzlKTEJndFNTTjVOT1U9; NTU2MjQ5QDMxMzkyZTM0MmUzMGlRSEtBUnZ2dElOTGZpai9rdkJVYXFLRmZpVjlYcGJSZ3dkWmZNT0NpT3c9");
            _worker = worker;    
            _logger = worker.logger;
            _conf = worker.conf;
            _docsManager = new(_worker.logger, _worker.conf, "dblu.Docs");

            return true;
        }
        /// <summary>
        /// Supported operations for this plug-in
        /// </summary>
        /// <returns></returns>
        public List<string> OperationNames()
        {
            return new List<string>() { "print_attach", "email_capture", "resume_suspended" };
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
            _worker.script.SetObject("docs", _docsManager);
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


        private bool ProcessaPostaInArrivo(batch_operation Operation, string OperationKey)
        {
            bool res = true;
            try
            {

                var mailMan = new MailManager(_logger, _conf);
                mailMan.TipoAllegato = Operation.parameters["attachment_type"].ToString();
                mailMan.Connessione = _conf.GetConnectionString(Operation.connection_name);
                mailMan.Bpm_url = _conf["Camunda:Ip"];
                mailMan.Bpm_User = _conf["Camunda:User"];
                mailMan.Bpm_Password = _conf["Camunda:Password"];

                mailMan.StatoIniziale = StatoAllegato.Attivo;
                try
                {
                    int s = (int)StatoAllegato.Attivo;
                    int.TryParse(Operation.parameters["attachment_status"].ToString(), out s);
                    mailMan.StatoIniziale = (StatoAllegato)s;
                }
                catch
                {
                }
                string Servers = Operation.parameters["server"].ToString();
                if (Servers == "*")
                    Servers = "";
                res = Task.Run(() =>  mailMan.ProcessaEmail(Servers, new System.Threading.CancellationToken())).Result;
            }
            catch (Exception ex)
            {
                res = false;
                _logger.LogError($"ProcessaPostaInArrivo: {ex.Message}");
            };
            return res;
        }

    }
}
