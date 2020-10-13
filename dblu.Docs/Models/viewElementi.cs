using Dapper.Contrib.Extensions;
using dblu.Docs.Classi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations.Schema;

namespace dblu.Docs.Models
{
    public enum vStatoElemento
    {
        Attivo = 1,
        Elaborato = 2,
        Chiuso = 5,
        Annullato = 9
    }
    [Table("vListaElementi")]
    public partial class viewElementi
    {
        [ExplicitKey]
        public Guid IdFascicolo { get; set; }
        public string CodiceSoggetto { get; set; }
        public Guid IdElemento { get; set; }
        public short Revisione { get; set; }
        public string TipoElemento { get; set; }
        public string DscTipoElemento { get; set; }
        public string DscElemento { get; set; }
        public vStatoElemento Stato { get; set; }
        public DateTime DataC { get; set; }
        public string UtenteC { get; set; }
        public DateTime DataUM { get; set; }
        public string UtenteUM { get; set; }
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


        public viewElementi()
        {
            IdFascicolo = Guid.NewGuid();
            CodiceSoggetto = "";
            IdElemento = Guid.NewGuid();
            Revisione = 0;
            TipoElemento = "";
            DscTipoElemento = "";
            DscElemento = "";
            Stato = vStatoElemento.Attivo;
            DataC = DateTime.Now;
            DataUM = DateTime.Now;
            UtenteC = "";
            UtenteUM = "";
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
