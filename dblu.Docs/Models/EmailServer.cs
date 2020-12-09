using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;

namespace dblu.Docs.Models
{
    public enum TipiRecordServer
    {
        CartellaMail = 0,
        CartellaFile = 1
    }
    [Table("EmailServer")]
    public partial class EmailServer
    {

        [ExplicitKey] 
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Server { get; set; }
        public int Porta { get; set; }
        public bool Ssl { get; set; }
        public string Utente { get; set; }
        public string Password { get; set; }
        public int Intervallo { get; set; } = 60000;
        public bool Attivo { get; set; }
        public string Cartella { get; set; } = "";
        public bool InUscita { get; set; }
        public string NomeProcesso { get; set; } = "";
        public string CartellaArchivio { get; set; } = "";
        public string NomeServerInUscita { get; set; } = "";
        public TipiRecordServer TipoRecord { get; set; } = TipiRecordServer.CartellaMail;

    }
}
