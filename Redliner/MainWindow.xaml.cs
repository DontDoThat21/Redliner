using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Redliner.ViewModels;
using Redliner.Data;
using Redliner.Services;
using Microsoft.EntityFrameworkCore;

namespace Redliner;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private double _zoomFactor = 1.0;
    private const double ZoomIncrement = 0.1;
    private const double MinZoom = 0.1;
    private const double MaxZoom = 5.0;
    private MainViewModel? _viewModel;
    
    // Annotation drawing state
    private bool _isDrawingAnnotation = false;
    private Point _annotationStartPoint;
    private UIElement? _currentAnnotationPreview;
    
    // Annotation rendering
    private readonly AnnotationRenderer _annotationRenderer = new();

    public MainWindow()
    {
        InitializeComponent();
        InitializeAsync();
        
        // Add mouse wheel event for zooming
        DocumentViewer.PreviewMouseWheel += DocumentViewer_PreviewMouseWheel;
        
        // Handle document container size changes
        DocumentContainer.SizeChanged += DocumentContainer_SizeChanged;
    }

    private async void InitializeAsync()
    {
        // Initialize database
        using var context = new RedlinerDbContext();
        await context.Database.EnsureCreatedAsync();
        
        // Seed test data
        var seeder = new DatabaseSeeder(context);
        await seeder.SeedTestDataAsync();

        // Set up ViewModel
        var viewModel = new MainViewModel();
        DataContext = viewModel;

        // Subscribe to zoom events
        viewModel.ZoomInRequested += () => ZoomIn();
        viewModel.ZoomOutRequested += () => ZoomOut();
        viewModel.FitToWindowRequested += () => FitToWindow();
        
        // Subscribe to annotation events
        viewModel.AnnotationsUpdated += OnAnnotationsUpdated;

        // Store ViewModel reference for cleanup
        _viewModel = viewModel;
    }

    private void DocumentViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (Keyboard.Modifiers == ModifierKeys.Control)
        {
            e.Handled = true;
            
            if (e.Delta > 0)
            {
                ZoomIn();
            }
            else
            {
                ZoomOut();
            }
        }
    }

    private void ZoomIn()
    {
        _zoomFactor = Math.Min(_zoomFactor + ZoomIncrement, MaxZoom);
        ApplyZoom();
    }

    private void ZoomOut()
    {
        _zoomFactor = Math.Max(_zoomFactor - ZoomIncrement, MinZoom);
        ApplyZoom();
    }

    private void ApplyZoom()
    {
        var transform = new ScaleTransform(_zoomFactor, _zoomFactor);
        DocumentViewbox.LayoutTransform = transform;
    }

    public void FitToWindow()
    {
        DocumentViewbox.Stretch = Stretch.Uniform;
        DocumentViewbox.LayoutTransform = null;
        _zoomFactor = 1.0;
    }

    private void AnnotationCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var canvas = sender as Canvas;
        if (canvas == null) return;

        _isDrawingAnnotation = true;
        _annotationStartPoint = e.GetPosition(canvas);
        canvas.CaptureMouse();

        // Create a preview rectangle for demonstration
        var previewRect = new Rectangle
        {
            Stroke = Brushes.Red,
            StrokeThickness = 2,
            Fill = new SolidColorBrush(Color.FromArgb(50, 255, 0, 0)),
            Width = 0,
            Height = 0
        };

        Canvas.SetLeft(previewRect, _annotationStartPoint.X);
        Canvas.SetTop(previewRect, _annotationStartPoint.Y);
        
        canvas.Children.Add(previewRect);
        _currentAnnotationPreview = previewRect;
    }

    private void AnnotationCanvas_MouseMove(object sender, MouseEventArgs e)
    {
        if (!_isDrawingAnnotation || _currentAnnotationPreview == null) return;

        var canvas = sender as Canvas;
        if (canvas == null) return;

        var currentPoint = e.GetPosition(canvas);
        var rect = _currentAnnotationPreview as Rectangle;
        if (rect == null) return;

        // Update preview rectangle dimensions
        var width = Math.Abs(currentPoint.X - _annotationStartPoint.X);
        var height = Math.Abs(currentPoint.Y - _annotationStartPoint.Y);
        
        rect.Width = width;
        rect.Height = height;
        
        // Adjust position if dragging up or left
        Canvas.SetLeft(rect, Math.Min(_annotationStartPoint.X, currentPoint.X));
        Canvas.SetTop(rect, Math.Min(_annotationStartPoint.Y, currentPoint.Y));
    }

    private void AnnotationCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        var canvas = sender as Canvas;
        if (canvas == null || !_isDrawingAnnotation) return;

        _isDrawingAnnotation = false;
        canvas.ReleaseMouseCapture();

        // Convert preview to permanent annotation
        if (_currentAnnotationPreview is Rectangle rect && rect.Width > 5 && rect.Height > 5)
        {
            // Keep the annotation visible
            rect.Stroke = Brushes.Red;
            rect.StrokeThickness = 2;
            rect.Fill = new SolidColorBrush(Color.FromArgb(30, 255, 0, 0));
            
            // Save annotation to database
            if (_viewModel != null && _viewModel.CurrentDocumentId > 0)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _viewModel.SaveAnnotationAsync(new Models.Annotation
                        {
                            DocumentId = _viewModel.CurrentDocumentId,
                            Type = "Rectangle",
                            X = Canvas.GetLeft(rect),
                            Y = Canvas.GetTop(rect),
                            Width = rect.Width,
                            Height = rect.Height,
                            Color = "#FF0000",
                            StrokeThickness = 2.0,
                            Layer = "Default"
                        });
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error saving annotation: {ex.Message}");
                    }
                });
            }
        }
        else
        {
            // Remove tiny or invalid annotations
            if (_currentAnnotationPreview != null)
                canvas.Children.Remove(_currentAnnotationPreview);
        }

        _currentAnnotationPreview = null;
    }

    private void OnAnnotationsUpdated(IEnumerable<Models.Annotation> annotations)
    {
        // Render annotations on the canvas
        Dispatcher.Invoke(() =>
        {
            System.Diagnostics.Debug.WriteLine($"Rendering {annotations.Count()} annotations on canvas");
            
            // Ensure canvas size matches document content
            if (DocumentPresenter.ActualWidth > 0 && DocumentPresenter.ActualHeight > 0)
            {
                AnnotationCanvas.Width = DocumentPresenter.ActualWidth;
                AnnotationCanvas.Height = DocumentPresenter.ActualHeight;
                System.Diagnostics.Debug.WriteLine($"Canvas size set to {AnnotationCanvas.Width}x{AnnotationCanvas.Height}");
            }
            else
            {
                // Set default size or wait for layout update
                AnnotationCanvas.Width = 800;
                AnnotationCanvas.Height = 600;
                System.Diagnostics.Debug.WriteLine("Canvas size set to default 800x600");
            }
            
            _annotationRenderer.RenderAnnotations(AnnotationCanvas, annotations);
            System.Diagnostics.Debug.WriteLine($"Canvas now has {AnnotationCanvas.Children.Count} child elements");
        });
    }

    private void DocumentContainer_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        // Update canvas size to match document container
        AnnotationCanvas.Width = e.NewSize.Width;
        AnnotationCanvas.Height = e.NewSize.Height;
    }
}