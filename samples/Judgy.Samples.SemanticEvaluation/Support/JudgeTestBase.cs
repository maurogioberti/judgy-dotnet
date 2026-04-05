using Judgy.Evaluation;
using Judgy.Providers.Http;
using Judgy.Providers.Ollama;
using Xunit;

namespace Judgy.Samples.SemanticEvaluation;

public abstract class JudgeTestBase
{
    protected CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    protected HttpProvider CreateSourceProvider()
    {
        var settings = SampleSettingsLoader.Load();

        return new HttpProvider(new HttpProviderOptions
        {
            Endpoint = settings.SourceApi.Endpoint,
            RequestTemplate = settings.SourceApi.RequestTemplate,
            ResponseJsonPath = settings.SourceApi.ResponseJsonPath,
            Timeout = TimeSpan.FromSeconds(settings.SourceApi.TimeoutSeconds)
        });
    }

    protected SemanticEvaluator CreateJudgeEvaluator()
    {
        var settings = SampleSettingsLoader.Load();

        var judgeProvider = new OllamaProvider(new OllamaProviderOptions
        {
            BaseUrl = settings.JudgeOllama.BaseUrl,
            Model = settings.JudgeOllama.Model,
            ApiKey = settings.JudgeOllama.ApiKey,
            Timeout = TimeSpan.FromSeconds(settings.JudgeOllama.TimeoutSeconds)
        });

        return new SemanticEvaluator(judgeProvider);
    }
}
