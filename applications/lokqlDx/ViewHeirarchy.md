

# MainViewModel
Responsible for managing the query engine, global context and current query set. Also manages Workspace serialisation.

The query set is maintained as a collection of QueryDocuments

In the MainView, the main Tab/DockControl is bound to the collection of QueryDocuments
The tab template binds to the QueryViewModel property which is then rendered in a
QueryView

# QueryViewModel
