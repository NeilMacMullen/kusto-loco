using CommunityToolkit.Mvvm.Messaging.Messages;

public class ToggleCursorMessage(string s) : ValueChangedMessage<string>(s);
