using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

namespace Redliner.ViewModels;

/// <summary>
/// Main ViewModel for the application's main window
/// </summary>
public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private string title = "Redliner - Engineering Annotator";

    [ObservableProperty]
    private string statusText = "Ready";

    [ObservableProperty]
    private bool isFileLoaded = false;

    [ObservableProperty]
    private string? currentFilePath;

    [RelayCommand]
    private async Task OpenFileAsync()
    {
        // TODO: Implement file opening logic
        StatusText = "Opening file...";
        await Task.Delay(100); // Placeholder
        StatusText = "Ready";
    }

    [RelayCommand]
    private async Task SaveFileAsync()
    {
        // TODO: Implement file saving logic
        StatusText = "Saving file...";
        await Task.Delay(100); // Placeholder
        StatusText = "Ready";
    }

    [RelayCommand]
    private void Exit()
    {
        // TODO: Implement exit logic
        System.Windows.Application.Current.Shutdown();
    }
}