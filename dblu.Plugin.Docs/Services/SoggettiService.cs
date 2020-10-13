﻿using dblu.Docs.Classi;
using dblu.Docs.Interfacce;
using dblu.Docs.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NToastNotify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dblu.Portale.Plugin.Docs.Services
{
    public class SoggettiService : dblu.Docs.Interfacce.ISoggettiService
    {

        public readonly dbluDocsContext _context;
        public readonly ILogger _logger;
        private readonly IToastNotification _toastNotification;
        public IConfiguration _config { get; }

        public string UrlServizio() {
             return "/Docs/CercaSoggetti";
        }

        public string UrlServizioRicercaElementi()
        {
            return "/Fascicolo/CercaElementiSoggetto";
        }

        private readonly SoggettiManager _sggMan;

        public SoggettiService(dbluDocsContext db,
            ILoggerFactory loggerFactory,
            IToastNotification toastNotification,
            IConfiguration config
            )
        {
            _toastNotification = toastNotification;
            _context = db; //new dbluDocsContext(db.Connessione);
            _logger = loggerFactory.CreateLogger("SoggettiService");
            _config = config;
            _sggMan = new SoggettiManager(_context.Connessione, _logger);
        }

        public List<ISoggetti> GetSoggetti()
        {
            return _sggMan.GetAll();
        }

        public async Task<List<ISoggetti>> GetSoggettiAsync()
        {
            return await _sggMan.GeAllAsync();
        }

        public ISoggetti GetSoggetto(string Codice)
        {
            return _sggMan.Get(Codice);
        }

        public async Task<List<ISoggettoElementiAperti>> GetElementiAperti(string Codice)
        {
            var lista = new List<ISoggettoElementiAperti>();

            return await Task.FromResult(lista);
        }
        public async Task<bool> NotificaAssociazione(string Utente, string CodiceSoggetto, string IdAllegato) {
            return await Task.FromResult(true);
        }

     }
}
