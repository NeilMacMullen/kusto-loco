using CommunityToolkit.Mvvm.Messaging.Messages;

public class SaveFileMessage(string path) : AsyncRequestMessage<Task>
{
    public readonly string Path = path;
}
