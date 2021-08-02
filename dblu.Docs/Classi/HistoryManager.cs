using Dapper;
using Dapper.Contrib.Extensions;
using dblu.Docs.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dblu.Docs.Classi
{
    /// <summary>
    /// Class for managing History  (CRUD operation on "Processi" and "Attivita"
    /// </summary>
    public class HistoryManager
    {
        /// <summary>
        /// Connection string to database
        /// </summary>
        private string ConnectionString;

        /// <summary>
        /// Log interface
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Star Weight for a process
        /// </summary>
        private int nWeightProcess { get; set; } = 5;

        /// <summary>
        /// Stat Weight for a task
        /// </summary>
        private int nWeightTask { get; set; } = 2;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="nConnectionString"> Connection string to database</param>
        /// <param name="logger">Log interface</param>
        public HistoryManager(string nConnectionString, ILogger logger)
        {
            ConnectionString = nConnectionString;
            _logger = logger;
        }

        /// <summary>
        /// Retreive the full list of processes into system
        /// </summary>
        /// <returns>List of all available processes</returns>
        public async Task<List<Processi>> GetAll()
        {
            List<Processi> Processes = new List<Processi>();
            try
            {
                Stopwatch sw = Stopwatch.StartNew();
                var query = "SELECT * FROM Processi LEFT OUTER JOIN Attivita ON Attivita.IdProcesso = Processi.Id";
                using (SqlConnection cn = new SqlConnection(ConnectionString))
                {
                    cn.Query<Processi, Attivita, Processi>(query, (z, m) =>
                    {
                        Processi Z = Processes.FirstOrDefault(d => d.Id == z.Id);
                        if (Z == null)
                        {
                            Processes.Add(z);
                            Z = Processes[Processes.Count - 1];
                        }
                        Z.Attivita.Add(m);
                        return Z;
                    });
                }

                foreach (Processi P in Processes)
                    P.Stars = nWeightProcess + nWeightTask * P.Attivita.Count;
                _logger.LogInformation($"HistoryManager.GetAll: Retreived {Processes.Count} Proceses in {sw.ElapsedMilliseconds} ms");
                return Processes;
            }catch(Exception ex)
            {
                _logger.LogError($"HistoryManager.GetAll: Unexpected exception {ex.Message}");
                return Processes;
            }
        }

        /// <summary>
        /// Get the process of specified ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns>The specified element</returns>
        public Processi Get(string id)
        {
            if (string.IsNullOrEmpty(id))  return null;
            try
            {
            Processi all = null;
            Stopwatch sw = Stopwatch.StartNew();
            
            using (SqlConnection cn = new SqlConnection(ConnectionString))           
                all = cn.Get<Processi>(id);

                if (all != null)
                {
                    all.Stars = nWeightProcess + nWeightTask * all.Attivita.Count;
                    _logger.LogInformation($"HistoryManager.Get: Retreived process {id} in {sw.ElapsedMilliseconds} ms");
                }
            return all;
            }
            catch (Exception ex)
            {
                _logger.LogError($"HistoryManager.Get: Unexpected exception {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get the activity of the specified Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>The specified elemen</returns>
        public Attivita GetActivity(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;

            Attivita Act = null;
            Stopwatch sw = Stopwatch.StartNew();

            try
            {
               
                using (SqlConnection cn = new SqlConnection(ConnectionString))
                    Act = cn.Get<Attivita>(id);
                _logger.LogInformation($"HistoryManager.GetActivity: Retreived activity {Act.Id} in {sw.ElapsedMilliseconds} ms");
            }              
            catch (Exception ex)
            {
                _logger.LogError($"HistoryManager.GetActivity: Unexpected exception {ex.Message}");
                return null;
            }

            return Act;
        }

        /// <summary>
        /// Get the Actvities of a process
        /// </summary>
        /// <param name="id">Id of the process</param>
        /// <returns>
        /// List of the available processes
        /// </returns>
        public List<Attivita> GetProcessActivities(string id)
        {
            if (string.IsNullOrEmpty(id)) return new List<Attivita>();

            List<Attivita> l = new List<Attivita>();
            Stopwatch sw = Stopwatch.StartNew();
          
            try
            {
                using (SqlConnection cn = new SqlConnection(ConnectionString))
                    l = cn.Query<Attivita>($"Select * FROM Attivita where IdProcesso=@IdProcesso", new { IdProcesso=id }).ToList();
                _logger.LogInformation($"HistoryManager.GetProcessActivities: Retreived {l.Count} actvities in {sw.ElapsedMilliseconds} ms");
            }
            catch (Exception ex)
            {
                _logger.LogError($"HistoryManager.GetProcessActivities: Unexpected exception {ex.Message}");
            }
            return l;
        }

        /// <summary>
        /// Get the last change on processes
        /// </summary>
        /// <returns>
        /// The date and time abuot the last change on processes
        /// </returns>
        public DateTime? GetLastProcessTime()
        {
            try
            {
                DateTime? dt = DateTime.MinValue;
                Stopwatch sw = Stopwatch.StartNew();
                using (SqlConnection cn = new SqlConnection(ConnectionString))
                    dt = cn.Query<DateTime?>("SELECT MAX(DataUM) from Processi").FirstOrDefault();
                _logger.LogInformation($"HistoryManager.GetLastProcessTime: Retreived {dt} in {sw.ElapsedMilliseconds} ms");
                return dt;
            }
            catch(Exception ex)
            {
                _logger.LogError($"HistoryManager.GetLastProcessTime: Unexpected exception {ex.Message}");
                return DateTime.MinValue;
            }
        }

        /// <summary>
        /// Save the specified process
        /// </summary>
        /// <param name="obj">Process that need to be saved</param>
        /// <returns>
        /// True if the process is properly saved
        /// </returns>
        public bool Save(Processi obj)
        {
            var bres = false;
            try
            {
                Stopwatch sw = Stopwatch.StartNew();
                using (SqlConnection cn = new SqlConnection(ConnectionString))
                {
                    var c = cn.Get<Processi>(obj.Id);
                    if (c == null)
                    {
                        bres = cn.Insert<Processi>(obj) > 0;
                        _logger.LogInformation($"HistoryManager.Save: Create process {obj.Id} in {sw.ElapsedMilliseconds} ms");
                    }
                    else
                    {
                        cn.Update<Processi>(obj);
                        _logger.LogInformation($"HistoryManager.Save: Update process {obj.Id} in {sw.ElapsedMilliseconds} ms");
                    }
                    bres = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"HistoryManager.Save: Unexpected exception  {ex.Message}");
            }
            return bres;
        }

        /// <summary>
        /// Save the specified actvity
        /// </summary>
        /// <param name="obj">Activity that need to be saved</param>
        /// <returns>
        /// True if activity has been properly saved
        /// </returns>
        public bool SaveActivity(Attivita obj)
        {
            var bres = false;
            try
            {
                Stopwatch sw = Stopwatch.StartNew();
                using (SqlConnection cn = new SqlConnection(ConnectionString))
                {
                    var c = cn.Get<Attivita>(obj.Id);
                    if (c == null)
                    {
                        bres = cn.Insert<Attivita>(obj) > 0;
                        _logger.LogInformation($"HistoryManager.SaveActivity: Create activity {obj.Id} in {sw.ElapsedMilliseconds} ms");
                    }
                    else
                    {
                        cn.Update<Attivita>(obj);
                        _logger.LogInformation($"HistoryManager.SaveActivity: Update activity {obj.Id} in {sw.ElapsedMilliseconds} ms");
                    }
                    bres = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"HistoryManager.SaveActivity: Unexpected exception  {ex.Message}");
            }
            return bres;
        }
    }
}
