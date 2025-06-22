using System.Collections.ObjectModel;
using Avalonia.Media;
using AvaloniaEdit.Document;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocumentFormat.OpenXml.Drawing;

namespace LokqlDx.ViewModels.Dialogs;

public partial class RenameDialogModel : ObservableObject, IDialogViewModel
{
    private readonly RenamableText _initialText;

    private readonly TaskCompletionSource _completionSource;
    [ObservableProperty] private string  _text;

    public RenameDialogModel(RenamableText initialText)
    {
        _initialText = initialText;

        Text = _initialText.InitialText;
        _completionSource = new TaskCompletionSource();
        Result = _completionSource.Task;
    }

    public Task Result { get; }

    [RelayCommand]
    private void Cancel() => _completionSource.SetResult();

    [RelayCommand]
    private void Save()
    {
        _initialText.NewText = Text;
        _completionSource.SetResult();
    }
}
