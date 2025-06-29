using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Redliner.Services;

/// <summary>
/// Interface for document viewing operations
/// </summary>
public interface IDocumentViewerService
{
    Task<UIElement?> LoadDocumentAsync(string filePath);
    bool CanViewFileType(string filePath);
    Task<BitmapSource?> RenderPageAsync(string filePath, int pageNumber = 1, double dpi = 96);
}

/// <summary>
/// Service for viewing different document types
/// </summary>
public class DocumentViewerService : IDocumentViewerService
{
    private readonly string[] _supportedExtensions = { ".pdf", ".dxf", ".dwg", ".dwf" };

    public bool CanViewFileType(string filePath)
    {
        if (!File.Exists(filePath))
            return false;

        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return _supportedExtensions.Contains(extension);
    }

    public async Task<UIElement?> LoadDocumentAsync(string filePath)
    {
        if (!CanViewFileType(filePath))
            return null;

        var extension = Path.GetExtension(filePath).ToLowerInvariant();

        return extension switch
        {
            ".pdf" => await LoadPdfDocumentAsync(filePath),
            ".dwg" or ".dxf" or ".dwf" => await LoadCadDocumentAsync(filePath),
            _ => null
        };
    }

    public async Task<BitmapSource?> RenderPageAsync(string filePath, int pageNumber = 1, double dpi = 96)
    {
        if (!CanViewFileType(filePath))
            return null;

        var extension = Path.GetExtension(filePath).ToLowerInvariant();

        return extension switch
        {
            ".pdf" => await RenderPdfPageAsync(filePath, pageNumber, dpi),
            ".dwg" or ".dxf" or ".dwf" => await RenderCadPageAsync(filePath, dpi),
            _ => null
        };
    }

    private async Task<UIElement?> LoadPdfDocumentAsync(string filePath)
    {
        try
        {
            // Render the PDF page on a background thread
            var bitmap = await RenderPdfPageAsync(filePath, 1, 150);
            
            if (bitmap == null)
                return null;

            // Create UI components on the UI thread
            return await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var stackPanel = new StackPanel { Orientation = Orientation.Vertical };
                
                var image = new Image
                {
                    Source = bitmap,
                    Stretch = Stretch.Uniform,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Top
                };
                
                stackPanel.Children.Add(image);
                return (UIElement)stackPanel;
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading PDF: {ex.Message}");
            return CreateErrorDisplay($"Error loading PDF: {ex.Message}");
        }
    }

    private async Task<BitmapSource?> RenderPdfPageAsync(string filePath, int pageNumber, double dpi)
    {
        try
        {
            // Create the bitmap data on a background thread
            var bitmapData = await Task.Run(() =>
            {
                var width = (int)(8.5 * dpi); // Standard letter width
                var height = (int)(11 * dpi); // Standard letter height
                var stride = width * 3; // 3 bytes per pixel for BGR24
                var pixels = new byte[height * stride];
                
                // Fill with white
                for (int i = 0; i < pixels.Length; i += 3)
                {
                    pixels[i] = 255;     // Blue
                    pixels[i + 1] = 255; // Green
                    pixels[i + 2] = 255; // Red
                }
                
                return new { Width = width, Height = height, Pixels = pixels, Stride = stride };
            });
            
            // Create and populate the bitmap on the UI thread
            return await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var bitmap = new WriteableBitmap(bitmapData.Width, bitmapData.Height, dpi, dpi, PixelFormats.Bgr24, null);
                bitmap.WritePixels(new Int32Rect(0, 0, bitmapData.Width, bitmapData.Height), bitmapData.Pixels, bitmapData.Stride, 0);
                bitmap.Freeze(); // Make it thread-safe for future use
                return (BitmapSource)bitmap;
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error rendering PDF page: {ex.Message}");
            return null;
        }
    }

    private async Task<UIElement?> LoadCadDocumentAsync(string filePath)
    {
        try
        {
            return await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                // CAD file viewing would require specialized libraries like Aspose.CAD or similar
                // For now, show a placeholder
                return CreatePlaceholderDisplay($"CAD Document: {Path.GetFileName(filePath)}", 
                    "CAD file viewing requires additional libraries.\nShowing placeholder for now.");
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading CAD document: {ex.Message}");
            return CreateErrorDisplay($"Error loading CAD document: {ex.Message}");
        }
    }

    private async Task<BitmapSource?> RenderCadPageAsync(string filePath, double dpi)
    {
        // Placeholder for CAD rendering
        return await Task.FromResult<BitmapSource?>(null);
    }

    private UIElement CreatePlaceholderDisplay(string title, string message)
    {
        var stackPanel = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(20)
        };

        var titleBlock = new TextBlock
        {
            Text = title,
            FontSize = 16,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 10)
        };

        var messageBlock = new TextBlock
        {
            Text = message,
            FontSize = 12,
            HorizontalAlignment = HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center,
            Foreground = Brushes.Gray
        };

        stackPanel.Children.Add(titleBlock);
        stackPanel.Children.Add(messageBlock);

        return stackPanel;
    }

    private UIElement CreateErrorDisplay(string errorMessage)
    {
        var stackPanel = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(20)
        };

        var errorBlock = new TextBlock
        {
            Text = "? Error Loading Document",
            FontSize = 16,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            Foreground = Brushes.Red,
            Margin = new Thickness(0, 0, 0, 10)
        };

        var messageBlock = new TextBlock
        {
            Text = errorMessage,
            FontSize = 12,
            HorizontalAlignment = HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center,
            Foreground = Brushes.Gray
        };

        stackPanel.Children.Add(errorBlock);
        stackPanel.Children.Add(messageBlock);

        return stackPanel;
    }
}