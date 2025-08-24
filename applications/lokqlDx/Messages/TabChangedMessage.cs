using CommunityToolkit.Mvvm.Messaging.Messages;
using DependencyPropertyGenerator;
using LokqlDx.ViewModels;

public class TabChangedMessage(QueryDocument active) : ValueChangedMessage<QueryDocument>(active);
