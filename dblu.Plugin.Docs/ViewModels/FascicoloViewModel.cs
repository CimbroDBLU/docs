using dblu.Docs.Models;
using dblu.Portale.Plugin.Docs.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace dblu.Portale.Plugin.Docs.ViewModels
{
    public class FascicoloViewModel
    {

        public Fascicoli Fascicolo { get; set; }
        public string UrlRefer   { get; set; } // 

        public bool IsInsideTask { get; set; }
        //public List<Allegati> Allegati { get; set; }
        //public Cliente Cliente { get; set; }
    }


    //public class Cliente
    //{
    //    public string Nome { get; set; }
    //    public string Email { get; set; }
    //}
}
