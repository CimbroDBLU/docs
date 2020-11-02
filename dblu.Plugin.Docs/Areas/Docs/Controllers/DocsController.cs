using System;
using System.Collections.Generic;
using System.Text;
using dblu.Portale.Core.Infrastructure.Class;
using dblu.Portale.Core.Infrastructure.Interfaces;
using dblu.Portale.Core.PluginBase.Class;
using dblu.Portale.Plugin.Docs.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace dblu.Portale.Plugin.Factory.Areas.Widget.Controllers
{
    [Area("Docs")]
    public class WidgetController : Controller
    {
        public WidgetController() { }


        [HttpGet("/Test/ShowGraph")]
        public IActionResult Index()
        {
            var param = Request.QueryString; // query string che è possibile parsare per 
                                             // estrarre i comandi da usare

            //modello pagina che sovrascrive quello di default (IndexModel) per la pagina corrente
            var model = new IndexModel { };

            return View(model);
        }


    }
}
