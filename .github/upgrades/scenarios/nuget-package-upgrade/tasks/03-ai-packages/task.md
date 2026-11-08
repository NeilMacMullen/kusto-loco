# 03-ai-packages: ai-packages

Update AI-related packages: `Microsoft.Extensions.AI`, `Microsoft.Extensions.AI.Abstractions`, `Microsoft.Extensions.AI.OpenAI`, `OpenAI`, `ModelContextProtocol`, `Anthropic.SDK`. `Microsoft.Extensions.AI.OpenAI` and `OpenAI` have breaking API changes reported — attempt small fixes to call sites; revert both together if not feasible without large rework.

**Done when**: Packages updated where feasible, affected call sites compile, tests pass. Any package reverted is documented with reason.
