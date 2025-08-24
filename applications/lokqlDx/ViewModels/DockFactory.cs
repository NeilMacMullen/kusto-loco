using Dock.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm;
using Vanara.PInvoke;

namespace LokqlDx.ViewModels;

public class DockFactory : Factory
{
    private readonly Func<QueryDocumentViewModel> _create;

    public DockFactory(Func<QueryDocumentViewModel> create)
    {
        _create = create;
    }
    
    public QueryDocumentDock? MainDock;

    public override IDocumentDock CreateDocumentDock() => new QueryDocumentDock(_create);

    public override IRootDock CreateLayout()
    {
        var documentDock = new QueryDocumentDock(_create)
        {
            IsCollapsable = false,
            VisibleDockables = CreateList<IDockable>(),
            CanCreateDocument = true,
            CanCloseLastDockable = true,
        };

        var rootDock = CreateRootDock();

        rootDock.IsCollapsable = false;
        rootDock.ActiveDockable = documentDock;
        rootDock.DefaultDockable = documentDock;
        rootDock.VisibleDockables = CreateList<IDockable>(documentDock);

        MainDock = documentDock;
        return rootDock;
    }

    public override void InitLayout(IDockable layout)
    {
        HostWindowLocator = new Dictionary<string, Func<IHostWindow?>>
        {
            [nameof(IDockWindow)] = () => new HostWindow()
        };

        base.InitLayout(layout);
    }

    public void AddDocument(QueryDocumentViewModel query)
    {
        MainDock?.CreateNewDocument();
    }
}
