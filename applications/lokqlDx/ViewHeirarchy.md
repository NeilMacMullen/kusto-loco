

# MainViewModel
Responsible for managing the query engine, global context and current query set. Also manages Workspace serialisation and the single ConsoleView

The query set is maintained as a collection of QueryDocuments

In the MainView, the main Tab/DockControl is bound to the collection of QueryDocuments
The tab template binds to the QueryViewModel property which is then rendered in a
QueryView

# QueryViewModel
This manages both the editor pane (QueryEditorViewModel) for the query as well as the rendering surface (RenderingSurfaceViewModel)

# QueryEditorViewModel
This is a wrapper around an AvaloniaEdit texteditor.  It's reponsible for setting up the editor with intellisense and processing text input (most notable the 'run query' keyboard shortcut)

# RenderingSurfaceViewModel

Contains a tabcontrol with two panes:
- TreeDataGrid for table display
- ChartView for rendering charts

# ChartView
ChartView uses a ScottPlot chart which is not really bindable so all the logic is handled in the code-behind.

# ConsoleViewModel
ConsoleViewModel is effectively a singleton tool that is managed by MainViewModel
