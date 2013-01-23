using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Flurrysticks
{
    class Util
    {

        public static T FindFirstElementInVisualTree<T>(DependencyObject parentElement) where T : DependencyObject
        {
            var count = VisualTreeHelper.GetChildrenCount(parentElement);
            if (count == 0)
                return null;

            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parentElement, i);

                if (child != null && child is T)
                {
                    return (T)child;
                }
                else
                {
                    var result = FindFirstElementInVisualTree<T>(child);
                    if (result != null)
                        return result;

                }
            }
            return null;
        }

        public static int getLabelIntervalByCount(int c)
        {
            int result = c / 5;
            if (result == 0) { result = 1; }
            return result;
        }

        public static int getLabelInterval(DateTime startD, DateTime endD)
        {
            TimeSpan t = endD - startD;
            int result = (int)t.TotalDays / 5;
            if (result == 0) { result = 1; }
            return result;
        }

        public static string shrinkString(String what)
        {
            String result = what;
            if (what.Length > 14)
            {
                result = what.Substring(0, 14).Trim() + "...";
            }
            return result;
        }

    }
}
