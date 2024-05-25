using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using NexusUploader.Commands;
using NexusUploader.Extensions;
using NexusUploader.Services;
using NexusUploader.Utils;

using Spectre.Console.Cli;

using System;
using System.Threading.Tasks;

namespace NexusUploader;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        return await CreateHostBuilder()
            .Build()
            .RunAsync(args);
    }

    private static IHostBuilder CreateHostBuilder() => Host.CreateDefaultBuilder()
        .ConfigureAppConfiguration((ctx, builder) =>
        {
            builder.AddFile("unex");
            builder.AddEnvironmentVariables("UNEX_");
        })
        .ConfigureLogging((ctx, logging) =>
        {
            logging.SetMinimumLevel(LogLevel.Trace);
            logging.ClearProviders();
            logging.AddInlineSpectreConsole(c => { c.LogLevel = GetLogLevel(); });
            logging.AddFilter("System.Net.Http", LogLevel.Warning);
        })
        .ConfigureServices((ctx, services) =>
        {
            var assemblyName = typeof(Program).Assembly.GetName();
            var userAgent = $"{assemblyName.Name ?? "ERROR"} v{assemblyName.Version?.ToString() ?? "ERROR"} (github.com/BUTR)";

            services.AddHttpClient<UsersClient>(client =>
            {
                client.BaseAddress = new Uri("https://users.nexusmods.com");
                client.DefaultRequestHeaders.Add("User-Agent", userAgent);
            }).ConfigurePrimaryHttpMessageHandler<NexusCookieHandler>();

            services.AddHttpClient<ManageClient>(client =>
            {
                client.BaseAddress = new Uri("https://www.nexusmods.com");
                client.DefaultRequestHeaders.Add("User-Agent", userAgent);
            }).ConfigurePrimaryHttpMessageHandler<NexusCookieHandler>();

            services.AddHttpClient<UploadClient>(client =>
            {
                client.BaseAddress = new Uri("https://upload.nexusmods.com");
                client.DefaultRequestHeaders.Add("User-Agent", userAgent);
            }).ConfigurePrimaryHttpMessageHandler<NexusCookieHandler>();

            services.AddHttpClient<ApiV1Client>(client =>
            {
                client.BaseAddress = new Uri("https://api.nexusmods.com");
                client.DefaultRequestHeaders.Add("User-Agent", userAgent);
            });

            services.AddSingleton<NexusCookieHandler>();
            services.AddSingleton<CookieService>();

            // Add command line with default command
            services.AddCommandLine(config =>
            {
                config.SetApplicationName("unex");

                if (GetLogLevel() < LogLevel.Information)
                    config.PropagateExceptions();

                config.AddCommand<ChangelogCommand>("changelog")
                    .WithDescription("Add a changelog entry for a specific mod version")
                    .WithExample(["changelog", "<version>", "-c", "<changelog>"]);

                config.AddCommand<UploadCommand>("upload")
                    .WithDescription("Upload a mod")
                    .WithExample(["upload", "<mod-id>", "<archive-file>", "-v", "<version>"]);

                config.AddCommand<CheckCommand>("check")
                    .WithDescription("Check the validity on an API Key and/or Session Cookie")
                    .WithExample(["check", "-s", "<session-cookie>", "-k", "<api-key>"]);

                config.AddCommand<RefreshCommand>("refresh")
                    .WithDescription("Refresh the session cookie")
                    .WithExample(["refresh", "-s", "<session-cookie>"]);
            });
        });

    private static LogLevel GetLogLevel()
    {
        var envVar = Environment.GetEnvironmentVariable("UNEX_DEBUG");
        return string.IsNullOrWhiteSpace(envVar)
            ? LogLevel.Information
            : envVar.ToLower() == "trace"
                ? LogLevel.Trace
                : LogLevel.Debug;
    }
}