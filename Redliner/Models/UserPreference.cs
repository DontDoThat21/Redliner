using System.ComponentModel.DataAnnotations;

namespace Redliner.Models;

/// <summary>
/// Represents application settings and user preferences
/// </summary>
public class UserPreference
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Key { get; set; } = string.Empty;

    public string? Value { get; set; }

    public DateTime LastModified { get; set; } = DateTime.UtcNow;
}