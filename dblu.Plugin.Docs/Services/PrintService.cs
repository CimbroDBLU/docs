
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
            try
            {
                string ServerName = _config["PrintServer:Server"];
                string PrinterName = _config["PrintServer:Printer"];
                if (nPrinterName != null)
                    PrinterName = nPrinterName;

                RestClient restClient = new RestClient(ServerName);
                RestRequest restRequest = new RestRequest("api/PrinterServer");
                restRequest.RequestFormat = DataFormat.Json;
                restRequest.Method = Method.POST;
                restRequest.AddHeader("Content-Type", "multipart/form-data");
                restRequest.AddFile("File", FileName);
                restRequest.AddParameter("Printer", PrinterName, ParameterType.RequestBody);
               
                var response = restClient.Execute(restRequest);
                if (response.Content == "0")
                {
                    _logger.LogInformation($"PrintService.AddJob: Sent print {FileName} to {PrinterName} from {User}");
                    return 0;

                }
                _logger.LogError($"PrintService.AddJob: Print {FileName} to {PrinterName} from {User} not worked! Service is off? ");
                return 1;
            }catch(Exception ex)
            {
                _logger.LogError($"PrintService.AddJob: Print {FileName} from {User} not worked! Exception:{ex} ");
                return 2;
            }
        }
    }
}
