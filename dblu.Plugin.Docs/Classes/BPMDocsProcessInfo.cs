using dblu.CamundaClient;
using dblu.Docs.Classi;
using System;
using System.Collections.Generic;
using System.Text;

namespace dblu.Portale.Plugin.Docs.Class
{

    public enum AzioneOggetto { 
        INSERIMENTO =1,
        MODIFICA = 2,
        ANNULLAMENTO = 3
    }

    public class BPMDocsProcessInfo : BPMProcessInfo
    {
            public TipiOggetto TipoOggetto { get; set; }
            public int Stato { get; set; }
            public int StatoPrec { get; set; }
            public AzioneOggetto Azione { get; set; }
    }

}
