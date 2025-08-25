using CommunityToolkit.Mvvm.Messaging.Messages;

public class InsertTextMessage(string text) : ValueChangedMessage<string>(text);
