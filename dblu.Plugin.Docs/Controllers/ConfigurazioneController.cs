using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dblu.Docs.Models;
using dblu.Portale.Core.Infrastructure;
using dblu.Portale.Plugin.Docs.Services;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using NToastNotify;

namespace dblu.Portale.Plugin.Docs.Controllers
{
    public class ConfigurazioneController : Controller
    {
        private readonly IToastNotification _toastNotification;
        private AllegatiService _doc;
        private List<Colonna> _Colonne;
        public ConfigurazioneController(AllegatiService doc,
            IToastNotification toastNotification)
        {
            _doc = doc;
            _toastNotification = toastNotification;
        }

        
        [HasPermission("50.1.1")]
        public ActionResult ConfigGrid_Read([DataSourceRequest] DataSourceRequest request, string NomeConfig = "")
        {

            IEnumerable<Colonna> lista = _doc.GetColonne(NomeConfig);
            _Colonne = new List<Colonna>() ;
            _Colonne.AddRange(lista);
           // _Colonne = lista.ToList();
            return Json(lista.ToDataSourceResult(request));
        }


        
        [AcceptVerbs("Post")]
        [HasPermission("50.1.1")]
        public ActionResult ConfigGrid_Update([DataSourceRequest] DataSourceRequest request, Colonna Col,string NomeConfig = "")
        {

            if (Col != null && ModelState.IsValid)
            {
                List<Colonna> lista = _doc.GetColonne(NomeConfig).ToList();
                Colonna xCol = lista.Find(x => x.Field == Col.Field);
                xCol.Des = Col.Des;
                xCol.Visible = Col.Visible;
                _doc.SalvaColonne(NomeConfig, lista);
            }

            return Json(new[] { Col }.ToDataSourceResult(request, ModelState));
        }

       

    }
}
