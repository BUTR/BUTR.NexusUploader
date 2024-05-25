using NexusUploader.Utils;

using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace NexusUploader.Services;

public class UsersClient
{
    private readonly HttpClient _httpClient;
    private readonly NexusCookieHandler _cookieHandler;

    public UsersClient(HttpClient httpClient, NexusCookieHandler cookieHandler)
    {
        _httpClient = httpClient;
        _cookieHandler = cookieHandler;
    }

    public async Task<bool> RefreshSession()
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, string.Empty);
        using var resp = await _httpClient.SendAsync(req);
        if (!resp.IsSuccessStatusCode)
            return false;

        var cookies = _cookieHandler.CookieContainer.GetAllCookies().Where(x => x.Name == "nexusmods_session").Select(x => x.Value).ToArray();

        var count = cookies.Distinct().Count();
        return count == 1;
    }
}