using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NToastNotify;
//using dblu.Portale.Plugin.Docs.Models;
using dblu.Portale.Plugin.Docs.Services;
using Kendo.Mvc.UI;
using dblu.Docs.Classi;
using dblu.Docs.Models;
using Microsoft.Extensions.Logging;
using dblu.Portale.Plugin.Docs.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Kendo.Mvc.Extensions;
using Newtonsoft.Json;
using dblu.Docs.Interfacce;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Security.Claims;
using dblu.Portale.Core.Infrastructure.Enums;
using dblu.Portale.Core.PluginBase.Interfaces;
using dblu.Portale.Core.Infrastructure;
using dblu.Portale.Core.Infrastructure.Identity.Class;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace dblu.Portale.Controllers
{
    public class DocsController : Controller
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IToastNotification _toastNotification;
        public readonly ILogger _logger;
        private FascicoliManager _fasMan;
        private ElementiManager _eleMan;
        private AllegatiManager _allMan;
        private ServerEmailManager _serMan;
        private LogDocManager _logMan;
        private ServerMailService _serverMailService;
        private readonly dbluDocsContext _context;
        public DocsController(IWebHostEnvironment hostingEnvironment,
            IToastNotification toastNotification,
            ILoggerFactory loggerFactory,
            dbluDocsContext db,
            ServerMailService serverMailService
            )
        {
            //_context = new dbluDocsContext(db.Connessione);
            _context = db;
            _logger = loggerFactory.CreateLogger("FileRepository");
            _hostingEnvironment = hostingEnvironment;
            _toastNotification = toastNotification;
            _fasMan = new FascicoliManager(_context.Connessione, _logger);
            _eleMan = new ElementiManager(_context.Connessione, _logger);
            _allMan = new AllegatiManager(_context.Connessione, _logger);
            _serMan = new ServerEmailManager(_context.Connessione, _logger);
            _logMan = new LogDocManager(_context.Connessione, _logger);
            _serverMailService = serverMailService;
            
        }
        
        [HttpGet("test")]
        public ActionResult<string> Test()
        {
            return "{ok}";
        }
        #region Categorie

        [HttpGet("/Docs/Categorie")]
        [Authorize]
        [HasPermission("50.1.1")]

        public ActionResult Categorie()
        {
            
             return View();

        }
        [HttpGet("/Docs/Tabelle")]
        [Authorize]
        [HasPermission("50.1.1")]

        public ActionResult Tabelle()
        {

            return View();

        }


        public ActionResult Categorie_Read([DataSourceRequest] DataSourceRequest request)
        {
            IEnumerable<Categorie> lista =  _fasMan.GetAllCategorie();
            
            return Json(lista.ToDataSourceResult(request));
        }

        [AcceptVerbs("Post")]
        public ActionResult Categorie_Create([DataSourceRequest] DataSourceRequest request, Categorie cat)
        {
            if (cat != null && ModelState.IsValid)
            {
                _fasMan.SalvaCategoria(cat);
            }

            return Json(new[] { cat }.ToDataSourceResult(request, ModelState));
        }

        [AcceptVerbs("Post")]
        public ActionResult Categorie_Update([DataSourceRequest] DataSourceRequest request, Categorie cat)
        {
            if (cat != null && ModelState.IsValid)
            {
                _fasMan.SalvaCategoria(cat);
            }

            return Json(new[] { cat }.ToDataSourceResult(request, ModelState));
        }

        [AcceptVerbs("Post")]
        public ActionResult Categorie_Destroy([DataSourceRequest] DataSourceRequest request, Categorie cat)
        {
            if (cat != null)
            {
                _fasMan.CancellaCategoria(cat);
            }

            return Json(new[] { cat }.ToDataSourceResult(request, ModelState));
        }

        [HttpPost("/Docs/editCategoria")]
        [Authorize]
        [HasPermission("50.1.1")]
        public ActionResult editCategoria(string codice)
        {
            Categorie model;
            if (codice != null)
            {
                model = _fasMan.GetCategoria(codice);
            }
            else
            {
                model = new Categorie();
            }
            PopulateTipi();
            PopulateVisibilita();
            HttpContext.Session.SetString("Categoria", JsonConvert.SerializeObject(model));
            // return PartialView(model);
            return PartialView(model);
        }

        private void PopulateTipi()
        {

            var Tipi = new List<dynamic> { new  { Id = "System.String", Des="Testo"},
                                           new  { Id = "System.Boolean", Des="Boolean" },
                                           new  { Id = "System.DateTime", Des="Data" } ,
                                           new  { Id = "System.Int32", Des="Intero" } ,
                                           new  { Id = "System.Double", Des="Decimale" } ,
                                           new  { Id = "System.Object", Des="Oggetto" } ,
                                        };

            ViewData["Tipi"] = Tipi;
            ViewData["defaultTipo"] = "System.String";

        }

        private void PopulateVisibilita()
        {
            var Visibilita = new List<dynamic>();
            int i = 0;
            foreach (string name in Enum.GetNames(typeof(Visibilita_Attributi)))
            {
               
                Visibilita.Add(new { Id = i.ToString(), Des = name });
                i++;
                    }
                 
            ViewData["Visibilita"] = Visibilita;
            ViewData["defaultVisibilita"] = Visibilita[0];

        }

        public ActionResult Descrizioni([DataSourceRequest] DataSourceRequest request, Categorie Cat)
        {
            List<string> lista = new List<string>();
            if (Cat != null) {
                try
                {
                    lista.Add(Cat.Attributi.DescrizioneChiave("Chiave1"));
                    lista.Add(Cat.Attributi.DescrizioneChiave("Chiave2"));
                    lista.Add(Cat.Attributi.DescrizioneChiave("Chiave3"));
                    lista.Add(Cat.Attributi.DescrizioneChiave("Chiave4"));
                    lista.Add(Cat.Attributi.DescrizioneChiave("Chiave5"));
                                                       }
                catch
                {

                }

            }
            return Json(lista.ToDataSourceResult(request));
        }



        [HttpPost]
        public ActionResult SalvaCategoria(Categorie Cat)
        {
            ViewBag.Message = "Categoria Salvata";
            Categorie c = Cat;
            try
            {
                string sc = HttpContext.Session.GetString("Categoria");
                if (!string.IsNullOrEmpty(sc))
                {
                    c = JsonConvert.DeserializeObject<Categorie>(sc);
                    c.Codice = !string.IsNullOrEmpty(Cat.Codice) ? Cat.Codice : c.Codice;
                    c.Descrizione = !string.IsNullOrEmpty(Cat.Descrizione) ? Cat.Descrizione : c.Descrizione;
                    c.ViewAttributi = !string.IsNullOrEmpty(Cat.ViewAttributi) ? Cat.ViewAttributi : c.ViewAttributi;

                    if (_fasMan.SalvaCategoria(c)) {
                        HttpContext.Session.SetString("Categoria", JsonConvert.SerializeObject(c));
                    };
                        
                }
            }
            catch
            {

            }

            return View("Categorie");
        }


        public ActionResult AttributiCategoria_Read([DataSourceRequest] DataSourceRequest request)
        {
            IEnumerable<Attributo> lista = new List<Attributo>();
            try
            {
                string sc = HttpContext.Session.GetString("Categoria");
                if (!string.IsNullOrEmpty(sc)) {
                    var c = JsonConvert.DeserializeObject<Categorie>(sc);
                    lista = c._listaAttributi;
                }
            }
            catch { 
            
            }
 
            return Json(lista.ToDataSourceResult(request));
        }

        [AcceptVerbs("Post")]
        public ActionResult AttributiCategoria_Create([DataSourceRequest] DataSourceRequest request, Attributo att)
        {
            if (att != null && ModelState.IsValid)
            {
                try
                {
                    string sc = HttpContext.Session.GetString("Categoria");
                    if (!string.IsNullOrEmpty(sc))
                    {
                        var c = JsonConvert.DeserializeObject<Categorie>(sc);
                        c.Attributi.Add(att);
                        HttpContext.Session.SetString("Categoria", JsonConvert.SerializeObject(c));
                    }
                }
                catch
                {

                }
            }

            return Json(new[] { att }.ToDataSourceResult(request, ModelState));
        }

        [AcceptVerbs("Post")]
        public ActionResult AttributiCategoria_Update([DataSourceRequest] DataSourceRequest request, Attributo att)
        {
            if (att != null && ModelState.IsValid)
            {
                try
                {
                    string sc = HttpContext.Session.GetString("Categoria");
                    if (!string.IsNullOrEmpty(sc))
                    {
                        var c = JsonConvert.DeserializeObject<Categorie>(sc);
                        c.Attributi.Remove(att.Nome);
                        c.Attributi.Add(att);
                        HttpContext.Session.SetString("Categoria", JsonConvert.SerializeObject(c));
                    }
                }
                catch
                {                }
            }

            return Json(new[] { att }.ToDataSourceResult(request, ModelState));
        }

        [AcceptVerbs("Post")]
        public ActionResult AttributiCategoria_Destroy([DataSourceRequest] DataSourceRequest request, Attributo att)
        {
            if (att != null && ModelState.IsValid)
            {
                try
                {
                    string sc = HttpContext.Session.GetString("Categoria");
                    if (!string.IsNullOrEmpty(sc))
                    {
                        var c = JsonConvert.DeserializeObject<Categorie>(sc);
                        c.Attributi.Remove(att.Nome);
                        HttpContext.Session.SetString("Categoria", JsonConvert.SerializeObject(c));
                    }
                }
                catch
                { }
            }
            return Json(new[] { att }.ToDataSourceResult(request, ModelState));
        }

        [HttpPost]
        [Authorize]
        [HasPermission("50.1.1")]
        public IActionResult duplicaCategoria(string codice)
        {
            Categorie model;
            if (codice != null)
            {
                model = _fasMan.GetCategoria(codice);
                if (model == null) model = new Categorie();
                model.Codice = "";
                model.Descrizione = "";
            }
            else
            {
                model = new Categorie();
            }
            PopulateTipi();
            PopulateVisibilita();
            HttpContext.Session.SetString("Categoria", JsonConvert.SerializeObject(model));
            return PartialView("editCategoria" ,model);
            
        }

        #endregion

        #region TipiElementi

        [HttpGet("/Docs/TipiElementi")]
        [Authorize]
        [HasPermission("50.1.1")]
        public ActionResult TipiElementi()
        {
            return View();
        }
        public ActionResult TipiElementi_Read([DataSourceRequest] DataSourceRequest request)
        {
            IEnumerable<TipiElementi> lista = _eleMan.GetAllTipiElementi();
            return Json(lista.ToDataSourceResult(request));
        }

        [AcceptVerbs("Post")]
        public ActionResult TipiElementi_Create([DataSourceRequest] DataSourceRequest request, TipiElementi obj)
        {
            if (obj != null && ModelState.IsValid)
            {
                _eleMan.SalvaTipoElemento(obj);
            }

            return Json(new[] { obj }.ToDataSourceResult(request, ModelState));
        }

        [AcceptVerbs("Post")]
        public ActionResult TipiElementi_Update([DataSourceRequest] DataSourceRequest request, TipiElementi obj)
        {
            if (obj != null && ModelState.IsValid)
            {
                _eleMan.SalvaTipoElemento(obj);
            }

            return Json(new[] { obj }.ToDataSourceResult(request, ModelState));
        }

        [AcceptVerbs("Post")]
        public ActionResult TipiElementi_Destroy([DataSourceRequest] DataSourceRequest request, TipiElementi obj)
        {
            if (obj != null)
            {
                _eleMan.CancellaTipoElemento(obj);
            }

            return Json(new[] { obj }.ToDataSourceResult(request, ModelState));
        }

        //[HttpPost("/Docs/editTipoElemento")]
        //[Authorize]
        //[HasPermission("50.1.1")]
        //public ActionResult editTipoElemento(string codice)
        //{
        //    TipiElementi model;
        //    if (codice != null)
        //    {
        //        model = _eleMan.GetTipoElemento(codice);
        //    }
        //    else
        //    {
        //        model = new TipiElementi();
        //    }
        //    PopulateTipi();
        //    PopulateVisibilita();
        //    ViewData["categorie"] = GetCategorie();
        //    HttpContext.Session.SetString("TipoElemento", JsonConvert.SerializeObject(model));
        //    //return PartialView(model);
        //    return View(model);
        //}

        [HttpGet("/Docs/editTipoElemento")]
        [Authorize]
        [HasPermission("50.1.1")]
        public IActionResult editTipoElemento(string codice)
        {
            TipiElementi model;
            if (codice != null && codice !="undefined")
            {
                model = _eleMan.GetTipoElemento(codice);
                if (model == null) model = new TipiElementi();
            }
            else
            {
                model = new TipiElementi();
            }
            PopulateTipi();
            PopulateVisibilita();
            ViewData["categorie"] = GetCategorie();
            HttpContext.Session.SetString("TipoElemento", JsonConvert.SerializeObject(model));
            return View(model);
        }



        private IEnumerable<Categorie> GetCategorie()
        {
            IEnumerable<Categorie> lista = _fasMan.GetAllCategorie();

            return lista;
        }


        private IEnumerable<EmailServer> GetServerUscita()
        {
            IEnumerable<EmailServer> lista = _serMan.GetServerEmailInUscita();
                
            return lista;
        }

       

       [HttpPost]
        public ActionResult SalvaTipoElemento(TipiElementi obj)
        {
            ViewBag.Message = "Tipo Elemento Salvato";
            TipiElementi c = obj;


           
            try
            {
                string sc = HttpContext.Session.GetString("TipoElemento");
                if (!string.IsNullOrEmpty(sc))
                {
                    c = JsonConvert.DeserializeObject<TipiElementi>(sc);

                 foreach (var item in Request.Form)
              {
           
                if(item.Key == "Codice") c.Codice = !string.IsNullOrEmpty(item.Value.ToString()) ? item.Value.ToString() : c.Codice;
                if(item.Key == "CategoriaList") c.Categoria = !string.IsNullOrEmpty(item.Value.ToString()) ? item.Value.ToString() : c.Categoria;
                if (item.Key == "Descrizione") c.Descrizione = !string.IsNullOrEmpty(item.Value.ToString()) ? item.Value.ToString() : c.Descrizione;
                if (item.Key == "Processo") c.Processo = !string.IsNullOrEmpty(item.Value.ToString()) ? item.Value.ToString() : c.Processo;
                if (item.Key == "ViewAttributi") c.ViewAttributi = !string.IsNullOrEmpty(item.Value.ToString()) ? item.Value.ToString() : c.ViewAttributi;
                if (item.Key == "Ruoli") c.RuoliCandidati = !string.IsNullOrEmpty(item.Value.ToString()) ? item.Value.ToString() : c.RuoliCandidati;
                if (item.Key == "Candidati") c.UtentiCandidati = !string.IsNullOrEmpty(item.Value.ToString()) ? item.Value.ToString() : c.UtentiCandidati;
                   
                    }
                    //c.Categoria= !string.IsNullOrEmpty(obj.Categoria) ? obj.Categoria : c.Categoria;
                    //c.Descrizione = !string.IsNullOrEmpty(obj.Descrizione) ? obj.Descrizione : c.Descrizione;
                    //c.Processo = !string.IsNullOrEmpty(obj.Processo) ? obj.Processo : c.Processo;
                    //c.ViewAttributi = !string.IsNullOrEmpty(obj.ViewAttributi) ? obj.ViewAttributi : c.ViewAttributi;
                    //c.RuoliCandidati = !string.IsNullOrEmpty(obj.RuoliCandidati) ? obj.RuoliCandidati : c.RuoliCandidati;
                    //c.UtentiCandidati = !string.IsNullOrEmpty(obj.UtentiCandidati) ? obj.UtentiCandidati : c.UtentiCandidati;
                    //c.AggregaAElemento =  obj.AggregaAElemento;
					c.AggregaAElemento = obj.AggregaAElemento;
                    if (_eleMan.SalvaTipoElemento(c))
                    {
                        HttpContext.Session.SetString("TipoElemento", JsonConvert.SerializeObject(c));
                    };

                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"SalvaTipoElemento: {ex.Message}");

            }

            return View("TipiElementi");
        }


        public ActionResult AttributiTipoElemento_Read([DataSourceRequest] DataSourceRequest request)
        {
            IEnumerable<Attributo> lista = new List<Attributo>();
            try
            {
                string sc = HttpContext.Session.GetString("TipoElemento");
                if (!string.IsNullOrEmpty(sc))
                {
                    var c = JsonConvert.DeserializeObject<TipiElementi>(sc);
                    lista = c._listaAttributi;
                }
            }
            catch
            {

            }

            return Json(lista.ToDataSourceResult(request));
        }

        [AcceptVerbs("Post")]
        public ActionResult AttributiTipoElemento_Create([DataSourceRequest] DataSourceRequest request, Attributo att)
        {
            if (att != null && ModelState.IsValid)
            {
                try
                {
                    string sc = HttpContext.Session.GetString("TipoElemento");
                    if (!string.IsNullOrEmpty(sc))
                    {
                        var c = JsonConvert.DeserializeObject<TipiElementi>(sc);
                        c.Attributi.Add(att);
                        HttpContext.Session.SetString("TipoElemento", JsonConvert.SerializeObject(c));
                    }
                }
                catch
                {

                }
            }

            return Json(new[] { att }.ToDataSourceResult(request, ModelState));
        }

        [AcceptVerbs("Post")]
        public ActionResult AttributiTipoElemento_Update([DataSourceRequest] DataSourceRequest request, Attributo att)
        {
            if (att != null && ModelState.IsValid)
            {
                try
                {
                    string sc = HttpContext.Session.GetString("TipoElemento");
                    if (!string.IsNullOrEmpty(sc))
                    {
                        var c = JsonConvert.DeserializeObject<TipiElementi>(sc);
                        c.Attributi.Remove(att.Nome);
                        c.Attributi.Add(att);
                        HttpContext.Session.SetString("TipoElemento", JsonConvert.SerializeObject(c));
                    }
                }
                catch
                { }
            }

            return Json(new[] { att }.ToDataSourceResult(request, ModelState));
        }

        [AcceptVerbs("Post")]
        public ActionResult AttributiTipoElemento_Destroy([DataSourceRequest] DataSourceRequest request, Attributo att)
        {
            if (att != null && ModelState.IsValid)
            {
                try
                {
                    string sc = HttpContext.Session.GetString("TipoElemento");
                    if (!string.IsNullOrEmpty(sc))
                    {
                        var c = JsonConvert.DeserializeObject<TipiElementi>(sc);
                        c.Attributi.Remove(att.Nome);
                        HttpContext.Session.SetString("TipoElemento", JsonConvert.SerializeObject(c));
                    }
                }
                catch
                { }
            }
            return Json(new[] { att }.ToDataSourceResult(request, ModelState));
        }


        public IActionResult RuoliElemento([DataSourceRequest]DataSourceRequest request, string Tipo)
        {
            return Json(_serverMailService.GetAllRolesForElemento(Tipo).ToDataSourceResult(request));
        }


        [AcceptVerbs("Post")]
        public ActionResult RemoveElementoFromRole([DataSourceRequest] DataSourceRequest request, string Tipo, [Bind(Prefix = "models")]IEnumerable<Role> Ruoli)
        {

            foreach (var ruolo in Ruoli)
            {
                _serverMailService.RemoveElementoFromRole(ruolo.RoleId, Tipo);

            }


            return Json(Ruoli.ToDataSourceResult(request, ModelState));
        }

        //public void AddRoleToServer(string RoleID, string ServerName)
        //{
        //    _serverMailService.AddRoleToServer(RoleID, ServerName);
        //}

        public ActionResult AddRoleToElemento(ElementiInRoles[] rol)
        {

            if (rol.Length > 0)
            {

                foreach (var item in rol)
                {
                    
                    _serverMailService.AddRoleToElemento(item.RoleId, item.Tipo);
                    // _usrManager.AddUtenteRuolo(item.RoleId, item.UserId);
                }

            }
            return Json(new
            {
                redirectUrl = "xxx"
            }); ;
        }




        public ActionResult RuoliNonAttivi_Elemento([DataSourceRequest]DataSourceRequest request, string Tipo)
        {
           
            return Json(_serverMailService.RuoliNonAttivi_Elemento(Tipo).ToDataSourceResult(request));
        }





        [HttpPost]
        [Authorize]
        [HasPermission("50.1.1")]
        public IActionResult duplicaTipoElemento(string codice)
        {
            TipiElementi model;
            if (codice != null && codice != "undefined")
            {
                model = _eleMan.GetTipoElemento(codice);
                model.Codice = "";
                model.Descrizione = "";
                if (model == null) model = new TipiElementi();
            }
            else
            {
                model = new TipiElementi();
            }
            PopulateTipi();
            PopulateVisibilita();
            ViewData["categorie"] = GetCategorie();
            HttpContext.Session.SetString("TipoElemento", JsonConvert.SerializeObject(model));
            return View("editTipoElemento", model);
        }



        #endregion

        #region TipiAllegati

        [HttpGet("/Docs/TipiAllegati")]
        [Authorize]
        [HasPermission("50.1.1")]
        public ActionResult TipiAllegati()
        {
            return View();
        }
        public ActionResult TipiAllegati_Read([DataSourceRequest] DataSourceRequest request)
        {
            IEnumerable<TipiAllegati> lista = _allMan.GetAllTipiAllegati();
            return Json(lista.ToDataSourceResult(request));
        }

        [AcceptVerbs("Post")]
        public ActionResult TipiAllegati_Create([DataSourceRequest] DataSourceRequest request, TipiAllegati obj)
        {
            if (obj != null && ModelState.IsValid)
            {
                _allMan.SalvaTipoAllegato(obj);
            }

            return Json(new[] { obj }.ToDataSourceResult(request, ModelState));
        }

        [AcceptVerbs("Post")]
        public ActionResult TipiAllegati_Update([DataSourceRequest] DataSourceRequest request, TipiAllegati obj)
        {
            if (obj != null && ModelState.IsValid)
            {
                _allMan.SalvaTipoAllegato(obj);
            }

            return Json(new[] { obj }.ToDataSourceResult(request, ModelState));
        }

        [AcceptVerbs("Post")]
        public ActionResult TipiAllegati_Destroy([DataSourceRequest] DataSourceRequest request, TipiAllegati obj)
        {
            if (obj != null)
            {
                _allMan.CancellaTipoAllegato(obj);
            }

            return Json(new[] { obj }.ToDataSourceResult(request, ModelState));
        }

        [HttpGet("/Docs/editTipoAllegato")]
        [Authorize]
        [HasPermission("50.1.1")]
        public ActionResult editTipoAllegato(string codice)
        {
            TipiAllegati model;
           
            if (codice != null && codice != "undefined")
            {
                model = _allMan.GetTipoAllegato(codice);
                if (model == null) model = new TipiAllegati();
            }
            else
            {
                model = new TipiAllegati();
            }



            PopulateTipi();
            PopulateVisibilita();
            HttpContext.Session.SetString("TipoAllegato", JsonConvert.SerializeObject(model));
            return View(model);
        }


       

        [HttpPost]
        public ActionResult SalvaTipoAllegato(TipiAllegati obj)
        {
            ViewBag.Message = "Tipo Allegato Salvato";
            TipiAllegati c = obj;
            try
            {
                string sc = HttpContext.Session.GetString("TipoAllegato");
                if (!string.IsNullOrEmpty(sc))
                {
                    c = JsonConvert.DeserializeObject<TipiAllegati>(sc);
                    c.Codice = !string.IsNullOrEmpty(obj.Codice) ? obj.Codice : c.Codice;
                    c.Descrizione = !string.IsNullOrEmpty(obj.Descrizione) ? obj.Descrizione : c.Descrizione;
                    c.ViewAttributi = !string.IsNullOrEmpty(obj.ViewAttributi) ? obj.ViewAttributi : c.ViewAttributi;

                    if (_allMan.SalvaTipoAllegato(c))
                    {
                        HttpContext.Session.SetString("TipoAllegato", JsonConvert.SerializeObject(c));
                    };

                }
            }
            catch
            {

            }

            return View("TipiAllegati");
        }


        public ActionResult AttributiTipoAllegato_Read([DataSourceRequest] DataSourceRequest request)
        {
            IEnumerable<Attributo> lista = new List<Attributo>();
            try
            {
                string sc = HttpContext.Session.GetString("TipoAllegato");
                if (!string.IsNullOrEmpty(sc))
                {
                    var c = JsonConvert.DeserializeObject<TipiAllegati>(sc);
                    lista = c._listaAttributi;
                }
            }
            catch
            {

            }

            return Json(lista.ToDataSourceResult(request));
        }

        [AcceptVerbs("Post")]
        public ActionResult AttributiTipoAllegato_Create([DataSourceRequest] DataSourceRequest request, Attributo att)
        {
            if (att != null && ModelState.IsValid)
            {
                try
                {
                    string sc = HttpContext.Session.GetString("TipoAllegato");
                    if (!string.IsNullOrEmpty(sc))
                    {
                        var c = JsonConvert.DeserializeObject<TipiAllegati>(sc);
                        c.Attributi.Add(att);
                        HttpContext.Session.SetString("TipoAllegato", JsonConvert.SerializeObject(c));
                    }
                }
                catch
                {

                }
            }

            return Json(new[] { att }.ToDataSourceResult(request, ModelState));
        }

        [AcceptVerbs("Post")]
        public ActionResult AttributiTipoAllegato_Update([DataSourceRequest] DataSourceRequest request, Attributo att)
        {
            if (att != null && ModelState.IsValid)
            {
                try
                {
                    string sc = HttpContext.Session.GetString("TipoAllegato");
                    if (!string.IsNullOrEmpty(sc))
                    {
                        var c = JsonConvert.DeserializeObject<TipiAllegati>(sc);
                        c.Attributi.Remove(att.Nome);
                        c.Attributi.Add(att);
                        HttpContext.Session.SetString("TipoAllegato", JsonConvert.SerializeObject(c));
                    }
                }
                catch
                { }
            }

            return Json(new[] { att }.ToDataSourceResult(request, ModelState));
        }

        [AcceptVerbs("Post")]
        public ActionResult AttributiTipoAllegato_Destroy([DataSourceRequest] DataSourceRequest request, Attributo att)
        {
            if (att != null && ModelState.IsValid)
            {
                try
                {
                    string sc = HttpContext.Session.GetString("TipoAllegato");
                    if (!string.IsNullOrEmpty(sc))
                    {
                        var c = JsonConvert.DeserializeObject<TipiAllegati>(sc);
                        c.Attributi.Remove(att.Nome);
                        HttpContext.Session.SetString("TipoAllegato", JsonConvert.SerializeObject(c));
                    }
                }
                catch
                { }
            }
            return Json(new[] { att }.ToDataSourceResult(request, ModelState));
        }

        [HttpPost]
        [Authorize]
        [HasPermission("50.1.1")]
        public IActionResult duplicaTipoAllegato(string codice)
        {
            TipiAllegati model;
            if (codice != null)
            {
                model = _allMan.GetTipoAllegato(codice);
                if (model == null) model = new TipiAllegati();
                model.Codice = "";
                model.Descrizione = "";
            }
            else
            {
                model = new TipiAllegati();
            }
            PopulateTipi();
            PopulateVisibilita();
            HttpContext.Session.SetString("TipoAllegato", JsonConvert.SerializeObject(model));
            return PartialView("editTipoAllegato", model);
           
        }

        public ActionResult AddRoleToAllegato(AllegatiInRoles[] rol)
        {

            if (rol.Length > 0)
            {

                foreach (var item in rol)
                {
                    _serverMailService.AddRoleToAllegato(item.RoleId, item.Tipo);
                    // _usrManager.AddUtenteRuolo(item.RoleId, item.UserId);
                }

            }
            return Json(new
            {
                redirectUrl = "xxx"
            }); ;
        }

        public IActionResult RuoliAllegato([DataSourceRequest] DataSourceRequest request, string Tipo)
        {
            return Json(_serverMailService.GetAllRolesForAllegato(Tipo).ToDataSourceResult(request));
        }

        public ActionResult RuoliNonAttivi_Allegato([DataSourceRequest] DataSourceRequest request, string Tipo)
        {

            return Json(_serverMailService.RuoliNonAttivi_Allegato(Tipo).ToDataSourceResult(request));
        }

        [AcceptVerbs("Post")]
        public ActionResult RemoveAllegatoFromRole([DataSourceRequest] DataSourceRequest request, string Tipo, [Bind(Prefix = "models")] IEnumerable<Role> Ruoli)
        {

            foreach (var ruolo in Ruoli)
            {
                _serverMailService.RemoveAllegatoFromRole(ruolo.RoleId, Tipo);

            }


            return Json(Ruoli.ToDataSourceResult(request, ModelState));
        }


        #endregion

        #region ServersEmail

        [HttpGet("/Docs/ServersEmail")]
        [Authorize]
        [HasPermission("50.1.1")]
        public ActionResult ServersEmail()
        {
            return View();
        }
        public ActionResult EmailServer_Read([DataSourceRequest] DataSourceRequest request)
        {
            IEnumerable<EmailServer> lista = _serMan.GetAllServersEmail();
                
            return Json(lista.ToDataSourceResult(request));
        }

        [AcceptVerbs("Post")]
        public ActionResult EmailServer_Create([DataSourceRequest] DataSourceRequest request, EmailServer obj)
        {
            if (obj != null && ModelState.IsValid)
            {
                _serMan.SalvaServerEmail(obj);
            }

            return Json(new[] { obj }.ToDataSourceResult(request, ModelState));
        }

        [AcceptVerbs("Post")]
        public ActionResult EmailServer_Update([DataSourceRequest] DataSourceRequest request, EmailServer obj)
        {
            if (obj != null && ModelState.IsValid)
            {
                _serMan.SalvaServerEmail(obj);
            }

            return Json(new[] { obj }.ToDataSourceResult(request, ModelState));
        }

        [AcceptVerbs("Post")]
        public ActionResult EmailServer_Destroy([DataSourceRequest] DataSourceRequest request, EmailServer obj)
        {
            if (obj != null)
            {
                _serMan.CancellaServerEmail(obj);
            }

            return Json(new[] { obj }.ToDataSourceResult(request, ModelState));
        }

        //[HttpPost("/Docs/editServerEmail")]
        //[Authorize]
        //[HasPermission("50.1.1")]
        //public ActionResult editServerEmail(string Nome)
        //{
        //    EmailServer model;
        //    if (Nome != null)
        //    {
        //        model = _serMan.GetServer(Nome);
        //    }
        //    else
        //    {
        //        model = new EmailServer();
        //    }
           
        //    HttpContext.Session.SetString("EmailServer", JsonConvert.SerializeObject(model));
        //    return View(model);
        //}
        
        [HttpGet("/Docs/editServerEmail")]
        [Authorize]
        [HasPermission("50.1.1")]
        public IActionResult editServerEmail()
        {
            string Nome = Request.Query["Nome"];
            string Tipo = Request.Query["Tipo"];
            EmailServer model;
            if (Nome != null && Nome != "undefined")
            {
                model = _serMan.GetServer(Nome);
                if (model== null) model = new EmailServer();
            }
            else
            {
                model = new EmailServer();
            }
           
            ViewData["ServerInUscita"] = GetServerUscita();
            HttpContext.Session.SetString("EmailServer", JsonConvert.SerializeObject(model));
            return View(model);
        }
        [HttpPost]
        public ActionResult SalvaEmailServer(EmailServer obj)
        {
            ViewBag.Message = "Server Email Salvato";
            EmailServer c = obj;
            try
            {
                string sc = HttpContext.Session.GetString("EmailServer");
                if (!string.IsNullOrEmpty(sc))
                {
                    c = JsonConvert.DeserializeObject<EmailServer>(sc);
                    c.Nome = !string.IsNullOrEmpty(obj.Nome) ? obj.Nome : c.Nome;
                    c.Attivo = obj.Attivo;
                    c.Cartella = !string.IsNullOrEmpty(obj.Cartella) ? obj.Cartella : c.Cartella;
                    c.CartellaArchivio = !string.IsNullOrEmpty(obj.CartellaArchivio) ? obj.CartellaArchivio : c.CartellaArchivio;
                    c.Email = !string.IsNullOrEmpty(obj.Email) ? obj.Email : c.Email;
                    c.NomeProcesso = !string.IsNullOrEmpty(obj.NomeProcesso) ? obj.NomeProcesso : c.NomeProcesso;
                    c.Password = !string.IsNullOrEmpty(obj.Password) ? obj.Password : c.Password;
                    c.Intervallo = obj.Intervallo;
                    c.Porta = obj.Porta;
                    c.InUscita = obj.InUscita;
                    c.Server = !string.IsNullOrEmpty(obj.Server) ? obj.Server : c.Server;
                    c.Ssl = obj.Ssl;
                    c.Utente = !string.IsNullOrEmpty(obj.Utente) ? obj.Utente : c.Utente;
                    c.NomeServerInUscita = !string.IsNullOrEmpty(obj.NomeServerInUscita) ? obj.NomeServerInUscita : c.NomeServerInUscita;

                    if (_serMan.SalvaServerEmail(c))
                    {
                        HttpContext.Session.SetString("EmailServer", JsonConvert.SerializeObject(c));
                    };

                }
            }
            catch
            {

            }

            return View("ServersEmail");
        }


        public IActionResult RuoliServer([DataSourceRequest]DataSourceRequest request, string ServerName)
        {
           // return Json("");
            return Json(_serverMailService.GetAllRolesForServer(ServerName).ToDataSourceResult(request));
        }


        //public void RemoveFromRole(string RoleID, string ServerName)
        //{
        //    _serverMailService.RemoveFromRole(ServerName, RoleID);
        //}



        [AcceptVerbs("Post")]
        public ActionResult RemoveFromRole([DataSourceRequest] DataSourceRequest request, string ServerName, [Bind(Prefix = "models")]IEnumerable<Role> Ruoli)
        {

            foreach (var ruolo in Ruoli)
            {
                _serverMailService.RemoveFromRole(ruolo.RoleId,ServerName);

            }


            return Json(Ruoli.ToDataSourceResult(request, ModelState));
        }

        //public void AddRoleToServer(string RoleID, string ServerName)
        //{
        //    _serverMailService.AddRoleToServer(RoleID, ServerName);
        //}

        public ActionResult AddRoleToServer(ServersInRole[] rol)
        {

            if (rol.Length > 0)
            {

                foreach (var item in rol)
                {
                    _serverMailService.AddRoleToServer(item.RoleId, item.idServer);
                    // _usrManager.AddUtenteRuolo(item.RoleId, item.UserId);
                }

            }
            return Json(new
            {
                redirectUrl = "xxx"
            }); ;
        }



        public ActionResult RuoliNonAttivi([DataSourceRequest]DataSourceRequest request, string ServerName)
        {
            //_serverMailService.RuoliNonAttivi(ServerName);

            return Json(_serverMailService.RuoliNonAttivi(ServerName).ToDataSourceResult(request));
        }


       
        #endregion

        [HttpPost("/Docs/CercaSoggetti")]
        [Authorize]
        [HasPermission("50.1.2")]
        public ActionResult CercaSoggetti()
        {
            ISoggetti model = new Soggetti();
            return View("CercaSoggetti", model);
        }

        #region LogDoc
        [HttpGet("/Docs/Logs")]
      
        public ActionResult Logs()
        {

            PopulateTipiElemento();
            PopulateOperazione();
            return View();
        }


        private void PopulateTipiElemento()
        {
            var TipiOgg = new List<dynamic>();
            int i = 0; 

            foreach (Object xx in Enum.GetNames(typeof(TipiOggetto)))
            {

                TipiOgg.Add(new { Id = i, Des = xx });
                i++;
            }
            ViewData["TipiOggetto"] = TipiOgg;
        }

        private void PopulateOperazione()
        {
            var Operazione = new List<dynamic>();
          
            foreach (Object xx in Enum.GetValues(typeof(TipoOperazione)))
            {
                Operazione.Add(new { Id = (int)xx, Des = xx });
            }
            ViewData["Operazione"] = Operazione;
        }

        [HasPermission("50.1.3")]
        public ActionResult GetLogs([DataSourceRequest] DataSourceRequest request)
        {
            IEnumerable<LogDoc> lista = _logMan.GetAll();

            return Json(lista.ToDataSourceResult(request));
        }

        [HasPermission("50.1.3")]
        public ActionResult GetLogsItem([DataSourceRequest] DataSourceRequest request, Guid IdItem, TipiOggetto TipoItem)
        {
            IEnumerable<LogDoc> lista = new List<LogDoc>();
            if (IdItem.ToString() != null) { 
                lista = _logMan.GetLogOggetto(IdItem, TipoItem);
            }
            
            return Json(lista.ToDataSourceResult(request));
        }
        #endregion

        [HttpGet("/Docs/ConfigGrid")]
        [Authorize]
        [HasPermission("50.1.1")]
        public ActionResult ConfigGrid()
        {
            return View();
        }

    }
}