namespace Redliner.Models;

/// <summary>
/// Enum defining the types of annotations supported
/// </summary>
public enum AnnotationType
{
    Text,
    Rectangle,
    Circle,
    Arrow,
    Highlight,
    Freehand,
    Measurement
}

/// <summary>
/// Enum defining the supported file types
/// </summary>
public enum FileType
{
    PDF,
    DXF,
    DWG
}

/// <summary>
/// Enum defining annotation layers
/// </summary>
public enum AnnotationLayer
{
    Default,
    Redlines,
    Notes,
    Markup,
    Measurements
}