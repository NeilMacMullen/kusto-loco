using CommunityToolkit.Mvvm.Messaging.Messages;


// Create a message
public class LayoutChangedMessage(int layout) : ValueChangedMessage<int>(layout);
