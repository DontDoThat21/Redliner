using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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

    public MainWindow()
    {
        InitializeComponent();
        InitializeAsync();
        
        // Add mouse wheel event for zooming
        DocumentViewer.PreviewMouseWheel += DocumentViewer_PreviewMouseWheel;
    }

    private async void InitializeAsync()
    {
        // Initialize database
        using var context = new RedlinerDbContext();
        await context.Database.EnsureCreatedAsync();

        // Set up ViewModel
        var viewModel = new MainViewModel();
        DataContext = viewModel;

        // Subscribe to zoom events
        viewModel.ZoomInRequested += () => ZoomIn();
        viewModel.ZoomOutRequested += () => ZoomOut();
        viewModel.FitToWindowRequested += () => FitToWindow();

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
}