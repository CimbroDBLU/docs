using System.Collections.Generic;
using dblu.Portale.Core.PluginBase.Interfaces;
using dblu.Portale.Core.PluginBase.Classes;
using ExtCore.Infrastructure;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.IO;
using System;

namespace dblu.Portale.Plugin.Documenti
{

    public class ExtensionMetadata : ExtensionBase, IExtensionMetadata
    {

        const string MODULE_ID = "50";
        public override string Description => "Modulo Documenti";
        public override string Name => "Documenti";
        public override string Version => "1.0.0";

        public Assembly CurrentAssembly => Assembly.GetExecutingAssembly();

        public string ModuleID { get { return MODULE_ID; } }



        public IEnumerable<ModuleItem> ModuleList()
        {
            List<ModuleItem> ml = new List<ModuleItem>
            {
                new ModuleItem( "50", "Plugin.Docs" ),
                new ModuleItem( "50.1", "Documenti" ),
                new ModuleItem( "50.1.1", "Tabelle" ),
                new ModuleItem( "50.1.2", "Fascicoli" ),
                new ModuleItem( "50.1.3", "Email in arrivo" ),
                new ModuleItem( "50.1.4", "File in arrivo" ),
                new ModuleItem( "50.1.5", "Smistamento" ),
                new ModuleItem( "50.1.6", "Statistiche" ),
                new ModuleItem( "50.2", "Amministrazione Docs" )
            };

            return ml;
        }

        public IEnumerable<MenuItem> MenuItems
        {
            get
            {
                ///Since i cannot inject IConfiguration, i'll use these lines below to read the conf once again and understand 
                ///if i have to show beta funcionalities
                ///
                var ENV=Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{ENV}.json", optional: true)
                .AddEnvironmentVariables();

                IConfigurationRoot _conf = builder.Build();

                var subItemT = new MenuItem[]
                {
                    new MenuItem("50.1.1", 1, "Docs/Categories", "Categorie", "fas fa-table", null),
                    new MenuItem("50.1.1", 2, "Docs/ItemsTypes", "Tipi elementi", "fas fa-folder", null),
                    new MenuItem("50.1.1", 3, "Docs/AttachmentsTypes", "Tipi allegati", "fas fa-paperclip", null),
                    new MenuItem("50.1.1", 4, "Docs/GridConfigurations", "Configurazione griglie", "fas fa-sliders-h", null),
                    new MenuItem("50.1.3", 5, "Docs/Servers", "Servers", "fas fa-server", null),
                };
 			    var subItemF = new MenuItem[]
                    {
                        new MenuItem("50.1.2", 1, "Docs/Dossiers", "Fascicoli", "fas fa-archive", null),
                        new MenuItem("50.1.2", 2, "Docs/Items", "Elementi", "fas fa-folder-open", null)
                     };
               
                var subItemS = new MenuItem[]
                     {
                        new MenuItem("50.1.6", 2, "Stats/History", "Storico", "fas fa-shoe-prints", null),
                     };
                    
                var subItemM= new MenuItem[]
                     {                                        
                    new MenuItem("50.1.3", 1,  string.IsNullOrEmpty(_conf["Beta"]) ? "MailView/InArrivo" : "Mail/Inbox", "Email in arrivo", "fas fa-envelope-open-text", null),
                    new MenuItem("50.1.3", 2,  string.IsNullOrEmpty(_conf["Beta"]) ? "MailView/emailProcessate" : "Mail/Processed", "Email processate", "fas fa-envelope", null),
                    new MenuItem("50.1.3", 3,  string.IsNullOrEmpty(_conf["Beta"]) ? "MailView/emailInviate" : "Mail/Sent", "Email inviate", "fas fa-paper-plane", null),
                     };

                var subItemZ = new MenuItem[]
                    {
                    new MenuItem("50.1.4", 1, string.IsNullOrEmpty(_conf["Beta"]) ? "ZipView/ZipInArrivo" : "Files/ZIP/Inbox", "Documenti in arrivo", "fas fa-file", null),
                    new MenuItem("50.1.4", 2, string.IsNullOrEmpty(_conf["Beta"]) ? "ZipView/ZipProcessati": "Files/ZIP/Processed","Documenti processati", "fas fa-file-excel", null),              
                    };

                var subItemZ1 = new MenuItem[]
                    {
                    new MenuItem("50.1.5", 1, string.IsNullOrEmpty(_conf["Beta"]) ? "ZipView/ZipInArrivo?Tipo=REQ" : "Files/REQ/Inbox", "Richieste in arrivo", "fas fa-comments", null),
                    new MenuItem("50.1.5", 2, string.IsNullOrEmpty(_conf["Beta"]) ? "ZipView/ZipProcessati?Tipo=REQ" : "Files/REQ/Processed", "Richieste processate", "fas fa-comment-slash", null),
                    };
                return new MenuItem[]
                {
                    new MenuItem("50.1.1", 1, "Docs/Tabelle", "Tabelle", "fas fa-table", subItemT),
                    new MenuItem("50.1.2", 2, "Docs/Fascicoli", "Fascicoli", "fas fa-archive", subItemF),                    
                    new MenuItem("50.1.5", 3, "MailView/Smistamento", "Smistamento", "fas fa-map-signs", null),
                    new MenuItem("50.1.3", 4, "Mail/Inbox", "Email", "fas fa-envelope-open-text", subItemM),
                    new MenuItem("50.1.4", 5, "Files/ZIP/Inbox", "Documenti", "fas fa-file", subItemZ),
                    new MenuItem("50.1.5", 6, "Files/REQ/Inbox", "Richieste", "fas fa-comments", subItemZ1),
                    new MenuItem("50.1.6", 7, "Stats/History", "Statistiche", "fas fas fa-chart-area", subItemS),
                    
                };
            }
        }

        public IEnumerable<IndexItem> IndexItem
        {
            get
            {
                // MAIL
                var myMail = new IndexItem("50.1.3", 2, "", "Mail", "MailDash", "");
                myMail.Size = new System.Drawing.Size(2, 6);
                myMail.ElementType = OBJECT_TYPE.TEMPLATE;
                myMail.Template = "dblu.Portale.Plugin.Docs.Pages.MailTile";

                // FILE
                var myFile = new IndexItem("50.1.4", 3, "", "File", "FileDash", "");
                myFile.Size = new System.Drawing.Size(2, 6);
                myFile.ElementType = OBJECT_TYPE.TEMPLATE;
                myFile.Template = "dblu.Portale.Plugin.Docs.Pages.FileTile";

                // DEALERS
                var myREQ = new IndexItem("50.1.4", 4, "", "Richieste", "FileDash", "");
                myREQ.Size = new System.Drawing.Size(2, 6);
                myREQ.ElementType = OBJECT_TYPE.TEMPLATE;
                myREQ.Template = "dblu.Portale.Plugin.Docs.Pages.RequestTile";

                // FASCICOLI
                var myFascicoli = new IndexItem("50.1.2", 5, "", "Fascicoli", "FascicoliDash", "");
                myFascicoli.Size = new System.Drawing.Size(2, 6);
                myFascicoli.ElementType = OBJECT_TYPE.TEMPLATE;
                myFascicoli.Template = "dblu.Portale.Plugin.Docs.Pages.DossierTile";

                // FASCICOLI
                var myElementi = new IndexItem("50.1.2", 6, "", "Items", "ItemsDash", "");
                myElementi.Size = new System.Drawing.Size(2, 6);
                myElementi.ElementType = OBJECT_TYPE.TEMPLATE;
                myElementi.Template = "dblu.Portale.Plugin.Docs.Pages.ItemsTile";

                // Smistamento
                var mySmista = new IndexItem("50.1.5", 1, "", "Smistamento", "MailDash", "");
                mySmista.Size = new System.Drawing.Size(2, 6);
                mySmista.ElementType = OBJECT_TYPE.TEMPLATE;
                mySmista.Template = "dblu.Portale.Plugin.Docs.Pages.AddressingTile";

                return new IndexItem[]
                {
                    myMail, myFile, myREQ, myFascicoli,myElementi, mySmista                
                };
            }
        }

        public IEnumerable<ToolbarItem> ToolbarItem
        {
            get
            {
                return new ToolbarItem[]
                {
                  // new ToolbarItem("1.4",1, "", "Test ToolbarItem Component","MyToolbar",""),
                };
            }
        }

       
        public IEnumerable<MenuGroupItem> MenuGroupItem
        {
            get
            {
                return new MenuGroupItem[]
                {
                    
                    new MenuGroupItem("50.1", 1,"Documenti", this.MenuItems, "fas fa-mail-bulk"),
                };
            }
        }

    }
}

