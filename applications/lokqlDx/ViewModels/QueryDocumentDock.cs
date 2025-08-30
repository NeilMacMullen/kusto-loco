using CommunityToolkit.Mvvm.Input;
using Dock.Model.Mvvm.Controls;

namespace LokqlDx.ViewModels;

public class QueryDocumentDock : DocumentDock
{
    public QueryDocumentDock()
    {
        CreateDocument = new RelayCommand(CreateNewDocument);
    }

    public void CreateNewDocument()
    {
        if (!CanCreateDocument) return;

        var index = VisibleDockables?.Count + 1;
        var title = $"query {index}";
        var msg = Messaging.Send(new CreateDocumentRequest(title));

        var document = msg.Model!;
        Factory?.AddDockable(this, document);
        Factory?.SetActiveDockable(document);
        Factory?.SetFocusedDockable(this, document);
    }
}
