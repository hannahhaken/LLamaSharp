namespace ConsoleApp1;

public interface ILlmService : IDisposable
{
    Task<string> GetChatResponse(string prompt, string model);
    
    Task<EmbeddingResponse> GetEmbeddings(string text, string model);

    void Dispose();
}