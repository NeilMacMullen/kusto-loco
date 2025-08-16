namespace Lokql.Engine;

/// <summary>
/// Represents a sequence of blocks that can be iterated over
/// </summary>
/// <remarks>
/// a block is a string that represents a logical unit of work
/// </remarks>
public class BlockSequence 
{
    public BlockSequence(string[] blocks)
    {
        Blocks = blocks.ToArray();
    }

    private int Index;
    private readonly string[] Blocks;

    /// <summary>
    /// True if all blocks have been processed
    /// </summary>
    public bool Complete => Index >= Blocks.Length;

    public string[] RemainingBlocks => Blocks.Skip(Index).ToArray();

    /// <summary>
    /// Get the next block in the sequence
    /// </summary>
    public string Next()
    {
        return Index < Blocks.Length ? Blocks[Index++] : string.Empty;
    }
}

