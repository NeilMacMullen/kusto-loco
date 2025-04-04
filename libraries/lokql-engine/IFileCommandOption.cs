namespace Lokql.Engine;

/// <summary>
/// Command option that takes a file path as input.
/// </summary>
public interface IFileCommandOption
{
    string File { get; }
}
