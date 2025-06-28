using System.Windows;
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
    public MainWindow()
    {
        InitializeComponent();
        InitializeAsync();
    }

    private async void InitializeAsync()
    {
        // Initialize database
        using var context = new RedlinerDbContext();
        await context.Database.EnsureCreatedAsync();

        // Set up ViewModel
        var viewModel = new MainViewModel();
        DataContext = viewModel;
    }
}