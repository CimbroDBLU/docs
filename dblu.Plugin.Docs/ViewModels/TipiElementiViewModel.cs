using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dblu.Docs.Classi;
using dblu.Docs.Models;
using dblu.Portale.Plugin.Docs.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;


namespace dblu.Portale.Plugin.Docs.ViewModels
{
    public class TipiElementiViewModel : TipiElementi
    {

        public TipiElementiViewModel(TipiElementi c) {
            base.Attributi = c.Attributi;
            base.Codice = c.Codice;
            base.Descrizione = c.Descrizione;
            base.ViewAttributi = c.ViewAttributi;

        }
        public TipiElementiViewModel()
        {
           
        }


    }
}