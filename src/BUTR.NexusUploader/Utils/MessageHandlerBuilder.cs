using BUTR.NexusUploader.Services;

using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;

using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace BUTR.NexusUploader.Utils;

public class NexusMessageHandlerBuilder : HttpMessageHandlerBuilder
{
    private readonly ILogger _logger;
    private readonly CookieService _cookies;

    public override string Name { get; set; } = nameof(NexusMessageHandlerBuilder);
    public override HttpMessageHandler PrimaryHandler { get; set; } = default!;

    public override IList<DelegatingHandler> AdditionalHandlers => new List<DelegatingHandler>();

    public NexusMessageHandlerBuilder(ILogger<NexusMessageHandlerBuilder> logger, CookieService cookieService)
    {
        _logger = logger;
        _cookies = cookieService;
    }

    // Our custom builder doesn't care about any of the above.
    public override HttpMessageHandler Build()
    {
        var container = GetCookies();
        return new HttpClientHandler
        {
            CookieContainer = container
            // Our custom settings
        };
    }

    private CookieContainer GetCookies()
    {
        var c = new CookieContainer();
        try
        {
            foreach (var (name, value) in _cookies.GetCookies())
            {
                if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(value))
                {
                    c.Add(new Cookie(name, value) { Domain = "nexusmods.com" });
                }
            }
        }
        catch
        {
            _logger.LogError("Error encountered while loading cookies! [bold] This probably won't work![/]");
        }

        return c;
    }
}