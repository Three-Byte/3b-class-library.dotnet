using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Data.Linq;
using System.Windows.Media.Imaging;
using System.IO;

//Soundweb specific converter for producing meaningful level values from data
namespace ThreeByte.Converters
{
    public class LevelToByteConverter : IValueConverter
    {
        private static readonly byte InputMin = 0;
        private static readonly byte InputMax = 255;
        private static readonly int OutputMin = -1000;
        private static readonly int OutputMax = 0;

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            int levelIn = (int)value;

            int levelOut = (int)((double)(levelIn - InputMin) / (double)(InputMax - InputMin) * (OutputMax - OutputMin)) + OutputMin;

            //clamp
            levelOut = Math.Max(OutputMin, Math.Min(OutputMax, levelOut));  
        
            //Convert to bytes
            return BitConverter.GetBytes(levelOut);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            byte[] bytesOut = (byte[])value;
            if (BitConverter.IsLittleEndian)
            {
                byte[] tempBytes = new byte[4];
                for (int i = 0; i < 4; i++)
                {
                    tempBytes[i] = bytesOut[3 - i];
                }
                bytesOut = tempBytes;
            }
            int levelOut = BitConverter.ToInt32(bytesOut, 0);

            byte levelIn = (byte)(((double)(levelOut - OutputMin) / (double)(OutputMax - OutputMin) * (InputMax - InputMin)) + InputMin);

            //clamp
            levelIn = Math.Max(InputMin, Math.Min(InputMax, levelIn));

            //Convert to bytes
            return levelIn;
        }
    }
}
