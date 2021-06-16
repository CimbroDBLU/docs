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
using System.Diagnostics;

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
                ElementiManager _ItemsManager = new ElementiManager(conf["ConnectionStrings:dblu.Docs"], log);


                log.LogInformation("MantenianceWorker.Engine: Started Manteniance service");
                if (stoppingToken.IsCancellationRequested) return;

                    //Recupera la lista dei tipi allegati
                    List<TipiAllegati> LTA = _AttachManager.GetAllTipiAllegati();
                    foreach (TipiAllegati TA in LTA.Where(x => x._listaCancellazioni.Count() != 0))
                        foreach (CleanSchedule CS in TA._listaCancellazioni)
                            if (!string.IsNullOrEmpty(CS.CronExp))
                                Schedules.Add(new dbluScheduledTask($"PURGE ATTACH TYPE: {TA.Codice} - STATE {CS.State}", CS.CronExp, () => { CleanUP_Attachments(TA.Codice, CS.State, CS.RetentionDays); }, log));

                    //Recupera la lista dei tipi elementi
                    List<TipiElementi> LTE = _ItemsManager.GetAllTipiElementi();
                    foreach (TipiElementi TE in LTE.Where(x => x._listaCancellazioni.Count() != 0))
                        foreach (CleanSchedule CS in TE._listaCancellazioni)
                            if (!string.IsNullOrEmpty(CS.CronExp))
                                Schedules.Add(new dbluScheduledTask($"PURGE ITEM TYPE: {TE.Codice} - STATE {CS.State}", CS.CronExp, () => { CleanUP_Items(TE.Codice, CS.State, CS.RetentionDays); }, log));

                    while (!stoppingToken.IsCancellationRequested)
                    {
                        Thread.Sleep(100);
                    }

                    foreach (dbluScheduledTask S in Schedules)
                        S.Stop();                      
            }
            catch(Exception Ex)
            {
                log.LogError($"MantenianceWorker.Engine: Unexpected exception {Ex}");
            }
        
        }

        /// <summary>
        /// EXternal method for requesting to syncronize again with DB, the scheduled cleans.
        /// </summary>
        public void Resync()
        {
            log.LogInformation("MantenianceWorker.Engine: Re-syncronizing for changes into rule tables");
            this.StopAsync(new System.Threading.CancellationToken());
            Task.WaitAll(Schedules.Select(x => x.WorkerTask).ToArray());
            this.StartAsync(new System.Threading.CancellationToken());
        }

        /// <summary>
        /// Procedure that phisically clean up the attachments once needed, according to retention date
        /// </summary>
        /// <param name="Code">Type of Attachments to delete</param>
        /// <param name="RetentionDays">Max retention days</param>
        private void CleanUP_Attachments(string Code,int State, int RetentionDays)
        {
            try
            {
                AllegatiManager _AttachManager = new AllegatiManager(conf["ConnectionStrings:dblu.Docs"], log);
                List<Allegati> LA = _AttachManager.GetAllegati(Code,State) ?? new();
                int tot = LA.Count();

                LA = LA.Where(d => ((DateTime.Now - d.DataUM).TotalDays > RetentionDays)).ToList();
                log.LogInformation($"MantenianceWorker.CleanUP_Attachments[{Code}]: Identified {LA.Count}/{tot} attachments to delete ");
                for (int i = 0; i < LA.Count; i++)
                {
                    try
                    {
                        log.LogInformation($"MantenianceWorker.CleaCleanUP_AttachmentsnUP[{Code}]: Deleting Attachment {i + 1}/{LA.Count} - {LA[i].Id}-{LA[i].Descrizione} ");
                    //    _AttachManager.Cancella(LA[i].Id);
                    }
                    catch (Exception Ex)
                    {
                        log.LogError($"MantenianceWorker.CleanUP_Attachments[{Code}]: Unable to delete Attachment {LA[i].Id}-{LA[i].Descrizione} ");
                        log.LogError($"MantenianceWorker.CleanUP_Attachments[{Code}]: Unable to delete Exception {Ex}");
                    }
                }
            } catch (Exception Ex)
            {
                log.LogError($"MantenianceWorker.CleanUP_Attachments[{Code}]: Unexpected exception {Ex}");
            }
        }

        /// <summary>
        /// Procedure that phisically clean up the attachments once needed, according to retention date
        /// </summary>
        /// <param name="Code">Type of Attachments to delete</param>
        /// <param name="RetentionDays">Max retention days</param>
        private void CleanUP_Items(string Code, int State, int RetentionDays)
        {
            try
            {
                ElementiManager _ItemsManager = new ElementiManager(conf["ConnectionStrings:dblu.Docs"], log);
                FascicoliManager _DossierManager = new FascicoliManager(conf["ConnectionStrings:dblu.Docs"], log);
                List<Elementi> LE = _ItemsManager.GetElementi(Code,State) ?? new();
                int tot = LE.Count();

                LE = LE.Where(d => (DateTime.Now - d.DataUM).TotalDays > RetentionDays).ToList();
                log.LogInformation($"MantenianceWorker.CleanUP_Items[{Code}]: Identified {LE.Count}/{tot} items to delete ");
                for (int i = 0; i < LE.Count; i++)
                {
                    try
                    {
                        log.LogInformation($"MantenianceWorker.CleanUP_Items[{Code}]: Deleting item {i + 1}/{LE.Count} - {LE[i].Id}-{LE[i].Descrizione} ");
                      //  _ItemsManager.Cancella(LE[i].Id,0);
                    }
                    catch (Exception Ex)
                    {
                        log.LogError($"MantenianceWorker.CleanUP_Items[{Code}]: Unable to delete item {LE[i].Id}-{LE[i].Descrizione} ");
                        log.LogError($"MantenianceWorker.CleanUP_Items[{Code}]: Unable to delete Exception {Ex}");
                    }
                }

                _DossierManager.CancellaFascicoliVuoti();
            }
            catch (Exception Ex)
            {
                log.LogError($"MantenianceWorker.CleanUP[{Code}]: Unexpected exception {Ex}");
            }
        }
    }
}
