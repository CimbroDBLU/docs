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
    ///     -> Add Fields on Processi
    /// </summary>
    [Migration(20211124)]
    public class HistoryVars : Migration
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
            Alter.Table("Processi")
               .AddColumn("IdElemento").AsString(36).Nullable()
               .AddColumn("IdAllegato").AsString(36).Nullable()
               .AddColumn("JAttributi").AsCustom("nvarchar(max)").WithDefaultValue("").Nullable();

        }
    }
}
