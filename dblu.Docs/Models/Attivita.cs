using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace dblu.Docs.Models
{
    /// <summary>
    /// Model that describe an activity
    /// </summary>
    [Table("Attivita")]
    public class Attivita
    {
        /// <summary>
        /// Id of the actvity
        /// </summary>
        [ExplicitKey]
        public string Id { get; set; }

        /// <summary>
        /// Name of the actvity
        /// </summary>
        public string Nome { get; set; }

        /// <summary>
        /// Description of activity
        /// </summary>
        public string Descrizione { get; set; }

        /// <summary>
        /// Starting time of the activity
        /// </summary>
        public DateTime? Avvio { get; set; }

        /// <summary>
        /// End time of the activity
        /// </summary>
        public DateTime? Fine { get; set; }

        /// <summary>
        /// Assegnee of this actvity
        /// </summary>
        public string Assegnatario { get; set; }

        /// <summary>
        /// Current state of the activity
        /// </summary>
        public string Stato { get; set; }

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
        /// Id of the process that is owning this task
        /// </summary>
        public string IdProcesso { get; set; }

    }

}
