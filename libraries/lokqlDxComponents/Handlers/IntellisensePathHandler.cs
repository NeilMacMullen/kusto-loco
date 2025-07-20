using System.CommandLine.Parsing;
using Intellisense;
using lokqlDxComponents.Models;
using lokqlDxComponents.Services;
using lokqlDxComponents.Views.Dialogs;
using Microsoft.Extensions.Logging;

namespace lokqlDxComponents.Handlers;

public class IntellisensePathHandler(
    IIntellisenseCommandsProvider commandsProvider,
    IntellisenseClient intellisenseClient,
    ILogger<IntellisensePathHandler> logger)
    : IIntellisenseHandler
{
    public async Task<CompletionRequest> GetCompletionRequest(HandleKeyDownMessage message)
    {
        // avoid unnecessary IO calls
        if (message.IsCompletionWindowOpen) return CompletionRequest.Empty;

        var currentLineText = message.Cursor.GetCurrentLineText();
        var result = await GetPathCompletions(currentLineText);

        return new CompletionRequest
        {
            Completions = result.Entries,
            OnCompletionWindowDataPopulated = completionWindow =>
            {
                // https://github.com/AvaloniaUI/AvaloniaEdit/blob/c8b0d481ec65eaebec2167165e79f62c363462b7/src/AvaloniaEdit/CodeCompletion/CompletionWindow.cs#L224-L257
                // https://github.com/AvaloniaUI/AvaloniaEdit/blob/c8b0d481ec65eaebec2167165e79f62c363462b7/src/AvaloniaEdit/CodeCompletion/CompletionList.cs#L336
                // Avalon(ia)Edit's CompletionWindow behavior is mostly independent of our code once the completion window is open.
                // Its internal filtering and selection algorithm is not a simple Contains or StartsWith. It calculates a quality score based on a prioritized list of string comparison strategies.
                // It is also transitively dependent on 2 variables with different lifecycles:
                // 1. The starting offset of the texteditor's caret when the CompletionWindow was first opened
                // 2. Its current offset of the texteditor's caret as the user types
                // Therefore, we need to be cognizant when implementing features related to the caret position.


                // This ensures consistent completion behavior when we lose focus and resume typing for paths.
                // Since path completion is unique in that path completion can start from a letter (e.g. "/myFol" => "/myFold"), we need to ensure that not only does the insertion "rewind" consistently,
                // the filtering algorithm also produces consistent results as we traverse through the lifecycles of both the starting offset and the current offset.
                // Typing "/myFold" should produce the same list of available results as typing "/myFol" (focus loss) + "d".
                // For every key stroke in "/myFolder", its result should be the same as doing the same, but pressing "ESC" before the next keystroke.
                // Since we already adjusted the cursor's starting offset, we do not need to add any additional "rewinding" in the ICompletionData we pass to the completion window.
                completionWindow.StartOffset -= result.Filter.Length;


                // when editor loses focus mid-path and user resumes typing, it won't require a second keypress to select the relevant result
                completionWindow.CompletionList.SelectItem(result.Filter);
                if (!completionWindow.CompletionList.CurrentList.Any())
                    completionWindow.Close();
            }
        };
    }

    private async Task<CompletionResult> GetPathCompletions(string currentLineText)
    {
        var args = CommandLineStringSplitter.Instance.Split(currentLineText).ToArray();

        if (args.Length < 2)
        {
            return CompletionResult.Empty;
        }

        var lastArg = args[^1];
        var command = args[0];

        var allowedCommandsAndExtensions = commandsProvider.GetAllowedCommandsAndExtensions();

        // check if it starts with a valid file IO command like ".save"
        if (!allowedCommandsAndExtensions.TryGetValue(command, out var extensions))
        {
            return CompletionResult.Empty;
        }

        var result = CompletionResult.Empty;
        try
        {
            result = await intellisenseClient.GetCompletionResultAsync(lastArg);
        }
        catch (IntellisenseException exc)
        {
            logger.LogWarning(exc, "Intellisense exception occurred");
        }
        catch (OperationCanceledException exc)
        {
            logger.LogDebug(exc, "Intellisense request cancelled");
        }

        if (extensions.Count > 0 && !result.IsEmpty())
        {
            return result with
            {
                Entries = result
                    .Entries
                    .Where(x =>
                        {
                            // permit folders (which do not have extensions). note that files without extensions will still be allowed
                            var ext = Path.GetExtension(x.Name);
                            return ext == string.Empty || extensions.Contains(ext);
                        }
                    )
                    .ToList()
            };
        }

        return result;
    }

    public int Priority => 0;
}
