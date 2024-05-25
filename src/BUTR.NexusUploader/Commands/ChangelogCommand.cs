using BUTR.NexusUploader.Extensions;
using BUTR.NexusUploader.Models;
using BUTR.NexusUploader.Services;
using BUTR.NexusUploader.Utils;

using Microsoft.Extensions.Logging;

using Spectre.Console;
using Spectre.Console.Cli;

using System.ComponentModel;
using System.Threading.Tasks;

namespace BUTR.NexusUploader.Commands;

public class ChangelogCommand : AsyncCommand<ChangelogCommand.Settings>
{
    private readonly ILogger _logger;
    private readonly ManageClient _client;
    private readonly ApiV1Client _apiV1;

    public ChangelogCommand(ILogger<ChangelogCommand> logger, ManageClient uploadClient, ApiV1Client apiV1Client)
    {
        _logger = logger;
        _client = uploadClient;
        _apiV1 = apiV1Client;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        _logger.LogInformation("Attempting to retrieve game details for '{Game}'", settings.Game);
        var gameId = await _apiV1.GetGameId(settings.Game, settings.ApiKey);
        var game = new GameRef(settings.Game, gameId);
        _logger.LogDebug("Game details loaded: {Game}/{GameId}", settings.Game, gameId);

        if (!await _client.AddChangelog(game, settings.ModId, settings.ModVersion, settings.ChangelogContent))
        {
            _logger.LogWarning("[bold orange3]Failed![/] There was an unknown error while updating the changelog!");
            _logger.LogWarning("Ensure that you have access to edit the requested mod and that it exists");
            return 1;
        }

        _logger.LogInformation("[green]Success![/] Your changelog has been added for version [bold]'{ModVersionn}'[/]", settings.ModVersion);
        return 0;
    }

    public class Settings : CommandSettings
    {
        [CommandOption("-k|--api-key")]
        [EnvironmentVariable("APIKEY")]
        [Description("The NexusMods API key.")]
        public string ApiKey { get; set; } = default!;

        [CommandOption("-g|--game")]
        [EnvironmentVariable("GAME")]
        [Description("The NexusMods game name (domain) to upload the mod to. Can be found in the URL of the game page.")]
        public string Game { get; set; } = default!;

        [CommandOption("-m|--mod-id")]
        [EnvironmentVariable("MODID")]
        [Description("The NexusMods mod Id to update the changelog for.")]
        public int ModId { get; set; } = default!;

        [CommandArgument(0, "<version>")]
        [Description("The version of the mod to update the changelog for.")]
        public string ModVersion { get; set; } = default!;

        [CommandOption("-c|--changelog <changelog>")]
        [EnvironmentVariable("CHANGELOG")]
        [Description("The changelog content to add.")]
        public string ChangelogContent { get; set; } = string.Empty;

        public override ValidationResult Validate()
        {
            if (!ChangelogContent.IsSet() || !ModVersion.IsSet() || !ApiKey.IsSet() || !Game.IsSet() || ModId == default)
                return ValidationResult.Error("Not all required settings provided in configuration or command line!");

            return base.Validate();
        }
    }
}