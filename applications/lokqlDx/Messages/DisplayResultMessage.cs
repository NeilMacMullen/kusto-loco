using CommunityToolkit.Mvvm.Messaging.Messages;
using LokqlDx.ViewModels;

public class DisplayResultMessage(PinnedKustoResult result) : ValueChangedMessage<PinnedKustoResult>(result);
