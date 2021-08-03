using System.Collections.Generic;
using dblu.Portale.Core.PluginBase.Interfaces;
using dblu.Portale.Core.PluginBase.Classes;
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
                        new MenuItem("50.1.2", 2, "Docs/Dossiers", "Fascicoli", "fas fa-archive", null)
                     };
               
                var subItemS = new MenuItem[]
                     {
                        new MenuItem("50.1.6", 2, "Stats/History", "Storico", "fas fa-shoe-prints", null),
                       // new MenuItem("50.1.6", 7, "Stats/HistoryPivot", "Dettaglio", "fas fas fa-chart-area", null),

                     };

                var subItemM= new MenuItem[]
                     {                                        
                    new MenuItem("50.1.3", 1, "MailView/InArrivo", "Email in arrivo", "fas fa-envelope-open-text", null),
                    new MenuItem("50.1.3", 2, "MailView/emailProcessate", "Email processate", "fas fa-envelope", null),
                    new MenuItem("50.1.3", 3, "MailView/emailInviate", "Email inviate", "fas fa-paper-plane", null),
                     };

                var subItemZ = new MenuItem[]
                    {
                    new MenuItem("50.1.4", 4, "ZipView/ZipInArrivo", "File in arrivo", "fas fa-file-archive", null),
                    new MenuItem("50.1.4", 5, "ZipView/ZipProcessati", "File processati", "fas fa-file-archive", null),
                    new MenuItem("50.1.4", 6, "ZipView/ZipInArrivo?Tipo=REQ", "Deals in arrivo", "fas fa-ruler", null),
                    new MenuItem("50.1.4", 6, "ZipView/ZipProcessati?Tipo=REQ", "Deals processati", "fas fa-pencil-ruler", null),
                    };

                return new MenuItem[]
                {
                    new MenuItem("50.1.1", 1, "Docs/Tabelle", "Tabelle", "fas fa-table", subItemT),
                    new MenuItem("50.1.2", 1, "Docs/Fascicoli", "Fascicoli", "fas fa-archive", subItemF),                    
                    new MenuItem("50.1.5", 1, "MailView/Smistamento", "Smistamento", "fas fa-map-signs", null),
                    new MenuItem("50.1.3", 1, "MailView/InArrivo", "Email", "fas fa-envelope-open-text", subItemM),
                    new MenuItem("50.1.4", 4, "ZipView/ZipInArrivo", "Altri documenti", "fas fa-file", subItemZ),       
                    new MenuItem("50.1.6", 7, "Stats/History", "Statistiche", "fas fas fa-chart-area", subItemS),
                    
                };
            }
        }

        public IEnumerable<IndexItem> IndexItem
        {
            get
            {
                // MAIL
                var myMail = new IndexItem("50.1.3", 1, "", "Mail", "MailDash", "");
                myMail.Size = new System.Drawing.Size(2, 6);
                myMail.ElementType = OBJECT_TYPE.TEMPLATE;
                myMail.Template = "dblu.Portale.Plugin.Docs.Pages.MailTile";

                // FILE
                var myFile = new IndexItem("50.1.4", 2, "", "File", "FileDash", "");
                myFile.Size = new System.Drawing.Size(2, 6);
                myFile.ElementType = OBJECT_TYPE.TEMPLATE;
                myFile.Template = "dblu.Portale.Plugin.Docs.Pages.FileTile";

                // DEALERS
                var myREQ = new IndexItem("50.1.4", 2, "", "Richieste", "FileDash", "");
                myREQ.Size = new System.Drawing.Size(2, 6);
                myREQ.ElementType = OBJECT_TYPE.TEMPLATE;
                myREQ.Template = "dblu.Portale.Plugin.Docs.Pages.RequestTile";

                // FASCICOLI
                var myFascicoli = new IndexItem("50.1.2", 3, "", "Fascicoli", "FascicoliDash", "");
                myFascicoli.Size = new System.Drawing.Size(2, 6);
                myFascicoli.ElementType = OBJECT_TYPE.TEMPLATE;
                myFascicoli.Template = "dblu.Portale.Plugin.Docs.Pages.DossierTile";

                // Smistamento
                var mySmista = new IndexItem("50.1.5", 4, "", "Smistamento", "MailDash", "");
                mySmista.Size = new System.Drawing.Size(2, 6);
                mySmista.ElementType = OBJECT_TYPE.TEMPLATE;
                mySmista.Template = "dblu.Portale.Plugin.Docs.Pages.AddressingTile";

                return new IndexItem[]
                {
                    myMail, myFile, myREQ, myFascicoli, mySmista                
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

