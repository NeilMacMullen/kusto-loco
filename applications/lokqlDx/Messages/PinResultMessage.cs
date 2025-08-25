using CommunityToolkit.Mvvm.Messaging.Messages;
using KustoLoco.Core;
using LokqlDx.ViewModels;
using Microsoft.Identity.Client;

public class PinResultMessage(QueryResultWithSender result) : ValueChangedMessage<QueryResultWithSender>(result);


public class DisplayResultMessage(NamedKustoResult result) : ValueChangedMessage<NamedKustoResult>(result);


public readonly record struct QueryResultWithSender(string Sender,KustoQueryResult Result);
