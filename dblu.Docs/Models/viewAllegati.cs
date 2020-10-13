using Dapper.Contrib.Extensions;
using dblu.Docs.Classi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations.Schema;

namespace dblu.Docs.Models
{
    public enum vStatoAllegato
    {
        Attivo = 1,
        Elaborato = 2,
        Chiuso = 5,
        Annullato = 9
    }
    [Table("vListaAllegati")]
    public partial class viewAllegati
    {
        [ExplicitKey]
        public Guid IdAllegato { get; set; }
        public string Descrizione { get; set; }
        public string NomeFile { get; set; }
        public string Tipo { get; set; }
        public string DscTipoAllegato{ get; set; }
        public Guid IdFascicolo { get; set; }
        public Guid IdElemento { get; set; }
        public vStatoAllegato Stato { get; set; }
        public string Note { get; set; }
        public DateTime DataC { get; set; }
        public string UtenteC { get; set; }
        public DateTime DataUM { get; set; }
        public string UtenteUM { get; set; }
        public string Origine { get; set; }
        public string Campo1 { get; set; }
        public string Campo2 { get; set; }
        public string Campo3 { get; set; }
        public string Campo4 { get; set; }
        public string Campo5 { get; set; }
        public string Campo6 { get; set; }
        public string Campo7 { get; set; }
        public string Campo8 { get; set; }
        public string Campo9 { get; set; }
        public string Campo10 { get; set; }

    
        public viewAllegati()
        {
            
            IdAllegato = Guid.NewGuid();
            Descrizione = "";
            NomeFile  = "";
            Tipo = "";
            DscTipoAllegato = "";
            IdFascicolo = Guid.NewGuid();
            IdElemento = Guid.NewGuid();
            Stato = vStatoAllegato.Attivo;
            Note = "";
            DataC = DateTime.Now;
            DataUM = DateTime.Now;
            UtenteC = "";
            UtenteUM = "";
            Origine = "";
            Campo1 = "";
            Campo2 = "";
            Campo3 = "";
            Campo4 = "";
            Campo5 = "";
            Campo6 = "";
            Campo7 = "";
            Campo8 = "";
            Campo9 = "";
            Campo10 = "";
        }

    }
}
