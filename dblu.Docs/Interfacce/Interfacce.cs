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
    public interface ISoggetti
    {
          string Codice { get; set; }

         string Nome { get; set; }
         string Indirizzo { get; set; }
         string CAP { get; set; }
         string Localita { get; set; }
         string Provincia { get; set; }
         string Nazione { get; set; }
         string Note { get; set; }
         StatoSoggetto Stato { get; set; }
         DateTime DataC { get; set; }
         string UtenteC { get; set; }
         DateTime DataUM { get; set; }
         string UtenteUM { get; set; }
         string Nomignolo { get; set; }
         string PartitaIVA { get; set; }
         ElencoAttributi elencoAttributi { get; set; }


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
