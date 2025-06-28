using Microsoft.EntityFrameworkCore;
using Redliner.Models;

namespace Redliner.Services;

/// <summary>
/// Interface for document service operations
/// </summary>
public interface IDocumentService
{
    Task<Document?> OpenDocumentAsync(string filePath);
    Task<bool> SaveDocumentAsync(Document document);
    Task<IEnumerable<Document>> GetRecentDocumentsAsync(int count = 10);
    Task<bool> DeleteDocumentAsync(int documentId);
    bool IsValidFileType(string filePath);
}

/// <summary>
/// Implementation of document service
/// </summary>
public class DocumentService : IDocumentService
{
    private readonly Data.RedlinerDbContext _context;
    private readonly string[] _supportedExtensions = { ".pdf", ".dxf", ".dwg" };

    public DocumentService(Data.RedlinerDbContext context)
    {
        _context = context;
    }

    public async Task<Document?> OpenDocumentAsync(string filePath)
    {
        if (!File.Exists(filePath) || !IsValidFileType(filePath))
            return null;

        var fileName = Path.GetFileName(filePath);
        var fileType = Path.GetExtension(filePath).ToLowerInvariant();

        // Check if document already exists
        var existingDoc = await _context.Documents
            .FirstOrDefaultAsync(d => d.FilePath == filePath);

        if (existingDoc != null)
        {
            existingDoc.LastModified = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return existingDoc;
        }

        // Create new document
        var document = new Document
        {
            FilePath = filePath,
            FileName = fileName,
            FileType = fileType,
            CreatedAt = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        };

        _context.Documents.Add(document);
        await _context.SaveChangesAsync();

        return document;
    }

    public async Task<bool> SaveDocumentAsync(Document document)
    {
        try
        {
            document.LastModified = DateTime.UtcNow;
            _context.Documents.Update(document);
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<IEnumerable<Document>> GetRecentDocumentsAsync(int count = 10)
    {
        return await _context.Documents
            .OrderByDescending(d => d.LastModified)
            .Take(count)
            .ToListAsync();
    }

    public async Task<bool> DeleteDocumentAsync(int documentId)
    {
        try
        {
            var document = await _context.Documents.FindAsync(documentId);
            if (document == null) return false;

            _context.Documents.Remove(document);
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool IsValidFileType(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return _supportedExtensions.Contains(extension);
    }
}