using CommunityToolkit.Mvvm.Messaging.Messages;
using DependencyPropertyGenerator;
using LokqlDx.ViewModels;

public class TabChangedMessage(QueryItemViewModel active) : ValueChangedMessage<QueryItemViewModel>(active);
