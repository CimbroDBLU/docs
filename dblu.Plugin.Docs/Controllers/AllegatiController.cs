using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dblu.Docs.Classi;
using dblu.Docs.Models;
using dblu.Portale.Core.Infrastructure.Classes;
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
    public class AllegatiController : Controller
    {
        private AllegatiService _allService;

        public AllegatiController(AllegatiService allsvc)
        {
            _allService = allsvc;
        }


        [HttpPost("aggiorna")]
        public ActionResult<dResult> Aggiorna( string Utente , Allegati Allegato ) {

            dResult r = new dResult();
            try
            {
                bool flNew = true;
                if (Allegato.Id != Guid.Empty) {
                    Allegati a = _allService._allMan.Get(Allegato.Id);
                    if (a != null)
                    {
                        flNew = false;
                    }
                    else {
                        Allegato.UtenteC = Utente;
                    }
                }
                else {
                    Allegato.Id = Guid.NewGuid();
                }
                Allegato.TipoNavigation = _allService._allMan.GetTipoAllegato(Allegato.Tipo);
                Allegato.elencoAttributi = Allegato.TipoNavigation.Attributi;
                Allegato.elencoAttributi.SetValori(Allegato.Attributi);
                foreach (var attr in Allegato.elencoAttributi.ToList())
                    if(!string.IsNullOrEmpty( attr.Alias))
                        Allegato.SetAttributo(attr.Nome, attr.Valore);
                
                Allegato.UtenteUM = Utente;
                r.Success = _allService._allMan.Salva(Allegato, flNew);
                if (r.Success)
                {
                    r.ReturnData = _allService._allMan.Get(Allegato.Id);
                }
            }
            catch (Exception ex){
                r.ErrorMsg = ex.Message;
            }
            return r;        
        }

        [HttpPost("aggiornafile")]
        public async Task<ActionResult<dResult>> AggiornaFile(string Id )
        {

            dResult r = new dResult();
            try
            {
                if (Request.Form.Files.Count > 0) {
                    IFormFile f = Request.Form.Files[0];

                    using (MemoryStream m = new MemoryStream()) {
                        await f.CopyToAsync(m);
                        r.Success = await _allService._allMan.SalvaFileAsync(Id, m);
                    }
                        
                }
            }
            catch (Exception ex)
            {
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
                Allegati a = _allService._allMan.Get(Id);
                r.Success = true;
                r.ReturnData = a; 
            }
            catch (Exception ex)
            {
                r.ErrorMsg = ex.Message;
            }
            return r;
        }


        [HttpPost("cerca")]
        public ActionResult<dResult> Cerca( Allegati Allegato)
        {

            dResult r = new dResult();
            List<Allegati> la = _allService._allMan.CercaAllegati(Allegato);
            r.Success = la.Count > 0;
            r.ReturnData = la;

            return r;
        }


    }

}
