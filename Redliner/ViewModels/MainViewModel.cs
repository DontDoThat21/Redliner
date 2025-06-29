using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using Redliner.Services;
using Redliner.Data;

namespace Redliner.ViewModels;

/// <summary>
/// Main ViewModel for the application's main window
/// </summary>
public partial class MainViewModel : ViewModelBase
{
    private readonly IDocumentService _documentService;
    private readonly IDocumentViewerService _documentViewerService;

    [ObservableProperty]
    private string title = "Redliner - Engineering Annotator";

    [ObservableProperty]
    private string statusText = "Ready";

    [ObservableProperty]
    private bool isFileLoaded = false;

    [ObservableProperty]
    private string? currentFilePath;

    [ObservableProperty]
    private DocumentTreeViewModel documentTree;

    [ObservableProperty]
    private UIElement? documentContent;

    [ObservableProperty]
    private bool isDocumentLoading = false;

    // Event to communicate with the View for zoom operations
    public event Action? ZoomInRequested;
    public event Action? ZoomOutRequested;
    public event Action? FitToWindowRequested;

    public MainViewModel()
    {
        _documentService = new DocumentService(new RedlinerDbContext());
        _documentViewerService = new DocumentViewerService();
        DocumentTree = new DocumentTreeViewModel(_documentService);
        
        // Subscribe to document tree events
        DocumentTree.DocumentSelected += OnDocumentTreeItemSelected;
    }

    private async void OnDocumentTreeItemSelected(string filePath)
    {
        if (File.Exists(filePath))
        {
            await OpenDocumentAsync(filePath);
        }
        else
        {
            StatusText = "Selected file no longer exists.";
        }
    }

    [RelayCommand]
    private async Task OpenFileAsync()
    {
        try
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Open Document",
                Filter = "All Supported Files|*.pdf;*.dwg;*.dwf;*.dxf|PDF Files (*.pdf)|*.pdf|DWG Files (*.dwg)|*.dwg|DWF Files (*.dwf)|*.dwf|DXF Files (*.dxf)|*.dxf",
                FilterIndex = 1,
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == true)
            {
                await OpenDocumentAsync(openFileDialog.FileName);
            }
            else
            {
                StatusText = "Ready";
            }
        }
        catch (Exception ex)
        {
            StatusText = $"Error opening file: {ex.Message}";
        }
    }

    private async Task OpenDocumentAsync(string filePath)
    {
        try
        {
            IsDocumentLoading = true;
            StatusText = $"Opening {Path.GetFileName(filePath)}...";
            
            // Validate file type
            if (!_documentService.IsValidFileType(filePath))
            {
                StatusText = "Invalid file type. Please select a PDF, DWG, DWF, or DXF file.";
                return;
            }

            // Add/update document in database
            var document = await _documentService.OpenDocumentAsync(filePath);
            if (document != null)
            {
                CurrentFilePath = filePath;
                
                // Update the document tree
                await DocumentTree.AddRecentDocumentAsync(filePath);
                
                // Load document content into viewer
                StatusText = $"Rendering {Path.GetFileName(filePath)}...";
                DocumentContent = await _documentViewerService.LoadDocumentAsync(filePath);
                
                if (DocumentContent != null)
                {
                    IsFileLoaded = true;
                    StatusText = $"Opened {Path.GetFileName(filePath)}";
                }
                else
                {
                    StatusText = "Failed to render document content.";
                    IsFileLoaded = false;
                    DocumentContent = null;
                }
            }
            else
            {
                StatusText = "Failed to open document.";
                IsFileLoaded = false;
                DocumentContent = null;
            }
        }
        catch (Exception ex)
        {
            StatusText = $"Error opening document: {ex.Message}";
            IsFileLoaded = false;
            DocumentContent = null;
        }
        finally
        {
            IsDocumentLoading = false;
        }
    }

    [RelayCommand]
    private async Task SaveFileAsync()
    {
        try
        {
            if (!IsFileLoaded)
            {
                StatusText = "No document loaded to save.";
                return;
            }

            var saveFileDialog = new SaveFileDialog
            {
                Title = "Save Document",
                Filter = "PDF Files (*.pdf)|*.pdf|DWG Files (*.dwg)|*.dwg|DWF Files (*.dwf)|*.dwf|DXF Files (*.dxf)|*.dxf",
                FilterIndex = 1,
                FileName = CurrentFilePath != null ? Path.GetFileNameWithoutExtension(CurrentFilePath) : "document"
            };

            // Set default extension based on current file
            if (!string.IsNullOrEmpty(CurrentFilePath))
            {
                var extension = Path.GetExtension(CurrentFilePath).ToLowerInvariant();
                switch (extension)
                {
                    case ".pdf":
                        saveFileDialog.FilterIndex = 1;
                        saveFileDialog.DefaultExt = "pdf";
                        break;
                    case ".dwg":
                        saveFileDialog.FilterIndex = 2;
                        saveFileDialog.DefaultExt = "dwg";
                        break;
                    case ".dwf":
                        saveFileDialog.FilterIndex = 3;
                        saveFileDialog.DefaultExt = "dwf";
                        break;
                    case ".dxf":
                        saveFileDialog.FilterIndex = 4;
                        saveFileDialog.DefaultExt = "dxf";
                        break;
                }
            }

            if (saveFileDialog.ShowDialog() == true)
            {
                StatusText = $"Saving {Path.GetFileName(saveFileDialog.FileName)}...";
                
                // TODO: Implement actual save logic with annotations
                // For now, we'll just copy the original file and add annotations in the future
                if (!string.IsNullOrEmpty(CurrentFilePath) && File.Exists(CurrentFilePath))
                {
                    await Task.Run(() => File.Copy(CurrentFilePath, saveFileDialog.FileName, true));
                    StatusText = $"Saved {Path.GetFileName(saveFileDialog.FileName)}";
                    
                    // TODO: Export annotations to the saved file
                    // var fileService = new FileService(new Data.RedlinerDbContext());
                    // await fileService.ExportAnnotationsAsync(saveFileDialog.FileName, documentId);
                }
                else
                {
                    StatusText = "No source file to save.";
                }
            }
            else
            {
                StatusText = "Ready";
            }
        }
        catch (Exception ex)
        {
            StatusText = $"Error saving file: {ex.Message}";
        }
    }

    [RelayCommand]
    private void ZoomIn()
    {
        ZoomInRequested?.Invoke();
        StatusText = "Zoomed in";
    }

    [RelayCommand]
    private void ZoomOut()
    {
        ZoomOutRequested?.Invoke();
        StatusText = "Zoomed out";
    }

    [RelayCommand]
    private void FitToWindow()
    {
        FitToWindowRequested?.Invoke();
        StatusText = "Fit to window";
    }

    [RelayCommand]
    private void Exit()
    {
        System.Windows.Application.Current.Shutdown();
    }
}