using FluentMigrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dblu.Portale.Plugin.Docs.DataLayer.Migrations
{
    /// <summary>
    /// Migration:
    ///     -> Add Support for History
    ///     -> Add Description and Position on Email Server
    /// </summary>
    [Migration(20210730)]
    public class History : Migration
    {
        /// <summary>
        /// Migrate to this migration
        /// </summary>
        public override void Up()
        {
            Alter.Table("EmailServer")
                .AddColumn("Descrizione")
                .AsString(255)
                .WithDefaultValue("")
                .Nullable();

            Alter.Table("EmailServer")
                .AddColumn("Posizione")
                .AsInt32()
                .WithDefaultValue("")
                .Nullable();

            Create.Table("Processi")
                .WithColumn("Id").AsString(36).PrimaryKey()
                .WithColumn("Nome").AsCustom("nvarchar(max)").Nullable()
                .WithColumn("Descrizione").AsCustom("nvarchar(max)").Nullable()
                .WithColumn("Avvio").AsDateTime2().Nullable()
                .WithColumn("Fine").AsDateTime2().Nullable()
                .WithColumn("UtenteAvvio").AsString(250).Nullable()
                .WithColumn("Stato").AsString(250).Nullable()
                .WithColumn("Diagramma").AsString(250).Nullable()
                .WithColumn("Versione").AsString(250).Nullable()
                .WithColumn("DataC").AsDateTime2().Nullable()
                .WithColumn("UtenteC").AsString(20).Nullable()
                .WithColumn("DataUM").AsDateTime2().Nullable()
                .WithColumn("UtenteUM").AsString(20).Nullable();

            Create.Table("Attivita")
                .WithColumn("Id").AsString(36).PrimaryKey()
                .WithColumn("Nome").AsCustom("nvarchar(max)").Nullable()
                .WithColumn("Descrizione").AsCustom("nvarchar(max)").Nullable()
                .WithColumn("Avvio").AsDateTime2().Nullable()
                .WithColumn("Fine").AsDateTime2().Nullable()
                .WithColumn("Assegnatario").AsString(250).Nullable()
                .WithColumn("Stato").AsString(250).Nullable()
                .WithColumn("DataC").AsDateTime2().Nullable()
                .WithColumn("UtenteC").AsString(20).Nullable()
                .WithColumn("DataUM").AsDateTime2().Nullable()
                .WithColumn("UtenteUM").AsString(20).Nullable()
                .WithColumn("IdProcesso").AsString(36).PrimaryKey();

            Create.ForeignKey("FK_Attivita_IdProcesso_Processi_Id")
                 .FromTable("Attivita").ForeignColumn("IdProcesso")
                 .ToTable("Processi").PrimaryColumn("Id");
        }

        /// <summary>
        /// Roll back this Migration
        /// </summary>
        public override void Down()
        {
        }
    }
}
