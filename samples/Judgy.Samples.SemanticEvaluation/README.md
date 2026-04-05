# Judgy.Samples.SemanticEvaluation

This sample shows how to use `SemanticEvaluator` and `SemanticAssertionPolicy` directly, without xUnit assertion helpers.

## Scenario

A question is sent to a source system via `HttpProvider`. The answer is then evaluated by a judge (`OllamaProvider`) to verify it meets the expectation.

```
HttpProvider (source) -> answer -> SemanticEvaluator (OllamaProvider judge) -> EvaluationResult -> SemanticAssertionPolicy
```

By default, `HttpProvider` points at Ollama `/api/chat` so the sample runs out of the box. Replace `SourceApi.Endpoint` in `appsettings.json` with your own RAG or API endpoint to test your real system.

## Project layout

- `Settings/` - Ollama and source API configuration
- `Support/` - base class that wires up HttpProvider and OllamaProvider
- `Tests/` - the sample test

## Why Ollama

- Free
- Runs locally
- No API key required

Any provider can replace `OllamaProvider` as the judge. The source endpoint in `appsettings.json` can point to any HTTP API that returns text.

## Configuration

`appsettings.json` has two sections under `Sample`:

- `SourceApi` - the HTTP endpoint that answers questions (defaults to Ollama `/api/chat`)
- `JudgeOllama` - the Ollama instance used as the semantic judge

Replace `SourceApi.Endpoint` with your actual RAG API to test your own system.

## Run

```powershell
dotnet test samples/Judgy.Samples.SemanticEvaluation/Judgy.Samples.SemanticEvaluation.csproj
```

Requires Ollama running at `http://localhost:11434` with `llama3:8b` pulled.
