using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Dock.Model.Mvvm.Controls;

namespace LokqlDx.ViewModels;

public partial class QueryDocumentViewModel : Document
{

    public QueryViewModel QueryViewModel { get; private set; }

    public QueryDocumentViewModel(string title, QueryViewModel content)
    {
        Title = title;
        QueryViewModel = content;
        WeakReferenceMessenger.Default.Register<TabChangedMessage>(this, (r, m) =>
        {
            QueryViewModel.IsActive = m.Value == this;
        });
    }
}
