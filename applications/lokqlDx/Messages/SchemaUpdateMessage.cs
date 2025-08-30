using CommunityToolkit.Mvvm.Messaging.Messages;
using Lokql.Engine;

public class SchemaUpdateMessage(SchemaLine[] lines) : ValueChangedMessage<SchemaLine[]>(lines);