using Avalonia.Media;
using Avalonia.Metadata;
using Dock.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm;
using Dock.Model.Mvvm.Controls;
using Dock.Model.Mvvm.Core;
using DocumentFormat.OpenXml.Drawing.Diagrams;

namespace LokqlDx.ViewModels;

public class DockFactory : Factory
{
    private readonly ConsoleViewModel _console;
    private readonly Func<QueryDocumentViewModel> _create;
    private readonly Action<QueryDocumentViewModel> _onActiveChanged;

    public QueryDocumentDock? DocumentDock;

    public DockFactory(ConsoleViewModel console, Func<QueryDocumentViewModel> create,
        Action<QueryDocumentViewModel> onActiveChanged)
    {
        _console = console;
        _create = create;
        _onActiveChanged = onActiveChanged;
    }

    public IRootDock Layout { get; set; } = new RootDock();

    public ToolDock? ToolDock { get; set; }

    public override IDocumentDock CreateDocumentDock() => new QueryDocumentDock(_create);

    public void CloseLayout()
    {
        if (Layout is not IDock dock) return;
        if (dock.Close.CanExecute(null))
            dock.Close.Execute(null);
    }

    public void ResetLayout()
    {
        if (Layout.Close.CanExecute(null))
            Layout.Close.Execute(null);
    }

    public IRootDock? GetOrResetLayout()
    {

        (IDock,IDock)[] GetBases(IDock parent)
            => parent.VisibleDockables ==null ? []: parent.VisibleDockables.OfType<IDock>()
                .Select(b => (b, parent))
                .ToArray();
        
        (QueryDocumentDock,IDock)[] Find(IDock item,IDock parent)
        {
            if (item is QueryDocumentDock qd)
                return [(qd,parent )];
            var bases = GetBases(item);
            return bases.SelectMany(f=>Find(f.Item1,f.Item2))
                .ToArray();
        }
        
        if (DocumentDock is not null)
        {
            var hostWindows = HostWindows.OfType<HostWindow>().ToArray();
            foreach (var h in hostWindows)
            {
                if (h.DataContext is RootDock root)
                {
                    var docWindows =Find(root,new RootDock());
                    foreach (var (queryDocumentDock,parent) in docWindows)
                    {
                        var docs = queryDocumentDock.VisibleDockables!.OfType<QueryDocumentViewModel>().ToArray();
                        foreach(var d in docs)
                            RemoveVisibleDockable(queryDocumentDock,d);
                    }
                    if (root.VisibleDockables!.Count==0)
                        h.Close();
                }
            }
            RemoveAllVisibleDockables(DocumentDock);
            return null;
        }

        return CreateLayout();
    }

    public override IRootDock CreateLayout()
    {
        //CloseLayout();


        var documentDock = new QueryDocumentDock(_create)
        {
            IsCollapsable = false,
            VisibleDockables = CreateList<IDockable>(),
            CanCreateDocument = true,
            CanCloseLastDockable = true
        };

        var c = new ConsoleDocumentViewModel(_console);

        var tooldockExists = ToolDock != null;
        ToolDock ??= new ToolDock
        {
            CanDrag = true,
            CanFloat = true,
            CanDrop = true,
            ActiveDockable = c,
            Alignment = Alignment.Top,
            GripMode = GripMode.Visible,
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
                new ProportionalDockSplitter(),
                ToolDock
            )
        };

        var rootDock = CreateRootDock();

        rootDock.IsCollapsable = false;
        rootDock.ActiveDockable = mainLayout;
        rootDock.DefaultDockable = mainLayout;
        rootDock.VisibleDockables = CreateList<IDockable>(mainLayout);

        DocumentDock = documentDock;
        Layout = rootDock;
        return rootDock;
    }

    public override void InitLayout(IDockable layout)
    {
        HostWindowLocator = new Dictionary<string, Func<IHostWindow?>>
        {
            [nameof(IDockWindow)] = () => new HostWindow(){Background =Brushes.Black}
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
