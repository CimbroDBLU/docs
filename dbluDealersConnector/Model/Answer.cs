using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dbluDealersConnector.Model
{
    /// <summary>
    /// Answer for a login action
    /// </summary>
    public class Answer
    {
        /// <summary>
        /// Out code , 0 means ok, != mean error
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// Payload of the answer, contains the token or an error message
        /// </summary>
        public string Payload { get; set; }
    }
}
