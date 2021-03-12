using dbluDealersConnector.Workers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace dblu.Docs.Service
{
    /// <summary>
    /// Class for configuring the services hosted
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configuration">Injected configurator</param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// Injected configurator
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        ///  This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();
            services.AddHostedService<RequestSynchronizer>();
            services.AddHostedService<RequestCleaner>();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "dbluDealersConnector", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        Description = @"JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below. \r\n\r\nExample: 'Bearer 12345abcdef'",
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.ApiKey,
                        Scheme = "Bearer"
                    });

                    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                    {
                        {
                          new OpenApiSecurityScheme
                          {
                            Reference = new OpenApiReference
                              {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                              },
                              Scheme = "oauth2",
                              Name = "Bearer",
                              In = ParameterLocation.Header,

                            },
                            new List<string>()
                        }
                    });
                    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                    c.IncludeXmlComments(xmlPath);
                });


            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = true;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SigningKey"])),
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                };
            });

        }

        /// <summary>
        ///  This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDeveloperExceptionPage();            
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "dbluDealersConnector v1"));
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseAuthorization();
           
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
