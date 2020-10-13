using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace dblu.Portale.Plugin.Docs.Pages.Components.FascicoloControl
{
    public class FascicoloControlViewComponent : ViewComponent
    {

        public FascicoloControlViewComponent() { }
        public IViewComponentResult Invoke(string ratingControlType)
        {
            return View("Default", ratingControlType);
        }

    }
}
