using CommunityToolkit.Mvvm.ComponentModel;
using Dock.Model.Mvvm.Controls;

namespace LokqlDx.ViewModels;

public partial class LokqlTool : Tool
{
    [ObservableProperty] private bool _isVisible;
}

public class ConsoleDocumentViewModel : LokqlTool
{
    public ConsoleDocumentViewModel(ConsoleViewModel model)
    {
        Title = "Output";
        CanFloat = true;
        CanDrop = true;
        CanPin = true;
        Model = model;
    }

    public ConsoleViewModel Model { get; private set; }
}
