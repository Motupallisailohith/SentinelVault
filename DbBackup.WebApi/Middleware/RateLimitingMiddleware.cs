using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace DbBackup.WebApi.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly RateLimitingOptions _options;
    private readonly RateLimitingStore _store;

    public RateLimitingMiddleware(RequestDelegate next, IOptions<RateLimitingOptions> options)
    {
        _next = next;
        _options = options.Value;
        _store = new RateLimitingStore();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientIp = context.Connection.RemoteIpAddress?.ToString();
        if (string.IsNullOrEmpty(clientIp)) return;

        var endpoint = context.GetEndpoint();
        var rateLimit = _options.Rules.FirstOrDefault(r => r.Path == endpoint?.DisplayName);

        if (rateLimit != null)
        {
            var key = $"{clientIp}:{endpoint.DisplayName}";
            var requestCount = _store.GetRequestCount(key);

            if (requestCount >= rateLimit.Limit)
            {
                context.Response.StatusCode = 429; // Too Many Requests
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"error\":\"Rate limit exceeded\"}");
                return;
            }

            _store.IncrementRequestCount(key);
        }

        await _next(context);
    }
}

public class RateLimitingOptions
{
    public RateLimitRule[] Rules { get; set; } = Array.Empty<RateLimitRule>();
}

public class RateLimitRule
{
    public string Path { get; set; } = string.Empty;
    public int Limit { get; set; } = 100; // requests per period
    public TimeSpan Period { get; set; } = TimeSpan.FromMinutes(1);
}

public class RateLimitingStore
{
    private readonly Dictionary<string, RateLimitEntry> _entries = new();
    private readonly object _lock = new();

    public int GetRequestCount(string key)
    {
        lock (_lock)
        {
            if (!_entries.TryGetValue(key, out var entry))
            {
                _entries[key] = entry = new RateLimitEntry();
            }

            if (entry.LastRequest.AddMinutes(1) < DateTime.UtcNow)
            {
                entry.Count = 0;
            }

            return entry.Count;
        }
    }

    public void IncrementRequestCount(string key)
    {
        lock (_lock)
        {
            if (!_entries.TryGetValue(key, out var entry))
            {
                _entries[key] = entry = new RateLimitEntry();
            }

            entry.Count++;
            entry.LastRequest = DateTime.UtcNow;
        }
    }
}

public class RateLimitEntry
{
    public int Count { get; set; }
    public DateTime LastRequest { get; set; }
}
