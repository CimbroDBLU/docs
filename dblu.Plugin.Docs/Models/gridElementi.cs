using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace dblu.Portale.Plugin.Docs.Models
{
    public class gridElementi
    {

        [Display(Name = "Guid")]
        public Guid id { get; set; }

        [Display(Name = "Data")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy HH:mm:ss}")]
        [DataType(DataType.DateTime)]
        public DateTime date  { get; set; }

        [Display(Name = "Data Modifica")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy HH:mm:ss}")]
        [DataType(DataType.DateTime)]
        public DateTime datemodifica { get; set; }

        [Display(Name = "Utente")]
        public string utente { get; set; }

        [Display(Name = "Utente Modifica")]
        public string utentemodifica { get; set; }

        [Display(Name = "Nota")]
        public string contenuto { get; set; }
    }
}
