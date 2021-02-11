using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dbluDealersConnector.DealersAPI
{
    /// <summary>
    /// Reference of the request
    /// </summary>
    public class DealersRequestReferences
    {
        /// <summary>
        /// Id of the reference
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Tenant Id
        /// </summary>
        public Guid? TenantId { get; set; }

        /// <summary>
        /// Request Id
        /// </summary>
        public Guid? RequestId { get; set; }

        /// <summary>
        /// Id of the item (ELEMENTO)
        /// </summary>
        public Guid? ItemId { get; set; }

        /// <summary>
        /// Id of the Dossier (FASCICOLO)
        /// </summary>
        public Guid? DossierId { get; set; }

        /// <summary>
        /// Year of protocol
        /// </summary>
        public string Year { get; set; }

        /// <summary>
        /// Number of protocol
        /// </summary>
        public string Number { get; set; }

        /// <summary>
        /// As source of the request
        /// </summary>
        public bool InReferece { get; set; }

    }
}
