using System.Text;
using LLama;
using LLama.Common;
using LLama.Sampling;
using LLama.Transformers;

namespace ConsoleApp1;

public class LlamaChatService : ILlmService, IDisposable
{
    private readonly ChatSession _chatSession;
    private readonly ChatHistory _chatHistory = new();
    private readonly LLamaContext _context;
    private readonly LLamaWeights _model;

    public LlamaChatService(string modelPath)
    {
        var modelParameters = new ModelParams(modelPath)
        {
            ContextSize = 2048,
            GpuLayerCount = 0
        };

        _model = LLamaWeights.LoadFromFile(modelParameters);
        _context = new LLamaContext(_model, modelParameters);
        var executor = new InteractiveExecutor(_context);

        _chatSession = new ChatSession(executor, _chatHistory);

        _chatSession.WithHistoryTransform(new PromptTemplateTransformer(_model, withAssistant: true));
        _chatSession.WithOutputTransform(new LLamaTransforms.KeywordTextOutputStreamTransform(
            new[] { "User:", "�" }, redundancyLength: 5));
    }

    public async Task<string> GetChatResponse(string prompt)
    {
        var response = new StringBuilder();

        await foreach (var token in _chatSession.ChatAsync(
                           new ChatHistory.Message(AuthorRole.User, prompt),
                           new InferenceParams
                           {
                               SamplingPipeline = new DefaultSamplingPipeline { Temperature = 0.6f },
                               MaxTokens = 512,
                               AntiPrompts = new List<string> { "User:" }
                           }))
        {
            response.Append(token);
        }

        return response.ToString();
    }

    public void Dispose()
    {
        _context.Dispose();
        _model.Dispose();
    }
}