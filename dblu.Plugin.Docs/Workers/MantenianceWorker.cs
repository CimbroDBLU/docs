using dblu.Portale.Plugin.Docs.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using dblu.Docs.Models;
using Microsoft.Extensions.DependencyInjection;
using dblu.Docs.Classi;
using dblu.Portale.Core.Infrastructure.Classes;

namespace dblu.Portale.Plugin.Docs.Workers
{
    /// <summary>
    /// Class for managin the clean up of old/already managed attachments
    /// </summary>
    public class MantenianceWorker : BackgroundService
    {
        /// <summary>
        /// Injected log inteface
        /// </summary>
        private readonly ILogger<MantenianceWorker> log;

        /// <summary>
        /// Injected configuration
        /// </summary>
        private readonly IConfiguration conf;

        /// <summary>
        /// Schedules activated
        /// </summary>
        private List<dbluScheduledTask> Schedules { get; set; } = new List<dbluScheduledTask>();

        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="logger">Injected log inteface</param>
        /// <param name="nConfiguration">Injected configuration</param>
        public MantenianceWorker(ILogger<MantenianceWorker> logger, IConfiguration nConfiguration)
        {
            log = logger;
            conf = nConfiguration;
        }

        /// <summary>
        /// Activate the Manteinance procedure
        /// </summary>
        /// <param name="stoppingToken">Token for force the closing of the procedure</param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _= Task.Run(() => Engine(stoppingToken));
            return;
        }


        /// <summary>
        /// Main procedure:
        ///     Activate a scheduler for each configured AttachmentType
        /// </summary>
        /// <param name="stoppingToken">Token for force the closing of the procedure</param>
        private void Engine(CancellationToken stoppingToken)
        {
            try
            {
                AllegatiManager _AttachManager = new AllegatiManager(conf["ConnectionStrings:dblu.Docs"], log);
                log.LogInformation("MantenianceWorker.Engine: Started Manteniance service");
                if (stoppingToken.IsCancellationRequested) return;

                //Recupera la lista dei tipi allegati
                List<TipiAllegati> LTA = _AttachManager.GetAllTipiAllegati();
                foreach (TipiAllegati TA in LTA.Where(x => !string.IsNullOrEmpty(x.CronPulizia)))
                    Schedules.Add(new dbluScheduledTask($"PURGE {TA.Codice}", TA.CronPulizia, () => { CleanUP(TA.Codice, TA.GiorniDaMantenere); }, log));

                while (!stoppingToken.IsCancellationRequested)
                    Thread.Sleep(100);

                foreach (dbluScheduledTask S in Schedules)
                    S.Stop();
            }catch(Exception Ex)
            {
                log.LogError($"MantenianceWorker.Engine: Unexpected exception {Ex}");
            }
        
        }

        /// <summary>
        /// Procedure that phisically clean up the attachments once needed, according to retention date
        /// </summary>
        /// <param name="Code">Type of Attachments to delete</param>
        /// <param name="RetentionDays">Max retention days</param>
        private void CleanUP(string Code,int RetentionDays)
        {
            try
            {
                AllegatiManager _AttachManager = new AllegatiManager(conf["ConnectionStrings:dblu.Docs"], log);
                List<Allegati> LA = _AttachManager.GetAllegati(Code) ?? new();
                int tot = LA.Count();

                LA = LA.Where(d => d.Stato > StatoAllegato.Chiuso && d.Stato > 0 && (DateTime.Now - d.DataUM).TotalDays > RetentionDays).ToList();
                log.LogInformation($"MantenianceWorker.CleanUP[{Code}]: Identified {LA.Count}/{tot} attachments to delete ");
                for (int i = 0; i < LA.Count; i++)
                {
                    try
                    {
                        log.LogInformation($"MantenianceWorker.CleanUP[{Code}]: Deleting Attachment {i + 1}/{LA.Count} - {LA[i].Id}-{LA[i].Descrizione} ");
                        _AttachManager.Cancella(LA[i].Id);
                    }
                    catch (Exception Ex)
                    {
                        log.LogError($"MantenianceWorker.CleanUP[{Code}]: Unable to delete Attachment {LA[i].Id}-{LA[i].Descrizione} ");
                        log.LogError($"MantenianceWorker.CleanUP[{Code}]: Unable to delete Exception {Ex}");
                    }
                }
            } catch (Exception Ex)
            {
                log.LogError($"MantenianceWorker.CleanUP[{Code}]: Unexpected exception {Ex}");
            }
        }


    }
}
