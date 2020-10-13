using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dblu.Portale.Plugin.Docs.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace dblu.Portale.Pages.Docs
{
    public class CategorieModel : PageModel
    {
        private DocsService _service;
        private IConfiguration _config;
        public CategorieModel(DocsService ser)
        {
            _service = ser;
            _config = ser._config;
        }

          public IActionResult OnGet()
        {

            if (_service == null)
            {
                _service = new DocsService(_config);
            }

            return Page();
        }

   
    }
}