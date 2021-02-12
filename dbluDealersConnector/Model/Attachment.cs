using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dbluDealersConnector.Model
{
    /// <summary>
    /// Model for an Attachment
    /// </summary>
    public class Attachment
    {
        /// <summary>
        /// Id of the attachment
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Name of the attachment
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// Description of the attachment
        /// </summary>
        public string Description { get; set; }

    }
}
