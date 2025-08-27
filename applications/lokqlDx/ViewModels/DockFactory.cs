using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Messaging;
using Dock.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm;
using Dock.Model.Mvvm.Controls;

namespace LokqlDx.ViewModels;

public class DockFactory : Factory
{
    private readonly ConsoleViewModel _console;
    private readonly Func<QueryDocumentViewModel> _create;
    private readonly QueryLibraryViewModel _library;
    private readonly Action<QueryDocumentViewModel> _onActiveChanged;
    private readonly SchemaViewModel _schema;

    public QueryDocumentDock? DocumentDock;

    private QueryDocumentViewModel? LastActiveDockable;

    public DockFactory(ConsoleViewModel console, QueryLibraryViewModel library,
        SchemaViewModel schema,
        //TODO - replace with messages!
        Func<QueryDocumentViewModel> create,
        Action<QueryDocumentViewModel> onActiveChanged)
    {
        _console = console;
        _library = library;
        _schema = schema;
        _create = create;
        _onActiveChanged = onActiveChanged;
        WeakReferenceMessenger.Default.Register<InsertTextMessage>(this, InsertTextInActiveWindow);
        WeakReferenceMessenger.Default.Register<DisplayResultMessage>(this, DisplayResult);
        WeakReferenceMessenger.Default.Register<ShowQueryRequestMessage>(this, ShowQuery);
    }

    public IRootDock Layout { get; set; } = new RootDock();

    public IDockable? ToolDock { get; set; }


    private void DisplayResult(object recipient, DisplayResultMessage message)
    {
        var named = message.Value;
        var model = new ResultDisplayViewModel(named.Name, named.Result);
        AddDockable(new RootDock(), model);
        FloatDockable(model);
    }

    private void InsertTextInActiveWindow(object recipient, InsertTextMessage message) => InsertText(message.Value);

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

    public void RemoveAllDocuments()
    {
        var allDocs = _library.Queries.ToArray();
        foreach (var d in allDocs) RemoveDockable(d, true);
    }

    public IRootDock? GetOrResetLayout()
    {
        IDock[] GetBases(IDock parent)
        {
            return parent.VisibleDockables == null
                ? []
                : parent.VisibleDockables.OfType<IDock>()
                    .Select(b => b)
                    .ToArray();
        }

        QueryDocumentDock[] FindDocuments(IDock item)
        {
            if (item is QueryDocumentDock qd)
                return [qd];
            var bases = GetBases(item);
            return bases.SelectMany(FindDocuments)
                .ToArray();
        }

        if (DocumentDock is not null)
        {
            RemoveAllDocuments();

            var hostWindows = HostWindows.OfType<HostWindow>().ToArray();
            foreach (var h in hostWindows)
                if (h.DataContext is RootDock root)
                {
                    var docWindows = FindDocuments(root);
                    foreach (var queryDocumentDock in docWindows)
                    {
                        var docs = queryDocumentDock.VisibleDockables!.OfType<QueryDocumentViewModel>().ToArray();
                        foreach (var d in docs)
                            RemoveVisibleDockable(queryDocumentDock, d);
                        if (queryDocumentDock.VisibleDockables!.Count == 0)
                        {
                            var parent = FindRoot(queryDocumentDock);
                            //RemoveDockable(queryDocumentDock);
                        }
                    }

                    if (root.VisibleDockables!.Count == 0)
                        h.Close();
                }

            RemoveAllVisibleDockables(DocumentDock);

            return null;
        }

        return CreateLayout();
    }


    private ToolDock Create(params Tool[] tools) =>
        new()
        {
            CanDrag = true,
            CanFloat = true,
            CanDrop = true,
            ActiveDockable = tools.First(),
            Alignment = Alignment.Top,
            GripMode = GripMode.Visible,
            IsCollapsable = true,
            VisibleDockables = CreateList<IDockable>(tools.OfType<IDockable>().ToArray()),
            CanCloseLastDockable = true
        };

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

        var console = new ConsoleDocumentViewModel(_console);

        var con = Create(console);
        var pinnedResults = new PinnedResultsViewModel();
        var rest = Create(_library, _schema, pinnedResults);
        con.Proportion = 0.6;
        rest.Proportion = 0.4;
        ToolDock = new ProportionalDock
        {
            Orientation = Orientation.Horizontal,
            VisibleDockables = CreateList<IDockable>
            (
                con,
                new ProportionalDockSplitter(),
                rest
            )
        };
        documentDock.Proportion = 0.8;
        ToolDock.Proportion = 0.2;
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

    public void InsertText(string text)
    {
        if (LastActiveDockable == null)
            return;


        //do it in the background otherwise the sender reclaims focus
        Dispatcher.UIThread.Post(() =>
        {
            LastActiveDockable.QueryViewModel.QueryEditorViewModel.Insert(text);
            SetActiveDockable(LastActiveDockable);
            SetFocusedDockable(FindRoot(LastActiveDockable)!, LastActiveDockable);
        });
    }

    public override void InitLayout(IDockable layout)
    {
        HostWindowLocator = new Dictionary<string, Func<IHostWindow?>>
        {
            [nameof(IDockWindow)] = () =>
            {
                var window = new HostWindow();

                window.Bind(Window.BackgroundProperty,
                    new Binding("DockThemeForegroundBrush"));

                return window;
            }
        };

        base.InitLayout(layout);
    }

    public override IDockWindow? CreateWindowFrom(IDockable dockable)
    {
        var window = base.CreateWindowFrom(dockable);

        if (window != null) window.Title = "NewWindow";
        return window;
    }

    public void AddDocument(QueryDocumentViewModel query)
    {
        if (query.IsVisible)
            DocumentDock?.AddDocument(query);
    }

    public IRootDock? GetActiveRoot() =>
        Find(d => d is IRootDock { IsActive: true })
            .OfType<IRootDock>()
            .FirstOrDefault();

    public QueryDocumentViewModel? GetActive()
    {
        var focussed = Find(d => d is IRootDock)
            .OfType<IRootDock>()
            .Select(r => r.FocusedDockable)
            .OfType<QueryDocumentViewModel>()
            .ToArray();
        return focussed.FirstOrDefault();
    }

    public override void OnActiveDockableChanged(IDockable? dockable)
    {
        if (dockable is QueryDocumentViewModel vm)
            LastActiveDockable = vm;

        base.OnActiveDockableChanged(dockable);
        if (LastActiveDockable != null)
            _onActiveChanged?.Invoke(LastActiveDockable);
    }

    public override void OnDockableClosed(IDockable? dockable)
    {
        if (dockable is QueryDocumentViewModel query) _library.ChangeVisibility(query, false);
        base.OnDockableClosed(dockable);
    }


    private void ShowQuery(object recipient, ShowQueryRequestMessage message)
    {
        var model = message.Value;
        if (!model.IsVisible)
            AddDocument(model);
        else
            SetActiveDockable(model);
    }
}
