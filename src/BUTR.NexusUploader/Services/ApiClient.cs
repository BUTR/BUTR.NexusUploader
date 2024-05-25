using Semver;

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace BUTR.NexusUploader.Services;

public class ApiV1Client
{
    private readonly HttpClient _httpClient;

    public ApiV1Client(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<int> GetGameId(string gameName, string apiKey)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, $"v1/games/{gameName}.json");
        req.Headers.Add("apikey", apiKey);
        var resp = await _httpClient.SendAsync(req);
        var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(await resp.Content.ReadAsStringAsync());
        var latest = dict["id"].ToString();
        return int.Parse(latest);
    }

    public async Task<bool> CheckValidKey(string apiKey)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, "v1/users/validate.json");
        req.Headers.Add("apikey", apiKey);
        var resp = await _httpClient.SendAsync(req);
        if (resp.IsSuccessStatusCode)
        {
            var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(await resp.Content.ReadAsStringAsync());
            return !string.IsNullOrWhiteSpace(dict["name"].ToString()) && dict["key"].ToString() == apiKey;
        }
        else
        {
            return false;
        }
    }

    public async Task<int?> GetLatestFileId(string gameName, int modId, string apiKey)
    {
        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, $"v1/games/{gameName}/mods/{modId}/files.json");
            req.Headers.Add("apikey", apiKey);
            var resp = await _httpClient.SendAsync(req);
            var dict = JsonSerializer.Deserialize<Nexus.NexusFilesResponse>(await resp.Content.ReadAsStringAsync());
            var mainFiles = dict.Files.Where(f => f.CategoryName is "MAIN").ToList();
            if (mainFiles.Count == 1)
            {
                //well that was easy
                return mainFiles.First().FileId;
            }
            else
            {
                var semvers = mainFiles.Select(mf => (mf.FileVersion, SemVersion.Parse(mf.FileVersion, SemVersionStyles.Any))).ToList();
                semvers.Sort((r1, r2) =>
                {
                    if (r1.Item2 == null && r2.Item2 == null) return 0;
                    if (r1.Item2 == null) return -1;
                    if (r2.Item2 == null) return 1;
                    return r1.Item2.ComparePrecedenceTo(r2.Item2);
                });
                // semvers.Reverse();
                var highestV = mainFiles.FirstOrDefault(f => f.FileVersion == semvers.Last().FileVersion);
                return highestV.FileId;
            }
        }
        catch (System.Exception)
        {
            return null;
        }
    }
}