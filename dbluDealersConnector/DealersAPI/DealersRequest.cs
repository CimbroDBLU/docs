using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dbluDealersConnector.DealersAPI
{
    public enum RequestType
    {
        Ordine,
        Sostituzione,
        Chiarimento
    }
    public enum RequestState
    {
        Preparing = 3,
        Ready = 4,
        Processing = 1,
        Processed = 2,
        Closed = 5,
        Aborted = 9
    }
    public class DealersRequest 
    {
        public Guid Id { get; set; }

        public Guid? TenantId { get; set; }

        public string Cli { get; set; }
        public RequestType Tipo { get; set; }

        public string Testo { get; set; }

        public string Descrizione { get; set; }

        public DateTime DataC { get; set; }

        public string NomeFile { get; set; }

        public string ElencoFile { get; set; }

        public RequestState State { get; set; }

        public Guid? ItemId { get; set; }
    }
}
