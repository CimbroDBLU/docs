using System;
using System.Collections.Generic;
using System.Text;
using FluentMigrator;
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
                //TODO:
                // METTERE QUI LA CREAZIONE DEL DOCS!!
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
