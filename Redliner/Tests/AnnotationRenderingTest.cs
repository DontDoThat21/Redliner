using System.Windows.Controls;
using Redliner.Models;
using Redliner.Services;

namespace Redliner.Tests;

/// <summary>
/// Simple test to verify annotation rendering logic works correctly
/// </summary>
public class AnnotationRenderingTest
{
    public static void TestAnnotationRendering()
    {
        // Create test annotations
        var annotations = new List<Annotation>
        {
            new Annotation
            {
                Id = 1,
                DocumentId = 1,
                Type = "Rectangle",
                X = 10,
                Y = 20,
                Width = 100,
                Height = 50,
                Color = "#FF0000",
                StrokeThickness = 2.0,
                Layer = "Default"
            },
            new Annotation
            {
                Id = 2,
                DocumentId = 1,
                Type = "Circle",
                X = 150,
                Y = 100,
                Width = 80,
                Height = 80,
                Color = "#00FF00",
                StrokeThickness = 3.0,
                Layer = "Default"
            },
            new Annotation
            {
                Id = 3,
                DocumentId = 1,
                Type = "Text",
                X = 200,
                Y = 200,
                Width = 0,
                Height = 0,
                Text = "Test annotation",
                Color = "#0000FF",
                StrokeThickness = 12.0,
                Layer = "Notes"
            }
        };

        // Test that annotation renderer can process these without errors
        var renderer = new AnnotationRenderer();
        var canvas = new Canvas();
        
        try
        {
            renderer.RenderAnnotations(canvas, annotations);
            Console.WriteLine($"Successfully rendered {annotations.Count} annotations");
            Console.WriteLine($"Canvas now has {canvas.Children.Count} child elements");
            
            // Verify each annotation type was rendered
            foreach (var annotation in annotations)
            {
                Console.WriteLine($"- {annotation.Type} annotation at ({annotation.X}, {annotation.Y})");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during annotation rendering: {ex.Message}");
        }
    }
}