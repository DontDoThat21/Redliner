using System.ComponentModel.DataAnnotations;

namespace Redliner.Models;

/// <summary>
/// Represents an annotation on a document
/// </summary>
public class Annotation
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int DocumentId { get; set; }

    [Required]
    public string Type { get; set; } = string.Empty; // Text, Rectangle, Circle, Arrow, Highlight, etc.

    public double X { get; set; }
    
    public double Y { get; set; }
    
    public double Width { get; set; }
    
    public double Height { get; set; }

    public string? Text { get; set; }

    public string Color { get; set; } = "#FF0000"; // Default red

    public double StrokeThickness { get; set; } = 2.0;

    public string? Layer { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime LastModified { get; set; } = DateTime.UtcNow;

    // Navigation property
    public virtual Document Document { get; set; } = null!;
}