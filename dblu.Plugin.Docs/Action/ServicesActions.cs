using dblu.Docs.Interfacce;
using dblu.Docs.Models;
using dblu.Portale.Plugin.Docs.Data;
using dblu.Portale.Plugin.Docs.Services;
using dblu.Portale.Plugin.Docs.Workers;
using ExtCore.Infrastructure.Actions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;


namespace dblu.Portale.Plugin.Docs.Actions
{
    public class ServicesAction : IConfigureServicesAction
    {
        public int Priority => 1000;

        public void Execute(IServiceCollection services, IServiceProvider serviceProvider)
        {
            services.AddScoped<dbluDocsContext>();
            services.AddTransient<ISoggettiService,SoggettiService>();
            services.AddScoped<AllegatiService>();
            services.AddScoped<MailService>();
            services.AddScoped<ServerMailService>();
            services.AddScoped<PrintService>();
            services.AddScoped<ZipService>();
            services.AddScoped<PdfEditService>();
            services.AddHostedService<MantenianceWorker>();
        }
    }
}
