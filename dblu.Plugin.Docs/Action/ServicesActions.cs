using dblu.Docs.Classi;
using dblu.Docs.Interfacce;
using dblu.Docs.Models;
using dblu.Portale.Core.Infrastructure.Classes;
using dblu.Portale.Plugin.Docs.Services;
using dblu.Portale.Plugin.Docs.Workers;
using ExtCore.Infrastructure.Actions;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;

namespace dblu.Portale.Plugin.Docs.Actions
{
    /// <summary>
    /// Class for register services of this plug in
    /// </summary>
    public class ServicesAction : IConfigureServicesAction
    {
        /// <summary>
        /// Registration priority
        /// </summary>
        public int Priority => 1001;

        /// <summary>
        /// Registration function
        /// </summary>
        /// <param name="services">Service collection of the server</param>
        /// <param name="serviceProvider">Service provider of the service</param>
        public void Execute(IServiceCollection services, IServiceProvider serviceProvider)
        {
            services.AddDbContext<dbluDocsContext>();
            services.AddTransient<ISoggettiService, SoggettiService>();
            services.AddScoped<AllegatiService>();
            services.AddScoped<MailService>();
            services.AddScoped<DocumentTransformationService>();
            services.AddScoped<ServerMailService>();
            services.AddScoped<PrintService>();
            services.AddScoped<ZipService>();
            services.AddScoped<PdfEditService>();

            /// I use the 2 methods below instead of standard below
            /// services.AddHostedService<MantenianceWorker>();
            /// since in this way i can inject the Hosted service into a Blazor controller!
            services.AddSingleton<MantenianceWorker>();
            services.AddSingleton<IHostedService>(p => p.GetService<MantenianceWorker>());

            ///Add a background workor fpor importing camunda data
            services.AddSingleton<HistoryWorker>();
            services.AddSingleton<IHostedService>(p => p.GetService<HistoryWorker>());

            var conf = (IConfiguration)serviceProvider.GetService(typeof(IConfiguration));
            services.AddFluentMigrator(serviceProvider, conf[$"ConnectionStrings:dblu.Docs"], typeof(dblu.Docs.DataLayer.Migrations.Init), "DOCS");
        }
   }
}
