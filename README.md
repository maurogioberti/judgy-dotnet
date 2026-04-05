# Judgy

Judgy is a family of .NET libraries for semantic testing of AI and LLM output.

Instead of asserting on exact strings, Judgy lets you evaluate whether generated output satisfies an expectation and then fail with useful semantic evidence.

Current packages target `.NET 10`.

## Providers Are Optional

`Judgy.Core` defines the provider abstraction used by `SemanticEvaluator`:

- `Judgy.Providers.ILlmProvider`
- `Judgy.Providers.LlmRequest`
- `Judgy.Providers.LlmResponse`

That means the `Judgy.Providers.*` packages are optional implementations. You can use the built-in providers for HTTP and common LLM runtimes, or you can implement `ILlmProvider` yourself if you want to connect Judgy to your own gateway, SDK, wrapper, or in-house model service.

## Install the packages you need

Install the core package, add `Judgy.Xunit` if you want xUnit helpers, and then choose the provider package that matches your model/runtime:

```bash
dotnet add package Judgy.Core
dotnet add package Judgy.Xunit

dotnet add package Judgy.Providers.Http
dotnet add package Judgy.Providers.Ollama
dotnet add package Judgy.Providers.OpenAI
dotnet add package Judgy.Providers.Anthropic
dotnet add package Judgy.Providers.Google
dotnet add package Judgy.Providers.AzureOpenAI
dotnet add package Judgy.Providers.Mistral
dotnet add package Judgy.Providers.Moonshot
dotnet add package Judgy.Providers.DeepSeek
```

If you already have your own LLM integration, you can skip the optional provider packages and implement `ILlmProvider` directly.

## Packages

| Package | Purpose |
| --- | --- |
| `Judgy.Core` | Core evaluation models, provider abstractions, and assertion policy primitives |
| `Judgy.Xunit` | xUnit assertion helpers such as `Assert.JudgyAsync(...)` |
| `Judgy.Providers.Http` | Optional generic HTTP provider for calling text-producing endpoints |
| `Judgy.Providers.Ollama` | Optional Ollama provider |
| `Judgy.Providers.OpenAI` | Optional OpenAI provider |
| `Judgy.Providers.Anthropic` | Optional Anthropic provider |
| `Judgy.Providers.Google` | Optional Google Gemini provider |
| `Judgy.Providers.AzureOpenAI` | Optional Azure OpenAI provider |
| `Judgy.Providers.Mistral` | Optional Mistral provider |
| `Judgy.Providers.Moonshot` | Optional Moonshot provider |
| `Judgy.Providers.DeepSeek` | Optional DeepSeek provider |

## Quick Start With xUnit

The example below uses `Judgy.Providers.OpenAI`, but the same evaluator flow works with any supported provider package. Swap in `HttpProvider`, `OllamaProvider`, `AnthropicProvider`, `GoogleProvider`, `AzureOpenAiProvider`, `MistralProvider`, `MoonshotProvider`, or `DeepSeekProvider` as needed.

```csharp
using Judgy.Evaluation;
using Judgy.Providers.OpenAI;
using Xunit;

var provider = new OpenAiProvider(new OpenAiProviderOptions
{
    ApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")!,
    Model = "gpt-4o"
});

var evaluator = new SemanticEvaluator(provider);

await Assert.JudgyAsync(
    actualOutput,
    "The answer should mention refund deadlines",
    evaluator,
    minimumScore: 0.80);
```

## Quick Start Without xUnit

The same pattern also works with any provider package under `Judgy.Providers.*`.

```csharp
using Judgy.Assertions;
using Judgy.Evaluation;
using Judgy.Providers.OpenAI;

var provider = new OpenAiProvider(new OpenAiProviderOptions
{
    ApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")!,
    Model = "gpt-4o"
});

var evaluator = new SemanticEvaluator(provider);
var result = await evaluator.EvaluateAsync(
    actualOutput,
    "The answer should mention refund deadlines");

var decision = SemanticAssertionPolicy.Evaluate(result, new SemanticAssertionOptions(0.80));
```

## Custom Provider Example

If you do not want to use one of the optional `Judgy.Providers.*` packages, implement `ILlmProvider` yourself:

```csharp
using Judgy.Providers;

public sealed class MyProvider : ILlmProvider
{
    public async Task<LlmResponse> CompleteAsync(
        LlmRequest request,
        CancellationToken cancellationToken = default)
    {
        // Call your own SDK, gateway, or service here and return the raw
        // evaluator response text that Judgy will parse.
        var text = await Task.FromResult(
            "{\"confidence\":0.95,\"reasoning\":\"Looks good.\"}");

        return new LlmResponse(text, "MyProvider");
    }
}
```

## How It Works

```text
LLM Provider -> SemanticEvaluator -> Evidence -> Assertion Policy -> Test Assertion
```

Judgy keeps provider calls, semantic evaluation, and assertion policy separate so tests stay readable while evaluation logic stays reusable.

## Samples

The repository includes runnable sample projects under:

- `samples/Judgy.Samples.SemanticEvaluation`
- `samples/Judgy.Samples.XunitAssertions`

Browse the repository for sample setup and usage details:

- `https://github.com/maurogioberti/judgy-dotnet`

## Status

Judgy is usable today and still evolving. Expect API and package refinements before a stable `1.0` release.

## License

MIT
