using NCrontab;
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

namespace dblu.Docs.Service
{
    public class RequestWorker : BackgroundService
    {
        private readonly ILogger<RequestWorker> log;
        private readonly IConfiguration conf;

        public RequestWorker(ILogger<RequestWorker> logger,IConfiguration nConf )
        {
            log = logger;
            conf = nConf; 
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(1000, stoppingToken);
            log.LogInformation($"RequestWorker.ExecuteAsync: Remote procedure with this schedule {conf["cronschedule"]}");
            DateTime LastOccurency = DateTime.Now;
            CrontabSchedule Scheduler = CrontabSchedule.Parse(conf["cronschedule"], new CrontabSchedule.ParseOptions() { IncludingSeconds = true }); ;
            while (!stoppingToken.IsCancellationRequested)
            {
                DateTime T = Scheduler.GetNextOccurrence(LastOccurency).ToUniversalTime();
                if (Math.Abs((DateTime.UtcNow - T).TotalMilliseconds) < 1000)
                    {                    
                    log.LogInformation($"RequestWorking.ExecuteAsync: >>>> Execution planned for {T}");
                    Engine();
                    log.LogInformation($"RequestWorking.ExecuteAsync: <<<< ");
                    LastOccurency = T;
                    }
                await Task.Delay(100, stoppingToken);
            }
        }

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
            AM.SalvaTipoAllegato(C);
            return C;

        }

        private async void Engine()
        {
            DealersClient DC = new DealersClient(new Uri(conf["dbluDealer:Url"]));
            AllegatiManager AM = new AllegatiManager(conf["dbluDocs:db"], log);
            ElementiManager EM = new ElementiManager(conf["dbluDocs:db"], log);
       
            log.LogInformation($"RequestWorking.Engine: Connecting dealer {conf["dbluDealer:Url"]}");
            int Res = DC.Login(conf["dbluDealer:Tenant"], conf["dbluDealer:Login"], conf["dbluDealer:Password"]);
            if (Res == 0)
                log.LogInformation($"RequestWorking.Engine: Connected");
            else
            {
                log.LogWarning($"RequestWorking.Engine: Connection to dealer {conf["dbluDealer:Url"]} failed");
                return;
            }


            List<DealersRequest> PR = DC.PendingRequest();
            log.LogInformation($"RequestWorking.Engine: Read {PR.Count} Pending request");
            if (PR.Count == 0)
            {
                log.LogInformation($"RequestWorking.Engine: Well, no pending request: nothing to do man!");
                return;
            }

            List<DealersRequest> ReadyList = PR.Where(x => x.State == RequestState.Ready).ToList();
            if (ReadyList.Count == 0)
                log.LogInformation($"RequestWorking.Engine: There's no READY requests");

            foreach (DealersRequest R in ReadyList)
            {
                log.LogInformation($"RequestWorking.Engine: Request: {R.Id}-{R.Descrizione} has to be stored in Docs");
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

                await AM.SalvaAsync(A, M, true);
                log.LogInformation($"RequestWorking.Engine: Created Attachment for Request: {R.Id} Attachment: {A.Id}-{A.NomeFile}");

                await DC.ChangeState(R.Id, RequestState.Processing);
                log.LogInformation($"RequestWorking.Engine: Saved Attachment {A.Id}-{A.NomeFile}");
            }

            List<DealersRequest> ToMoveForwardList = PR.Where(x => x.State != RequestState.Ready && x.State != RequestState.Preparing).ToList();
            if (ToMoveForwardList.Count == 0)
                log.LogInformation($"RequestWorking.Engine: There's no ACTIVE requests");

            foreach (DealersRequest R in ToMoveForwardList)
            {
                log.LogInformation($"RequestWorking.Engine: Pending Request: {R.Id}-{R.Descrizione} has to be monitored");

                Allegati A=AM.CercaAllegati(new Allegati { Id=Guid.Empty, NomeFile = R.NomeFile }).FirstOrDefault();
                if(A==null)
                {
                    log.LogWarning($"RequestWorking.Engine: Attachment {R.NomeFile} not found in archive, it has been lost?");
                    continue;
                }

                RequestState Next = A.Stato.ToRequestState(); 

                if(R.State!=Next)
                {
                    if (Next == RequestState.Closed || Next == RequestState.Aborted)
                    {
                        log.LogInformation($"RequestWorking.Engine: Request: {R.Id} needs to be syncronized"); 
                        //recupero id elementi eventualmente generati, se il task e' chiuso.
                        List <Elementi> Items = EM.GetElementiDaAllegato(A.Id);
                        List<DealersRequestReferences> lst = new List<DealersRequestReferences>();
                        foreach (Elementi e in Items)
                            lst.Add(new DealersRequestReferences() { RequestId = R.Id, DossierId = e.IdFascicolo, ItemId = e.Id });

                        int ItemCount = await DC.SyncReferences(R.Id, lst);
                        log.LogInformation($"RequestWorking.Engine: Request: {R.Id} has now {ItemCount} references");
                    }

                    log.LogInformation($"RequestWorking.Engine: Request: {R.Id} has move from {R.State} to {Next}");

                    await DC.ChangeState(R.Id, Next);

                }
            }
        }
    }
}