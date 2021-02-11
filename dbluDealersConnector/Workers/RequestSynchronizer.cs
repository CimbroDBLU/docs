﻿using NCrontab;
using dblu.Docs.Classi;
using dblu.Docs.Models;
using dbluDealersConnector.Classes;
using dbluDealersConnector.DealersAPI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace dblu.Docs.Service
{

    /// <summary>
    /// Class that synchronize dbludealer request into dbludocs (in order to make them processed by JobAID)
    /// </summary>
    public class RequestSynchronizer : BackgroundService
    {
        /// <summary>
        /// Injected configuration
        /// </summary>
        private readonly IConfiguration conf;
        /// <summary>
        /// Injected logger
        /// </summary>
        private readonly ILogger<RequestSynchronizer> log;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger">Injected logger</param>
        /// <param name="nConf">Injected configuration</param>
        public RequestSynchronizer(ILogger<RequestSynchronizer> logger,IConfiguration nConf )
        {
            log = logger;
            conf = nConf; 
        }

        /// <summary>
        /// Procedure that periodically scan the dbluDealers and move forward the pending request
        /// </summary>
        /// <param name="stoppingToken">Stopping Token for a fast\safe stop of the procedure</param>
        /// <returns>
        /// The task done
        /// </returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(1000, stoppingToken);
            if (conf["cronschedule"].Length == 0)
            {
                log.LogInformation($"RequestWorker.ExecuteAsync: No sync required");
                return;
            }

            log.LogInformation($"RequestWorker.ExecuteAsync: Sync procedure with this schedule {conf["cronschedule"]}");
            DateTime LastOccurency = DateTime.Now;
            CrontabSchedule Scheduler = CrontabSchedule.Parse(conf["cronschedule"], new CrontabSchedule.ParseOptions() { IncludingSeconds = true }); ;
            while (!stoppingToken.IsCancellationRequested)
            {
                DateTime T = Scheduler.GetNextOccurrence(LastOccurency).ToUniversalTime();
                if (Math.Abs((DateTime.UtcNow - T).TotalMilliseconds) < 1000)
                    {                    
                    log.LogInformation($"RequestSynchronizer.ExecuteAsync: >>>> Execution planned for {T}");
                    Engine();
                    log.LogInformation($"RequestSynchronizer.ExecuteAsync: <<<< ");
                    LastOccurency = T;
                    }
                await Task.Delay(100, stoppingToken);
            }
        }

        /// <summary>
        /// Retrive and if necessary create a REQ Attach Type
        /// </summary>
        /// <returns>
        /// The REQ attach type.
        /// </returns>
        private TipiAllegati GetAttachType()
        {
            AllegatiManager AM = new AllegatiManager(conf["dbluDocs:db"], log);
            TipiAllegati C = AM.GetTipoAllegato("REQ");
            if (C != null)
                return C;

            C = new TipiAllegati() { Codice = "REQ",Descrizione="dbluDealers request", Cartella="",Estensione="ZIP"  };
            C.Attributi = new ElencoAttributi();
            C.Attributi.Add(new Attributo() { Nome = "Tipo", Descrizione = "Tipo di richiesta", Alias = "", Obbligatorio = false, Tipo = "System.String", Valore = "", SystemType = Type.GetType("System.String"), Visibilità = Visibilita_Attributi.VISIBLE, ValorePredefinito = "", Sequenza = "1" });
            C.Attributi.Add(new Attributo() { Nome = "Testo", Descrizione = "Testo", Alias = "", Obbligatorio = false, Tipo = "System.String", Valore = "", SystemType = Type.GetType("System.String"), Visibilità = Visibilita_Attributi.VISIBLE, ValorePredefinito = "", Sequenza = "2" });
            C.Attributi.Add(new Attributo() { Nome = "Descrizione", Descrizione = "Descrizione", Alias = "", Obbligatorio = false, Tipo = "System.String", Valore = "", SystemType = Type.GetType("System.String"), Visibilità = Visibilita_Attributi.VISIBLE, ValorePredefinito = "", Sequenza = "3" });
            C.Attributi.Add(new Attributo() { Nome = "ElencoFile", Descrizione = "Lista di nome files e relativa dimensione separati da |", Alias = "", Obbligatorio = false, Tipo = "System.String", Valore = "", SystemType = Type.GetType("System.String"), Visibilità = Visibilita_Attributi.VISIBLE, ValorePredefinito = "", Sequenza = "4" });
            C.Attributi.Add(new Attributo() { Nome = "Reference", Descrizione = "Riferimenti", Alias = "", Obbligatorio = false, Tipo = "System.String", Valore = "", SystemType = Type.GetType("System.String"), Visibilità = Visibilita_Attributi.VISIBLE, ValorePredefinito = "", Sequenza = "5" });
            C.Attributi.Add(new Attributo() { Nome = "RefYear", Descrizione = "Anno ordine eventuale di partenza", Alias = "", Obbligatorio = false, Tipo = "System.String", Valore = "", SystemType = Type.GetType("System.String"), Visibilità = Visibilita_Attributi.VISIBLE, ValorePredefinito = "", Sequenza = "6" });
            C.Attributi.Add(new Attributo() { Nome = "RefNumber", Descrizione = "Numero ordine eventuale di partenza", Alias = "", Obbligatorio = false, Tipo = "System.String", Valore = "", SystemType = Type.GetType("System.String"), Visibilità = Visibilita_Attributi.VISIBLE, ValorePredefinito = "", Sequenza = "7" });
            C.Attributi.Add(new Attributo() { Nome = "RefItemId", Descrizione = "Elemento ordine eventuale di partenza", Alias = "", Obbligatorio = false, Tipo = "System.Guid", Valore = "", SystemType = Type.GetType("System.Guid"), Visibilità = Visibilita_Attributi.VISIBLE, ValorePredefinito = "", Sequenza = "8" });
            C.Attributi.Add(new Attributo() { Nome = "RefDossierId", Descrizione = "Fascicolo ordine eventuale di partenza", Alias = "", Obbligatorio = false, Tipo = "System.Guid", Valore = "", SystemType = Type.GetType("System.Guid"), Visibilità = Visibilita_Attributi.VISIBLE, ValorePredefinito = "", Sequenza = "9" });
            AM.SalvaTipoAllegato(C);
            return C;

        }

        /// <summary>
        /// Procedure that scan the dbluDealers and move forward the pending requests, if any
        /// </summary>
        private async void Engine()
        {
            try
            {
                DealersClient DC = new DealersClient(new Uri(conf["dbluDealer:Url"]));
                AllegatiManager AM = new AllegatiManager(conf["dbluDocs:db"], log);
                ElementiManager EM = new ElementiManager(conf["dbluDocs:db"], log);

                try
                {
                    log.LogInformation($"RequestSynchronizer.Engine: Connecting dbludocs @ {conf["dbluDocs:db"]}");
                    using (SqlConnection cn = new SqlConnection(conf["dbluDocs:db"]))
                        cn.Open();
                    log.LogInformation($"RequestSynchronizer.Engine: Connected");

                }
                catch (Exception)
                {
                    log.LogWarning($"RequestSynchronizer.Engine: Connection to dbludocs @ {conf["dbluDocs:db"]} failed");
                    return;
                }

                log.LogInformation($"RequestSynchronizer.Engine: Connecting dbludealers @ {conf["dbluDealer:Url"]}");
                int Res = DC.Login(conf["dbluDealer:Tenant"], conf["dbluDealer:Login"], conf["dbluDealer:Password"]);
                if (Res == 0)
                    log.LogInformation($"RequestSynchronizer.Engine: Connected");
                else
                {
                    log.LogWarning($"RequestSynchronizer.Engine: Connection to dbludealers @ {conf["dbluDealer:Url"]} failed");
                    return;
                }


                List<DealersRequest> PR = DC.PendingRequest();
                log.LogInformation($"RequestSynchronizer.Engine: Read {PR.Count} Pending request");
                if (PR.Count == 0)
                {
                    log.LogInformation($"RequestSynchronizer.Engine: Well, no pending request: nothing to do man!");
                    return;
                }

                List<DealersRequest> ReadyList = PR.Where(x => x.State == RequestState.Ready).ToList();
                if (ReadyList.Count == 0)
                    log.LogInformation($"RequestSynchronizer.Engine: There's no READY requests");

                foreach (DealersRequest R in ReadyList)
                {
                    log.LogInformation($"RequestSynchronizer.Engine: Request: [{R.Id}] [{R.Descrizione}] is READY and has to be stored in Docs");

                    MemoryStream M = await DC.GetDocument(R.NomeFile);
                    Allegati A = new Allegati
                    {
                        NomeFile = R.NomeFile,
                        Origine = "dbluDealers",
                        Testo = R.Testo,
                        Tipo = "FILE",
                        TipoNavigation = GetAttachType(),
                        Stato = StatoAllegato.Attivo,
                        Descrizione = R.Descrizione
                    };
                    A.elencoAttributi = A.TipoNavigation.Attributi;
                    A.SetAttributo("Tipo", R.Tipo.ToString());
                    A.SetAttributo("Testo", R.Testo);
                    A.SetAttributo("Descrizione", R.Descrizione);
                    A.SetAttributo("ElencoFile", R.ElencoFile);
                    A.SetAttributo("Reference", R.Reference);

                    A.SetAttributo("RefYear", R.RefYear);
                    A.SetAttributo("RefNumber", R.RefNumber);
                    A.SetAttributo("RefItemId", R.RefItemId);
                    A.SetAttributo("RefDossierId", R.RefDossierId);

                    if (R.NomeFile == "")
                    {
                        log.LogWarning($"RequestSynchronizer.Engine: Request: [{R.Id}] has no attachments");
                        AM.Salva(A, true);
                    }
                    else await AM.SalvaAsync(A, M, true);

                    log.LogInformation($"RequestSynchronizer.Engine: Created Attachment for Request: [{R.Id}] Attachment: {A.Id}-{A.NomeFile}");

                    await DC.ChangeState(R.Id, RequestState.Processing);
                    log.LogInformation($"RequestSynchronizer.Engine: Saved Attachment [{A.Id}] [{A.NomeFile}]");
                }

                List<DealersRequest> ToMoveForwardList = PR.Where(x => x.State != RequestState.Ready && x.State != RequestState.Preparing).ToList();
                if (ToMoveForwardList.Count == 0)
                    log.LogInformation($"RequestSynchronizer.Engine: There's no ACTIVE requests");

                foreach (DealersRequest R in ToMoveForwardList)
                {
                    log.LogInformation($"RequestSynchronizer.Engine: ACTIVE Request: [{R.Id}] [{R.Descrizione}] is under monitoring");

                    Allegati A = AM.CercaAllegati(new Allegati { Id = Guid.Empty, NomeFile = R.NomeFile }).FirstOrDefault();
                    if (A == null)
                    {
                        log.LogWarning($"RequestSynchronizer.Engine: Attachment [R.NomeFile] not found in archive, it has been lost?");
                        continue;
                    }

                    RequestState Next = A.Stato.ToRequestState();

                    if (R.State != Next)
                    {
                        if (Next == RequestState.Closed || Next == RequestState.Aborted)
                        {
                            log.LogInformation($"RequestSynchronizer.Engine: Request: [{R.Id}] needs to be syncronized");
                            //recupero id elementi eventualmente generati, se il task e' chiuso.
                            List<Elementi> Items = EM.GetElementiDaAllegato(A.Id);
                            List<DealersRequestReferences> lst = new List<DealersRequestReferences>();
                            foreach (Elementi e in Items)
                                lst.Add(new DealersRequestReferences() { RequestId = R.Id, DossierId = e.IdFascicolo, ItemId = e.Id });

                            int ItemCount = await DC.SyncReferences(R.Id, lst);
                            log.LogInformation($"RequestSynchronizer.Engine: Request: [{R.Id}] has now {ItemCount} references");
                        }

                        log.LogInformation($"RequestSynchronizer.Engine: Request: [{R.Id}] has move from {R.State} to {Next}");

                        await DC.ChangeState(R.Id, Next);

                    }
                    else
                    {
                        log.LogInformation($"RequestSynchronizer.Engine: ACTIVE Request: [{R.Id}] is stable in state {Next}");
                    }
                }
            }catch(Exception Ex)
            {
                log.LogInformation($"RequestSynchronizer.Engine: Unexpected exception {Ex}");
            }
         }
    }
}