﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Data.Linq;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows;
using System.IO;


namespace ThreeByte.Converters
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public bool Opposite { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            bool val = System.Convert.ToBoolean(value);
            if(Opposite) {
                return (val ? Visibility.Hidden : Visibility.Visible);
            } else {
                return (val ? Visibility.Visible : Visibility.Hidden);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            Visibility? vis = value as Visibility?;
            if(Opposite) {
                return ((vis ?? Visibility.Visible) != Visibility.Visible);
            } else {
                return ((vis ?? Visibility.Visible) == Visibility.Visible);
            }
        }
    }
}
