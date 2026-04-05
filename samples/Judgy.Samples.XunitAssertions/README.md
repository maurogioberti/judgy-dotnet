# Judgy.Samples.XunitAssertions

This sample shows how to use Judgy through xUnit assertion helpers: `Assert.JudgyAsync`, `Assert.JudgyScore`, and `Assert.JudgyDuration`.

## Scenario

A hardcoded answer is evaluated against an expectation using `OllamaProvider` as the judge.

```
hardcoded answer -> SemanticEvaluator (OllamaProvider) -> Assert.JudgyAsync / JudgyScore / JudgyDuration
```

## Project layout

- `Settings/` - Ollama judge configuration
- `Support/` - base class that wires up OllamaProvider as the judge
- `Tests/` - sample tests for each assertion type

## Why Ollama

- Free
- Runs locally
- No API key required

Any provider can replace `OllamaProvider`. Swap it out for `OpenAiProvider`, `AnthropicProvider`, or any other supported provider.

## Configuration

`appsettings.json` has one section under `Sample`:

- `JudgeOllama` - the Ollama instance used as the semantic judge

## Test files

- `Tests/JudgyAsyncAssertionTests.cs` - `Assert.JudgyAsync`
- `Tests/JudgyMetricAssertionTests.cs` - `Assert.JudgyScore` and `Assert.JudgyDuration`

## Run

```powershell
dotnet test samples/Judgy.Samples.XunitAssertions/Judgy.Samples.XunitAssertions.csproj
```

Requires Ollama running at `http://localhost:11434` with `llama3:8b` pulled.
