using Dock.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm;
using Dock.Model.Mvvm.Controls;

namespace LokqlDx.ViewModels;

public class DockFactory : Factory
{
    private readonly Func<QueryDocumentViewModel> _create;
    private readonly Action<QueryDocumentViewModel> _onActiveChanged;

    public QueryDocumentDock? DocumentDock;

    public DockFactory(Func<QueryDocumentViewModel> create, Action<QueryDocumentViewModel> onActiveChanged)
    {
        _create = create;
        _onActiveChanged = onActiveChanged;
    }

    public override IDocumentDock CreateDocumentDock() => new QueryDocumentDock(_create);

    public override IRootDock CreateLayout()
    {
        var documentDock = new QueryDocumentDock(_create)
        {
            IsCollapsable = false,
            VisibleDockables = CreateList<IDockable>(),
            CanCreateDocument = true,
            CanCloseLastDockable = true
        };

        var c = new ConsoleDocumentViewModel();
        var toolDock = new ToolDock()
        {
            IsCollapsable = true,
            VisibleDockables = CreateList<IDockable>(c),
            CanCloseLastDockable = true
        };


        var mainLayout = new ProportionalDock
        {
            // EnableGlobalDocking = false,
            Orientation = Orientation.Vertical,
            VisibleDockables = CreateList<IDockable>
            (
                documentDock,
                new ProportionalDockSplitter { ResizePreview = true },
                documentDock,
                new ProportionalDockSplitter(),
                toolDock
            )
        };

        var rootDock = CreateRootDock();

        rootDock.IsCollapsable = false;
        rootDock.ActiveDockable = documentDock;
        rootDock.DefaultDockable = documentDock;
        rootDock.VisibleDockables = CreateList<IDockable>(mainLayout);

        DocumentDock = documentDock;
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
        DocumentDock?.AddDocument(query);
    }

    public IRootDock? GetActiveRoot() =>
        Find(d => d is IRootDock { IsActive: true })
            .OfType<IRootDock>()
            .FirstOrDefault();

    public QueryDocumentViewModel? GetActive() => GetActiveRoot()?.FocusedDockable as QueryDocumentViewModel;

    public override void OnActiveDockableChanged(IDockable? dockable)
    {
        var vm = dockable as QueryDocumentViewModel;
        base.OnActiveDockableChanged(dockable);
        if (vm != null)
            _onActiveChanged?.Invoke(vm);
    }
}
