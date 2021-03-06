﻿using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace zombiesnu.DayZeroLauncher.App.Ui.Converters
{
	public class PingToForegroundConverter : IValueConverter
	{
		public static SolidColorBrush Fastest = new SolidColorBrush(Color.FromArgb(255, 25, 253, 25));
		public static SolidColorBrush Fast = new SolidColorBrush(Color.FromArgb(255, 120, 239, 120));
		public static SolidColorBrush Medium = new SolidColorBrush(Colors.Yellow);
		public static SolidColorBrush Slow = new SolidColorBrush(Colors.Red);
		public static SolidColorBrush Nothing = new SolidColorBrush(Color.FromArgb(255, 204, 204, 204));

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null)
				return Nothing;

			var val = (long) value;
			if (val > 0 && val < 100)
				return Fastest;
			if (val >= 100 && val < 160)
				return Fast;
			if (val >= 160 && val < 275)
				return Medium;

			return Slow;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}