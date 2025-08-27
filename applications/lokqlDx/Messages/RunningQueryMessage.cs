using CommunityToolkit.Mvvm.Messaging.Messages;
using Lokql.Engine;
using LokqlDx.ViewModels;

public class RunningQueryMessage(bool isRunning) : AsyncRequestMessage<bool>
{
    public bool IsRunning { get; } = isRunning;
}


public class CreateDocumentRequest(string title) : RequestMessage<QueryDocumentViewModel>
{
    public string Title { get; set; } = title;
    public QueryDocumentViewModel? Model;
}

public class SchemaUpdateMessage(SchemaLine[] lines) : ValueChangedMessage<SchemaLine[]>(lines);


public class ShowToolMessage(string tool) : ValueChangedMessage<string>(tool);
