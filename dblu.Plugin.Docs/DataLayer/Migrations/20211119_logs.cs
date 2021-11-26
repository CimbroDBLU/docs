using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentMigrator;

namespace dblu.Portale.Plugin.Docs.DataLayer.Migrations
{
    /// <summary>
    /// Migration:
    ///     -> Add Fields on Attivita and Logs
    /// </summary>
    [Migration(20211119)]
    public class Docs_logs : Migration
    {
        /// <summary>
        /// Roll back this Migration
        /// </summary>
        public override void Down()
        {

        }

        /// <summary>
        /// Migrate to this migration
        /// </summary>
        public override void Up()
        {
            Alter.Table("LogDoc")
               .AddColumn("Descrizione").AsString(200).WithDefaultValue("").Nullable()
               .AddColumn("JAttributi").AsCustom("nvarchar(max)").WithDefaultValue("").Nullable();

            Alter.Table("Attivita")
               .AddColumn("IdElemento").AsString(36).Nullable()
               .AddColumn("IdAllegato").AsString(36).Nullable()
               .AddColumn("JAttributi").AsCustom("nvarchar(max)").WithDefaultValue("").Nullable();

        }
    }
}
