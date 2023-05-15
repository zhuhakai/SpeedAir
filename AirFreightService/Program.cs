
using AirFreightService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;




using var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((ctx, config) => {

        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        config.AddJsonFile($"appsettings.{ctx.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true);
        config.AddEnvironmentVariables();
        if (args != null)
            config.AddCommandLine(args);
    })
    .ConfigureServices((ctx, services) =>
    {
        services.AddSingleton<ServiceUtil>();

        services.AddSingleton<App>();
    }).Build();


using var scope = host.Services.CreateScope();

var services = scope.ServiceProvider;
var logger = services.GetRequiredService<ILogger<Program>>();

try
{

    await services.GetRequiredService<App>().RunAsync(args);
}
catch (Exception ex)
{
    logger.LogError(ex, "Program failed");
}