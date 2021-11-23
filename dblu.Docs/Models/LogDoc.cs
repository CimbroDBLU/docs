#if Framework48

using Dapper.Contrib.Extensions;
using dblu.Docs.Extensions;

#else

using dbluTools.Extensions;
using Innofactor.EfCoreJsonValueConverter;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

#endif
using dblu.Docs.Classi;
using System;


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
        public DateTime Data { get; set; }
        public string Utente { get; set; }
        public TipiOggetto TipoOggetto { get; set; }

        public Guid IdOggetto { get; set; }
        public TipoOperazione Operazione { get; set; }
        public string Descrizione { get; set; }

        /// <summary>
        /// A json field with all log attributes
        /// </summary>
#if Framework48
        [Write(false)]
#else
        [JsonField]
#endif
        public ExtAttributes JAttributi
        {
            get; set;
        } = new ExtAttributes();

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
