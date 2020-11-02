//using dblu.Docs.Classi;
//using dblu.Portale.Plugin.Docs.ViewModels;
//using Microsoft.AspNetCore.Mvc;
//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Threading.Tasks;

//namespace dblu.Portale.Plugin.Docs.Views.Shared.Components
//    {

//    [ViewComponent(Name = "EditAttributi")]
//    public class EditAttributiViewComponent : ViewComponent
//    {

//        public async Task<IViewComponentResult> InvokeAsync( AttributiViewModel viewModel )

//        {

//            return View(viewModel.NomeView, viewModel);
//        }

//    }
//}

using Microsoft.AspNetCore.Mvc;

using dblu.Portale.Plugin.Docs.ViewModels;

namespace dblu.Portale.Plugin.Test.Areas.Components
{
    [Area("Docs")]
    public class EditAttributiViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var model = new IndexModel { };
            return View(model);
        }
    }
}
