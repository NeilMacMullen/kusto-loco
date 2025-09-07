using CommunityToolkit.Mvvm.ComponentModel;
using KustoLoco.Core;
using KustoLoco.Core.Settings;

namespace LokqlDx.ViewModels;

public partial class PinnedKustoResult : ObservableObject
{
    [ObservableProperty] private DateTime _created = DateTime.MinValue;
    [ObservableProperty] private string _description = string.Empty;
    [ObservableProperty] private bool _editLocked = true;
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private KustoQueryResult _result = KustoQueryResult.Empty;

    public KustoSettingsProvider Settings;
    public PinnedKustoResult(QueryResultWithSender result)
    {
        Result = result.Result;
        Name = result.Sender + $" - {DateTime.Now:HH:mm:ss}";
        Created = DateTime.Now;
        Description = $"Rows:{Result.RowCount} Columns:{Result.ColumnCount}";
        Settings= result.Settings;
    }
}
