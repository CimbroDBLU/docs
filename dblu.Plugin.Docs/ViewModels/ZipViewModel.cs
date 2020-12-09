using BPMClient;
using dblu.Docs.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace dblu.Portale.Plugin.Docs.ViewModels
{
    public class ZipViewModel : BPMTaskDto
    {
        //public ZipViewModel(BPMTaskDto t )
        //{
        //    task = t;
        //}

        //public BPMTaskDto task { get; set; }

        //public string IdTask
        //{
        //    get {
        //        return task.id;
        //    }
        //    set {
        //        task.id = value;
        //    }
        //}

        //public string Nome
        //{
        //    get
        //    {
        //        return task.name;
        //    }
        //    set
        //    {
        //        task.name = value;
        //    }
        //}

        //public string Descrizione
        //{
        //    get
        //    {
        //        return task.description;
        //    }
        //    set
        //    {
        //        task.description = value;
        //    }
        //}
        public string IdAllegato { get; set; }
        public string TestoEmail { get; set; }
        public string CodiceSoggetto { get; set; }
        public string NomeSoggetto { get; set; }
        public string IdFascicolo { get; set; }
        public string IdElemento { get; set; }
        public string DescrizioneElemento { get; set; }
        public StatoElemento Stato { get; set; }
        public IEnumerable<EmailAttachments> FileAllegati { get; set; }
        
        public IEnumerable<TipiElementi> ListaTipiElementi { get; set; }

        public DateTime DataC
        {
            get
            {
                if (string.IsNullOrEmpty(created ))
                    return DateTime.MinValue;
                return Convert.ToDateTime(created, CultureInfo.InvariantCulture);
            }

        }
        public ZipViewModel()
        {
        }


    }


    public class ZipInArrivoViewModel
    {

        public string Ruolo { get; set; }
    }

}
