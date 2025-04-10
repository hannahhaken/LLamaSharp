using LLama;
using LLama.Common;
using LLama.Native;

namespace ConsoleApp1;

public interface IEmbedderService
{
    public Task<float[]> GetEmbeddingsAsync(string text);
}

public class LlamaEmbedderService : IEmbedderService, IDisposable
{
    private readonly LLamaEmbedder _embedder;
    private readonly LLamaWeights _model;
    private readonly ModelParams _modelParams;

    public LlamaEmbedderService(string modelPath)
    {
        _modelParams = new ModelParams(modelPath)
        {
            GpuLayerCount = 0,
            PoolingType = LLamaPoolingType.Mean
        };

        _model = LLamaWeights.LoadFromFile(_modelParams);
        _embedder = new LLamaEmbedder(_model, _modelParams);
    }

    public async Task<float[]> GetEmbeddingsAsync(string text)
    {
        return (await _embedder.GetEmbeddings(text)).Single();
    }

    public void Dispose()
    {
        _embedder.Dispose();
        _model.Dispose();
    }
}