using CommunityToolkit.Mvvm.Messaging.Messages;
using LokqlDx.ViewModels;

public class CreateDocumentRequest(string title) : RequestMessage<QueryDocumentViewModel>
{
    public string Title { get; set; } = title;
    public QueryDocumentViewModel? Model;
}