
using System;
using ExtCore.Infrastructure.Actions;
using Microsoft.AspNetCore.Builder;
using Syncfusion.Licensing;

namespace dblu.Portale.Plugin.Documenti.Actions
{
  public class ConfigureAction : IConfigureAction
  {
    public int Priority => 1000;

        public void Execute(IApplicationBuilder applicationBuilder, IServiceProvider serviceProvider)
        {
            //SyncfusionLicenseProvider.RegisterLicense("MjAyODY4QDMxMzcyZTM0MmUzMENlM0xhUTg1ekxPYUhKaTV3TStwTHZUT0FLamcrR3FFa2Fmcmw5V3IrLzg9");
            //SyncfusionLicenseProvider.RegisterLicense("MjA4NDUwQDMxMzcyZTM0MmUzMG50a0ZPUnV1NUNXV20wNC9HNnhjZjhKdWtJamNPY3k3OHpHRVhOenVBS289");
            //SyncfusionLicenseProvider.RegisterLicense("MjQ3ODQwQDMxMzgyZTMxMmUzMENMcldpS0lYNlV6MWR5NHlVVFJMeVJTazVnYnQxSUZiMXNzNmdFZklTN0E9");
            //SyncfusionLicenseProvider.RegisterLicense("MzE0MDAwQDMxMzgyZTMyMmUzMGxncEUzMXVnblQ1T214YXRNUDdDdk5MYWwzRGVYbzhmaFpPTTBOemlWZU09; MzE0MDAxQDMxMzgyZTMyMmUzMGdiQ2xHYWsydWpLNHhRSG1OVUtWOElWL1BhR1dRM2J0VGJ2THlDNTR6UzA9");
            //18.3.52
            //SyncfusionLicenseProvider.RegisterLicense("MzMwMTg0QDMxMzgyZTMzMmUzME5jenNTSGIvaDBHN3ptdE9LcXlndU9Wa05KL1BXeld6ak9OTmE5MVVZSkU9;MzMwMTg1QDMxMzgyZTMzMmUzMFVvSWtpWkE2UVIwUEpzdTY3dXU3b2pPRU9YMGlrTndxRk5wTjVNQnM5NHc9;MzMwMTg2QDMxMzgyZTMzMmUzMEkxTlZLM2FEa0xuN1h4U0c4NzR1RUpobmlHSzlsZitTSklKTXJnZ2QyZGc9");
            //18.4.35
            SyncfusionLicenseProvider.RegisterLicense("Mzg2NDgyQDMxMzgyZTM0MmUzMGhIdk9wajRvOWQvdUdZNkcyckFzK1VRY3NLcTVnSEdjd3IxSCtIaG5GZjA9; Mzg2NDgzQDMxMzgyZTM0MmUzMEpCNXJjUDVVWnNpVHV2S1BFenVhYXV4YTNqeFFKZERYM0RJUkRDZVAyZVU9");
            //19.1.55
           //SyncfusionLicenseProvider.RegisterLicense("NDI1NDc5QDMxMzkyZTMxMmUzMEVtV2pXZU5JanFMdFAwZnZLcG9EZnNzNm5nSzYxeGN0U0ZYQ1pHcDFRd1E9");
        }
  }
}