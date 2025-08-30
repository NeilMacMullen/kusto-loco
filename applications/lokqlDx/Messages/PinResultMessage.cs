using CommunityToolkit.Mvvm.Messaging.Messages;

public class PinResultMessage(QueryResultWithSender result) : ValueChangedMessage<QueryResultWithSender>(result);
