using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ConsoleApp1;


public sealed class EmbeddingResponse
{
    [JsonPropertyName("embedding")]
    public ReadOnlyMemory<float> Embeddings { get; set; }
}

public class OllamaService : ILlmService
{
    private readonly HttpClient _httpClient;
    private readonly string _model;

    public OllamaService(string model, string? ollamaEndpoint = null)
    {
        _httpClient = new HttpClient { BaseAddress = new Uri(ollamaEndpoint) };
        _model = model;
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }

    public async Task<EmbeddingResponse> GetEmbeddings(string text, string model)
    {
        var requestBody = new
        {
            model = model,
            prompt = text
        };

        var response = await _httpClient.PostAsJsonAsync("api/embeddings", requestBody);

        response.EnsureSuccessStatusCode();

        var embeddings = await response.Content.ReadFromJsonAsync<EmbeddingResponse>();
        
        return embeddings;
    }

    public async Task<string> GetChatResponse(string prompt, string model)
    {
        var requestBody = new
        {
            model = model,
            messages = new[]
            {
                new { role = "user", content = prompt }
            },
            stream = false
        };

        var response = await _httpClient.PostAsJsonAsync("api/chat", requestBody);

        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadFromJsonAsync<JsonElement>();

        var content = responseJson.GetProperty("message").GetProperty("content").GetString();

        return content ?? "[No response content from Ollama]";
    }
}