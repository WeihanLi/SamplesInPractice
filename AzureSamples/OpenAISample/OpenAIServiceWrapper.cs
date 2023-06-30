using OpenAI.Interfaces;
using OpenAI.ObjectModels.RequestModels;
using OpenAI.ObjectModels.ResponseModels;

namespace OpenAISample;

public interface IOpenAIServiceWrapper
{
    string Name { get; }
    IOpenAIService Service { get; }
}

public sealed class OpenAIServiceWrapper : IOpenAIServiceWrapper
{
    public string Name { get; }
    public IOpenAIService Service { get; }

    public OpenAIServiceWrapper(string name, IOpenAIService openAIService)
    {
        Name = name;
        Service = openAIService;
    }
}

public sealed class EmbeddingServiceWrapper : IEmbeddingService
{
    private readonly IOpenAIServiceFactory _openAIServiceFactory;

    public EmbeddingServiceWrapper(IOpenAIServiceFactory openAIServiceFactory)
    {
        _openAIServiceFactory = openAIServiceFactory;
    }
    
    public async Task<EmbeddingCreateResponse> CreateEmbedding(EmbeddingCreateRequest createEmbeddingModel,
        CancellationToken cancellationToken = new CancellationToken())
    {
        var result = await _openAIServiceFactory.GetExecutePolicy<EmbeddingCreateResponse>().ExecuteAsync(async () =>
        {
            var service = _openAIServiceFactory.GetService();
            return (service.Name, await service.Service.Embeddings.CreateEmbedding(createEmbeddingModel, cancellationToken));
        });
        return result.Response;
    }
}

public sealed class ChatCompletionServiceWrapper : IChatCompletionService
{
    private readonly IOpenAIServiceFactory _openAIServiceFactory;

    public ChatCompletionServiceWrapper(IOpenAIServiceFactory openAIServiceFactory)
    {
        _openAIServiceFactory = openAIServiceFactory;
    }
    
    public async Task<ChatCompletionCreateResponse> CreateCompletion(ChatCompletionCreateRequest chatCompletionCreate, string? modelId = null,
        CancellationToken cancellationToken = new CancellationToken())
    {
        var result = await _openAIServiceFactory.GetExecutePolicy<ChatCompletionCreateResponse>().ExecuteAsync(async () =>
        {
            var service = _openAIServiceFactory.GetService();
            return (service.Name, await service.Service.ChatCompletion.CreateCompletion(chatCompletionCreate, modelId, cancellationToken));
        });
        return result.Response;
    }

    public IAsyncEnumerable<ChatCompletionCreateResponse> CreateCompletionAsStream(ChatCompletionCreateRequest chatCompletionCreate, string? modelId = null,
        CancellationToken cancellationToken = new CancellationToken())
    {
        return _openAIServiceFactory.GetService().Service.ChatCompletion
            .CreateCompletionAsStream(chatCompletionCreate, modelId, cancellationToken);
    }
}
