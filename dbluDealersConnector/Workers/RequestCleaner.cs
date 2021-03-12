using dbluDealersConnector.DealersAPI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NCrontab;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace dbluDealersConnector.Workers
{
    /// <summary>
    /// Class tha in background cleans the old requests
    /// </summary>
    public class RequestCleaner : BackgroundService
    {
        /// <summary>
        /// Injected configuration
        /// </summary>
        private readonly IConfiguration conf;
        /// <summary>
        /// Injected logger
        /// </summary>
        private readonly ILogger<RequestCleaner> log;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger">Injected logger</param>
        /// <param name="nConf">Injected configuration</param>
        public RequestCleaner(ILogger<RequestCleaner> logger, IConfiguration nConf)
        {
            log = logger;
            conf = nConf;
        }

        /// <summary>
        /// Procedure that periodically scan the dbluDealers and remove closed requests
        /// </summary>
        /// <param name="stoppingToken">Stopping Token for a fast\safe stop of the procedure</param>
        /// <returns>
        /// The task done
        /// </returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (conf["cronpurge"] == null || conf["cronpurge"].Length == 0)
            {
                log.LogInformation($"RequestCleaner.ExecuteAsync: No purge required");
                return;
            }

            log.LogInformation($"RequestCleaner.ExecuteAsync: Sync procedure with this schedule {conf["cronpurge"]}");
            DateTime LastOccurency = DateTime.Now;
            CrontabSchedule Scheduler = CrontabSchedule.Parse(conf["cronpurge"], new CrontabSchedule.ParseOptions() { IncludingSeconds = true }); ;
            while (!stoppingToken.IsCancellationRequested)
            {
                DateTime T = Scheduler.GetNextOccurrence(LastOccurency).ToUniversalTime();
                if (Math.Abs((DateTime.UtcNow - T).TotalMilliseconds) < 100)
                {
                    log.LogInformation($"RequestCleaner.ExecuteAsync: >>>> Execution planned for {T.ToLocalTime()}");
                    Engine();
                    log.LogInformation($"RequestCleaner.ExecuteAsync: <<<< ");
                    LastOccurency = T;
                }
                await Task.Delay(50, stoppingToken);
            }
        }

        /// <summary>
        /// Engine that will drop expired requests
        /// </summary>
        private async void Engine()
        {
            bool.TryParse(conf["dbluDealer:allow_unsigned_https"], out bool allow);
            string DealersUriString = conf["dbluDealer:Url"];
            if (!DealersUriString.EndsWith("/"))
                DealersUriString += "/";
            DealersClient DC = new DealersClient(new Uri(DealersUriString), allow);

            log.LogInformation($"RequestCleaner.Engine: Connecting dbludealers @ {conf["dbluDealer:Url"]}");
            int Res = DC.Login(conf["dbluDealer:Tenant"], conf["dbluDealer:Login"], conf["dbluDealer:Password"]);
            if (Res == 0)
                log.LogInformation($"RequestCleaner.Engine: Connected");
            else
            {
                log.LogWarning($"RequestCleaner.Engine: Connection to dbludealers @ {conf["dbluDealer:Url"]} failed");
                return;
            }


            List<DealersRequest> PR = DC.ClosedRequests();
            log.LogInformation($"RequestCleaner.Engine: Read {PR.Count} closed requests");
            if (PR.Count == 0)
            {
                log.LogInformation($"RequestCleaner.Engine: Well, no closed requests: nothing to do man!");
                return;
            }

            int.TryParse(conf["purge_retention_days"], out int RetentionDays);
            if (RetentionDays <= 0) RetentionDays = 30;
            List<Guid> ToDelete = PR.Where(x => x.LastModificationTime != null && (DateTime.Now - (DateTime)x.LastModificationTime).TotalDays > RetentionDays).Select(y => y.Id).ToList();
            log.LogInformation($"RequestCleaner.Engine: There are {ToDelete.Count} request to purge ");

            List<List<string>> Slices = new List<List<string>>();
            for (int i = 0; i < ToDelete.Count; i += 10)
                Slices.Add(ToDelete.Skip(i).Take(10).Select(x=>x.ToString()).ToList());

            foreach (List<string> Slice in Slices)
                await DC.PurgeRequests(Slice);

            log.LogInformation($"RequestCleaner.Engine: Purged!");
        }
    }
}
