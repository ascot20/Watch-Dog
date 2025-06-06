using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace WatchDog.Converters;

public class StatusToColorConverter: IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
       
            string status = value?.ToString() ?? "";
            
            return status switch
            {
                "NotStarted" => new SolidColorBrush(Color.Parse("#666C75")),
                "InProgress" => new SolidColorBrush(Color.Parse("#5183DB")),
                "Completed" => new SolidColorBrush(Color.Parse("#438440")),
                "Closed" => new SolidColorBrush(Color.Parse("#8259DD")),
                "Question" => new SolidColorBrush(Color.Parse("#5183DB")),
                "Announcement" => new SolidColorBrush(Color.Parse("#8259DD")),
                "Milestone" => new SolidColorBrush(Color.Parse("#438440")),
                "Update" => new SolidColorBrush(Color.Parse("#438440")),
                _ => new SolidColorBrush(Color.Parse("#666C75"))

            };
    }
    
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}