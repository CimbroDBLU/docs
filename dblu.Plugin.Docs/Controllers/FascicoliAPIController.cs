using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using dblu.Docs.Models;
using dblu.Portale.Core.Infrastructure.Class;
using dblu.Portale.Plugin.Docs.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Http;
using Newtonsoft.Json;

namespace dblu.Portale.Plugin.Docs.Controllers
{

    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("api/v1/[Controller]")]
    [ApiController]
    public class FascicoliController : Controller
    {
        private AllegatiService _allService;

        public FascicoliController(AllegatiService allsvc)
        {
            _allService = allsvc;
        }

        [HttpPost("aggiorna")]
        public ActionResult<dResult> Aggiorna( string Utente , Fascicoli fascicolo ) {

            dResult r = new dResult();
            try
            {
                bool flNew = true;
                if (fascicolo.Id != null) {
                    Fascicoli a = _allService._fasMan.Get(fascicolo.Id);
                    if (a != null)
                    {
                        flNew = false;
                    }
                    else {
                        fascicolo.UtenteC = Utente;
                    }
                }
                else {
                    fascicolo.Id = Guid.NewGuid();
                }
                fascicolo.CategoriaNavigation = _allService._fasMan.GetCategoria(fascicolo.Categoria);
                fascicolo.elencoAttributi = fascicolo.CategoriaNavigation.Attributi;
                fascicolo.elencoAttributi.SetValori(fascicolo.Attributi);

                fascicolo.UtenteUM = Utente;
                r.Success = _allService._fasMan.Salva(fascicolo, flNew);
                if (r.Success)
                {
                    r.ReturnData = _allService._fasMan.Get(fascicolo.Id);
                }
            }
            catch (Exception ex){
                r.ErrorMsg = ex.Message;
            }
            return r;        
        }

        [HttpGet]
        public ActionResult<dResult> Get(string Id)
        {
            dResult r = new dResult();
            try
            {
                Fascicoli a = _allService._fasMan.Get(Id);
                r.Success = true;
                r.ReturnData = a; 
            }
            catch (Exception ex)
            {
                r.ErrorMsg = ex.Message;
            }
            return r;
        }

    }
}
