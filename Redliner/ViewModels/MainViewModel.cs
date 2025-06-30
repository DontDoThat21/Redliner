using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Collections.ObjectModel;
using Redliner.Services;
using Redliner.Data;
using Redliner.Models;

namespace Redliner.ViewModels;

/// <summary>
/// Main ViewModel for the application's main window
/// </summary>
public partial class MainViewModel : ViewModelBase
{
    private readonly IDocumentService _documentService;
    private readonly IDocumentViewerService _documentViewerService;
    private readonly IAnnotationService _annotationService;

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

    [ObservableProperty]
    private ObservableCollection<Annotation> currentAnnotations = new();

    [ObservableProperty]
    private int currentDocumentId = 0;

    // Event to communicate with the View for zoom operations
    public event Action? ZoomInRequested;
    public event Action? ZoomOutRequested;
    public event Action? FitToWindowRequested;
    
    // Event to communicate annotation updates to the View
    public event Action<IEnumerable<Annotation>>? AnnotationsUpdated;

    public MainViewModel()
    {
        var context = new RedlinerDbContext();
        _documentService = new DocumentService(context);
        _documentViewerService = new DocumentViewerService();
        _annotationService = new AnnotationService(context);
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
                    CurrentDocumentId = document.Id;
                    
                    // Load annotations for this document
                    await LoadDocumentAnnotationsAsync(document.Id);
                    
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

    private async Task LoadDocumentAnnotationsAsync(int documentId)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"Loading annotations for document ID: {documentId}");
            var annotations = await _annotationService.GetDocumentAnnotationsAsync(documentId);
            System.Diagnostics.Debug.WriteLine($"Found {annotations.Count()} annotations");
            
            CurrentAnnotations.Clear();
            foreach (var annotation in annotations)
            {
                CurrentAnnotations.Add(annotation);
                System.Diagnostics.Debug.WriteLine($"- {annotation.Type} at ({annotation.X}, {annotation.Y}) - {annotation.Color}");
            }
            
            // Notify the view to update annotation display
            AnnotationsUpdated?.Invoke(CurrentAnnotations);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading annotations: {ex.Message}");
        }
    }

    public async Task SaveAnnotationAsync(Annotation annotation)
    {
        try
        {
            var savedAnnotation = await _annotationService.CreateAnnotationAsync(annotation);
            CurrentAnnotations.Add(savedAnnotation);
            
            // Notify the view to update annotation display
            AnnotationsUpdated?.Invoke(CurrentAnnotations);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving annotation: {ex.Message}");
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