using NLog.Web;
using NLog;
using RPPP_WebApp;
using Microsoft.EntityFrameworkCore;
using RPPP_WebApp.Models;
using Microsoft.OpenApi.Models;
using System.Reflection;
//NOTE: Add dependencies/services in StartupExtensions.cs and keep this file as-is


var builder = WebApplication.CreateBuilder(args);
var logger = LogManager.Setup().GetCurrentClassLogger();
logger.Debug("init main");
try
{
    logger.Debug("init main");

    builder.Host.UseNLog(new NLogAspNetCoreOptions() { RemoveLoggerFactoryFilter = false });

    #region Swagger Api config
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "RPP13 Web API",
            Version = "v1"
        });
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        c.IncludeXmlComments(xmlPath);
    });



    #endregion

    var app = builder.ConfigureServices().ConfigurePipeline();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("../swagger/v1/swagger.json",
"RPPP13 WebAPI");
        c.DocumentTitle = "RPPP13 Web Api";
        c.RoutePrefix = "docs";
    });
    app.Run();

}
catch (Exception exception)
{
  // NLog: catch setup errors
  logger.Error(exception, "Stopped program because of exception");
  throw;
}
finally
{
  // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
  NLog.LogManager.Shutdown();
}

public partial class Program { }