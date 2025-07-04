namespace lokqlDxComponents.Services;

public interface ICompletionManagerServiceLocator : IIntellisenseResourceProvider
{
    IntellisenseClientAdapter _intellisenseClient { get; }
}