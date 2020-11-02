using System.Collections.Generic;
using dblu.Portale.Core.PluginBase.Interfaces;
using dblu.Portale.Core.PluginBase.Class;
using ExtCore.Infrastructure;
using System.Reflection;
using Microsoft.AspNetCore.Http;

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
                new ModuleItem( "50.2", "Amministrazione Docs" )
            };

            return ml;
        }

        public IEnumerable<MenuItem> MenuItems
        {
            get
            {

                var subItemT = new MenuItem[]
                {
                    new MenuItem("50.1.1", 1, "Docs/Categorie", "Categorie", "fa-table", null),
                    new MenuItem("50.1.1", 2, "Docs/TipiElementi", "Tipi elementi", "fa-table", null),
                    new MenuItem("50.1.1", 3, "Docs/TipiAllegati", "Tipi allegati", "fa-table", null),
                    new MenuItem("50.1.1", 4, "Docs/ConfigGrid", "Configurazione griglie", "fa-table", null),
                    new MenuItem("50.1.3", 5, "Docs/ServersEmail", "Servers Email", "fa-table", null),
                };
 			var subItemF = new MenuItem[]
                {
                    new MenuItem("50.1.2", 2, "Docs/Fascicolo", "Fascicoli", "fa-table", null),
                 };

                return new MenuItem[]
                {
                    new MenuItem("50.1.1", 1, "Docs/Tabelle", "Tabelle", "fa-file", subItemT),
                    new MenuItem("50.1.2", 1, "Docs/Fascicoli", "Fascicoli", "fa-file", subItemF),
                    new MenuItem("50.1.3", 1, "MailView/InArrivo", "Email in arrivo", "fa-table", null),
                    new MenuItem("50.1.3", 2, "MailView/emailProcessate", "Email processate", "fa-table", null),
                    new MenuItem("50.1.3", 3, "MailView/emailInviate", "Email inviate", "fa-table", null),
                    new MenuItem("50.1.3", 4, "ZipView/ZipTask", "File in arrivo", "fa-table", null),
                    new MenuItem("50.2", 1, "Docs/Logs", "Gestione Log", "fa-table", null),
                };
            }
        }

        public IEnumerable<IndexItem> IndexItem
        {
            get
            {
                // MAIL
                var myMail = new IndexItem("1.1.3", 1, "", "Mail", "MailDash", "");
                myMail.Size = new System.Drawing.Size(2, 6);
                myMail.ElementType = OBJECT_TYPE.TEMPLATE;
                myMail.Template = "TemplateMail";
                // MAIL
                var myFile = new IndexItem("1.1.4", 2, "", "File", "FileDash", "");
                myFile.Size = new System.Drawing.Size(2, 6);
                myFile.ElementType = OBJECT_TYPE.TEMPLATE;
                myFile.Template = "TemplateFile";

                return new IndexItem[]
                {
                    myMail, myFile
                 // new IndexItem("50.1.3", 1,  "", "Index", "DocsDash", "")
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
                    
                    new MenuGroupItem("50.1", 1,"Documenti", this.MenuItems, "fa-file"),
                };
            }
        }

    }
}

