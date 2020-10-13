using System;
using System.Collections.Generic;
using System.Text;

namespace dblu.Portale.Plugin.Docs.ViewModels
{
    public class ElementiViewModel
    {
        public Guid? IdFascicolo { get; set; }
        public string CodiceSoggetto { get; set; }
        public string  DscFascicolo { get; set; }
        public string DscCategoria { get; set; }
        public string Chiave1F { get; set; }
        public string Chiave2F { get; set; }
        public string Chiave3F { get; set; }
        public string Chiave4F { get; set; }
        public string Chiave5F { get; set; }
        public Guid? IdElemento { get; set; }
        public int Revisione { get; set; }
        public string  Tipo { get; set; }
        public string DscElemento { get; set; }
        public string DscTipo { get; set; }
        public int Stato { get; set; }
        public string Chiave1E { get; set; }
        public string Chiave2E{ get; set; }
        public string Chiave3E { get; set; }
        public string Chiave4E { get; set; }
        public string Chiave5E { get; set; }

    }
}
