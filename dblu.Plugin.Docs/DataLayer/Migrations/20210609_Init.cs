using System;
using System.Collections.Generic;
using System.Text;
using Dapper;
using FluentMigrator;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace dblu.Docs.DataLayer.Migrations
{
    [Migration(20210609)]
    public class Init : Migration
    {

        /// <summary>
        /// Migrate to this migration
        /// </summary>
        public override void Up()
        {
            if (!Schema.Table("TipiAllegati").Exists())
            {
                string dbName = "";
                var builder = new System.Data.Common.DbConnectionStringBuilder();
                builder.ConnectionString = ConnectionString;
                dbName = builder["Initial Catalog"].ToString();

      
                using (SqlConnection cn = new SqlConnection(ConnectionString))
                {
                    try
                    {
                        cn.Execute($"ALTER DATABASE  [{dbName}] ADD FILEGROUP [fsGroup] CONTAINS FILESTREAM;");
                    }
                    ///Se queste operation danno errore
                    catch (Exception) { }

                    string DefaulyPath=cn.QueryFirstOrDefault<string>("SELECT SERVERPROPERTY('InstanceDefaultDataPath') AS InstanceDefaultDataPath");
                    try
                    {
                        cn.Execute($@"ALTER DATABASE [{dbName}] ADD FILE(NAME = N'fsDocs', FILENAME = N'{DefaulyPath}fsDocs_{dbName}' ) TO FILEGROUP[fsGroup];");
                    }
                    catch (Exception) { }
                }
            
            


                Create.Table("Allegati")
                    .WithColumn("ID").AsGuid().NotNullable().PrimaryKey()
                    .WithColumn("Descrizione").AsString(255).NotNullable().WithDefaultValue("")
                    .WithColumn("NomeFile").AsString(255).Nullable()
                    .WithColumn("Tipo").AsString(20).NotNullable().WithDefaultValue("")
                    .WithColumn("IDFascicolo").AsGuid().Nullable()
                    .WithColumn("IDElemento").AsGuid().Nullable()
                    .WithColumn("Stato").AsInt16().Nullable().WithDefaultValue(1)
                    .WithColumn("Attributi").AsCustom("nvarchar(max)").WithDefaultValue("")
                    .WithColumn("Note").AsCustom("nvarchar(max)").WithDefaultValue("")
                    .WithColumn("DataC").AsDateTime2().Nullable().WithDefault(SystemMethods.CurrentDateTime)
                    .WithColumn("DataUM").AsDateTime2().Nullable().WithDefault(SystemMethods.CurrentDateTime)
                    .WithColumn("UtenteC").AsString(50).Nullable().WithDefaultValue("")
                    .WithColumn("UtenteUM").AsString(50).Nullable().WithDefaultValue("")
                    .WithColumn("Chiave1").AsString(255).NotNullable().WithDefaultValue("")
                    .WithColumn("Chiave2").AsString(255).NotNullable().WithDefaultValue("")
                    .WithColumn("Chiave3").AsString(255).NotNullable().WithDefaultValue("")
                    .WithColumn("Chiave4").AsString(255).NotNullable().WithDefaultValue("")
                    .WithColumn("Chiave5").AsString(255).NotNullable().WithDefaultValue("")
                    .WithColumn("Origine").AsString(50).Nullable().WithDefaultValue("")
                    .WithColumn("Testo").AsCustom("nvarchar(max)").Nullable().WithDefaultValue("");


                Create.Index("IX_Allegati").OnTable("Allegati").OnColumn("Origine").Ascending();

                Create.Table("AllegatiInRoles")
                    .WithColumn("Tipo").AsString(20).NotNullable().PrimaryKey()
                    .WithColumn("RoleId").AsString(64).NotNullable().PrimaryKey();


                Create.Table("Categorie")
                    .WithColumn("Codice").AsString(20).NotNullable().PrimaryKey()
                    .WithColumn("Descrizione").AsString(255).Nullable().WithDefaultValue("")
                    .WithColumn("ViewAttributi").AsString(255).Nullable().WithDefaultValue("")
                    .WithColumn("ListaAttributi").AsCustom("nvarchar(max)").NotNullable().WithDefaultValue("");

                Create.Table("Configurazione")
                    .WithColumn("Nome").AsString(10).NotNullable().PrimaryKey()
                    .WithColumn("Value").AsCustom("nvarchar(max)").Nullable();

                Create.Table("Elementi")
                    .WithColumn("ID").AsGuid().NotNullable().PrimaryKey()
                    .WithColumn("Revisione").AsInt16().NotNullable().WithDefaultValue(0)
                    .WithColumn("Tipo").AsString(20).NotNullable().WithDefaultValue("")
                    .WithColumn("Descrizione").AsString(255).NotNullable().WithDefaultValue("")
                    .WithColumn("Stato").AsInt16().NotNullable().WithDefaultValue(0)
                    .WithColumn("IDFascicolo").AsGuid().Nullable()
                    .WithColumn("Attributi").AsCustom("nvarchar(max)").WithDefaultValue("")
                    .WithColumn("Note").AsCustom("nvarchar(max)").WithDefaultValue("")
                    .WithColumn("DataC").AsDateTime2().Nullable().WithDefault(SystemMethods.CurrentDateTime)
                    .WithColumn("DataUM").AsDateTime2().Nullable().WithDefault(SystemMethods.CurrentDateTime)
                    .WithColumn("UtenteC").AsString(50).Nullable().WithDefaultValue("")
                    .WithColumn("UtenteUM").AsString(50).Nullable().WithDefaultValue("")
                    .WithColumn("Chiave1").AsString(255).NotNullable().WithDefaultValue("")
                    .WithColumn("Chiave2").AsString(255).NotNullable().WithDefaultValue("")
                    .WithColumn("Chiave3").AsString(255).NotNullable().WithDefaultValue("")
                    .WithColumn("Chiave4").AsString(255).NotNullable().WithDefaultValue("")
                    .WithColumn("Chiave5").AsString(255).NotNullable().WithDefaultValue("");

       
                Create.Table("ElementiInRoles")
                    .WithColumn("Tipo").AsString(20).NotNullable().PrimaryKey()
                    .WithColumn("RoleId").AsString(64).NotNullable().PrimaryKey();

                Create.Table("EmailServer")
                    .WithColumn("Nome").AsString(50).NotNullable().PrimaryKey()
                    .WithColumn("email").AsString(255).NotNullable().WithDefaultValue("")
                    .WithColumn("Server").AsString(50).NotNullable()
                    .WithColumn("Porta").AsInt32().NotNullable()
                    .WithColumn("SSL").AsBoolean().NotNullable().WithDefaultValue(false)
                    .WithColumn("Utente").AsString(50).NotNullable()
                    .WithColumn("Password").AsString(50).NotNullable()
                    .WithColumn("Intervallo").AsInt32().NotNullable().WithDefaultValue(1000)
                    .WithColumn("Attivo").AsBoolean().NotNullable().WithDefaultValue(false)
                    .WithColumn("Cartella").AsString(50).NotNullable().WithDefaultValue("")
                    .WithColumn("InUscita").AsBoolean().NotNullable().WithDefaultValue(false)
                    .WithColumn("NomeProcesso").AsString(255).NotNullable().WithDefaultValue("")
                    .WithColumn("CartellaArchivio").AsString(50).NotNullable().WithDefaultValue("")
                    .WithColumn("NomeServerInUscita").AsString(50).NotNullable().WithDefaultValue("")
                    .WithColumn("TipoRecord").AsInt16().Nullable().WithDefaultValue(0);

                Create.Table("EmailSoggetti")
                   .WithColumn("email").AsString(255).NotNullable().PrimaryKey()
                   .WithColumn("CodiceSoggetto").AsString(20).Nullable();

                Create.Index("IX_EmailSoggetti_CodiceSoggetto").OnTable("EmailSoggetti").OnColumn("CodiceSoggetto").Ascending();

                Create.Table("Fascicoli")
                    .WithColumn("ID").AsGuid().NotNullable().PrimaryKey()
                    .WithColumn("Descrizione").AsString(255).NotNullable().WithDefaultValue("")
                    .WithColumn("Categoria").AsString(20).NotNullable().WithDefaultValue("")
                    .WithColumn("Chiave1").AsString(255).NotNullable().WithDefaultValue("")
                    .WithColumn("Chiave2").AsString(255).NotNullable().WithDefaultValue("")
                    .WithColumn("Chiave3").AsString(255).NotNullable().WithDefaultValue("")
                    .WithColumn("Chiave4").AsString(255).NotNullable().WithDefaultValue("")
                    .WithColumn("Chiave5").AsString(255).NotNullable().WithDefaultValue("")
                    .WithColumn("DataC").AsDateTime2().Nullable().WithDefault(SystemMethods.CurrentDateTime)
                    .WithColumn("DataUM").AsDateTime2().Nullable().WithDefault(SystemMethods.CurrentDateTime)
                    .WithColumn("UtenteC").AsString(50).Nullable().WithDefaultValue("")
                    .WithColumn("UtenteUM").AsString(50).Nullable().WithDefaultValue("")
                    .WithColumn("Attributi").AsCustom("nvarchar(max)").NotNullable().WithDefaultValue("")
                    .WithColumn("CodiceSoggetto").AsString(20).Nullable();

                Create.Table("LogDoc")
                     .WithColumn("IdOggetto").AsGuid().Nullable()
                     .WithColumn("Data").AsDateTime2().Nullable()
                     .WithColumn("TipoOggetto").AsInt16().Nullable()                     
                     .WithColumn("Utente").AsString(20).Nullable().WithDefaultValue("")
                     .WithColumn("Operazione").AsInt16().Nullable();

                Create.Index("IX_LogDoc_id_data").OnTable("LogDoc")
                     .OnColumn("IdOggetto").Ascending()
                     .OnColumn("Data").Descending();

                Create.Table("ServersInRole")
                     .WithColumn("idServer").AsString(50).Nullable()
                     .WithColumn("RoleId").AsString(64).NotNullable();

                Create.Table("Soggetti")
                    .WithColumn("Codice").AsString(20).NotNullable().WithDefaultValue("").PrimaryKey()
                    .WithColumn("Nome").AsString(255).NotNullable().WithDefaultValue("")
                    .WithColumn("Indirizzo").AsString(255).Nullable().WithDefaultValue("")
                    .WithColumn("CAP").AsString(20).Nullable().WithDefaultValue("")
                    .WithColumn("Localita").AsString(255).Nullable().WithDefaultValue("")
                    .WithColumn("Provincia").AsString(20).Nullable().WithDefaultValue("")
                    .WithColumn("Note").AsString(255).Nullable().WithDefaultValue("")
                    .WithColumn("Stato").AsInt32().NotNullable()
                    .WithColumn("DataC").AsDateTime2().Nullable().WithDefault(SystemMethods.CurrentDateTime)
                    .WithColumn("DataUM").AsDateTime2().Nullable().WithDefault(SystemMethods.CurrentDateTime)
                    .WithColumn("UtenteC").AsString(50).Nullable().WithDefaultValue("")
                    .WithColumn("UtenteUM").AsString(50).Nullable().WithDefaultValue("")
                    .WithColumn("Attributi").AsCustom("nvarchar(max)").NotNullable().WithDefaultValue("")
                    .WithColumn("Nazione").AsString(20).Nullable().WithDefaultValue("")
                    .WithColumn("Nomignolo").AsString(50).Nullable().WithDefaultValue("")
                    .WithColumn("PartitaIVA").AsString(20).Nullable().WithDefaultValue("")
                    .WithColumn("NuovoCodice").AsString(20).Nullable().WithDefaultValue("");

                Create.Table("TipiAllegati")
                    .WithColumn("Codice").AsString(20).NotNullable().PrimaryKey()
                    .WithColumn("Descrizione").AsString(255).Nullable()
                    .WithColumn("Cartella").AsString(255).Nullable()
                    .WithColumn("ViewAttributi").AsString(255).Nullable().WithDefaultValue("")
                    .WithColumn("ListaAttributi").AsCustom("nvarchar(max)").NotNullable().WithDefaultValue("")
                    .WithColumn("Estensione").AsString(10).Nullable().WithDefaultValue("");

                Create.Table("TipiElementi")
                   .WithColumn("Codice").AsString(20).NotNullable().PrimaryKey()
                   .WithColumn("Descrizione").AsString(255).Nullable()
                   .WithColumn("ViewAttributi").AsString(255).Nullable().WithDefaultValue("")
                   .WithColumn("ListaAttributi").AsCustom("nvarchar(max)").NotNullable().WithDefaultValue("")
                   .WithColumn("Categoria").AsString(20).Nullable().WithDefaultValue(null)
                   .WithColumn("Processo").AsString(255).Nullable()
                   .WithColumn("AggregaAElemento").AsInt16().Nullable().WithDefaultValue(0)
                   .WithColumn("RuoliCandidati").AsCustom("nvarchar(max)").Nullable()
                   .WithColumn("UtentiCandidati").AsCustom("nvarchar(max)").Nullable();

                Create.Index("IX_TipiElementi_Categoria").OnTable("TipiElementi")
                    .OnColumn("Categoria").Ascending();

                Create.ForeignKey("FK_Allegati_Fascicoli")
                    .FromTable("Allegati").ForeignColumn("IDFascicolo")
                    .ToTable("Fascicoli").PrimaryColumn("ID")
                    .OnDeleteOrUpdate(System.Data.Rule.Cascade);

                Create.ForeignKey("FK_Allegati_TipiAllegati")
                    .FromTable("Allegati").ForeignColumn("Tipo")
                    .ToTable("TipiAllegati").PrimaryColumn("Codice");

                Create.ForeignKey("FK_Elementi_Fascicoli")
                   .FromTable("Elementi").ForeignColumn("IDFascicolo")
                   .ToTable("Fascicoli").PrimaryColumn("ID")
                   .OnDeleteOrUpdate(System.Data.Rule.Cascade);

                Create.ForeignKey("FK_Elementi_TipiElementi")
                    .FromTable("Elementi").ForeignColumn("Tipo")
                    .ToTable("TipiElementi").PrimaryColumn("Codice");

                //Create.ForeignKey("FK_Email_Soggetti")
                //    .FromTable("EmailSoggetti").ForeignColumn("CodiceSoggetto")
                //    .ToTable("Soggetti").PrimaryColumn("Codice");

                Create.ForeignKey("FK_TipiElementi_Categorie")
                    .FromTable("TipiElementi").ForeignColumn("Categoria")
                    .ToTable("Categorie").PrimaryColumn("Codice");

                Create.ForeignKey("FK_Fascicoli_Categorie")
                    .FromTable("Fascicoli").ForeignColumn("Categoria")
                    .ToTable("Categorie").PrimaryColumn("Codice")
                    .OnDeleteOrUpdate(System.Data.Rule.SetDefault);

                Execute.Sql("CREATE TABLE [dbo].[Docs] AS FILETABLE FILESTREAM_ON [fsGroup] WITH(FILETABLE_COLLATE_FILENAME = Latin1_General_CI_AS, FILETABLE_DIRECTORY = N'Docs');");

                Execute.EmbeddedScript(@$"sp_AggDoc.sql");
                Execute.EmbeddedScript(@$"sp_CancDoc.sql");
                Execute.EmbeddedScript(@$"sp_GetSoggettoEmail.sql");
                Execute.EmbeddedScript(@$"sp_InsDoc.sql");

                Execute.EmbeddedScript(@$"vDocs.sql");
                Execute.EmbeddedScript(@$"vElementi.sql");
                Execute.EmbeddedScript(@$"vListaAllegati.sql");
                Execute.EmbeddedScript(@$"vListaElementi.sql");
                Execute.EmbeddedScript(@$"vListaFascicoli.sql");
                Execute.EmbeddedScript(@$"vRicercaElementi.sql");
            }
            else
            {

            }
        }

        /// <summary>
        /// Roll back this Migration
        /// </summary>
        public override void Down()
        {
        }
    }
}
