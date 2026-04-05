# Samples

Open the sample that matches the integration style you want to learn:

- [Judgy.Samples.SemanticEvaluation](Judgy.Samples.SemanticEvaluation/README.md)
  - calls a RAG source via `HttpProvider`, evaluates the answer with `OllamaProvider` as judge
  - uses `SemanticEvaluator` and `SemanticAssertionPolicy` directly

- [Judgy.Samples.XunitAssertions](Judgy.Samples.XunitAssertions/README.md)
  - evaluates a hardcoded answer using `OllamaProvider` as the judge
  - uses `Assert.JudgyAsync(...)`, `Assert.JudgyScore(...)`, and `Assert.JudgyDuration(...)`
