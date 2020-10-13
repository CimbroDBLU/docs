using dblu.Docs.Classi;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace dblu.Portale.Plugin.Docs.ViewModels
{
    public class AttributiViewModel 
    {

        private string _nomeview = "AttributiView";
        public string NomeView {
            get {
                return _nomeview;
            } 
            

            set {
                if (!string.IsNullOrEmpty(value)) {
                    this._nomeview = value;
                }

            } 
            }

        public ElencoAttributi Attributi { get; set; }

        public string Valori { get
                {
                return this.Attributi.GetValori();
                }
            set {
                this.Attributi.SetValori(value);

             } 
        }


        //[UIHint("VisibilitaAttributo")]
        //public VisibilitaAttributoViewModel Visibile { get
        //    {
        //        return new VisibilitaAttributoViewModel
        //        {
        //            Id = base.Visibilità
        //        };
        //    }

        //    set {  if (value == null)
        //        {
        //            base.Visibilità = dblu.Docs.Classi.Visibilita_Attributi.VISIBLE;
        //        }
        //        else { 
        //            base.Visibilità = value.Id;
        //        }

        //     } 
        //}

    }

    public class VisibilitaAttributoViewModel 
    {
        public dblu.Docs.Classi.Visibilita_Attributi Id { get; set; }
        public string Descrizione {
            get {
                return Id.ToString();
            }
        }
    }

}
