using dblu.Docs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dblu.Portale.Plugin.Docs.ViewModels
{
   public class DocumentViewModel
    {

        public Allegati Allegato { get; set; }
        //public Guid DocumentId { get; set; }
        //public string DocumentPath { get; set; }
        //public string DocumentName { get; set; }
        //public Nullable<int> CategoryId { get; set; }
        //public string CategoryName { get; set; }

        public DocumentViewModel()
        {
            //DocumentId = Guid.Empty;
            //DocumentName = "";

        }
    }
}
