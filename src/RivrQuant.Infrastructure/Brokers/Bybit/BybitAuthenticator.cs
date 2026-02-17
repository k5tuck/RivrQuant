namespace RivrQuant.Infrastructure.Brokers.Bybit;

using System.Security.Cryptography;
using System.Text;

/// <summary>Handles HMAC-SHA256 request signing for Bybit API v5.</summary>
public sealed class BybitAuthenticator
{
    private readonly string _apiKey;
    private readonly byte[] _apiSecretBytes;

    /// <summary>Initializes a new instance of <see cref="BybitAuthenticator"/>.</summary>
    public BybitAuthenticator(string apiKey, string apiSecret)
    {
        _apiKey = apiKey;
        _apiSecretBytes = Encoding.UTF8.GetBytes(apiSecret);
    }

    /// <summary>
    /// Signs an HTTP request by adding Bybit v5 authentication headers.
    /// For GET: sign_str = timestamp + apiKey + recvWindow + queryString
    /// For POST: sign_str = timestamp + apiKey + recvWindow + body
    /// </summary>
    public void SignRequest(HttpRequestMessage request, string? body, int recvWindow)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
        var payload = body ?? string.Empty;

        if (request.Method == HttpMethod.Get && request.RequestUri?.Query?.Length > 1)
        {
            payload = request.RequestUri.Query[1..];
        }

        var signString = $"{timestamp}{_apiKey}{recvWindow}{payload}";
        var signature = ComputeHmacSha256(signString);

        request.Headers.Add("X-BAPI-API-KEY", _apiKey);
        request.Headers.Add("X-BAPI-SIGN", signature);
        request.Headers.Add("X-BAPI-TIMESTAMP", timestamp);
        request.Headers.Add("X-BAPI-RECV-WINDOW", recvWindow.ToString());
    }

    /// <summary>Computes HMAC-SHA256 signature for WebSocket authentication.</summary>
    public string ComputeWebSocketSignature(long expires)
    {
        var signString = $"GET/realtime{expires}";
        return ComputeHmacSha256(signString);
    }

    private string ComputeHmacSha256(string message)
    {
        using var hmac = new HMACSHA256(_apiSecretBytes);
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
