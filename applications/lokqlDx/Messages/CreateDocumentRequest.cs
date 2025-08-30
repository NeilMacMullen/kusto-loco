using CommunityToolkit.Mvvm.Messaging.Messages;
using LokqlDx.ViewModels;

public class CreateDocumentRequest(string title) : RequestMessage<QueryDocumentViewModel>
{
    public QueryDocumentViewModel? Model;
    public string Title { get; set; } = title;
}
