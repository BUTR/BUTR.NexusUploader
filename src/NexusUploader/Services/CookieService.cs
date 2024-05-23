using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NexusUploader.Services
{
    public class CookieService
    {
        private readonly ModConfiguration _config;

        public CookieService(ModConfiguration config)
        {
            _config = config;
        }
        /*
         * Not even close, past me: only truly required one seems to be sid_develop
         */

        public Dictionary<string, string> GetCookies()
        {
            if (Path.HasExtension(_config.Cookies) && File.Exists(_config.Cookies))
            {
                var ckTxt = File.ReadAllLines(Path.GetFullPath(_config.Cookies));
                var ckSet = ParseCookiesTxt(ckTxt);
                return ckSet;
            }
            else if (_config.Cookies.Contains('\n'))
            {
                var ckSet = ParseCookiesTxt(_config.Cookies.Split('\n'));
                return ckSet;
            }
            else
            {
                //almost certainly a session id, we'll assume it is
                var raw = Uri.UnescapeDataString(_config.Cookies);
                return new Dictionary<string, string> {["nexusmods_session"] = Uri.EscapeDataString(raw)};
            }

        }

        private Dictionary<string, string> ParseCookiesTxt(IEnumerable<string> ckTxt)
        {
            var ckSet = ckTxt
                .Select(t => t.Split('\t', StringSplitOptions.RemoveEmptyEntries))
                .Where(cv => cv.First().TrimStart('.') == "nexusmods.com")
                .ToDictionary(k => k[5], v => v.Length > 6 ? v[6] : string.Empty);
            return ckSet;
        }
    }
}