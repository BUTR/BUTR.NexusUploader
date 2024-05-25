using Microsoft.Extensions.Logging;

using NexusUploader.Extensions;
using NexusUploader.Services;
using NexusUploader.Utils;

using Spectre.Console;
using Spectre.Console.Cli;

using System.ComponentModel;
using System.Threading.Tasks;

namespace NexusUploader.Commands;

public class CheckCommand : AsyncCommand<CheckCommand.Settings>
{
    private readonly ILogger _logger;
    private readonly CookieService _cookieService;
    private readonly ApiV1Client _apiV1Client;
    private readonly ManageClient _manager;

    public CheckCommand(ILogger<CheckCommand> logger, CookieService cookieService, ApiV1Client apiV1Client, ManageClient manager)
    {
        _logger = logger;
        _cookieService = cookieService;
        _apiV1Client = apiV1Client;
        _manager = manager;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        _cookieService.SetSessionCookie(settings.SessionCookie);

        var apiValid = true;
        var ckValid = true;
        
        if (settings.ApiKey.IsSet())
        {
            apiValid = await _apiV1Client.CheckValidKey(settings.ApiKey);
            if (apiValid)
            {
                _logger.LogInformation("[green]API key successfully validated![/]");
            }
            else
            {
                _logger.LogWarning("[orange3]API key validation [bold]failed![/][/]");
            }
        }

        if (settings.SessionCookie.IsSet())
        {
            ckValid = await _manager.CheckValidSession();
            if (ckValid)
            {
                _logger.LogInformation("[green]Cookies successfully validated![/]");
            }
            else
            {
                _logger.LogWarning("[orange3]Cookie validation [bold]failed![/][/]");
            }
        }

        return ckValid && apiValid ? 0 : 1;
    }

    public class Settings : CommandSettings
    {
        [CommandOption("-k|--api-key")]
        [EnvironmentVariable("APIKEY")]
        [Description("The NexusMods API key.")]
        public string ApiKey { get; set; } = default!;

        [CommandOption("-s|--session-cookie")]
        [EnvironmentVariable("SESSION_COOKIE")]
        [Description("Value of the 'nexusmods_session' cookie. Can be a file path or the raw cookie value.")]
        public string SessionCookie { get; set; } = default!;

        public override ValidationResult Validate()
        {
            if (!ApiKey.IsSet() && !SessionCookie.IsSet())
                return ValidationResult.Error("You must specify either --key or --cookie to check API keys or cookies respectively.");

            return base.Validate();
        }
    }
}