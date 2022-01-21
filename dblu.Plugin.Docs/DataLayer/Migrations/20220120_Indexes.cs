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
    [Migration(20220120)]
    public class Indexes2 : Migration
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
            Create.Index("IdxElementi_Chiave1")
                .OnTable("Elementi")
                .OnColumn("Chiave1");

            Create.Index("IdxElementi_Chiave2")
                .OnTable("Elementi")
                .OnColumn("Chiave2");

            Create.Index("IdxElementi_Chiave3")
                .OnTable("Elementi")
                .OnColumn("Chiave3");

           Create.Index("IdxElementi_Chiave4")
                .OnTable("Elementi")
                .OnColumn("Chiave4");

            Create.Index("IdxElementi_Chiave5")
                 .OnTable("Elementi")
                 .OnColumn("Chiave5");
        }
    }
}
