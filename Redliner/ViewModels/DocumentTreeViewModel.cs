using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.IO;
using Redliner.Models;
using Redliner.Services;
using Redliner.Data;

namespace Redliner.ViewModels;

/// <summary>
/// ViewModel for the document tree showing recent documents
/// </summary>
public partial class DocumentTreeViewModel : ViewModelBase
{
    private readonly IDocumentService _documentService;

    [ObservableProperty]
    private ObservableCollection<DocumentTreeItemViewModel> recentDocuments = new();

    [ObservableProperty]
    private DocumentTreeItemViewModel? selectedDocument;

    public event Action<string>? DocumentSelected;

    public DocumentTreeViewModel()
    {
        _documentService = new DocumentService(new RedlinerDbContext());
        _ = LoadRecentDocumentsAsync();
    }

    public DocumentTreeViewModel(IDocumentService documentService)
    {
        _documentService = documentService;
        _ = LoadRecentDocumentsAsync();
    }

    public async Task LoadRecentDocumentsAsync()
    {
        try
        {
            var documents = await _documentService.GetRecentDocumentsAsync(20);
            
            RecentDocuments.Clear();
            
            foreach (var document in documents)
            {
                var treeItem = new DocumentTreeItemViewModel(document);
                treeItem.DocumentSelected += OnDocumentItemSelected;
                RecentDocuments.Add(treeItem);
            }
        }
        catch (Exception ex)
        {
            // Log error - for now just continue
            System.Diagnostics.Debug.WriteLine($"Error loading recent documents: {ex.Message}");
        }
    }

    public async Task AddRecentDocumentAsync(string filePath)
    {
        try
        {
            var document = await _documentService.OpenDocumentAsync(filePath);
            if (document != null)
            {
                // Remove existing entry if it exists
                var existingItem = RecentDocuments.FirstOrDefault(r => r.FilePath == filePath);
                if (existingItem != null)
                {
                    RecentDocuments.Remove(existingItem);
                }

                // Add to top of list
                var newItem = new DocumentTreeItemViewModel(document);
                newItem.DocumentSelected += OnDocumentItemSelected;
                RecentDocuments.Insert(0, newItem);

                // Keep only the most recent 20 documents
                while (RecentDocuments.Count > 20)
                {
                    var lastItem = RecentDocuments.Last();
                    lastItem.DocumentSelected -= OnDocumentItemSelected;
                    RecentDocuments.RemoveAt(RecentDocuments.Count - 1);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error adding recent document: {ex.Message}");
        }
    }

    private void OnDocumentItemSelected(string filePath)
    {
        DocumentSelected?.Invoke(filePath);
    }

    [RelayCommand]
    private async Task RemoveDocumentAsync(DocumentTreeItemViewModel documentItem)
    {
        if (documentItem?.DocumentId != null)
        {
            try
            {
                await _documentService.DeleteDocumentAsync(documentItem.DocumentId.Value);
                documentItem.DocumentSelected -= OnDocumentItemSelected;
                RecentDocuments.Remove(documentItem);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error removing document: {ex.Message}");
            }
        }
    }

    [RelayCommand]
    private async Task RefreshDocumentsAsync()
    {
        await LoadRecentDocumentsAsync();
    }
}

/// <summary>
/// ViewModel for individual document tree items
/// </summary>
public partial class DocumentTreeItemViewModel : ViewModelBase
{
    [ObservableProperty]
    private string fileName = string.Empty;

    [ObservableProperty]
    private string filePath = string.Empty;

    [ObservableProperty]
    private string fileType = string.Empty;

    [ObservableProperty]
    private DateTime lastModified;

    [ObservableProperty]
    private bool fileExists;

    [ObservableProperty]
    private string displayName = string.Empty;

    [ObservableProperty]
    private string tooltip = string.Empty;

    public int? DocumentId { get; private set; }

    public event Action<string>? DocumentSelected;

    public DocumentTreeItemViewModel(Document document)
    {
        DocumentId = document.Id;
        FileName = document.FileName;
        FilePath = document.FilePath;
        FileType = document.FileType;
        LastModified = document.LastModified;
        FileExists = File.Exists(document.FilePath);
        
        UpdateDisplayProperties();
    }

    private void UpdateDisplayProperties()
    {
        DisplayName = FileExists ? FileName : $"{FileName} (Missing)";
        
        var lastModifiedText = LastModified.ToString("yyyy-MM-dd HH:mm");
        Tooltip = FileExists 
            ? $"{FilePath}\nLast Modified: {lastModifiedText}\nType: {FileType.ToUpper()}"
            : $"{FilePath}\nLast Modified: {lastModifiedText}\nType: {FileType.ToUpper()}\n?? File not found";
    }

    [RelayCommand]
    private void SelectDocument()
    {
        if (FileExists)
        {
            DocumentSelected?.Invoke(FilePath);
        }
    }

    [RelayCommand]
    private void OpenFileLocation()
    {
        if (File.Exists(FilePath))
        {
            try
            {
                var directoryPath = Path.GetDirectoryName(FilePath);
                if (!string.IsNullOrEmpty(directoryPath))
                {
                    System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{FilePath}\"");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error opening file location: {ex.Message}");
            }
        }
    }
}