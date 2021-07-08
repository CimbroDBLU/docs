using dblu.Docs.Interfacce;
using dblu.Docs.Models;
using dblu.Portale.Core.Infrastructure.Classes;
using dblu.Portale.Plugin.Docs.Data;
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
    public class ServicesAction : IConfigureServicesAction
    {
        public int Priority => 1000;

        public void Execute(IServiceCollection services, IServiceProvider serviceProvider)
        {
            services.AddScoped<dbluDocsContext>();
            services.AddTransient<ISoggettiService, SoggettiService>();
            services.AddScoped<AllegatiService>();
            services.AddScoped<MailService>();
            services.AddScoped<ServerMailService>();
            services.AddScoped<PrintService>();
            services.AddScoped<ZipService>();
            services.AddScoped<PdfEditService>();

            /// I use the 2 methods below instead of standard below
            /// services.AddHostedService<MantenianceWorker>();
            /// since in this way i can inject the Hosted service into a Blazor controller!
            services.AddSingleton<MantenianceWorker>();
            services.AddSingleton<IHostedService>(p => p.GetService<MantenianceWorker>());

            var conf = (IConfiguration)serviceProvider.GetService(typeof(IConfiguration));
            services.AddFluentMigrator(serviceProvider, conf[$"ConnectionStrings:dblu.Docs"], typeof(dblu.Docs.DataLayer.Migrations.Init), "DOCS");
        }
   }
}
