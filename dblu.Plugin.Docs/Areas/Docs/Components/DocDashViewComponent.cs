using System;

using Microsoft.AspNetCore.Mvc;

using dblu.Portale.Plugin.Docs.ViewModels;

namespace dblu.Portale.Plugin.Test.Areas.Components
{
    [Area("Widget")]
    public class DocDashViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var model = new IndexModel { };
            return View(model);
        }
    }
}
