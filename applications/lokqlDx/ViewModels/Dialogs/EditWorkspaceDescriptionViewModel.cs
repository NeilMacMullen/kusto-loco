using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace LokqlDx.ViewModels.Dialogs;

public partial class EditWorkspaceDescriptionViewModel : ObservableObject, IDialogViewModel
{
    private readonly TaskCompletionSource _completionSource;
    private readonly Workspace _workspace;
    [ObservableProperty] private string _description;

    public EditWorkspaceDescriptionViewModel(Workspace workspace)
    {
        _workspace = workspace;
        _description = workspace.Description;
        _completionSource = new TaskCompletionSource();
        Result = _completionSource.Task;
    }

    public Task Result { get; }

    [RelayCommand]
    private void Cancel() => _completionSource.TrySetResult();

    [RelayCommand]
    private void Save()
    {
        _workspace.Description = Description;
        _completionSource.TrySetResult();
    }
}
