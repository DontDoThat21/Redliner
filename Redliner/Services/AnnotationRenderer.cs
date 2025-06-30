using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Redliner.Models;

namespace Redliner.Services;

/// <summary>
/// Service for rendering annotations on a canvas
/// </summary>
public class AnnotationRenderer
{
    /// <summary>
    /// Renders a collection of annotations on the specified canvas
    /// </summary>
    /// <param name="canvas">The canvas to render annotations on</param>
    /// <param name="annotations">The annotations to render</param>
    public void RenderAnnotations(Canvas canvas, IEnumerable<Annotation> annotations)
    {
        // Clear existing annotations
        canvas.Children.Clear();

        foreach (var annotation in annotations)
        {
            var element = CreateAnnotationElement(annotation);
            if (element != null)
            {
                canvas.Children.Add(element);
            }
        }
    }

    /// <summary>
    /// Creates a UI element for the specified annotation
    /// </summary>
    /// <param name="annotation">The annotation to create an element for</param>
    /// <returns>The created UI element, or null if the annotation type is not supported</returns>
    private UIElement? CreateAnnotationElement(Annotation annotation)
    {
        var color = ParseColor(annotation.Color);
        var brush = new SolidColorBrush(color);
        var strokeBrush = new SolidColorBrush(color);
        var fillBrush = new SolidColorBrush(Color.FromArgb(30, color.R, color.G, color.B));

        return annotation.Type.ToLowerInvariant() switch
        {
            "rectangle" => CreateRectangle(annotation, strokeBrush, fillBrush),
            "circle" => CreateCircle(annotation, strokeBrush, fillBrush),
            "text" => CreateText(annotation, brush),
            "arrow" => CreateArrow(annotation, strokeBrush),
            "highlight" => CreateHighlight(annotation, fillBrush),
            _ => null
        };
    }

    private Rectangle CreateRectangle(Annotation annotation, Brush stroke, Brush fill)
    {
        var rect = new Rectangle
        {
            Width = annotation.Width,
            Height = annotation.Height,
            Stroke = stroke,
            Fill = fill,
            StrokeThickness = annotation.StrokeThickness
        };

        Canvas.SetLeft(rect, annotation.X);
        Canvas.SetTop(rect, annotation.Y);

        return rect;
    }

    private Ellipse CreateCircle(Annotation annotation, Brush stroke, Brush fill)
    {
        var ellipse = new Ellipse
        {
            Width = annotation.Width,
            Height = annotation.Height,
            Stroke = stroke,
            Fill = fill,
            StrokeThickness = annotation.StrokeThickness
        };

        Canvas.SetLeft(ellipse, annotation.X);
        Canvas.SetTop(ellipse, annotation.Y);

        return ellipse;
    }

    private TextBlock CreateText(Annotation annotation, Brush foreground)
    {
        var textBlock = new TextBlock
        {
            Text = annotation.Text ?? string.Empty,
            Foreground = foreground,
            FontSize = Math.Max(annotation.StrokeThickness * 6, 12), // Use stroke thickness as relative font size
            Background = new SolidColorBrush(Colors.White) { Opacity = 0.8 }
        };

        Canvas.SetLeft(textBlock, annotation.X);
        Canvas.SetTop(textBlock, annotation.Y);

        return textBlock;
    }

    private Line CreateArrow(Annotation annotation, Brush stroke)
    {
        // Simple line for now - could be enhanced with arrowhead
        var line = new Line
        {
            X1 = annotation.X,
            Y1 = annotation.Y,
            X2 = annotation.X + annotation.Width,
            Y2 = annotation.Y + annotation.Height,
            Stroke = stroke,
            StrokeThickness = annotation.StrokeThickness
        };

        return line;
    }

    private Rectangle CreateHighlight(Annotation annotation, Brush fill)
    {
        var rect = new Rectangle
        {
            Width = annotation.Width,
            Height = annotation.Height,
            Fill = fill,
            Stroke = null,
            Opacity = 0.5
        };

        Canvas.SetLeft(rect, annotation.X);
        Canvas.SetTop(rect, annotation.Y);

        return rect;
    }

    private Color ParseColor(string colorString)
    {
        try
        {
            if (colorString.StartsWith("#"))
            {
                return (Color)ColorConverter.ConvertFromString(colorString);
            }
            return Colors.Red; // Default fallback
        }
        catch
        {
            return Colors.Red; // Default fallback
        }
    }
}