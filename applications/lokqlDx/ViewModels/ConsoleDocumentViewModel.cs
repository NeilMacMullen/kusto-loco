using Dock.Model.Mvvm.Controls;

namespace LokqlDx.ViewModels;

public class ConsoleDocumentViewModel : Tool
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
