using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Reversi.Models;

namespace Reversi.Converters;

public class PieceColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            Piece.Black => Brushes.Black,
            Piece.White => Brushes.White,
            _ => Brushes.Transparent
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
