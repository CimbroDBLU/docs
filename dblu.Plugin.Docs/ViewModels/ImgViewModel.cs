using BPMClient;
using dblu.Docs.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace dblu.Portale.Plugin.Docs.ViewModels
{
    public class ImgViewModel
    {
        public string IdAllegato { get; set; }
        public string NomeFile { get; set; }
        public bool isRelated { get; set; }
        public ImgViewModel()
        {
        }
        public ImgViewModel(string IdAllegato, string NomeFile, bool isRelated)
        {
            this.IdAllegato = IdAllegato;
            this.NomeFile = NomeFile;
            this.isRelated = isRelated;
        }

    }



}
