using System.ComponentModel.DataAnnotations;

namespace Redliner.Models;

/// <summary>
/// Represents a document that can be annotated
/// </summary>
public class Document
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string FilePath { get; set; } = string.Empty;

    [Required]
    public string FileName { get; set; } = string.Empty;

    public string FileType { get; set; } = string.Empty; // PDF, DXF, DWG

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime LastModified { get; set; } = DateTime.UtcNow;

    // Navigation property
    public virtual ICollection<Annotation> Annotations { get; set; } = new List<Annotation>();
}