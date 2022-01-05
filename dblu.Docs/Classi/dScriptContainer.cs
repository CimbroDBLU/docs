using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoonSharp.Interpreter;
using dblu.Docs.Models;

namespace dblu.Docs.Classi
{
    public class dScriptContainer : dbluTools.Scripting.ScriptContainer
    {
        public new bool Init(ILogger logger, IConfiguration conf)
        {
            bool res = base.Init(logger, conf);
            try
            {
                UserData.RegisterType<System.Guid>();
                UserData.RegisterType<System.Guid?>();
                UserData.RegisterType<Allegati>();
                UserData.RegisterType<Elementi>();
                UserData.RegisterType<Fascicoli>();
                UserData.RegisterType<EmailServer>();
                UserData.RegisterType<AllegatiManager>();
                UserData.RegisterType<ElementiManager>();
                UserData.RegisterType<FascicoliManager>();
                UserData.RegisterType<DocsManager>();

            }
            catch (Exception ex)
            {
                res = false;
                _logger.LogError($"Init docs script error : {ex.Message}");
            }
            return res;
        }

    }
}
