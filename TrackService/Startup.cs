using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Lib.AspNetCore.ServerSentEvents;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TrackService.IServices;
using TrackService.Models;
using TrackService.Services;
using RethinkDb.Driver;
using RethinkDb.Driver.Net;
using Microsoft.Extensions.Options;
using TrackService.RethinkDb_Changefeed.Model;
using TrackService.RethinkDb_Abstractions;
using RethinkDb.Driver.Ast;
using TrackService.RethinkDb_Changefeed;

namespace TrackService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            #region RethinkDB
            services.AddRethinkDb(options =>
            {
                options.Host = "127.0.0.1";
            });
            #endregion
            services.AddServerSentEvents();
            services.AddThreadStats();
            services.AddSignalR();
            services.Configure<RethinkDbOptions>(Configuration.GetSection("RethinkDbDev"));
            services.AddSingleton<IRethinkDbConnectionFactory, RethinkDbConnectionFactory>();

            services.AddSingleton<IRethinkDbStore, RethinkDbStore>();
            services.AddSingleton<TrackServiceHub, TrackServiceHub>();
            services.AddCors(c =>
            {
                c.AddPolicy("AllowOrigin", options => options.AllowAnyOrigin());
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory, IRethinkDbConnectionFactory connectionFactory, IRethinkDbStore store)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            app.UseCors(builder => builder
                .AllowAnyHeader()
                .AllowAnyMethod()
                .SetIsOriginAllowed(_ => true)
                .AllowCredentials()
            );
            store.InitializeDatabase();
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<TrackServiceHub>("/trackServiceHub");
            });
        }
    }
}
