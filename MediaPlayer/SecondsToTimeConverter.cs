using System;
using System.Globalization;
using System.Windows.Data;

namespace MediaPlayer
{
    public class SecondsToTimeConverter : IValueConverter
    {
        public string Format { get; set; }

        public SecondsToTimeConverter()
        {
            Format = @"mm\:ss";
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var timeSpan = TimeSpan.Zero;
            if (value is TimeSpan) timeSpan = (TimeSpan)value;
            else if (value is double) timeSpan = TimeSpan.FromSeconds((double)value);

            // no format: auto
            if (string.IsNullOrWhiteSpace(Format)) return ConvertSmart(timeSpan);
            
            return timeSpan.ToString(Format, CultureInfo.CurrentCulture);
        }

        private static object ConvertSmart(TimeSpan timeSpan)
        {
            if (timeSpan.TotalMinutes < 1) return timeSpan.ToString("ss", CultureInfo.CurrentCulture);

            if (timeSpan.TotalMinutes < 10) return timeSpan.ToString(@"m\:ss", CultureInfo.CurrentCulture);

            if (timeSpan.TotalHours < 1) return timeSpan.ToString(@"mm\:ss", CultureInfo.CurrentCulture);

            return timeSpan.ToString(@"hh\:mm\:ss", CultureInfo.CurrentCulture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}