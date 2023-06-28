using OpenAI.Interfaces;

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
