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
    ///     -> Add Index for fast searching elements
    /// </summary>
    [Migration(20220504)]
    public class ItemsType : Migration
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
            Alter.Table("TipiElementi")
                .AddColumn("Posizione")
                .AsInt32()
                .WithDefaultValue("")
                .Nullable();

            Alter.Table("TipiElementi")
                .AddColumn("Abilita")
                .AsInt32()
                .WithDefaultValue("0")
                .Nullable();

            Alter.Table("Elementi")
                .AddColumn("ElemRif")
                .AsGuid()                
                .Nullable();

        }
    }
}
