using Microsoft.EntityFrameworkCore;
using Redliner.Models;

namespace Redliner.Services;

/// <summary>
/// Interface for annotation service operations
/// </summary>
public interface IAnnotationService
{
    Task<Annotation> CreateAnnotationAsync(Annotation annotation);
    Task<bool> UpdateAnnotationAsync(Annotation annotation);
    Task<bool> DeleteAnnotationAsync(int annotationId);
    Task<IEnumerable<Annotation>> GetDocumentAnnotationsAsync(int documentId);
    Task<IEnumerable<Annotation>> GetAnnotationsByLayerAsync(int documentId, string layer);
}

/// <summary>
/// Implementation of annotation service
/// </summary>
public class AnnotationService : IAnnotationService
{
    private readonly Data.RedlinerDbContext _context;

    public AnnotationService(Data.RedlinerDbContext context)
    {
        _context = context;
    }

    public async Task<Annotation> CreateAnnotationAsync(Annotation annotation)
    {
        annotation.CreatedAt = DateTime.UtcNow;
        annotation.LastModified = DateTime.UtcNow;

        _context.Annotations.Add(annotation);
        await _context.SaveChangesAsync();

        return annotation;
    }

    public async Task<bool> UpdateAnnotationAsync(Annotation annotation)
    {
        try
        {
            annotation.LastModified = DateTime.UtcNow;
            _context.Annotations.Update(annotation);
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeleteAnnotationAsync(int annotationId)
    {
        try
        {
            var annotation = await _context.Annotations.FindAsync(annotationId);
            if (annotation == null) return false;

            _context.Annotations.Remove(annotation);
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<IEnumerable<Annotation>> GetDocumentAnnotationsAsync(int documentId)
    {
        return await _context.Annotations
            .Where(a => a.DocumentId == documentId)
            .OrderBy(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Annotation>> GetAnnotationsByLayerAsync(int documentId, string layer)
    {
        return await _context.Annotations
            .Where(a => a.DocumentId == documentId && a.Layer == layer)
            .OrderBy(a => a.CreatedAt)
            .ToListAsync();
    }
}