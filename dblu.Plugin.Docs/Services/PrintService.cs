
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace dblu.Portale.Plugin.Docs.Services
{
    /// <summary>
    /// Service for printing on a remote (serverside) printer
    /// </summary>
    public class PrintService
    {
        /// <summary>
        /// Injected configuration reader
        /// </summary>
        public readonly IConfiguration _config;
        
        /// <summary>
        /// Injected logger writer
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="config">Injected configuration reader</param>
        /// <param name="loggerFactory">Injected logger writer</param>
        public PrintService(IConfiguration config, ILoggerFactory loggerFactory)
        {
            _config = config;
            _logger = loggerFactory.CreateLogger("PrintService");
        }

        /// <summary>
        /// Queue a print job into printer
        /// </summary>
        /// <param name="User">User that requet the print</param>
        /// <param name="FileName">Name of the file to print out</param>
        /// <param name="nPrinterName">Name of the printer</param>
        /// <returns>
        /// Add a print job for this remote printer
        /// </returns>
        public async Task<int> AddJob(string User,string FileName,string nPrinterName="")
        {
            string ServerName = _config["PrintServer:Server"];
            string PrinterName = _config["PrintServer:Printer"];
            if (nPrinterName != null)
                PrinterName = nPrinterName;


            _logger.LogInformation($"PrintService.AddJob: User:{User} is printing {FileName} on {PrinterName}");

            Process objP = new Process();
            objP.StartInfo.FileName = "lpr";
            objP.StartInfo.Arguments = $" -S {ServerName} -P \"{PrinterName}\" -o l \"{FileName}\"";
            objP.StartInfo.RedirectStandardOutput = true;
            objP.StartInfo.UseShellExecute = false;
            objP.StartInfo.RedirectStandardError = true;
            objP.StartInfo.CreateNoWindow = true;
            objP.Start();
            objP.WaitForExit();

            if (objP.ExitCode == 0)
                _logger.LogInformation($"PrintService.AddJob: Printing of {FileName} on {PrinterName} has been finished");
            else
            {
                _logger.LogWarning($"PrintService.AddJob: Printing of {FileName} on {PrinterName} FAILED");
                _logger.LogWarning($"PrintService.AddJob: Ex {objP.StandardOutput.ReadToEnd()}");
            }
            return objP.ExitCode;
        }
    }
}
