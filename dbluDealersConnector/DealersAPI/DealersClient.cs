using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
    public class DealersClient
    {
        private readonly Uri baseUri;
        private readonly CookieContainer cookieContainer;
        private readonly HttpClientHandler clientHandler;
        private readonly HttpClient client;

        public DealersClient(Uri nBaseUri)
        {
            baseUri = nBaseUri;
            cookieContainer = new CookieContainer();
            clientHandler = new HttpClientHandler() { CookieContainer = cookieContainer };
            client = new HttpClient(clientHandler) { BaseAddress = baseUri };
        }

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

        public List<DealersRequest> PendingRequest()
        {            
            HttpResponseMessage R1 = client.GetAsync(baseUri.ToString() + @"Requests/PendingRequests").Result;
            if (R1.StatusCode == HttpStatusCode.OK)
                return JsonConvert.DeserializeObject<List<DealersRequest>>(R1.Content.ReadAsStringAsync().Result);
            return new List<DealersRequest>();
        }

        public async Task<int> SyncReferences(Guid RequestId,List<DealersRequestReferences> Data)
        {          

            string json = JsonConvert.SerializeObject(Data);
            var data = new StringContent(json, Encoding.UTF8, "application/json");


            HttpResponseMessage R1 = await client.PostAsync(baseUri.ToString() + @$"api/app/SyncReferences?RequestId={RequestId}", data);
            if (R1.StatusCode == HttpStatusCode.OK)
            {
                json = await R1.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<int>(json);
            }
            return 0;
        }

        public async Task<MemoryStream> GetDocument(string Doc)
        {
            MemoryStream M = new MemoryStream();
            HttpResponseMessage R1 = client.GetAsync(baseUri.ToString() + @$"Requests/GetArchive?Doc={Doc}").Result;
            if (R1.StatusCode == HttpStatusCode.OK)
            {
                Stream S = await R1.Content.ReadAsStreamAsync();
                S.CopyTo(M);
            }
            return M;
        }

        public async Task<bool> ChangeState(Guid RequestId, RequestState newState)
        {
            var data = new StringContent(((int)newState).ToString(), Encoding.UTF8, "application/json");
            HttpResponseMessage R1 = await client.PostAsync(baseUri.ToString() + @$"api/app/RequestChangeState?RequestId={RequestId}", data);
            if (R1.StatusCode == HttpStatusCode.OK)
                return true;
            return false;
        }
    }
}
