using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml.Templates;
using CommunityToolkit.Mvvm.Input;
using Dock.Model.Mvvm.Controls;
using LokqlDx.Views;

namespace LokqlDx.ViewModels;

public class QueryDocumentDock : DocumentDock
{
    private readonly Func<QueryDocumentViewModel> _create;

    public QueryDocumentDock(Func<QueryDocumentViewModel> create)
    {
        _create = create;
        CreateDocument = new RelayCommand(CreateNewDocument);
    }

    public void CreateNewDocument()
    {
        if (!CanCreateDocument)
        {
            return;
        }

        var index = VisibleDockables?.Count + 1;
        var document = _create();
        document.Title = $"{DateTime.Now.ToShortTimeString()}";

        Factory?.AddDockable(this, document);
        Factory?.SetActiveDockable(document);
        Factory?.SetFocusedDockable(this, document);
    }
    
}
