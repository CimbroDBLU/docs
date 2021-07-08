using BPMClient;
using dblu.Docs.Classi;
using dblu.Docs.Models;
using dblu.Portale.Core.Infrastructure.ViewModels;
using dblu.Portale.Plugin.Docs.Class;
using dblu.Portale.Plugin.Docs.Services;
using dblu.Portale.Services.Camunda;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NToastNotify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dblu.Portale.Plugin.Docs.Controllers
{
    public class TestController : Controller
    {
        private readonly IToastNotification _toastNotification;
        private MailService _mailService;
        private IConfiguration _config;
        private readonly IWebHostEnvironment _hostingEnvironment;
        public TestController(MailService mailservice,
            IToastNotification toastNotification,
            IConfiguration config,
            IWebHostEnvironment hostingEnvironment )
        {
            _mailService = mailservice;
            _toastNotification = toastNotification;
            _config = config;
            _hostingEnvironment = hostingEnvironment;
        }
#if DEBUG
        public async Task<IActionResult> Test() {

            try
            {

                BPMDocsProcessInfo Info = _mailService.GetProcessInfo(TipiOggetto.ELEMENTO, AzioneOggetto.MODIFICA);
                Elementi el = _mailService._elmMan.Get("2270450D-F775-4CC4-AECE-707B580256CF", 0);
                var variabili = new Dictionary<string, VariableValue>();
                variabili.Add("IdAllegato", VariableValue.FromObject("B71F90C3-6BBB-4570-B2AC-33BB7FF180AE"));

                _mailService.AvviaProcesso(Info, el, variabili);

                //string json = System.IO.File.ReadAllText("d:\\temp\\pdf\\new3.json");
                //List<SFPdfPageAnnotation> note = JsonConvert.DeserializeObject<SFPdfPageAnnotation>(json);
                //JToken ann = JsonConvert.DeserializeObject<JToken>(json);

                //dynamic[] pag = new dynamic[ann.Count()];
                //foreach (JProperty a in ann.Children())
                //{
                //    int i = int.Parse(a.Name);
                //   //SFPdfPageAnnotation pa = a.Value<SFPdfPageAnnotation>();

                //    //SFPdfPageAnnotation pa = a.Value<SFPdfPageAnnotation>();
                //    foreach (JToken n in a.Children())
                //    {
                //        SFPdfPageAnnotation pa = n.ToObject<SFPdfPageAnnotation>();
                //        foreach (JArray t in n.Children().Children())
                //        {
                //            for (var ni = 0; ni < t.Count(); ni++)
                //            {
                //                var s = t[ni]["AnnotationSettings"];
                //            }
                //        }
                //    }
                //}
                //json = JsonConvert.SerializeObject(ann);

            }
            catch (Exception ex) {
                _mailService._logger.LogError($" Test : {ex.Message}");
            }
            return await Task.FromResult(Ok());
        }


        public IActionResult Report()
        {
           // var rd = new ReportDetails();
         //  rd.ReportName = "AutReso.trdp";
            return View();
        }
#endif
        }


}
