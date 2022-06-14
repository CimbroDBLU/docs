using FluentMigrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dblu.Portale.Plugin.Docs.DataLayer.Migrations
{
    [Migration(20220527)]
    public class AttachFolder : Migration
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
            Alter.Table("Allegati").AddColumn("Folder").AsCustom("nvarchar(max)").WithDefaultValue("").Nullable();
        }
    }
}
