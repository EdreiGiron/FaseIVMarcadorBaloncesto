using System.Net;
using System.Net.Http.Headers;

namespace RegistroTorneos.Api.Infrastructure;

public class TorneosClient
{
    private readonly HttpClient _http;
    public TorneosClient(HttpClient http) => _http = http;

    public async Task<bool> ExistsAsync(string torneoId, string? bearerToken = null, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(torneoId)) return false;

        using var req = new HttpRequestMessage(HttpMethod.Get, $"/api/torneos/{torneoId}");

        if (!string.IsNullOrWhiteSpace(bearerToken))
        {
            var token = bearerToken.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                ? bearerToken.Substring("Bearer ".Length)
                : bearerToken;
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        try
        {
            using var resp = await _http.SendAsync(req, ct);
            if (resp.StatusCode == HttpStatusCode.NotFound) return false;
            return resp.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}

