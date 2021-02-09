using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dbluDealersConnector.DealersAPI
{
    public class DealersRequestReferences
    {
        public Guid Id { get; set; }

        public Guid? TenantId { get; set; }

        public Guid? RequestId { get; set; }

        public Guid? ItemId { get; set; }

        public Guid? DossierId { get; set; }

        public string Year { get; set; }

        public string Number { get; set; }

    }
}
