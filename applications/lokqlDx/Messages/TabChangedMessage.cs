using CommunityToolkit.Mvvm.Messaging.Messages;
using DependencyPropertyGenerator;
using LokqlDx.ViewModels;

public class TabChangedMessage(QueryDocumentViewModel active) : ValueChangedMessage<QueryDocumentViewModel>(active);
