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
    ///     -> Add Index for fast searching attachments
    /// </summary>
    [Migration(20210929)]
    public class Indexes : Migration
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
            Create.Index("IdxAllegati_Tipo_Stato_Origine")
                .OnTable("Allegati")
                .OnColumn("Tipo")
                .Ascending()
                .OnColumn("Stato")
                .Ascending()
                .OnColumn("Origine")
                .Ascending();
        }
    }
}
