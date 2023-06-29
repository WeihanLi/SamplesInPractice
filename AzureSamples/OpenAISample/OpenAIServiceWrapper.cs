using OpenAI.Interfaces;
using OpenAI.ObjectModels.RequestModels;
using OpenAI.ObjectModels.ResponseModels;
using Polly;
using Polly.Retry;

namespace OpenAISample;

public interface IOpenAIServiceWrapper: IOpenAIService
{
    string Name { get; }
    bool IsRestricted { get; }
    IOpenAIService Service { get; }
}

public sealed class OpenAIServiceWrapper : IOpenAIServiceWrapper
{
    public string Name { get; }
    public bool IsRestricted { get; set; }
    public IOpenAIService Service { get; }

    public OpenAIServiceWrapper(string name, IOpenAIService openAIService)
    {
        Name = name;
        Service = openAIService;
    }

    public void SetDefaultModelId(string modelId) => Service.SetDefaultModelId(modelId);

    public IModelService Models => Service.Models;

    public ICompletionService Completions => Service.Completions;

    public IEmbeddingService Embeddings => Service.Embeddings;

    public IFileService Files => Service.Files;

    public IFineTuneService FineTunes => Service.FineTunes;

    public IModerationService Moderation => Service.Moderation;

    public IImageService Image => Service.Image;

    public IEditService Edit => Service.Edit;

    public IChatCompletionService ChatCompletion => Service.ChatCompletion;

    public IAudioService Audio => Service.Audio;
}

public sealed class EmbeddingServiceWrapper : IEmbeddingService
{
    private readonly IOpenAIServiceFactory _openAIServiceFactory;
    private readonly AsyncRetryPolicy<(string ServiceName, EmbeddingCreateResponse Response)> _retryPolicy;

    public EmbeddingServiceWrapper(IOpenAIServiceFactory openAIServiceFactory)
    {
        _openAIServiceFactory = openAIServiceFactory;
        _retryPolicy = Policy<(string ServiceName, EmbeddingCreateResponse Response)>.HandleResult(x =>
        {
            if (x.Response.Error != null)
            {
                if (x.Response.Error is
                    {
                        Type: "insufficient_quota"
                    })
                {
                    _openAIServiceFactory.RateLimited(x.ServiceName, TimeSpan.FromDays(1));
                }

                return true;
            }
            return false;
        }).RetryAsync(5);
    }
    
    public async Task<EmbeddingCreateResponse> CreateEmbedding(EmbeddingCreateRequest createEmbeddingModel,
        CancellationToken cancellationToken = new CancellationToken())
    {
        var result = await _retryPolicy.ExecuteAsync(async () =>
        {
            var service = _openAIServiceFactory.GetService();
            return (service.Name, await service.Embeddings.CreateEmbedding(createEmbeddingModel, cancellationToken));
        });
        return result.Response;
    }
}

public sealed class ChatCompletionServiceWrapper : IChatCompletionService
{
    private readonly IOpenAIServiceFactory _openAIServiceFactory;
    private readonly AsyncRetryPolicy<(string ServiceName, ChatCompletionCreateResponse Response)> _retryPolicy;

    public ChatCompletionServiceWrapper(IOpenAIServiceFactory openAIServiceFactory)
    {
        _openAIServiceFactory = openAIServiceFactory;
        _retryPolicy = Policy<(string ServiceName, ChatCompletionCreateResponse Response)>.HandleResult(x =>
        {
            if (x.Response.Error != null)
            {
                if (x.Response.Error is
                    {
                        Type: "insufficient_quota"
                    })
                {
                    _openAIServiceFactory.RateLimited(x.ServiceName, TimeSpan.FromDays(1));
                }

                return true;
            }
            return false;
        }).RetryAsync(5);
    }
    
    public async Task<ChatCompletionCreateResponse> CreateCompletion(ChatCompletionCreateRequest chatCompletionCreate, string? modelId = null,
        CancellationToken cancellationToken = new CancellationToken())
    {
        var result = await _retryPolicy.ExecuteAsync(async () =>
        {
            var service = _openAIServiceFactory.GetService();
            return (service.Name, await service.ChatCompletion.CreateCompletion(chatCompletionCreate, modelId, cancellationToken));
        });
        return result.Response;
    }

    public IAsyncEnumerable<ChatCompletionCreateResponse> CreateCompletionAsStream(ChatCompletionCreateRequest chatCompletionCreate, string? modelId = null,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }
}
