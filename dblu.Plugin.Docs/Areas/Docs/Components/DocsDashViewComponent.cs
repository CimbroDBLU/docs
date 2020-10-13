using Microsoft.AspNetCore.Mvc;
using dblu.Portale.Core.Infrastructure.Interfaces;
using System.Collections.Generic;
using dblu.Portale.Plugin.Docs.Services;
using dblu.Docs.Models;

namespace dblu.Portale.Plugin.TaskListBase.Areas.Components
{
    [Area("Camunda")]
    public class DocsDashViewComponent : ViewComponent
    {
        private readonly MailService _service;

        public DocsDashViewComponent(MailService ser)
        {
            _service = ser;
        }


        public IViewComponentResult Invoke()
        {

            var model = new Allegati{};
           
            return View(model);
        }
    }
}
