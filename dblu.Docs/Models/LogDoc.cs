using Dapper.Contrib.Extensions;
using dblu.Docs.Classi;
using System;
using System.Collections.Generic;
using System.Text;

namespace dblu.Docs.Models
{
    public enum TipoOperazione
    {
        Nessuna=0,
        Creato = 1,
        Elaborato = 2,
        Stampato = 3,
        Cancellato = 4,
        Chiuso = 5,
        Riaperto = 6,
        Spostato = 7,
        Modificato = 8,
        Annullato = 9,
        Inoltrato = 10,
        Risposto = 11
    }


    [Table("LogDoc")]
    public partial class LogDoc
    {
        //[ExplicitKey]
        //public Guid ID { get; set; }
        public DateTime  Data { get; set; }
        public string Utente { get; set; }
        public TipiOggetto TipoOggetto { get; set; }
        public Guid IdOggetto { get; set; }
        public TipoOperazione Operazione { get; set; }

        public LogDoc()
        {
            //ID = Guid.NewGuid();
            Data = DateTime.Now;
            Utente = "";
            Operazione = TipoOperazione.Nessuna;
        }

        public LogDoc( string utente, TipiOggetto tipo, TipoOperazione operazione, Guid id )
        {
            Data = DateTime.Now;
            Utente = utente;
            TipoOggetto = tipo;
            Operazione = operazione;
            IdOggetto = id;
        }

    }
}
