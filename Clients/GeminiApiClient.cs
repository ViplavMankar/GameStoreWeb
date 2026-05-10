using System.Text.Json;
using System.Text;
using GameStoreWeb.DTOs;

namespace GameStoreWeb.Clients;

public class GeminiApiClient
{
    private readonly HttpClient _http;
    private readonly GeminiOptions _opts;
    private readonly ILogger<GeminiApiClient> _log;

    public GeminiApiClient(HttpClient http, Microsoft.Extensions.Options.IOptions<GeminiOptions> opts, ILogger<GeminiApiClient> log)
    {
        _http = http;
        _opts = opts.Value;
        _log = log;
    }

    public async Task<string> GenerateAsync(string prompt, int? maxOutputTokens = null, double? temperature = null, CancellationToken ct = default)
    {
        var path = $"v1beta/models/{_opts.Model}:generateContent";
        var req = new
        {
            contents = new[]
            {
                new { role = "user", parts = new[] { new { text = prompt } } }
            },
            generationConfig = new
            {
                temperature = temperature ?? _opts.Temperature,
                maxOutputTokens = maxOutputTokens ?? _opts.MaxOutputTokens
            }
        };

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        using var res = await _http.PostAsJsonAsync(path, req, options, ct);
        var json = await res.Content.ReadAsStringAsync(ct);

        // Log raw JSON for debugging
        _log.LogInformation("Gemini raw json: {Json}", json);

        if (!res.IsSuccessStatusCode)
            throw new InvalidOperationException($"Gemini error {(int)res.StatusCode}: {json}");

        // Now parse robustly (see below)
        var text = ExtractTextFromGeminiJson(json);
        return text;
    }

    // ... Serializer class etc. (keep as before)

    private static string ExtractTextFromGeminiJson(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        // 1) candidates -> [0] -> content -> parts -> concat text
        if (root.TryGetProperty("candidates", out var candidates) && candidates.ValueKind == JsonValueKind.Array)
        {
            var first = candidates.EnumerateArray().FirstOrDefault();
            if (first.ValueKind != JsonValueKind.Undefined && first.TryGetProperty("content", out var content))
            {
                // content.parts maybe present
                if (content.TryGetProperty("parts", out var parts) && parts.ValueKind == JsonValueKind.Array)
                {
                    var sb = new StringBuilder();
                    foreach (var p in parts.EnumerateArray())
                    {
                        if (p.TryGetProperty("text", out var t) && t.ValueKind == JsonValueKind.String)
                            sb.Append(t.GetString());
                    }
                    var result = sb.ToString();
                    if (!string.IsNullOrWhiteSpace(result)) return result;
                }

                // maybe content has "text" directly
                if (content.TryGetProperty("text", out var ctext) && ctext.ValueKind == JsonValueKind.String)
                {
                    var s = ctext.GetString();
                    if (!string.IsNullOrWhiteSpace(s)) return s;
                }
            }
        }

        // 2) fallback: top-level 'candidates'[0].message.content.parts etc.
        if (root.TryGetProperty("candidates", out candidates))
        {
            foreach (var cand in candidates.EnumerateArray())
            {
                foreach (var prop in cand.EnumerateObject())
                {
                    var found = TryExtractStringRecursively(prop.Value);
                    if (!string.IsNullOrWhiteSpace(found)) return found;
                }
            }
        }

        // 3) fallback: "output", "message", "response", any string fields
        var maybe = TryExtractStringRecursively(root);
        if (!string.IsNullOrWhiteSpace(maybe)) return maybe;

        // 4) give up: return empty string so caller can detect it
        return string.Empty;
    }

    private static string TryExtractStringRecursively(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.String:
                return element.GetString() ?? string.Empty;
            case JsonValueKind.Object:
                foreach (var prop in element.EnumerateObject())
                {
                    var result = TryExtractStringRecursively(prop.Value);
                    if (!string.IsNullOrWhiteSpace(result)) return result;
                }
                break;
            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                {
                    var result = TryExtractStringRecursively(item);
                    if (!string.IsNullOrWhiteSpace(result)) return result;
                }
                break;
        }
        return string.Empty;
    }
}
