using System;
using ArchiveTrackService.Abstraction;
using ArchiveTrackService.Helper.CronJobServices;
using ArchiveTrackService.Helper.CronJobServices.CronJobExtensionMethods;
using ArchiveTrackService.Models.DBModels;
using ArchiveTrackService.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ArchiveTrackService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddCronJob<RemoveSynced>(c =>
            {
                c.TimeZoneInfo = TimeZoneInfo.Local;
                //c.CronExpression = @"*/1 * * * * *";
                c.CronExpression = @"0 3 1 */2 *"; //  Run every 60 days at 3 AM
            });

            services.AddDbContext<archivetrackserviceContext>(options =>
            {
                options.UseMySql(Configuration.GetConnectionString("DefaultConnection"));
            });

            //Registered services
            services.AddScoped<ICoordinateRepository, CoordinateRepository>();
            services.AddScoped<IVehicleRepository, VehicleRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

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
