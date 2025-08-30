using CommunityToolkit.Mvvm.Messaging.Messages;

public class ShowToolMessage(string tool) : ValueChangedMessage<string>(tool);