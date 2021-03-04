using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NLog.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dblu.Docs.Service
{
    /// <summary>
    /// Program class
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ContractResolver = new DefaultContractResolver()
            };

            CreateHostBuilder(args).Build().Run();
        }

        /// <summary>
        /// Create the web server
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)                
             .ConfigureLogging((logging) =>
             {
                 logging.ClearProviders();
             }).UseNLog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    
                });
    }
}
