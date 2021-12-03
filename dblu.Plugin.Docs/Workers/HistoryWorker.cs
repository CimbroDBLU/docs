using dblu.Docs.Classi;
using dblu.Docs.Models;
using dblu.Portale.Core.Infrastructure.Classes;
using BPMClient.DataLayer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace dblu.Portale.Plugin.Docs.Workers
{
    public class HistoryWorker : BackgroundService
    {


        /// <summary>
        /// Injected log inteface
        /// </summary>
        private readonly ILogger<HistoryWorker> log;

        /// <summary>
        /// Injected configuration
        /// </summary>
        private readonly IConfiguration conf;

        /// <summary>
        /// Parallel task for exucuting job scheduled by cron
        /// </summary>
        private dbluScheduledTask SyncTask { get; set; }

        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="logger">Injected log inteface</param>
        /// <param name="nConfiguration">Injected configuration</param>
        public HistoryWorker(ILogger<HistoryWorker> logger, IConfiguration nConfiguration)
        {
            log = logger;
            conf = nConfiguration;
        }

        /// <summary>
        /// Activate the History importing procedure
        /// </summary>
        /// <param name="stoppingToken">Token for force the closing of the procedure</param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _ = Task.Run(() => Engine(stoppingToken));
            return;
        }

        /// <summary>
        /// Main procedure:
        ///     Activate a task for import camunda history time by time
        /// </summary>
        /// <param name="stoppingToken">Token for force the closing of the procedure</param>
        private void Engine(CancellationToken stoppingToken)
        {
            try
            {

                log.LogInformation("HistoryWorker.Engine: Started Camunda History Sync service");
                if (stoppingToken.IsCancellationRequested) return;

                SyncTask = new dbluScheduledTask("CAMUNDA_HISTORY", conf["Docs:CamundaHistoryCron"], () => { Syncronizze(); }, log);

                while (!stoppingToken.IsCancellationRequested)
                    Thread.Sleep(100);

                SyncTask.Stop();
                SyncTask = null;
            }                        
            catch(Exception Ex)
            {
                log.LogError($"HistoryWorker.Engine: Unexpected exception {Ex}");
            }
        }

        /// <summary>
        /// Sincronized task from CAMUNDA.
        /// Import Processe and eventually task only if they are new according to last data saved into db. 
        /// </summary>
        private void Syncronizze()
        {
            try
            {
            HistoryManager History = new HistoryManager(conf["ConnectionStrings:dblu.Docs"], log);
            Stopwatch sw = Stopwatch.StartNew();

            DateTime? LastImport = History.GetLastProcessTime();
            if (LastImport is null)
                LastImport = DateTime.MinValue;

            using CAMContext dbCAM = new CAMContext(conf["ConnectionStrings:CamundaDbConnection"]);
            List<CAMHiProcess> nProcesses = dbCAM.HistoryProcesses.Include(c => c.Definition).Include(t => t.Tasks).Include(v=>v.Variables).Where(x=>x.START_TIME_>LastImport || x.END_TIME_> LastImport || x.Tasks.Any(x => x.START_TIME_ > LastImport || x.END_TIME_ > LastImport) ).AsNoTracking().ToList();

            if (nProcesses.Count == 0)
                log.LogInformation($"HistoryWorker.Syncronizze: No changes detected in {sw.ElapsedMilliseconds} ms");
            else
            {
                foreach (CAMHiProcess CP in nProcesses)
                {
                    Processi P = null;
                    P = History.Get(CP.ID_);
                    if (P == null) P = new();

                    P.Id = CP.ID_;
                    P.Nome = CP.Definition?.NAME_ ?? CP.PROC_DEF_KEY_;
                    P.Stato = CP.STATE_;
                    P.UtenteAvvio = CP.START_USER_ID_;
                    P.Versione = CP.Definition?.VERSION_TAG_ ?? "";
                    P.Diagramma = CP.Definition?.RESOURCE_NAME_ ?? "";
                    P.Descrizione = CP.Definition?.NAME_ ?? "";
                    P.Avvio = CP.START_TIME_;
                    P.Fine = CP.END_TIME_;
                    if (P.DataC is null) P.DataC = DateTime.Now;
                    P.DataUM = DateTime.Now;

                    P.IdAllegato = CP.Variables.Where(x => x.NAME_.ToUpper().Contains("IDALLEGATO"))?.FirstOrDefault()?.TEXT_??"";
                    P.IdElemento = CP.Variables.Where(x => x.NAME_.ToUpper().Contains("IDELEMENTO"))?.FirstOrDefault()?.TEXT_ ?? "";

                    History.Save(P);

                    log.LogInformation($"HistoryWorker.Syncronizze: Updated process {P.Id}");

                    foreach (CAMHiTask CT in CP.Tasks)
                    {
                        Attivita A = null;
                        A = History.GetActivity(CT.ID_);
                        if (A == null) A = new();

                        A.Id = CT.ID_;
                        A.Nome = CT.NAME_;
                        A.Descrizione = CT.DESCRIPTION_;
                        A.Stato = CT.DELETE_REASON_;
                        A.Avvio = CT.START_TIME_;
                        A.Fine = CT.END_TIME_;
                        A.Assegnatario = CT.ASSIGNEE_;
                        A.IdProcesso = CT.Process.ID_;
                        if (A.DataC is null) A.DataC = DateTime.Now;
                        A.DataUM = DateTime.Now;
                        History.SaveActivity(A);
                        log.LogInformation($"HistoryWorker.Syncronizze: Updated activity {A.Id}");
                    }

                }

                log.LogInformation($"HistoryWorker.Syncronizze: Syncronized {nProcesses.Count} processes in {sw.ElapsedMilliseconds} ms");
            }
            }
            catch (Exception Ex)
            {
                log.LogError($"HistoryWorker.Syncronizze: Unexpected exception {Ex}");
            }

        }
    }


}
