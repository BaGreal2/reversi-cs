using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Reversi.Converters;

public class ValidMoveBackgroundConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return (value is true) 
            ? new SolidColorBrush(Color.Parse("#7FB77E"))
            : new SolidColorBrush(Color.Parse("#2E5633"));
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
