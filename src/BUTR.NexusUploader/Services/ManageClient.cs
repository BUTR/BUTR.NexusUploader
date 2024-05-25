using BUTR.NexusUploader.Extensions;
using BUTR.NexusUploader.Models;
using BUTR.NexusUploader.Nexus;

using Microsoft.Extensions.Logging;

using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace BUTR.NexusUploader.Services;

public class ManageClient
{
    private readonly ILogger _logger;
    private readonly HttpClient _httpClient;

    public ManageClient(ILogger<ManageClient> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<bool> CheckValidSession()
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, "/Core/Libs/Common/Managers/Mods?GetDownloadHistory");
        req.Headers.Add("X-Requested-With", "XMLHttpRequest");
        var resp = await _httpClient.SendAsync(req);
        if (!resp.IsSuccessStatusCode)
        {
            return false;
        }

        return resp.StatusCode == HttpStatusCode.OK;
    }

    public async Task<bool> AddChangelog(GameRef game, int modId, string version, string changeMessage)
    {
        changeMessage = HttpUtility.HtmlEncode(changeMessage).Replace(@"\n", "\n");
        using var message = new HttpRequestMessage(HttpMethod.Post, "/Core/Libs/Common/Managers/Mods?SaveDocumentation");
        message.Headers.Add("X-Requested-With", "XMLHttpRequest");
        message.Headers.Add("Referer", $"https://www.nexusmods.com/{game.Name}/mods/edit/?step=docs&id={modId}");
        var content = new MultipartFormDataContent();
        content.Add(new StringContent(game.Id), "game_id");
        content.Add(new StringContent(string.Empty), "new_version[]");
        content.Add(new StringContent(string.Empty), "new_change[]");
        foreach (var change in changeMessage.Split('\n'))
        {
            content.Add(new StringContent(version), "new_version[]");
            content.Add(new StringContent(change), "new_change[]");
        }

        content.Add("save".ToContent(), "action");
        content.Add(new StringContent(modId.ToString()), "id");
        message.Content = content;
        var resp = await _httpClient.SendAsync(message);
        if (resp.IsSuccessStatusCode)
        {
            var strResponse = await resp.Content.ReadAsStringAsync();
            var data = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(strResponse);
            return data.ContainsKey("status") && data["status"].ToString() == true.ToString();
        }

        return false;
    }

    public async Task<bool> AddFile(GameRef game, int modId, UploadedFile upload, FileOptions options)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "/Core/Libs/Common/Managers/Mods?AddFile");
        message.Headers.Add("X-Requested-With", "XMLHttpRequest");
        message.Headers.Add("Referer", $"https://www.nexusmods.com/{game.Name}/mods/edit/?step=docs&id={modId}");
        using var content = new MultipartFormDataContent();
        content.Add(game.Id.ToContent(), "game_id");
        content.Add(options.Name.ToContent(), "name");
        content.Add(options.Version.ToContent(), "file-version");
        content.Add(options.UpdateMainVersion ? 1.ToContent() : 0.ToContent(), "update-version");
        content.Add(1.ToContent(), "category");
        if (options.PreviousFileId.HasValue)
        {
            content.Add(1.ToContent(), "new-existing");
            content.Add(options.PreviousFileId.Value.ToContent(), "old_file_id");
        }

        content.Add(options.Description.ToContent(), "brief-overview");
        content.Add(options.RemoveDownloadWithManager ? 1.ToContent() : 0.ToContent(), "remove_nmm_button");
        content.Add(options.SetAsMainVortex != null
            ? options.SetAsMainVortex.Value ? 1.ToContent() : 0.ToContent()
            : options.UpdateMainVersion ? 1.ToContent() : 0.ToContent(), "set_as_main_nmm");
        content.Add(upload.Id.ToContent(), "file_uuid");
        content.Add(upload.FileSize.ToContent(), "file_size");
        content.Add(modId.ToContent(), "mod_id");
        content.Add(modId.ToContent(), "id");
        content.Add("add".ToContent(), "action");
        content.Add(upload.FileName.ToContent(), "uploaded_file");
        content.Add(upload.OriginalFile.ToContent(), "original_file");
        message.Content = content;
        var resp = await _httpClient.SendAsync(message);
        if (resp.IsSuccessStatusCode)
        {
            var strResponse = await resp.Content.ReadAsStringAsync();
            var data = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(strResponse);
            var success = data.ContainsKey("status") && data["status"].ToString() == true.ToString();
            if (success)
            {
                return true;
            }
            else
            {
                _logger.LogWarning("Response received from Nexus Mods: {Message}", data["message"]);
            }
        }

        return false;
    }
}