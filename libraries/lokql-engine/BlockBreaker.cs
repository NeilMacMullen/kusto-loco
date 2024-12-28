using System.Text;
using NotNullStrings;

namespace Lokql.Engine;

/// <summary>
/// A class to break a block of text into individual executable blocks
/// </summary>
/// <remarks>
/// A block is a set of lines that can be executed as a single unit.
/// A block is terminated by a blank line or command
/// </remarks>
public class BlockBreaker
{
    public string[] Blocks = [];

    public BlockBreaker(string block)
    {
        var blocks = new List<string>();
        //make the processing easier by adding an empty line at the end to act as a terminator
        var lines = block.Split(Environment.NewLine)
            .Append(string.Empty)
            .ToArray();


        var sb = new StringBuilder();
        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            //A blank line between blocks indicates we should complete the previous one
            if (trimmedLine.IsBlank())
            {
                CompleteBlock();
                continue;
            }

            //if the current line is a 'dot' command in can only be one line

            if (IsSingleLineCommand(trimmedLine))
            {
                //complete the previous block if any
                CompleteBlock();
                //add a single block for this command
                sb.AppendLine(trimmedLine);
                CompleteBlock();
                continue;
            }


            sb.AppendLine(line);
        }

        Blocks = blocks.ToArray();

        void CompleteBlock()
        {
            if (sb.Length > 0)
            {
                blocks.Add(sb.ToString().Trim());
                sb.Clear();
            }
        }
    }

    private static bool IsSingleLineCommand(string trimmedLine)
    {
        return trimmedLine.StartsWith(".") ||
               trimmedLine.StartsWith("#");
    }
}
