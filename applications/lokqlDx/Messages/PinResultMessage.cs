using CommunityToolkit.Mvvm.Messaging.Messages;
using KustoLoco.Core;
using LokqlDx.ViewModels;

public class PinResultMessage(QueryResultWithSender result) : ValueChangedMessage<QueryResultWithSender>(result);


public class DisplayResultMessage(NamedKustoResult result) : ValueChangedMessage<NamedKustoResult>(result);


public readonly record struct QueryResultWithSender(string Sender, KustoQueryResult Result, bool ImmediateDisplay);



public class ThemeChangedMessage(string theme) : ValueChangedMessage<string>(theme);


public class ShowQueryRequestMessage(QueryDocumentViewModel query) : ValueChangedMessage<QueryDocumentViewModel>(query);


