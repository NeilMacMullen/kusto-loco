namespace Intellisense;

public record struct IntellisenseEntry(
    string Name = "",
    string Description = "",
    string Syntax = "",
    IntellisenseHint Hint = IntellisenseHint.None
);
