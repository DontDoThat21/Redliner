using Microsoft.EntityFrameworkCore;
using Redliner.Data;
using Redliner.Models;

namespace Redliner.Services;

/// <summary>
/// Service for initializing test data in the database
/// </summary>
public class DatabaseSeeder
{
    private readonly RedlinerDbContext _context;

    public DatabaseSeeder(RedlinerDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Seeds the database with test documents and annotations
    /// </summary>
    public async Task SeedTestDataAsync()
    {
        // Check if test data already exists
        if (await _context.Documents.AnyAsync())
        {
            return; // Data already exists
        }

        // Create test documents
        var testDocuments = new List<Document>
        {
            new Document
            {
                FilePath = "C:\\Users\\Test\\Documents\\dummy.pdf",
                FileName = "dummy.pdf",
                FileType = "PDF",
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                LastModified = DateTime.UtcNow.AddDays(-2)
            },
            new Document
            {
                FilePath = "C:\\Users\\Test\\Documents\\sample-local-pdf.pdf",
                FileName = "sample-local-pdf.pdf",
                FileType = "PDF",
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                LastModified = DateTime.UtcNow.AddDays(-1)
            }
        };

        _context.Documents.AddRange(testDocuments);
        await _context.SaveChangesAsync();

        // Create test annotations for the first document
        var testAnnotations = new List<Annotation>
        {
            new Annotation
            {
                DocumentId = testDocuments[0].Id,
                Type = "Rectangle",
                X = 100,
                Y = 150,
                Width = 200,
                Height = 100,
                Color = "#FF0000",
                StrokeThickness = 2.0,
                Layer = "Default",
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new Annotation
            {
                DocumentId = testDocuments[0].Id,
                Type = "Circle",
                X = 350,
                Y = 200,
                Width = 80,
                Height = 80,
                Color = "#00FF00",
                StrokeThickness = 3.0,
                Layer = "Markup",
                CreatedAt = DateTime.UtcNow.AddHours(-12)
            },
            new Annotation
            {
                DocumentId = testDocuments[0].Id,
                Type = "Text",
                X = 150,
                Y = 300,
                Width = 0,
                Height = 0,
                Text = "Sample annotation text",
                Color = "#0000FF",
                StrokeThickness = 14.0,
                Layer = "Notes",
                CreatedAt = DateTime.UtcNow.AddHours(-6)
            },
            new Annotation
            {
                DocumentId = testDocuments[0].Id,
                Type = "Arrow",
                X = 250,
                Y = 400,
                Width = 100,
                Height = 50,
                Color = "#FF8000",
                StrokeThickness = 3.0,
                Layer = "Redlines",
                CreatedAt = DateTime.UtcNow.AddHours(-3)
            },
            new Annotation
            {
                DocumentId = testDocuments[0].Id,
                Type = "Highlight",
                X = 120,
                Y = 500,
                Width = 180,
                Height = 25,
                Color = "#FFFF00",
                StrokeThickness = 1.0,
                Layer = "Default",
                CreatedAt = DateTime.UtcNow.AddHours(-1)
            }
        };

        _context.Annotations.AddRange(testAnnotations);
        await _context.SaveChangesAsync();

        System.Diagnostics.Debug.WriteLine($"Seeded database with {testDocuments.Count} documents and {testAnnotations.Count} annotations");
    }
}