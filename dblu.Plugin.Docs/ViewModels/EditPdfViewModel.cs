using System;
using System.Collections.Generic;
using System.Text;

namespace dblu.Portale.Plugin.Docs.ViewModels
{
    public class EditPdfViewModel
    {
        public string TipoAllegato { get; set; }
        public string IdAllegato { get; set; }
        public string IdElemento { get; set; }
        public bool AbilitaSalva { get; set; }

    }
}
