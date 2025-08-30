using CommunityToolkit.Mvvm.Messaging.Messages;

public class RunningQueryMessage(bool isRunning) : AsyncRequestMessage<bool>
{
    public bool IsRunning { get; } = isRunning;
}
