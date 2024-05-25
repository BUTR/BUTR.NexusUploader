﻿using Microsoft.Extensions.Logging;

using NexusUploader.Services;

using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace NexusUploader.Utils;

public class NexusCookieHandler : HttpClientHandler
{
    private readonly ILogger _logger;
    private readonly CookieService _cookies;

    public NexusCookieHandler(ILogger<NexusCookieHandler> logger, CookieService cookieService)
    {
        _logger = logger;
        _cookies = cookieService;
    }

    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        CookieContainer = GetCookies();
        return base.Send(request, cancellationToken);
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        CookieContainer = GetCookies();
        return base.SendAsync(request, cancellationToken);
    }

    private CookieContainer GetCookies()
    {
        var c = new CookieContainer();
        try
        {
            foreach (var (name, value) in _cookies.GetCookies().Where(cookie => !string.IsNullOrWhiteSpace(cookie.Key) && !string.IsNullOrWhiteSpace(cookie.Value)))
            {
                c.Add(new Cookie(name, value) { Domain = "nexusmods.com" });
            }
        }
        catch
        {
            _logger.LogError("Error encountered while loading cookies! [bold] This probably won't work![/]");
        }

        return c;
    }
}