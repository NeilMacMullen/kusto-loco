using CommunityToolkit.Mvvm.Messaging.Messages;
using LokqlDx.ViewModels;

public class DisplayResultMessage(NamedKustoResult result) : ValueChangedMessage<NamedKustoResult>(result);