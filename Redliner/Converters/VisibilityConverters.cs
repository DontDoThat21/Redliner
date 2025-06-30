using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Redliner.Converters;

/// <summary>
/// Converter to show placeholder message only when not loading and no file loaded
/// </summary>
public class ShowPlaceholderConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length == 2 && values[0] is bool isLoading && values[1] is bool isFileLoaded)
        {
            // Show placeholder only when not loading AND no file is loaded
            return (!isLoading && !isFileLoaded) ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converter to show document content only when file is loaded and not loading
/// </summary>
public class ShowDocumentContentConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length == 2 && values[0] is bool isLoading && values[1] is bool isFileLoaded)
        {
            // Show content only when not loading AND file is loaded
            return (!isLoading && isFileLoaded) ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}