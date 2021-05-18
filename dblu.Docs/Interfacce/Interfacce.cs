using dblu.Docs.Classi;
using dblu.Docs.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace dblu.Docs.Interfacce
{
    public class ISoggetti
    {
         public string Codice { get; set; }

        public string Nome { get; set; }
        public string Indirizzo { get; set; }
        public string CAP { get; set; }
        public string Localita { get; set; }
        public string Provincia { get; set; }
        public string Nazione { get; set; }
        public string Note { get; set; }
        public StatoSoggetto Stato { get; set; }
        public DateTime DataC { get; set; }
        public string UtenteC { get; set; }
        public DateTime DataUM { get; set; }
        public string UtenteUM { get; set; }
        public string Nomignolo { get; set; }
        public string PartitaIVA { get; set; }
        public ElencoAttributi elencoAttributi { get; set; }


    }

    public interface ISoggettiService
    {

        //bool Init(IConfiguration conf, ILogger logger);

        Task<List<ISoggetti>> GetSoggettiAsync();

        List<ISoggetti> GetSoggetti();

        List<ISoggetti> GetSoggettiByMail(string Mail);

        ISoggetti GetSoggetto(string Codice);

        string UrlServizio();

        string UrlServizioRicercaElementi();

        Task<List<ISoggettoElementiAperti>> GetElementiAperti(string Codice);

        Task<bool> NotificaAssociazione(String Utente, string CodiceSoggetto, string IdAllegato);

    }

    public interface ISoggettoElementiAperti
    {
        
        string Numero { get;  }
        string Riferimento { get; }
        DateTime DataConsegna { get;  }
        string Stato { get;  }
        string Causale { get; }
    }

}
