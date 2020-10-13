using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using dblu.Docs.Models;
using dblu.Docs.Classi;

namespace dbluMailService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger _logger;
        //private static readonly NLog.Logger _nlogger = NLog.LogManager.GetCurrentClassLogger();

        private IConfiguration _conf;
        public Worker(ILogger<Worker> logger, IConfiguration conf)
        {
            _logger = logger;
            _conf = conf;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await ProcessaPostaInArrivo(stoppingToken);
            }
        }


        private async Task ProcessaPostaInArrivo(CancellationToken stoppingToken)
            {

            try {

                int intervallo = _conf.GetValue<int>("Intervallo");

                var mailMan = new MailManager(_logger, _conf);
                mailMan.TipoAllegato = _conf.GetValue<string>("TipoAllegato");
                mailMan.Connessione =_conf.GetValue<string>("Connessione_dbluDocs");
                mailMan.Bpm_url = _conf.GetValue<string>("Camunda:Ip");
                mailMan.Bpm_User = _conf.GetValue<string>("Camunda:User");
                mailMan.Bpm_Password = _conf.GetValue<string>("Camunda:Password");

                _logger.LogDebug($"Processa server di posta");

                await mailMan.ProcessaEmail( stoppingToken);
                await Task.Delay(intervallo, stoppingToken);

            }
            catch (Exception ex){
                _logger.LogError($"Errore: {ex.Message}");
            };

        }


    }
}
