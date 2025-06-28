using Microsoft.EntityFrameworkCore;
using Redliner.Models;
using System.IO;

namespace Redliner.Services;

/// <summary>
/// Interface for file handling operations
/// </summary>
public interface IFileService
{
    Task<byte[]?> ReadFileAsync(string filePath);
    bool IsValidFile(string filePath);
    string GetFileType(string filePath);
    Task<bool> ExportAnnotationsAsync(string filePath, int documentId);
}

/// <summary>
/// Implementation of file service for handling different file types
/// </summary>
public class FileService : IFileService
{
    private readonly Data.RedlinerDbContext _context;

    public FileService(Data.RedlinerDbContext context)
    {
        _context = context;
    }

    public async Task<byte[]?> ReadFileAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                return null;

            return await File.ReadAllBytesAsync(filePath);
        }
        catch
        {
            return null;
        }
    }

    public bool IsValidFile(string filePath)
    {
        if (!File.Exists(filePath))
            return false;

        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return extension is ".pdf" or ".dxf" or ".dwg" or ".dwf";
    }

    public string GetFileType(string filePath)
    {
        return Path.GetExtension(filePath).ToLowerInvariant();
    }

    public async Task<bool> ExportAnnotationsAsync(string filePath, int documentId)
    {
        try
        {
            var annotations = await _context.Annotations
                .Where(a => a.DocumentId == documentId)
                .ToListAsync();

            // TODO: Implement export logic based on file type
            // This would involve creating a new file with annotations embedded

            return true;
        }
        catch
        {
            return false;
        }
    }
}