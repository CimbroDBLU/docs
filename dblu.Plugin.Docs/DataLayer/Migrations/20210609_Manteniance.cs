using FluentMigrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dblu.Portale.Plugin.Docs.DataLayer.Migrations
{
    [Migration(20210610)]
    public class Manteniance : Migration
    {

        /// <summary>
        /// Migrate to this migration
        /// </summary>
        public override void Up()
        {
            Alter.Table("TipiAllegati")
                .AddColumn("CronPulizia")
                .AsString(50).Nullable()
                .WithDefaultValue("")
                .Nullable();

            Alter.Table("TipiAllegati")
                 .AddColumn("GiorniDaMantenere")
                 .AsInt32()
                 .WithDefaultValue(90)
                 .Nullable();
        }

        /// <summary>
        /// Roll back this Migration
        /// </summary>
        public override void Down()
        {
        }
    }
}
