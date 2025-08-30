using CommunityToolkit.Mvvm.Messaging.Messages;

public class LoadFileMessage(string path) : AsyncRequestMessage<Task>
{
    public readonly string Path = path;
}
