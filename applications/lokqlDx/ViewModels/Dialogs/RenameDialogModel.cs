using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace LokqlDx.ViewModels.Dialogs;

public partial class RenameDialogModel : ObservableObject, IDialogViewModel
{
    private readonly TaskCompletionSource _completionSource;
    private readonly RenamableText _initialText;
    [ObservableProperty] private string _text;

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
