using CommunityToolkit.Mvvm.Messaging.Messages;
using LokqlDx.ViewModels;

public class ShowQueryRequestMessage(QueryDocumentViewModel query) : ValueChangedMessage<QueryDocumentViewModel>(query);
