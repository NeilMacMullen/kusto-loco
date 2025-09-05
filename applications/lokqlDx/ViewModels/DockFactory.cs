using Avalonia.Threading;
using Dock.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm;
using Dock.Model.Mvvm.Controls;

namespace LokqlDx.ViewModels;

public class DockFactory : Factory
{
    private readonly ToolManager _toolManager;

    public QueryDocumentDock? DocumentDock;

    private QueryDocumentViewModel? LastActiveDockable;

    public DockFactory(ToolManager toolManager)
    {
        _toolManager = toolManager;

        Messaging.RegisterForValue<InsertTextMessage, string>(this, InsertTextInActiveWindow);
        Messaging.RegisterForValue<DisplayResultMessage, PinnedKustoResult>(this, DisplayResult);
        Messaging.RegisterForValue<ShowQueryRequestMessage, QueryDocumentViewModel>(this, ShowQuery);
        Messaging.RegisterForValue<ShowToolMessage, string>(this, ShowTool);
        Messaging.RegisterForEvent<ThemeChangedMessage>(this, UpdateBackgrounds);
    }


    public IRootDock Layout { get; set; } = new RootDock();

    public IDock? ToolDock { get; set; }

    private void UpdateBackgrounds()
    {
        var brush = ApplicationHelper.GetBackgroundForCurrentTheme();
        foreach (var hostWindow in HostWindows.OfType<HostWindow>()) hostWindow.Background = brush;
    }


    private void DisplayResult(PinnedKustoResult pinned)
    {
        var model = new ResultDisplayViewModel(pinned.Name, pinned.Result);
        AddDockable(new RootDock(), model);
        FloatDockable(model);
    }

    private void InsertTextInActiveWindow(string text) => InsertText(text);

    public override IDocumentDock CreateDocumentDock() => new QueryDocumentDock();

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
        var allDocs = _toolManager.LibraryViewModel.Queries.ToArray();
        foreach (var d in allDocs) RemoveDockable(d, true);
    }

    private bool _layoutInit;
    public IRootDock? GetOrResetLayout()
    {
        if (_layoutInit)
            return Layout;
        _layoutInit = true;
       
        var layout = CreateLayout();
        InitLayout(layout);
        return layout;
    }


    private ToolDock Create(params LokqlTool[] tools)
    {
        foreach (var lokqlTool in tools) lokqlTool.IsVisible = true;
        return new ToolDock
        {
            CanDrag = true,
            CanFloat = true,
            CanDrop = true,
            ActiveDockable = tools.First(),
            Alignment = Alignment.Top,
            GripMode = GripMode.Visible,
            IsCollapsable = true,
            VisibleDockables = CreateList(tools.OfType<IDockable>().ToArray()),
            CanCloseLastDockable = true
        };
    }


    public override IRootDock CreateLayout()
    {
        var documentDock = new QueryDocumentDock
        {
            IsCollapsable = false,
            VisibleDockables = CreateList<IDockable>(),
            CanCreateDocument = true,
            CanCloseLastDockable = true
        };


        var con = Create(_toolManager.Console);

        var rest = Create(_toolManager.LibraryViewModel, _toolManager.SchemaViewModel,
            _toolManager.PinnedResults);
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
        var brush = ApplicationHelper.GetBackgroundForCurrentTheme();
        HostWindowLocator = new Dictionary<string, Func<IHostWindow?>>
        {
            [nameof(IDockWindow)] = () =>
            {
                var window = new HostWindow
                {
                    Background = brush
                };


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
        if (dockable is not QueryDocumentViewModel vm)
            return;

        LastActiveDockable = vm;
        base.OnActiveDockableChanged(dockable);
        if (LastActiveDockable != null)
            Messaging.Send(new TabChangedMessage(vm));
    }

    public override void OnDockableClosed(IDockable? dockable)
    {
        if (dockable is QueryDocumentViewModel query)
            _toolManager.LibraryViewModel.ChangeVisibility(query, false);
        if (dockable is LokqlTool tool)
            tool.IsVisible = false;
        base.OnDockableClosed(dockable);
    }


    public void ShowQuery(QueryDocumentViewModel model)
    {
        if (!model.IsVisible)
        {
            model.IsVisible = true;
            AddDocument(model);
        }
        else
        {
            SetActiveDockable(model);
        }
    }

    private void ShowTool(string tool)
    {
        void CreateIfNotVisible(LokqlTool tool)
        {
            if (tool.IsVisible)
                return;
            AddDockable(new RootDock(), tool);
            FloatDockable(tool);
        }

        switch (tool)
        {
            case "console":
                CreateIfNotVisible(_toolManager.Console);
                break;
            case "queries":
                CreateIfNotVisible(_toolManager.LibraryViewModel);
                break;
            case "results":
                CreateIfNotVisible(_toolManager.PinnedResults);
                break;
            case "schema":
                CreateIfNotVisible(_toolManager.SchemaViewModel);
                break;
        }
    }
}
