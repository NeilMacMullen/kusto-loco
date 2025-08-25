using CommunityToolkit.Mvvm.Messaging.Messages;
using KustoLoco.Core;

public class PinResultMessage(KustoQueryResult result) : ValueChangedMessage<KustoQueryResult>(result);


public class DisplayResultMessage(KustoQueryResult result) : ValueChangedMessage<KustoQueryResult>(result);
