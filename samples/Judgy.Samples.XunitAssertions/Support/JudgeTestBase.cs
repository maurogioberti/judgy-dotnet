using Judgy.Evaluation;
using Judgy.Providers.Ollama;
using Xunit;

namespace Judgy.Samples.XunitAssertions;

public abstract class JudgeTestBase
{
    protected CancellationToken CancellationToken => TestContext.Current.CancellationToken;

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

    protected TimeSpan GetMaximumDuration()
    {
        var settings = SampleSettingsLoader.Load();
        return TimeSpan.FromSeconds(settings.JudgeOllama.TimeoutSeconds);
    }
}
