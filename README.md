# Judgy

Judgy is a .NET library for semantic testing of AI and LLM output.

Instead of asserting on exact strings, Judgy lets you check whether generated output satisfies an expectation, then fail tests with useful evaluation context.

## Why Use Judgy

- Test meaning instead of exact phrasing
- Keep LLM assertions readable in xUnit
- Plug in different model providers without changing test structure
- Reuse the same core evaluation and assertion primitives across projects

## Quick Start

Install the xUnit package and a provider package:

```bash
dotnet add package Judgy.Xunit
dotnet add package Judgy.Providers.OpenAI
```

Create a provider, build an evaluator, and assert semantically:

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
    evaluator);
```

You can also set a custom threshold:

```csharp
await Assert.JudgyAsync(
    actualOutput,
    "The answer should mention refund deadlines",
    evaluator,
    minimumScore: 0.80);
```

## Packages

| Package | Purpose |
| --- | --- |
| `Judgy.Core` | Core evaluation and assertion primitives |
| `Judgy.Xunit` | xUnit assertion API |
| `Judgy.Providers.OpenAI` | OpenAI provider |
| `Judgy.Providers.Ollama` | Ollama provider |
| `Judgy.Providers.Http` | Generic HTTP provider |
| `Judgy.Providers.Anthropic` | Anthropic provider |
| `Judgy.Providers.Google` | Google Gemini provider |
| `Judgy.Providers.AzureOpenAI` | Azure OpenAI provider |
| `Judgy.Providers.Mistral` | Mistral provider |
| `Judgy.Providers.Moonshot` | Moonshot provider |
| `Judgy.Providers.DeepSeek` | DeepSeek provider |

## How It Works

```text
LLM Provider -> Evaluator -> Evidence -> Assertion Policy -> xUnit Assert
```

Judgy keeps provider calls, semantic evaluation, and assertion policy separate so tests stay simple while the evaluation logic stays flexible.

## Status

Judgy is usable today and still evolving. Expect improvements and API refinements before a stable `1.0` release.

## License

MIT
