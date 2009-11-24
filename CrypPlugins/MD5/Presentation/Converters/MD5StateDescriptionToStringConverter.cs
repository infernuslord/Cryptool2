using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Cryptool.MD5.Algorithm;

namespace Cryptool.MD5.Presentation.Converters
{
    class MD5StateDescriptionToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            MD5StateDescription state = (MD5StateDescription)value;
            switch (state)
            {
                case MD5StateDescription.UNINITIALIZED:
                    return "Algorithm uninitialized";
                case MD5StateDescription.INITIALIZED:
                    return "Initialization";
                case MD5StateDescription.READING_DATA:
                    return "Reading data";
                case MD5StateDescription.READ_DATA:
                    return "Read data";
                case MD5StateDescription.STARTING_PADDING:
                    return "Beginning padding process";
                case MD5StateDescription.ADDING_PADDING_BYTES:
                    return "Adding the padding bytes";
                case MD5StateDescription.ADDED_PADDING_BYTES:
                    return "Added the padding bytes";
                case MD5StateDescription.ADDING_LENGTH:
                    return "Adding the data length";
                case MD5StateDescription.ADDED_LENGTH:
                    return "Added the data length";
                case MD5StateDescription.STARTING_COMPRESSION:
                    return "Starting the compression";
                case MD5StateDescription.STARTING_ROUND:
                    return "Starting a compression round";
                case MD5StateDescription.STARTING_ROUND_STEP:
                    return "Before compression step";
                case MD5StateDescription.FINISHED_ROUND_STEP:
                    return "After compression step";
                case MD5StateDescription.FINISHED_ROUND:
                    return "Finished compression round";
                case MD5StateDescription.FINISHING_COMPRESSION:
                    return "Finalizing compression";
                case MD5StateDescription.FINISHED_COMPRESSION:
                    return "Finished compression";
                case MD5StateDescription.FINISHED:
                    return "Finished";
                default:
                    return "Unknown state";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
