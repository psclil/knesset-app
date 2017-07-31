using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;

namespace knesset_app
{
    // a class to convert list<string>, list<presence> or list<invitation> to a command seperated string so we can display
    // it in a data template without hurting performance by using ItemsControl
    public class ListToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(string))
                throw new InvalidOperationException("The target must be a String");

            if (value == null) return string.Empty;

            if (value is ICollection<DBEntities.Presence>)
            {
                value = ((ICollection<DBEntities.Presence>)value).Select(x => x.pn_name).ToList();
            }
            else if (value is ICollection<DBEntities.Invitation>)
            {
                value = ((ICollection<DBEntities.Invitation>)value).Select(x => x.pn_name).ToList();
            }

            return String.Join(", ", ((List<string>)value).ToArray());
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
