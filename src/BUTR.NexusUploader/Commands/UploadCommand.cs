using BUTR.NexusUploader.Extensions;
using BUTR.NexusUploader.Models;
using BUTR.NexusUploader.Services;
using BUTR.NexusUploader.Utils;

using HandlebarsDotNet;

using Microsoft.Extensions.Logging;

using Spectre.Console;
using Spectre.Console.Cli;

using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

using FileOptions = BUTR.NexusUploader.Models.FileOptions;

namespace BUTR.NexusUploader.Commands;

public class UploadCommand : AsyncCommand<UploadCommand.Settings>
{
    private readonly ILogger _logger;
    private readonly ManageClient _manager;
    private readonly ApiV1Client _apiV1Client;
    private readonly UploadClient _uploader;

    public UploadCommand(ILogger<UploadCommand> logger, ManageClient client, ApiV1Client apiV1Client, UploadClient uploader)
    {
        _logger = logger;
        _manager = client;
        _apiV1Client = apiV1Client;
        _uploader = uploader;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var fileOpts = new FileOptions(settings.FileName, settings.FileVersion)
        {
            Description = settings.FileDescription
        };

        try
        {
            var compAction = Handlebars.Compile(fileOpts.Description);
            var result = compAction?.Invoke(fileOpts);
            if (!string.IsNullOrWhiteSpace(result) && !string.Equals(fileOpts.Description, result))
            {
                AnsiConsole.MarkupLine("Compiled description template using current file options.");
                fileOpts.Description = result;
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[bold red]ERROR[/]: {ex.Message}");
        }

        if (settings.RemoveDownloadWithManager.IsSet)
        {
            _logger.LogInformation("Remove Download with Manager option: {RemoveDownloadWithManager}", settings.RemoveDownloadWithManager.Value);
            fileOpts.RemoveDownloadWithManager = settings.RemoveDownloadWithManager.Value;
        }

        if (settings.SkipMainVersionUpdate is { IsSet: true, Value: true })
        {
            _logger.LogWarning("Skipping mod version update!");
            fileOpts.UpdateMainVersion = false;
        }

        if (settings.SetMainVortexFile.IsSet)
        {
            _logger.LogInformation("Setting file as main Vortex file: {SetMainVortexFile}", settings.SetMainVortexFile.Value);
            fileOpts.SetAsMainVortex = settings.SetMainVortexFile.Value;
        }

        _logger.LogInformation("Attempting to retrieve game details for \'{ConfigGame}\'", settings.Game);
        var gameId = await _apiV1Client.GetGameId(settings.Game, settings.ApiKey);
        var game = new GameRef(settings.Game, gameId);
        _logger.LogInformation("Game details loaded: {Game}/{GameId}", settings.Game, gameId);

        if (!string.IsNullOrWhiteSpace(settings.PreviousFile))
        {
            if (string.Equals(settings.PreviousFile, "auto", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("Automatic file update detection enabled! Retrieving previous file versions");
                var fileId = await _apiV1Client.GetLatestFileId(settings.Game, settings.ModId, settings.ApiKey);
                if (fileId.HasValue)
                {
                    _logger.LogInformation("Uploaded file will replace existing file \'{FileId}", fileId.Value);
                    fileOpts.PreviousFileId = fileId;
                }
            }
            else
            {
                if (int.TryParse(settings.PreviousFile, out var previousId))
                {
                    _logger.LogInformation("Uploaded file will replace existing file \'{PreviousId}", previousId);
                    fileOpts.PreviousFileId = previousId;
                }
            }
        }

        _logger.LogInformation("Preparing to upload \'{ModFilePath}\' to Nexus Mods upload API", settings.ModFilePath);
        var upload = await _uploader.UploadFile(game, settings.ModId, new FileInfo(Path.GetFullPath(settings.ModFilePath)));
        _logger.LogInformation("File successfully uploaded to Nexus Mods with ID \'{Id}\'", upload.Id);

        var available = await _uploader.CheckStatus(upload);
        _logger.LogDebug("File \'{Id}\' confirmed as assembled: {Available}", upload.Id, available);

        _logger.LogInformation("Adding uploaded file to mod {ModId}", settings.ModId);
        _logger.LogDebug("Using file options: {FileOpts}", fileOpts.ToString());
        if (!await _manager.AddFile(game, settings.ModId, upload, fileOpts))
        {
            _logger.LogWarning("There was an error adding {OriginalFile} to mod {ModId}!", upload.OriginalFile, settings.ModId);
            return 1;
        }

        _logger.LogInformation("{OriginalFile} successfully uploaded and added to mod {ModId}!", upload.OriginalFile, settings.ModId);
        _logger.LogInformation("Now go ask @Pickysaurus when a real API will be available! ;)");
        return 0;
    }

    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<mod-id>")]
        [Description("The NexusMods mod Id to upload the file to.")]
        public int ModId { get; set; } = default!;

        [CommandArgument(1, "<archive-file>")]
        [Description("Path to the mod archive file to upload.")]
        public string ModFilePath { get; set; } = default!;

        [CommandOption("-k|--api-key")]
        [EnvironmentVariable("APIKEY")]
        [Description("The NexusMods API key. Available Environment Variable: UNEX_APIKEY")]
        public string ApiKey { get; set; } = default!;

        [CommandOption("-g|--game")]
        [EnvironmentVariable("GAME")]
        [Description("The NexusMods game name (domain) to upload the mod to. Can be found in the URL of the game page. Available Environment Variable: UNEX_GAME")]
        public string Game { get; set; } = default!;

        [CommandOption("-f|--file-name")]
        [EnvironmentVariable("FILENAME")]
        [Description("Name for the file on NexusMods. Available Environment Variable: UNEX_FILENAME")]
        public string FileName { get; set; } = default!;

        [CommandOption("-v|--version <value>")]
        [EnvironmentVariable("FILEVERSION")]
        [Description("Version for your uploaded file. May also update your main version. Available Environment Variable: UNEX_FILEVERSION")]
        public string FileVersion { get; set; } = default!;

        [EnvironmentVariable("FILEDESCRIPTION")]
        [Description("Description for the file on NexusMods. Available Environment Variable: UNEX_FILEDESCRIPTION")]
        public string FileDescription { get; set; } = default!;

        [EnvironmentVariable("PREVIOUSFILE")]
        [Description("The Id of the previous file to remove. Available Environment Variable: UNEX_PREVIOUSFILE")]
        public string PreviousFile { get; set; } = default!;

        [CommandOption("--remove-download-with-manager [value]")]
        [EnvironmentVariable("REMOVEDOWNLOADWITHMANAGER")]
        [Description("Removes the Download With Manager button. Available Environment Variable: UNEX_REMOVEDOWNLOADWITHMANAGER")]
        public FlagValue<bool> RemoveDownloadWithManager { get; set; } = default!;

        [CommandOption("--no-version-update [value]")]
        [EnvironmentVariable("SKIPMAINVERSIONUPDATE")]
        [Description("Skips updating your mod's main version to match this file's version. Available Environment Variable: UNEX_SKIPMAINVERSIONUPDATE")]
        public FlagValue<bool> SkipMainVersionUpdate { get; set; } = default!;

        [CommandOption("--set-main-vortex [value]")]
        [EnvironmentVariable("SETMAINVORTEXFILE")]
        [Description("Sets this file as the main Vortex file (for the Download with Manager buttons). Available Environment Variable: UNEX_SETMAINVORTEXFILE")]
        public FlagValue<bool> SetMainVortexFile { get; set; } = default!;

        private bool AreSettingsValid() => ModFilePath.IsSet() && FileVersion.IsSet() && ApiKey.IsSet() && FileName.IsSet() && ModId != default;

        public override ValidationResult Validate()
        {
            if (!AreSettingsValid())
            {
                return ValidationResult.Error("Not all required settings provided!");
            }

            return base.Validate();
        }
    }
}