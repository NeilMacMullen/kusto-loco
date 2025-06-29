using Intellisense;
using Microsoft.Extensions.Logging;

namespace lokqlDxComponents.Services;

public interface ICompletionManagerServiceLocator : IIntellisenseResourceProvider
{
    IntellisenseClient _intellisenseClient { get; }
    ILogger _logger { get; }
    Dictionary<string, HashSet<string>> _allowedCommandsAndExtensions { get; }
}