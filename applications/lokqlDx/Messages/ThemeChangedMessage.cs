using CommunityToolkit.Mvvm.Messaging.Messages;

public class ThemeChangedMessage(string theme) : ValueChangedMessage<string>(theme);