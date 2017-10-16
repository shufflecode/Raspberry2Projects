using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppUwaInterfacesDemoV1
{
    public class Scenario
    {
        public string Title { get; set; }

        public Windows.UI.Xaml.Controls.Page PageObj { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }

    //public class ScenarioBindingConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, string language)
    //    {
    //        Scenario s = value as Scenario;
    //        return (MainPage.Current.Scenarios.IndexOf(s) + 1) + ") " + s.Title;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, string language)
    //    {
    //        return true;
    //    }
    //}
}
