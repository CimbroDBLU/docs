using System;
using System.Collections.Generic;
using System.Text;

namespace dblu.Portale.Plugin.Docs.Models
{
    public class RisultatoAzione
    {
        public bool Successo { get; set; }
        public string Messaggio  { get; set; }

        public RisultatoAzione() {
            Successo = true;
            Messaggio = "";
        }

    }
}
