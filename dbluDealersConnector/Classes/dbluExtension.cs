using dblu.Docs.Models;
using dbluDealersConnector.DealersAPI;
using dbluDealersConnector.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dbluDealersConnector.Classes
{
    public static class dbluExtension
    {
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
