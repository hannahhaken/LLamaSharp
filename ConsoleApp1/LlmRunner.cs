using ConsoleApp1.Config;

namespace ConsoleApp1;

public class LlmRunner
{
    private readonly IEmbedderService _embedderService;
    private readonly LlmConfig _config;
    private readonly ILlmService _llmService;
    private ChromaService? _chromaService;

    // public LlmRunner(IEmbedderService embedderService, LlmConfig llmConfig, ILlmService llmService)
    // {
    //     _embedderService = embedderService;
    //     _config = llmConfig;
    //     _llmService = llmService;
    // }
    
    public LlmRunner(LlmConfig llmConfig, ILlmService llmService)
    {
        _config = llmConfig;
        _llmService = llmService;
    }

    private async Task InitChromaService()
    {
        try
        {
            if (_chromaService is null)
                _chromaService = await ChromaService.CreateAsync(_config.ChromaUri, _config.CollectionName);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    public async Task AddDocumentationAsync()
    {
        await InitChromaService(); 
        //await ExtractDocumentChunks(_chromaService!);
    }

    public async Task ExecutePromptAsync(string userQuery)
    {
        await InitChromaService(); 
        await _chromaService!.RunQuery(userQuery, _embedderService, _llmService);
    }


}