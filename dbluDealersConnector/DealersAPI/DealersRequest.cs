using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dbluDealersConnector.DealersAPI
{
    /// <summary>
    /// Type of the request
    /// </summary>
    public enum RequestType
    {
        /// <summary>
        /// Order
        /// </summary>
        Ordine,
        /// <summary>
        /// Replacement
        /// </summary>
        Sostituzione,
        /// <summary>
        /// Clarification
        /// </summary>
        Chiarimento
    }

    /// <summary>
    ///  Request status
    /// </summary>
    public enum RequestState
    {
        /// <summary>
        /// Preparing
        /// </summary>
        Preparing = 3,
        /// <summary>
        /// Ready
        /// </summary>
        Ready = 4,
        /// <summary>
        /// Processing
        /// </summary>
        Processing = 1,
        /// <summary>
        /// Processed
        /// </summary>
        Processed = 2,
        /// <summary>
        /// Closed
        /// </summary>
        Closed = 5,
        /// <summary>
        /// Aborted
        /// </summary>
        Aborted = 9
    }

    /// <summary>
    /// Request
    /// </summary>
    public class DealersRequest 
    {
        /// <summary>
        /// Id of the request
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Tenant id any
        /// </summary>
        public Guid? TenantId { get; set; }

        /// <summary>
        /// Code of the shop
        /// </summary>
        public string Cli { get; set; }

        /// <summary>
        /// Type of the request
        /// </summary>
        public RequestType Tipo { get; set; }

        /// <summary>
        /// Text
        /// </summary>
        public string Testo { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        public string Descrizione { get; set; }

        /// <summary>
        /// Creation Date
        /// </summary>
        public DateTime DataC { get; set; }

        /// <summary>
        /// FileName
        /// </summary>
        public string NomeFile { get; set; }

        /// <summary>
        /// List of contents on the ZIP file
        /// </summary>
        public string ElencoFile { get; set; }

        /// <summary>
        /// Status of the request
        /// </summary>
        public RequestState State { get; set; }

    }
}
