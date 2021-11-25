using Dapper.Contrib.Extensions;
using dbluTools.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace dblu.Docs.Models
{

    /// <summary>
    /// Model that describe a process
    /// </summary>
    [Table("Processi")]
    public class Processi
    {
        /// <summary>
        /// Id of theprocess
        /// </summary>
        [ExplicitKey]
        public string Id { get; set; }

        /// <summary>
        /// Name of the process
        /// </summary>
        public string Nome { get; set; }

        /// <summary>
        /// Description of the process
        /// </summary>
        public string Descrizione { get; set; }

        /// <summary>
        /// Starting time of the process 
        /// </summary>
        public DateTime? Avvio { get; set; }

        /// <summary>
        /// End time of the process
        /// </summary>
        public DateTime? Fine { get; set; }

        /// <summary>
        /// User that has starte up the projects
        /// </summary>
        public string UtenteAvvio { get; set; }

        /// <summary>
        /// State of the process
        /// </summary>
        public string Stato { get; set; }

        /// <summary>
        /// BPMN Diagram of the process
        /// </summary>
        public string Diagramma { get; set; }
        
        /// <summary>
        /// Version of the process
        /// </summary>
        public string Versione { get; set; }

        /// <summary>
        /// Date Time of creation
        /// </summary>
        public DateTime? DataC { get; set; } 
        
        /// <summary>
        /// Creating User
        /// </summary>
        public string UtenteC { get; set; }

        /// <summary>
        /// Date Time of last change
        /// </summary>
        public DateTime? DataUM { get; set; }

        /// <summary>
        /// Editign user
        /// </summary>
        public string UtenteUM { get; set; }

        /// <summary>
        /// List of activity linked to this process
        /// </summary>
        [Computed]
        public List<Attivita> Attivita { get; set; } = new List<Attivita>();

        /// <summary>
        /// Number of stars of this process
        /// </summary>
        [Computed]
        public int Stars { get; set; }

        /// <summary>
        /// String with Month and yeat of starting time
        /// </summary>
        [Computed]
        public string MonthYear => $"{Avvio:yyyy} {Avvio:MM}";

        /// <summary>
        /// Processes label
        /// </summary>
        [Computed]
        public string Processes => $"Processi";

        /// <summary>
        /// Duration of the process
        /// </summary>
        [Computed]
        public TimeSpan? Durata => (Avvio != null && Fine != null) ? Fine - Avvio:null;

        /// <summary>
        /// Id of Item used if any
        /// </summary>
        public string IdElemento { get; set; }

        /// <summary>
        /// Id of attachment used if any
        /// </summary>
        public string IdAllegato { get; set; }

        /// <summary>
        /// A json field with any attributes needed
        /// </summary>
        public ExtAttributes JAttributi
        {
            get; set;
        } = new ExtAttributes();
    }
    
}
