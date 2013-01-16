using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Diagnostics;
using System.Xml.Linq;
using Telerik.UI.Xaml.Controls.Chart;
using Callisto.Controls;
using Flurrystics.Data;
using Flurrysticks.DataModel;
using Flurrysticks;
using Telerik.UI.Xaml.Controls.Primitives;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace Flurrystics
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class AppMetrics : Flurrystics.Common.LayoutAwarePage
    {

        string apiKey;
        string appapikey = ""; // initial apikey of the app
        string[] AppMetricsNames = { "ActiveUsers", "ActiveUsersByWeek", "ActiveUsersByMonth", "NewUsers", "MedianSessionLength", "AvgSessionLength", "Sessions", "RetainedUsers" };
        string[] AppMetricsNamesFormatted = { "Active Users", "Active Users By Week", "Active Users By Month", "New Users", "Median Session Length", "Avg Session Length", "Sessions", "Retained Users" };
        string EndDate;
        string StartDate;
        int actualMetricsIndex = 0;
        DownloadHelper dh = new DownloadHelper();
        Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
        TimeRange myTimeRange = new TimeRange();

        public AppMetrics()
        {
            this.InitializeComponent();
        }

        private void ParseXML(
                                XDocument what, 
                                int i, 
                                RadCartesianChart targetChart,
                                RadCustomHubTile rt1, RadCustomHubTile rt2, RadCustomHubTile rt3,
                                TextBlock t1, TextBlock t2, TextBlock t3, // stats numbers
                                TextBlock tb, // total info
                                String sDate, String eDate,
                                TextBlock tr1, // total info number
                                TextBlock tr2, // total info date/time range
                                int targetSeries // if it is basic or compare function
            ) {
            /*
             * // old wp7 code
            Telerik.Windows.Controls.RadCartesianChart targetChart, Microsoft.Phone.Controls.PerformanceProgressBar progressBar,
                                        RadCustomHubTile rt1, RadCustomHubTile rt2, RadCustomHubTile rt3,
                                        TextBlock t1, TextBlock t2, TextBlock t3, TextBlock tb, 
                                        String sDate, String eDate, TextBlock tr1, TextBlock tr2,
                                        int targetSeries
            */
            Debug.WriteLine("Processing..." + what);

            DataSource.getChartData()[i] = from query in what.Descendants("day")
                               select new ChartDataPoint
                               {
                                   Value = (double)query.Attribute("value"),
                                   Label = (string)query.Attribute("date")
                               };
            Debug.WriteLine("Setting DataContext of loaded data");
            /*
            var mydate = new Windows.Globalization.DateTimeFormatting.DateTimeFormatter("month day");
            var mydatepattern = mydate.Patterns[0];
            sDate = String.Format(mydatepattern, DateTime.Parse(sDate));
            eDate = String.Format(mydatepattern, DateTime.Parse(eDate));
            */
            if (targetSeries > 0)
            {
                // progressBar.Visibility = System.Windows.Visibility.Visible;
                rt1.IsFrozen = false;
                rt2.IsFrozen = false;
                rt3.IsFrozen = false;
                tr2.Visibility = Visibility.Visible;
                tr2.Text = "(" +  sDate + " - " + eDate + ")";
            }
            else  // reset compare chart
            {
                TextBlock[] totals = { info4Text1, info4Text2, info4Text3, info4Text4, info4Text5, info4Text6, info4Text7, info4Text8 };
                if (i < 8)
                {
                    totals[i].Visibility = Visibility.Collapsed;
                }
                // targetChart.Series[1].ItemsSource = null;
                tr1.Visibility = Visibility.Visible;
                tr2.Visibility = Visibility.Collapsed;
                tr1.Text = "(" + sDate + " - " + eDate + ")";
                //VisualStateManager.GoToState(rt1, "NotFlipped", true);
                //VisualStateManager.GoToState(rt2, "NotFlipped", true);
                //VisualStateManager.GoToState(rt3, "NotFlipped", true);
                rt1.IsFlipped = false;
                rt2.IsFlipped = false;
                rt3.IsFlipped = false;
                rt1.IsFrozen = true;
                rt2.IsFrozen = true;
                rt3.IsFrozen = true;
            }

            targetChart.DataContext = DataSource.getChartData()[i];
            targetChart.HorizontalAxis.LabelInterval = Util.getLabelIntervalByCount(DataSource.getChartData()[i].Count());

            // count max,min,latest,total for display purposes
            double latest = 0, minim = 9999999999999, maxim = 0, totalCount = 0;
            IEnumerator<ChartDataPoint> Myenum = DataSource.getChartData()[i].GetEnumerator();
            while (Myenum.MoveNext())
            {
                ChartDataPoint oneValue = Myenum.Current;
                latest = oneValue.Value;
                minim = Math.Min(minim, oneValue.Value);
                maxim = Math.Max(maxim, oneValue.Value);
                totalCount = totalCount + oneValue.Value;
            }

            t1.Text = latest.ToString();
            t2.Text = minim.ToString();
            t3.Text = maxim.ToString();
            switch (AppMetricsNames[i])
            {
                case "MedianSessionLength":
                case "AvgSessionLength":
                    tb.Text = "N/A"; // makes no sense for these metrics
                    break;
                default:
                    tb.Text = totalCount.ToString();
                    break;
            }

            tb.Visibility = Visibility.Visible; 


        } // ParseXML

        private async void loadData(int metricsIndex)
        {
            Debug.WriteLine("loadData() " + metricsIndex);
            RadCartesianChart[] targetCharts = { radChart1, radChart2, radChart3, radChart4, radChart5, radChart6, radChart7, radChart8 };
            RadCustomHubTile[] t1s = { tile1Text1, tile1Text2, tile1Text3, tile1Text4, tile1Text5, tile1Text6, tile1Text7, tile1Text8 };
            RadCustomHubTile[] t2s = { tile2Text1, tile2Text2, tile2Text3, tile2Text4, tile2Text5, tile2Text6, tile2Text7, tile2Text8 };
            RadCustomHubTile[] t3s = { tile3Text1, tile3Text2, tile3Text3, tile3Text4, tile3Text5, tile3Text6, tile3Text7, tile3Text8 };
            TextBlock[] c1s = { tile1Number1Text1, tile1Number1Text2, tile1Number1Text3, tile1Number1Text4, tile1Number1Text5, tile1Number1Text6, tile1Number1Text7, tile1Number1Text8 };
            TextBlock[] c2s = { tile2Number1Text1, tile2Number1Text2, tile2Number1Text3, tile2Number1Text4, tile2Number1Text5, tile2Number1Text6, tile2Number1Text7, tile2Number1Text8 };
            TextBlock[] c3s = { tile3Number1Text1, tile3Number1Text2, tile3Number1Text3, tile3Number1Text4, tile3Number1Text5, tile3Number1Text6, tile3Number1Text7, tile3Number1Text8 };
            TextBlock[] totals = { info2Text1, info2Text2, info2Text3, info2Text4, info2Text5, info2Text6, info2Text7, info2Text8 };
            TextBlock[] d1s = { info3Text1, info3Text2, info3Text3, info3Text4, info3Text5, info3Text6, info3Text7, info3Text8 };
            TextBlock[] d2s = { info5Text1, info5Text2, info5Text3, info5Text4, info5Text5, info5Text6, info5Text7, info5Text8 };

            string metrics = AppMetricsNames[metricsIndex]; // this will be selectable
            if (ProgressBar1 != null)
            {
                ProgressBar1.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
            // check if it's loaded, if not - load it up

            if (DataSource.getChartData()[metricsIndex] == null) // if no data present
            {

                bool success;
                XDocument result = null;
                string callURL = "http://api.flurry.com/appMetrics/" + metrics + "?apiAccessCode=" + apiKey + "&apiKey=" + appapikey + "&startDate=" + StartDate + "&endDate=" + EndDate;
                Debug.WriteLine(callURL);
                try
                {
                    result = await dh.DownloadXML(callURL); // load it   
                    success = true;
                }
                catch (System.Net.Http.HttpRequestException)
                {   // load failed
                    success = false;
                }
                Debug.WriteLine("Success:" + success);
                if (success) { 
                                int c = metricsIndex;
                                ParseXML(result,c,targetCharts[c],t1s[c],t2s[c],t3s[c],c1s[c],c2s[c],c3s[c],totals[c],StartDate,EndDate,d1s[c],d2s[c],0);
                                /*
                                RadCustomHubTile rt1, RadCustomHubTile rt2, RadCustomHubTile rt3,
                                TextBlock t1, TextBlock t2, TextBlock t3, // stats numbers
                                TextBlock tb, // total info
                                String sDate, String eDate,
                                TextBlock tr1, // total info number
                                TextBlock tr2, // total info date/time range
                                int targetSeries // if it is basic or compare function
                                 * */
                             }

            }
            else
            {
                Debug.WriteLine("Setting DataContext of already loaded data:" + DataSource.getChartData()[metricsIndex].Count());
                targetCharts[metricsIndex].DataContext = DataSource.getChartData()[metricsIndex];
            }

            //if (App.taskCount == 0)
            //{
                ProgressBar1.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            //}

        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            Debug.WriteLine("AppMetrics - LoadState");
            CallApp what = (CallApp)navigationParameter;
            pageTitle.Text = what.Name;
            platformTitle.Text = what.Platform;
            apiKey = what.ApiKey;
            appapikey = what.AppApiKey;

            try
            {
                StartDate = (string)localSettings.Values["StartDate"];
                EndDate = (string)localSettings.Values["EndDate"];
            }
            catch (System.NullReferenceException)
            {
                EndDate = String.Format("{0:yyyy-MM-dd}", DateTime.Now.AddDays(-1));
                StartDate = String.Format("{0:yyyy-MM-dd}", DateTime.Now.AddMonths(-1));
            }

            if ((StartDate == null) || (EndDate == null))
            {
                EndDate = String.Format("{0:yyyy-MM-dd}", DateTime.Now.AddDays(-1));
                StartDate = String.Format("{0:yyyy-MM-dd}", DateTime.Now.AddMonths(-1));
            }
            
            myTimeRange.StartTime = DateTime.Parse(StartDate);
            myTimeRange.EndTime = DateTime.Parse(EndDate);

            datePicker1.DataContext = myTimeRange;
            datePicker2.DataContext = myTimeRange;

            DataSource.clearChartData();

            loadData(actualMetricsIndex); 
        }
     
        /*
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Debug.WriteLine("AppMetrics - OnNavigatedTo");
            CallApp what = (CallApp)e.Parameter; // navigationParameter;
            pageTitle.Text = what.Name;
            apiKey = what.ApiKey;
            appapikey = what.AppApiKey;
            loadData(actualMetricsIndex);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }        
        */

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
        }

        private void changeMetrics(int index)
        {
            actualMetricsIndex = index;
            loadData(actualMetricsIndex);
            if (pageTitle2 != null)
            {
                pageTitle2.Text = AppMetricsNamesFormatted[actualMetricsIndex];
            }
            if (flipView1 != null)
            {
                flipView1.SelectedIndex = actualMetricsIndex;
            }
        }

        private void flipView1_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            Debug.WriteLine("flipView1 SelectionChanged");
            if (flipView1 != null)
            {
                changeMetrics(((FlipView)sender).SelectedIndex);
            }
        }

        private void ChartTrackBallBehavior_TrackInfoUpdated_1(object sender, TrackBallInfoEventArgs e)
        {

        }

        private void changeMetricsClicked(object sender, TappedRoutedEventArgs e)
        {
            MenuItem what = ((MenuItem)sender);
            changeMetrics((int)what.Tag);
        }

        private void StackPanel_Tapped_1(object sender, TappedRoutedEventArgs e)
        { // handle change metrics
            Debug.WriteLine("headerMenuClicked");
            // Create a menu containing two items
            var menu = new Menu();
            for (int i = 0; i <= AppMetricsNamesFormatted.Length-1; i++)
            {
                var newItem = new MenuItem { Text = AppMetricsNamesFormatted[i], Tag = i /* processingAccount.ApiKey */ };
                newItem.Tapped += changeMetricsClicked;
                menu.Items.Add(newItem);
            }

            // Show the menu in a flyout anchored to the header title
            var flyout = new Flyout();
            flyout.Placement = PlacementMode.Bottom;
            flyout.HorizontalAlignment = HorizontalAlignment.Right;
            flyout.HorizontalContentAlignment = HorizontalAlignment.Left;
            flyout.PlacementTarget = pageDropDown;
            flyout.Content = menu;
            flyout.IsOpen = true;
        }

        private void TimeRangeButton_Click_1(object sender, RoutedEventArgs e)
        { // AppMenu click on TimeRange
            if (!TimeRangeControl.IsOpen) // if not open - start anim
            {
                RootPopupBorder.Width = 320;
                TimeRangeControl.HorizontalOffset = 0;
                TimeRangeControl.VerticalOffset = Window.Current.Bounds.Height - 520;
                TimeRangeControl.IsOpen = true;
            }    
        }

        private void setClick_Click_1(object sender, RoutedEventArgs e)
        { // set new timerange
            TimeRangeControl.IsOpen = false;
            StartDate = String.Format("{0:yyyy-MM-dd}",myTimeRange.StartTime);
            Debug.WriteLine("StartDate:" + StartDate);
            EndDate = String.Format("{0:yyyy-MM-dd}",myTimeRange.EndTime);
            Debug.WriteLine("EndDate:" + EndDate);
            localSettings.Values["StartDate"] = StartDate;
            localSettings.Values["EndDate"] = EndDate;
            DataSource.clearChartData();
            loadData(actualMetricsIndex); 
        }

        private void cancelClick_Click_1(object sender, RoutedEventArgs e)
        { // cancel - just close timerange
            TimeRangeControl.IsOpen = false;
        }

        private void last14days_Click_1(object sender, RoutedEventArgs e)
        {
            TimeRangeControl.IsOpen = false;
            EndDate = String.Format("{0:yyyy-MM-dd}", DateTime.Now.AddDays(-1));
            StartDate = String.Format("{0:yyyy-MM-dd}", DateTime.Now.AddDays(-15));
            localSettings.Values["StartDate"] = StartDate;
            localSettings.Values["EndDate"] = EndDate;
            DataSource.clearChartData();
            loadData(actualMetricsIndex); 
        }

        private void lastMonth_Click_1(object sender, RoutedEventArgs e)
        {
            TimeRangeControl.IsOpen = false;
            EndDate = String.Format("{0:yyyy-MM-dd}", DateTime.Now.AddDays(-1));
            StartDate = String.Format("{0:yyyy-MM-dd}", DateTime.Now.AddDays(-1).AddMonths(-1));
            localSettings.Values["StartDate"] = StartDate;
            localSettings.Values["EndDate"] = EndDate;
            DataSource.clearChartData();
            loadData(actualMetricsIndex); 
        }

        private void lastQuarter_Click_1(object sender, RoutedEventArgs e)
        {
            TimeRangeControl.IsOpen = false;
            EndDate = String.Format("{0:yyyy-MM-dd}", DateTime.Now.AddDays(-1));
            StartDate = String.Format("{0:yyyy-MM-dd}", DateTime.Now.AddDays(-1).AddMonths(-3));
            localSettings.Values["StartDate"] = StartDate;
            localSettings.Values["EndDate"] = EndDate;
            DataSource.clearChartData();
            loadData(actualMetricsIndex); 
        }

        private void lastSixMonths_Click_1(object sender, RoutedEventArgs e)
        {
            TimeRangeControl.IsOpen = false;
            EndDate = String.Format("{0:yyyy-MM-dd}", DateTime.Now.AddDays(-1));
            StartDate = String.Format("{0:yyyy-MM-dd}", DateTime.Now.AddDays(-1).AddMonths(-6));
            localSettings.Values["StartDate"] = StartDate;
            localSettings.Values["EndDate"] = EndDate;
            DataSource.clearChartData();
            loadData(actualMetricsIndex); 
        }

    }
}
