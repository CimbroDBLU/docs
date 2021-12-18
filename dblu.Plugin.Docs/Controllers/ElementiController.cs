
using dblu.Docs.Classi;
using dblu.Docs.Models;
using dblu.Portale.Core.Infrastructure.Classes;
using dblu.Portale.Plugin.Docs.Services;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NToastNotify;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;


namespace dblu.Portale.Plugin.Docs.Controllers
{

    [Authorize]
    public class ElementiController : Controller
    {
        private AllegatiService _AllegatiService;
        private readonly IToastNotification _toastNotification;
        private IConfiguration _config;

        public ElementiController(AllegatiService allegatiservice,
            IToastNotification toastNotification,
            IConfiguration config
         )
        {
            _AllegatiService = allegatiservice;
            _toastNotification = toastNotification;
            _config = config;
          
        }

        public ActionResult Elemento_Read([DataSourceRequest] DataSourceRequest request)
        {

            return Ok();
            //return Json(productService.Read().ToDataSourceResult(request));
        }

        [AcceptVerbs("Post")]
        public ActionResult Elemento_Create([DataSourceRequest] DataSourceRequest request)
        {
            //var results = new List<ProductViewModel>();

            //if (products != null && ModelState.IsValid)
            //{
            //    foreach (var product in products)
            //    {
            //        productService.Create(product);
            //        results.Add(product);
            //    }
            //}
            return Ok();
            //return Json(results.ToDataSourceResult(request, ModelState));
        }

        [HttpGet]
        public IActionResult GetPdf([FromQuery]string IdAllegato)
        {
            try
            {
                if (IdAllegato is null)
                {
                    _toastNotification.AddErrorToastMessage("Error in LoadPDF!");
                    return BadRequest();
                }
               
                MemoryStream ff = _AllegatiService._allMan.GetFileAsync(IdAllegato).Result;  //GetFileWrapper(IdAllegato, NomeFile).Result;

                return new FileStreamResult(ff, "application/pdf");

            }
            catch (Exception ex)
            {
                _toastNotification.AddErrorToastMessage("Errore nella lettura del pdf.");
                return BadRequest(ex);
            }

            //return null;

        }


        //[AcceptVerbs("Post")]
        //public ActionResult Elemento_Update([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<ProductViewModel> products)
        //{
        //    //if (products != null && ModelState.IsValid)
        //    //{
        //    //    foreach (var product in products)
        //    //    {
        //    //        productService.Update(product);
        //    //    }
        //    //}
        //    return Ok();
        //    //return Json(products.ToDataSourceResult(request, ModelState));
        //}

        //[AcceptVerbs("Post")]
        //public ActionResult Elemento_Destroy([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<ProductViewModel> products)
        //{
        //    //if (products.Any())
        //    //{
        //    //    foreach (var product in products)
        //    //    {
        //    //        productService.Destroy(product);
        //    //    }
        //    //}

        //    return Json(products.ToDataSourceResult(request, ModelState));
        //}

        //public ActionResult Orders_Read([DataSourceRequest]DataSourceRequest request)
        //{
        //   // return Json(GetOrders().ToDataSourceResult(request));
        //}

        [HttpPost("aggiorna")]
        public ActionResult<dResult> Aggiorna(string Utente, Elementi Elemento)
        {

            dResult r = new dResult();
            try
            {
                bool flNew = true;
                if (Elemento.Id != Guid.Empty)
                {
                    Elementi e = _AllegatiService._elmMan.Get((Guid?)Elemento.Id, Elemento.Revisione);
                    if (e != null)
                    {
                        flNew = false;
                    }
                    else
                    {
                        Elemento.UtenteC = Utente;
                    }
                }
                else
                {
                    Elemento.Id = Guid.NewGuid();
                }
                Elemento.TipoNavigation = _AllegatiService._elmMan.GetTipoElemento(Elemento.Tipo);
                Elemento.elencoAttributi = Elemento.TipoNavigation.Attributi;
                Elemento.elencoAttributi.SetValori(Elemento.Attributi);
                foreach (var attr in Elemento.elencoAttributi.ToList())
                    if (!string.IsNullOrEmpty(attr.Alias))
                        Elemento.SetAttributo(attr.Nome, attr.Valore);

                Elemento.UtenteUM = Utente;
                r.Success = _AllegatiService._elmMan.Salva(Elemento, flNew);
                if (r.Success)
                {
                    r.ReturnData = _AllegatiService._elmMan.Get(Elemento.Id.ToString(), Elemento.Revisione);
                }
            }
            catch (Exception ex)
            {
                r.ErrorMsg = ex.Message;
            }
            return r;
        }


        [HttpGet]
        public ActionResult<dResult> Get(string Id, short Revisione)
        {
            dResult r = new dResult();
            try
            {
                Elementi e = _AllegatiService._elmMan.Get(Id, Revisione);
                r.Success = true;
                r.ReturnData = e;
            }
            catch (Exception ex)
            {
                r.ErrorMsg = ex.Message;
            }
            return r;
        }

        [HttpPost("cerca")]
        public ActionResult<dResult> Cerca(Elementi Elemento)
        {
    
            dResult r = new dResult();
            List<Elementi> la = _AllegatiService._elmMan.CercaElementi(Elemento);
            r.Success = la.Count > 0;
            r.ReturnData = la;
            return r;
        }

    }
}
