using BUTR.NexusUploader.Utils;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Spectre.Console.Cli;

using System;
using System.Threading.Tasks;

namespace BUTR.NexusUploader.Extensions;

public static class HostingExtensions
{
    public static IServiceCollection AddCommandLine(this IServiceCollection services, Action<IConfigurator> configurator)
    {
        var app = new CommandApp(new TypeRegistrar(services));
        app.Configure(configurator);
        services.AddSingleton<ICommandApp>(app);

        return services;
    }

    public static async Task<int> RunAsync(this IHost host, string[] args)
    {
        ArgumentNullException.ThrowIfNull(host);

        var app = host.Services.GetService<ICommandApp>();
        if (app == null)
            throw new InvalidOperationException("Command application has not been configured.");

        return await app.RunAsync(args);
    }
}