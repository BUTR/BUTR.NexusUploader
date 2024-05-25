using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BUTR.NexusUploader.Services;

public class CookieService
{
    private string? _sessionCookie;

    public void SetSessionCookie(string sessionCookie) => _sessionCookie = sessionCookie;

    public Dictionary<string, string> GetCookies()
    {
        if (_sessionCookie is null)
            return new Dictionary<string, string>();

        if (Path.HasExtension(_sessionCookie) && File.Exists(_sessionCookie))
        {
            var ckTxt = File.ReadAllLines(Path.GetFullPath(_sessionCookie));
            var ckSet = ParseCookiesTxt(ckTxt);
            return ckSet;
        }

        var raw = Uri.UnescapeDataString(_sessionCookie);
        return new Dictionary<string, string> { ["nexusmods_session"] = Uri.EscapeDataString(raw) };
    }

    // TODO:
    private static Dictionary<string, string> ParseCookiesTxt(IEnumerable<string> ckTxt)
    {
        var ckSet = ckTxt
            .Select(t => t.Split('\t', StringSplitOptions.RemoveEmptyEntries))
            .Where(cv => cv.First().TrimStart('.') == "nexusmods.com")
            .ToDictionary(k => k[5], v => v.Length > 6 ? v[6] : string.Empty);
        return ckSet;
    }
}