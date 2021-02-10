using dblu.Docs.Models;
using dbluDealersConnector.DealersAPI;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dbluDealersConnector.Classes
{
    /// <summary>
    /// Class for manaing syntax sugar for helping code-writing
    /// </summary>
    public static class dbluExtension
    {
        /// <summary>
        /// Reflect tthe state of the attachment on the request
        /// </summary>
        /// <param name="Stato">State of the attachment</param>
        /// <returns>
        /// The state that the request ha assumed according
        /// </returns>
        public static RequestState ToRequestState(this StatoAllegato Stato)
        {
            switch(Stato)
            {
                case StatoAllegato.DaSmistare:
                case StatoAllegato.Attivo: 
                    return RequestState.Processing;
                case StatoAllegato.Spedito:
                case StatoAllegato.Stampato:                
                    return RequestState.Processed;
                case StatoAllegato.Elaborato:
                case StatoAllegato.Chiuso: 
                     return RequestState.Closed;
                case StatoAllegato.Annullato: 
                    return RequestState.Aborted;
            }
            return RequestState.Processing;
        }
    }
}
