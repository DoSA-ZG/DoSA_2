using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RPPP_WebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RPPP_WebApp
{
  public static class StartupExtensions
  {
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
      builder.Services.AddControllersWithViews();
            var appSection = builder.Configuration.GetSection("AppSettings");
            builder.Services.Configure<AppSettings>(appSection);
            builder.Services.AddDbContext<RPPP13Context>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("rppp13")));

            builder.Services.AddControllersWithViews();
            

            return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
      #region Needed for nginx and Kestrel (do not remove or change this region)
      app.UseForwardedHeaders(new ForwardedHeadersOptions
      {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                           ForwardedHeaders.XForwardedProto
      });
      string pathBase = app.Configuration["PathBase"];
      if (!string.IsNullOrWhiteSpace(pathBase))
      {
        app.UsePathBase(pathBase);
      }
      #endregion

      if (app.Environment.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }

      app.UseStaticFiles()
         .UseRouting()
         .UseEndpoints(endpoints =>
         {
           endpoints.MapDefaultControllerRoute();
         });
        app.MapDefaultControllerRoute();
      return app;
    }
  }
}