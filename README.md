# Judgy

Judgy is a .NET library for semantic testing of AI and LLM output.

Instead of relying only on exact string matching, Judgy evaluates whether generated output satisfies an expectation, produces structured evaluation evidence, and lets tests assert against that evidence using familiar .NET patterns.

## Why It Exists

Traditional assertions are often too brittle for AI-generated text. Judgy exists to make semantic evaluation practical in automated .NET test suites while keeping the core evaluation and assertion logic portable and provider-agnostic.

## Current Status

This repository is being reconstructed into a clean public open-source .NET implementation. Core libraries, providers, and tests are being migrated in small, reviewable commits.

Samples are intentionally postponed and will be added later.

## Planned Structure

```text
src/
  Judgy.Core/
  Judgy.Xunit/
  Judgy.Providers.*

tests/
  Judgy.Core.Tests/
  Judgy.Xunit.Tests/
  Judgy.Providers.*.Tests/
```

## Design Shape

```text
LLM Provider -> Evaluator -> Evidence -> Assertion Policy -> xUnit Assert
```

## License

MIT
