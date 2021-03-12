using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace dbluDealersConnector.DealersAPI
{
    /// <summary>
    /// Class for interacting with dbludealers
    /// </summary>
    public class DealersClient
    {
        /// <summary>
        /// base url of the dbludealers
        /// </summary>
        private readonly Uri baseUri;

        /// <summary>
        /// Storage for saving the cookies
        /// </summary>
        private readonly CookieContainer cookieContainer;

        /// <summary>
        /// Client http used for dealer comunications
        /// </summary>
        private readonly HttpClient client;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="nBaseUri">Base url fo the dbludealers</param>
        /// <param name="nAllowHttpsUnsigned">Allow the connection to unsigned https</param>
        public DealersClient(Uri nBaseUri,bool nAllowHttpsUnsigned)
        {

            baseUri = nBaseUri;
            cookieContainer = new CookieContainer();
            HttpClientHandler clientHandler = new HttpClientHandler() { CookieContainer = cookieContainer };

          
            if (nAllowHttpsUnsigned)
                clientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
            client = new HttpClient(clientHandler) { BaseAddress = baseUri };
        }

        /// <summary>
        /// Get the tenant id seaching for Tenant name
        /// </summary>
        /// <param name="TenantName">Tenant name</param>
        /// <param name="Tenant">Tenant id</param>
        /// <returns>
        /// 0 if request is gone well
        /// =! 0 otherwise
        /// </returns>
        public  int GetTenant(string TenantName, ref string Tenant)
        {
            HttpResponseMessage R0 = client.GetAsync(baseUri.ToString() + @"api/abp/multi-tenancy/tenants/by-name/" + TenantName).Result;
        dynamic data = JObject.Parse(R0.Content.ReadAsStringAsync().Result);

        if (data.success == true)
           {
           Tenant = data.tenantId;
           return 0;
           }        
        return 1;
        }

        /// <summary>
        /// Login into the system (dbluDealers)
        /// </summary>
        /// <param name="nTenantName">Tenant Name</param>
        /// <param name="nLogin">Login</param>
        /// <param name="nPassword">Password</param>
        /// <returns>
        /// 0 if request is gone well
        /// =! 0 otherwise
        /// </returns>
        public int Login(string nTenantName, string nLogin, string nPassword)
        {
            try
            {
                string tenantID = "";
                if (nTenantName?.Length != 0)
                    if (GetTenant(nTenantName, ref tenantID) != 0)
                        return 1;

                dynamic Credentials = new ExpandoObject();
                Credentials.userNameOrEmailAddress = nLogin;
                Credentials.password = nPassword;
                Credentials.rememberMe = "true";

                var jsonString = JsonConvert.SerializeObject(Credentials);
                var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
                if (tenantID!.Length != 0)
                    cookieContainer.Add(baseUri, new Cookie("__tenant", tenantID.ToString()));

                HttpResponseMessage R1 = client.PostAsync(baseUri.ToString() + @"api/account/login", content).Result;
                dynamic data1 = JObject.Parse(R1.Content.ReadAsStringAsync().Result);
                if (data1.result == 1)
                {
                    IEnumerable<Cookie> responseCookies = cookieContainer.GetCookies(baseUri).Cast<Cookie>();
                    return 0;
                }

                return 2;
            }
            catch (Exception)
            { return 3; }
        }

        /// <summary>
        /// Get the list of the pending request
        /// </summary>
        /// <returns>
        /// List of the pending request
        /// </returns>
        public List<DealersRequest> PendingRequest()
        {            
            HttpResponseMessage R1 = client.GetAsync(baseUri.ToString() + @"api/app/requests/PendingRequests").Result;
            if (R1.StatusCode == HttpStatusCode.OK)
                return JsonConvert.DeserializeObject<List<DealersRequest>>(R1.Content.ReadAsStringAsync().Result);
            return new List<DealersRequest>();
        }

        /// <summary>
        /// Get the list of the closed request
        /// </summary>
        /// <returns>
        /// List of the closest request
        /// </returns>
        public List<DealersRequest> ClosedRequests()
        {
            HttpResponseMessage R1 = client.GetAsync(baseUri.ToString() + @"api/app/requests/ClosedRequests").Result;
            if (R1.StatusCode == HttpStatusCode.OK)
                return JsonConvert.DeserializeObject<List<DealersRequest>>(R1.Content.ReadAsStringAsync().Result);
            return new List<DealersRequest>();
        }

        /// <summary>
        /// Synchoronize the references of the request with the elements generated by the attachement
        /// </summary>
        /// <param name="RequestId">Id of the reqquest</param>
        /// <param name="Data">List of elements generated from the attachments</param>
        /// <returns>
        /// Retreive the number of references inserted into dbludealer
        /// </returns>
        public async Task<int> SyncReferences(Guid RequestId,List<DealersRequestReferences> Data)
        {          

            string json = JsonConvert.SerializeObject(Data);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage R1 = await client.PostAsync(baseUri.ToString() + @$"api/app/requestReferences/SyncReferences?RequestId={RequestId}", data);
            if (R1.StatusCode == HttpStatusCode.OK)
            {
                json = await R1.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<int>(json);
            }
            return 0;
        }

        /// <summary>
        /// Retreive the document related to a Request
        /// </summary>
        /// <param name="Doc">Id of the document</param>
        /// <returns>
        /// Memory stream hosting the document
        /// </returns>
        public async Task<MemoryStream> GetDocument(string Doc)
        {
            MemoryStream M = new MemoryStream();
            HttpResponseMessage R1 = client.GetAsync(baseUri.ToString() + @$"api/app/requests/GetArchive?Doc={Doc}").Result;
            if (R1.StatusCode == HttpStatusCode.OK)
            {
                Stream S = await R1.Content.ReadAsStreamAsync();
                S.CopyTo(M);
            }
            return M;
        }

        /// <summary>
        /// Remove a list of requests
        /// </summary>
        /// <param name="Ids">List of ids of the request we have to remove</param>
        /// <returns>
        /// True if everithign goes fine
        /// </returns>
        public async Task<bool> PurgeRequests(List<string> Ids)
        {
            string json = JsonConvert.SerializeObject(Ids);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage R1 = await client.PostAsync(baseUri.ToString() + @$"api/app/requests/PurgeRequests", data);
            if (R1.StatusCode == HttpStatusCode.OK)
                return true;
            return false;
        }

        /// <summary>
        /// Change the state of the Request id
        /// </summary>
        /// <param name="RequestId">Request Id</param>
        /// <param name="newState">State of the request</param>
        /// <returns>
        /// true if the status has been upgraded properly
        /// </returns>
        public async Task<bool> ChangeState(Guid RequestId, RequestState newState)
        {
            var data = new StringContent(((int)newState).ToString(), Encoding.UTF8, "application/json");
            HttpResponseMessage R1 = await client.PostAsync(baseUri.ToString() + @$"api/app/requests/RequestChangeState?RequestId={RequestId}", data);
            if (R1.StatusCode == HttpStatusCode.OK)
                return true;
            return false;
        }
    }
}
