#if Framework48
    using dblu.Docs.Extensions;
#else
    using dbluTools.Extensions;
#endif
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
            SqlMapper.AddTypeHandler(typeof(ExtAttributesTypeHandler), new ExtAttributesTypeHandler());
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
                        if(m!=null)
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
        /// Gett all process by element ID
        /// </summary>
        /// <param name="id">Id of the element/param>
        /// <returns>
        /// List of projects related to this element
        /// </returns>
        public async Task<List<Processi>> GetAllByElementID(string id)
        {
            List<Processi> Processes = new List<Processi>();
            try
            {
                Stopwatch sw = Stopwatch.StartNew();
                var query = "SELECT * FROM Processi LEFT OUTER JOIN Attivita ON Attivita.IdProcesso = Processi.Id where Processi.IdElemento=@IdElemento order by Attivita.Avvio";
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
                        if (m != null)
                            Z.Attivita.Add(m);
                        return Z;
                    }, new { IdElemento = id });
                }

                foreach (Processi P in Processes)
                    P.Stars = nWeightProcess + nWeightTask * P.Attivita.Count;
                _logger.LogInformation($"HistoryManager.GetAllByElementID: Retreived {Processes.Count} Proceses in {sw.ElapsedMilliseconds} ms");
                return Processes;
            }
            catch (Exception ex)
            {
                _logger.LogError($"HistoryManager.GetAllByElementID: Unexpected exception {ex.Message}");
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
                     all = cn.QueryFirstOrDefault<Processi>("Select Id, Nome, Descrizione, Avvio, Fine, UtenteAvvio, Stato, Diagramma, Versione, DataC, UtenteC, DataUM, UtenteUM, IdElemento, IdAllegato, JAttributi  as sAttributi from Processi where Id=@Id ", new { Id = id });
                //all = cn.Get<Processi>(id);

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
                    Act =  cn.QueryFirstOrDefault<Attivita>("Select Id, Nome, Descrizione, Avvio, Fine, Assegnatario, Stato, DataC, UtenteC, DataUM, UtenteUM, IdProcesso, IdElemento, IdAllegato, JAttributi as sAttributi from Attivita where Id=@Id ", new { Id = id });
                    //                   Act = cn.Get<Attivita>(id);

                if (Act!=null)
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
                    //var c = cn.Get<Processi>(obj.Id);
                    var c = cn.QueryFirstOrDefault<Processi>("Select Id, Nome, Descrizione, Avvio, Fine, UtenteAvvio, Stato, Diagramma, Versione, DataC, UtenteC, DataUM, UtenteUM, IdElemento, IdAllegato, JAttributi  as sAttributi from Processi where Id=@Id ", new { Id = obj.Id});
                    if (c == null)
                    {
                        //bres = cn.Insert<Processi>(obj) > 0;
                        string sql = @" INSERT INTO dbo.Processi 
    (Id, Nome, Descrizione, Avvio, Fine, UtenteAvvio, Stato, Diagramma, Versione, DataC, UtenteC, DataUM, UtenteUM, IdElemento, IdAllegato, JAttributi )
    VALUES( @Id, @Nome  , @Descrizione, @Avvio, @Fine, @UtenteAvvio, @Stato, @Diagramma,  @Versione, @DataC, @UtenteC, @DataUM, @UtenteUM, @IdElemento, @IdAllegato, @sAttributi) ";
                        cn.Execute(sql, obj);

                        _logger.LogInformation($"HistoryManager.Save: Create process {obj.Id} in {sw.ElapsedMilliseconds} ms");
                    }
                    else
                    {
                        // cn.Update<Processi>(obj);
                        string sql = @"UPDATE dbo.Processi
   SET Nome = @Nome
      ,Descrizione = @Descrizione
      ,Avvio = @Avvio
      ,Fine = @Fine
      ,UtenteAvvio = @UtenteAvvio
      ,Stato = @Stato
      ,Diagramma = @Diagramma
      ,Versione = @Versione
      ,DataC = @DataC
      ,UtenteC = @UtenteC
      ,DataUM = @DataUM
      ,UtenteUM = @UtenteUM
      ,IdElemento = @IdElemento
      ,IdAllegato = @IdAllegato
      ,JAttributi = @sAttributi
 WHERE  Id = @Id ";
                        cn.Execute(sql, obj);
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
                    //var c = cn.Get<Attivita>(obj.Id);
                    var c = cn.QueryFirstOrDefault<Attivita>("Select Id, Nome, Descrizione, Avvio, Fine, Assegnatario, Stato, DataC, UtenteC, DataUM, UtenteUM, IdProcesso, IdElemento, IdAllegato, JAttributi as sAttributi from Attivita where Id=@Id ", new { Id = obj.Id });
                    if (c == null)
                    {

                        // bres = cn.Insert<Attivita>(obj) > 0;
                        string sql = @" INSERT INTO dbo.Attivita 
    (Id, Nome, Descrizione, Avvio, Fine, Assegnatario, Stato, DataC, UtenteC, DataUM, UtenteUM, IdProcesso, IdElemento, IdAllegato, JAttributi )
    VALUES( @Id, @Nome  , @Descrizione, @Avvio, @Fine, @Assegnatario, @Stato, @DataC, @UtenteC, @DataUM, @UtenteUM, @IdProcesso, @IdElemento, @IdAllegato, @sAttributi) ";
                        cn.Execute(sql, obj);
                        _logger.LogInformation($"HistoryManager.SaveActivity: Create activity {obj.Id} in {sw.ElapsedMilliseconds} ms");
                    }
                    else
                    {
                        // cn.Update<Attivita>(obj);
                        string sql = @"UPDATE dbo.Attivita 
    SET Nome = @Nome
    ,Descrizione = @Descrizione
    ,Avvio = @Avvio
    ,Fine = @Fine
    ,Assegnatario = @Assegnatario
    ,Stato = @Stato
    ,DataC = @DataC
    ,UtenteC = @UtenteC
    ,DataUM = @DataUM
    ,UtenteUM = @UtenteUM
    ,IdProcesso = @IdProcesso
    ,IdElemento = @IdElemento
    ,IdAllegato = @IdAllegato
    ,JAttributi = @sAttributi 
    WHERE  Id = @Id ";
                        cn.Execute(sql, obj); 
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
