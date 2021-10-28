﻿using dblu.Docs.Models;
using ExtCore.Infrastructure.Actions;
using ExtCore.Mvc.Infrastructure.Actions;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.OData.Edm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dblu.Portale.Plugin.Docs.Action
{
    /// <summary>
    /// Class for configure services of this plug in
    /// </summary>
    public class ServicesConfigure : IConfigureAction
    {
        /// <summary>
        /// Configuration priority
        /// </summary>
        public int Priority => 1000;

        /// <summary>
        /// Configure the service injected fort this plug in
        /// </summary>
        /// <param name="applicationBuilder">Application builder of this program</param>
        /// <param name="serviceProvider">Service provider of this program</param>
        public void Execute(IApplicationBuilder applicationBuilder, IServiceProvider serviceProvider)
        {
            applicationBuilder.UseEndpoints(endpoints =>
            {
              endpoints.MapODataRoute("Dossiers", "Dossiers", GetDossiersEDMModel()).Select().Count().Filter().OrderBy().MaxTop(100).SkipToken().Expand();
              endpoints.MapODataRoute("History", "History", GetHistoryEDMModel()).Select().Count().Filter().OrderBy().MaxTop(100).SkipToken().Expand();
              endpoints.MapODataRoute("Attachs", "Attachs", GetMailsEDMModel()).Select().Count().Filter().OrderBy().MaxTop(100).SkipToken().Expand();
          //    endpoints.MapODataRoute("Files", "Files", GetFilesEDMModel()).Select().Count().Filter().OrderBy().MaxTop(100).SkipToken().Expand();
            });
        }

        /// <summary>
        /// Make a model for the Dossier entities
        /// </summary>
        /// <returns>Return the EDM model for the service</returns>
        IEdmModel GetDossiersEDMModel()
        {
            var odataBuilder = new ODataConventionModelBuilder();
            odataBuilder.EntitySet<viewFascicoli>("ODATA_Dossiers");
            return odataBuilder.GetEdmModel();
        }

        /// <summary>
        /// Make a model for the History entities
        /// </summary>
        /// <returns>Return the EDM model for the service</returns>
        IEdmModel GetHistoryEDMModel()
        {
            var odataBuilder = new ODataConventionModelBuilder();
            odataBuilder.EntitySet<Processi>("ODATA_History");
            return odataBuilder.GetEdmModel();
        }


        IEdmModel GetMailsEDMModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<AllegatoEmail>("ODATA_Mails");
            var function = builder.Function("GetMails");
            function.Parameter<string>("Type").Required();
            function.Parameter<string>("MailBox").Required();
            function.ReturnsCollectionFromEntitySet<AllegatoEmail>("ODATA_Mails");

            builder.EntitySet<Allegati>("ODATA_Files");
            function = builder.Function("GetFiles");
            function.Parameter<string>("Doc").Required();
            function.Parameter<string>("Type").Required();
            function.Parameter<string>("Folder").Required();
            function.ReturnsCollectionFromEntitySet<Allegati>("ODATA_Files");

            return builder.GetEdmModel();
        }

        //IEdmModel GetFilesEDMModel()
        //{
        //    var builder = new ODataConventionModelBuilder();
        //    builder.EntitySet<Allegati>("ODATA_Files");
        //    var function = builder.Function("GetFiles");
        //    function.Parameter<string>("Doc").Required();
        //    function.Parameter<string>("Type").Required();
        //    function.Parameter<string>("Folder").Required();
        //    function.ReturnsCollectionFromEntitySet<Allegati>("ODATA_Files");


        //    return builder.GetEdmModel();
        //}

    }
}
