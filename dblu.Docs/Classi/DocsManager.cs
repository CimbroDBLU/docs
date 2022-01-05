using dblu.Docs.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dblu.Docs.Classi
{
    public  class DocsManager
    {
        internal ILogger _logger;
        private IConfiguration _conf;
        private dScriptContainer sc;
        public string nome_connessione { get; set; }
        internal string connessione { get; set; }

        private AllegatiManager _allegatiManager { get; set; }
        private ElementiManager _elementiManager { get; set; }
        private FascicoliManager _fascicoliManager { get; set; }

        public DocsManager(ILogger logger, IConfiguration conf, string nomeconn)
        {
            bool res = true;
            _logger = logger;
            _conf = conf;
            nome_connessione = nomeconn;
            connessione = _conf.GetConnectionString(nome_connessione);
             _allegatiManager = new(connessione, _logger);
            _elementiManager = new(connessione, _logger);
            _fascicoliManager = new(connessione, _logger);           
            
            sc = new dScriptContainer();
            res = sc.Init(logger, conf);
            sc.SetObject("docs", this);
        }

        public Fascicoli get_fascicolo(Guid? guid)=> _fascicoliManager.Get(guid);
        public bool cancella_fascicolo(Guid? guid) => _fascicoliManager.Cancella(guid);
        public bool salva_fascicolo(Fascicoli f, bool isnew) => _fascicoliManager.Salva(f, isnew);
        public void cancella_fascicoli_vuoti() => _fascicoliManager.CancellaFascicoliVuoti();

        public Elementi get_elemento(Guid? guid, short rev) => _elementiManager.Get(guid, rev);
        public bool cancella_elemento(Guid? guid, short revisione) => _elementiManager.Cancella(guid, revisione);
        public bool salva_elemento(Elementi el, bool isnew) => _elementiManager.Salva(el, isnew);

        public Allegati get_allegato(Guid? guid) => _allegatiManager.Get(guid);
        public bool cancella_allegato(Guid? guid) => _allegatiManager.Cancella(guid);
        public bool salva_allegato(Allegati a, bool isnew) => _allegatiManager.Salva(a, isnew);

    }
}
