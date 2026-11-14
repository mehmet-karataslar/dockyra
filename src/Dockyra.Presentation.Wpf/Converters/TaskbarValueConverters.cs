using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Dockyra.Presentation.Wpf.Converters;

public sealed class Bgra32ToImageSourceConverter : IValueConverter
{
    private const int Dimension = 32;
    private const int ExpectedByteLength = Dimension * Dimension * 4;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not byte[] bytes || bytes.Length != ExpectedByteLength)
        {
            return null;
        }

        try
        {
            BitmapSource bitmap = BitmapSource.Create(
                Dimension,
                Dimension,
                96,
                96,
                PixelFormats.Bgra32,
                null,
                bytes,
                Dimension * 4);
            bitmap.Freeze();
            return bitmap;
        }
        catch
        {
            return null;
        }
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        Binding.DoNothing;
}

public sealed class BytesPresenceToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool hasIcon = value is byte[] { Length: 32 * 32 * 4 };
        bool invert = string.Equals(parameter as string, "Invert", StringComparison.OrdinalIgnoreCase);
        bool visible = invert ? !hasIcon : hasIcon;
        return visible ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        Binding.DoNothing;
}

public sealed class HexToBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        string hex = value as string ?? "#3B82F6";
        try
        {
            if (ColorConverter.ConvertFromString(hex) is Color color)
            {
                if (parameter is string alphaStr && byte.TryParse(alphaStr, out byte alpha))
                {
                    color = Color.FromArgb(alpha, color.R, color.G, color.B);
                }

                var brush = new SolidColorBrush(color);
                brush.Freeze();
                return brush;
            }
        }
        catch
        {
            // Fallback
        }

        return new SolidColorBrush(Color.FromRgb(59, 130, 246));
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        Binding.DoNothing;
}

