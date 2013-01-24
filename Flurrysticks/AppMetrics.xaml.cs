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
using System.Collections.ObjectModel;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace Flurrystics
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class AppMetrics : Flurrystics.Common.LayoutAwarePage
    {

        string apiKey;
        string appApiKey = ""; // initial apikey of the app
        string appName;
        string appPlatform;
        string[] AppMetricsNames = { "ActiveUsers", "ActiveUsersByWeek", "ActiveUsersByMonth", "NewUsers", "MedianSessionLength", "AvgSessionLength", "Sessions", "RetainedUsers" };
        string[] AppMetricsNamesFormatted = { "Active Users", "Active Users By Week", "Active Users By Month", "New Users", "Median Session Length", "Avg Session Length", "Sessions", "Retained Users", "Events List" };
        string[] EventMetrics = { "usersLastDay", "usersLastWeek", "usersLastMonth", "avgUsersLastDay", "avgUsersLastWeek", "avgUsersLastMonth", "totalSessions", "totalCount" };
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
            Debug.WriteLine("Processing..." + what);

            DataSource.getChartData()[i,targetSeries] = from query in what.Descendants("day")
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
                tb.Visibility = Visibility.Visible;
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
                targetChart.Series[1].ItemsSource = null;
                tr1.Visibility = Visibility.Visible;
                tr2.Visibility = Visibility.Collapsed;
                tr1.Text = "(" + sDate + " - " + eDate + ")";
                rt1.IsFlipped = false;
                rt2.IsFlipped = false;
                rt3.IsFlipped = false;
                rt1.IsFrozen = true;
                rt2.IsFrozen = true;
                rt3.IsFrozen = true;
            }
            Debug.WriteLine("Setting DataContext targetSeries:" + targetSeries);

            if (targetSeries > 0) // if it's compare we have to fake time
            {
                var previousData = DataSource.getChartData()[i, 0];
                ObservableCollection<ChartDataPoint> newData = new ObservableCollection<ChartDataPoint>();
                IEnumerator<ChartDataPoint> enumerator = previousData.GetEnumerator() as System.Collections.Generic.IEnumerator<ChartDataPoint>;
                int p = 0;
                while (enumerator.MoveNext())
                {
                    ChartDataPoint c = enumerator.Current;
                    Debug.WriteLine("Old Label:" + DataSource.getChartData()[i, 1].ElementAt<ChartDataPoint>(p).Label + " New Label:" + c.Label);
                    ChartDataPoint n = new ChartDataPoint { Value = DataSource.getChartData()[i, 1].ElementAt<ChartDataPoint>(p).Value, Label = c.Label };
                    newData.Add(n);
                    Debug.WriteLine("New label set:" + DataSource.getChartData()[i, 1].ElementAt<ChartDataPoint>(p).Label);
                    p++;
                }

                DataSource.getChartData()[i, 1] = newData;

            }

            targetChart.Series[targetSeries].ItemsSource = DataSource.getChartData()[i, targetSeries];
            targetChart.HorizontalAxis.LabelInterval = Util.getLabelIntervalByCount(DataSource.getChartData()[i, targetSeries].Count());

            // count max,min,latest,total for display purposes
            double latest = 0, minim = 9999999999999, maxim = 0, totalCount = 0;
            IEnumerator<ChartDataPoint> Myenum = DataSource.getChartData()[i,targetSeries].GetEnumerator();
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

        private async void loadData(int metricsIndex, string SDate, string EDate, int targetSeries)
        {

            // events
            if (metricsIndex == 8)
            {
                LoadUpXMLEvents(SDate, EDate, EventsMetricsListPicker.SelectedIndex);
                return;
            }

            Debug.WriteLine("loadData() " + metricsIndex);
            RadCartesianChart[] targetCharts = { radChart1, radChart2, radChart3, radChart4, radChart5, radChart6, radChart7, radChart8 };
            RadCustomHubTile[] t1s = { tile1Text1, tile1Text2, tile1Text3, tile1Text4, tile1Text5, tile1Text6, tile1Text7, tile1Text8 };
            RadCustomHubTile[] t2s = { tile2Text1, tile2Text2, tile2Text3, tile2Text4, tile2Text5, tile2Text6, tile2Text7, tile2Text8 };
            RadCustomHubTile[] t3s = { tile3Text1, tile3Text2, tile3Text3, tile3Text4, tile3Text5, tile3Text6, tile3Text7, tile3Text8 };
            
            TextBlock[] d1s = { info3Text1, info3Text2, info3Text3, info3Text4, info3Text5, info3Text6, info3Text7, info3Text8 };
            TextBlock[] d2s = { info5Text1, info5Text2, info5Text3, info5Text4, info5Text5, info5Text6, info5Text7, info5Text8 };

            TextBlock[] totals;
            TextBlock[] c1s;
            TextBlock[] c2s;
            TextBlock[] c3s;
            if (targetSeries==0) {
                TextBlock[] c1s_1 = { tile1Number1Text1, tile1Number1Text2, tile1Number1Text3, tile1Number1Text4, tile1Number1Text5, tile1Number1Text6, tile1Number1Text7, tile1Number1Text8 };
                TextBlock[] c2s_1 = { tile2Number1Text1, tile2Number1Text2, tile2Number1Text3, tile2Number1Text4, tile2Number1Text5, tile2Number1Text6, tile2Number1Text7, tile2Number1Text8 };
                TextBlock[] c3s_1 = { tile3Number1Text1, tile3Number1Text2, tile3Number1Text3, tile3Number1Text4, tile3Number1Text5, tile3Number1Text6, tile3Number1Text7, tile3Number1Text8 };
                TextBlock[] totals_1 = { info2Text1, info2Text2, info2Text3, info2Text4, info2Text5, info2Text6, info2Text7, info2Text8 };
                totals = totals_1;
                c1s = c1s_1;
                c2s = c2s_1;
                c3s = c3s_1;
            } else {
                TextBlock[] c1s_2 = { tile1Number2Text1, tile1Number2Text2, tile1Number2Text3, tile1Number2Text4, tile1Number2Text5, tile1Number2Text6, tile1Number2Text7, tile1Number2Text8 };
                TextBlock[] c2s_2 = { tile2Number2Text1, tile2Number2Text2, tile2Number2Text3, tile2Number2Text4, tile2Number2Text5, tile2Number2Text6, tile2Number2Text7, tile2Number2Text8 };
                TextBlock[] c3s_2 = { tile3Number2Text1, tile3Number2Text2, tile3Number2Text3, tile3Number2Text4, tile3Number2Text5, tile3Number2Text6, tile3Number2Text7, tile3Number2Text8 };
                TextBlock[] totals_2 = { info4Text1, info4Text2, info4Text3, info4Text4, info4Text5, info4Text6, info4Text7, info4Text8 };
                totals = totals_2;
                c1s = c1s_2;
                c2s = c2s_2;
                c3s = c3s_2;
            }

            string metrics = AppMetricsNames[metricsIndex]; // this will be selectable
            if (ProgressBar1 != null)
            {
                ProgressBar1.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
            // check if it's loaded, if not - load it up

            if (DataSource.getChartData()[metricsIndex,targetSeries] == null) // if no data present
            {

                bool success;
                XDocument result = null;
                string callURL = "http://api.flurry.com/appMetrics/" + metrics + "?apiAccessCode=" + apiKey + "&apiKey=" + appApiKey + "&startDate=" + SDate + "&endDate=" + EDate;
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
                                ParseXML(result,c,targetCharts[c],t1s[c],t2s[c],t3s[c],c1s[c],c2s[c],c3s[c],totals[c],SDate,EDate,d1s[c],d2s[c],targetSeries);
                             }
            }
            else
            {
                Debug.WriteLine("Setting DataContext of already loaded data:" + DataSource.getChartData()[metricsIndex,targetSeries].Count());
                targetCharts[metricsIndex].Series[targetSeries].ItemsSource = DataSource.getChartData()[metricsIndex,targetSeries];
                if (targetSeries > 0)
                {
                    t1s[metricsIndex].IsFrozen = false;
                    t2s[metricsIndex].IsFrozen = false;
                    t3s[metricsIndex].IsFrozen = false;
                    d2s[metricsIndex].Visibility = Visibility.Visible;
                    totals[metricsIndex].Visibility = Visibility.Visible;
                    d2s[metricsIndex].Text = "(" + SDate + " - " + EDate + ")";
                }
            }

            //if (App.taskCount == 0)
            //{
                ProgressBar1.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            //}

        }

        private async void LoadUpXMLEvents(String SDate, String EDate, int orderByIndex)
        {

            bool success;
            IEnumerable<EventItem> dataEvents = null;
            if (ProgressBar1 != null) // display progressbar
            {
                ProgressBar1.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
            if (DataSource.dataEventsXML == null)
            {

                string callURL = "http://api.flurry.com/eventMetrics/Summary?apiAccessCode=" + apiKey + "&apiKey=" + appApiKey + "&startDate=" + SDate + "&endDate=" + EDate;
                Debug.WriteLine(callURL);
                try
                {
                    DataSource.dataEventsXML = await dh.DownloadXML(callURL); // load it 
                    success = true;
                }
                catch (System.Net.Http.HttpRequestException)
                {   // load failed
                    success = false;
                }

            }
            else // already loaded
            {
                success = true;
            }

                Debug.WriteLine("Success:" + success);
                if (success) { 
                            dataEvents = from query in DataSource.dataEventsXML.Descendants("event")
                               orderby (int)query.Attribute(EventMetrics[orderByIndex]) descending
                               select new EventItem
                               {
                                   eventName = (string)query.Attribute("eventName"),
                                   eventValue = (string)query.Attribute(EventMetrics[orderByIndex])
                                   /*
                                   usersLastDay = ,
                                   usersLastWeek = (string)query.Attribute(EventMetrics[1]),
                                   usersLastMonth = (string)query.Attribute(EventMetrics[2]),
                                   avgUsersLastDay = (string)query.Attribute(EventMetrics[3]),
                                   avgUsersLastWeek = (string)query.Attribute(EventMetrics[4]),
                                   avgUsersLastMonth = (string)query.Attribute(EventMetrics[5]),
                                   totalSessions = (string)query.Attribute(EventMetrics[6]),
                                   totalCount = (string)query.Attribute(EventMetrics[7])
                                    * */
                               };
                                //int c = metricsIndex;
                                //ParseXML(result,c,targetCharts[c],t1s[c],t2s[c],t3s[c],c1s[c],c2s[c],c3s[c],totals[c],SDate,EDate,d1s[c],d2s[c],targetSeries);
                             } // success

                EventsListBox.ItemsSource = dataEvents;
                EventsListBoxShrinked.ItemsSource = dataEvents;
                if (ProgressBar1 != null)
                {
                    ProgressBar1.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                }
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
            appName = what.Name;
            pageTitle.Text = appName;
            appPlatform = what.Platform;
            platformTitle.Text = appPlatform;
            apiKey = what.ApiKey;
            appApiKey = what.AppApiKey;

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

            loadData(actualMetricsIndex,StartDate,EndDate,0); 
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
                loadData(actualMetricsIndex, StartDate, EndDate, 0);
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
            loadData(actualMetricsIndex,StartDate,EndDate,0); 
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
            loadData(actualMetricsIndex, StartDate, EndDate, 0); 
        }

        private void lastMonth_Click_1(object sender, RoutedEventArgs e)
        {
            TimeRangeControl.IsOpen = false;
            EndDate = String.Format("{0:yyyy-MM-dd}", DateTime.Now.AddDays(-1));
            StartDate = String.Format("{0:yyyy-MM-dd}", DateTime.Now.AddDays(-1).AddMonths(-1));
            localSettings.Values["StartDate"] = StartDate;
            localSettings.Values["EndDate"] = EndDate;
            DataSource.clearChartData();
            loadData(actualMetricsIndex, StartDate, EndDate, 0);  
        }

        private void lastQuarter_Click_1(object sender, RoutedEventArgs e)
        {
            TimeRangeControl.IsOpen = false;
            EndDate = String.Format("{0:yyyy-MM-dd}", DateTime.Now.AddDays(-1));
            StartDate = String.Format("{0:yyyy-MM-dd}", DateTime.Now.AddDays(-1).AddMonths(-3));
            localSettings.Values["StartDate"] = StartDate;
            localSettings.Values["EndDate"] = EndDate;
            DataSource.clearChartData();
            loadData(actualMetricsIndex, StartDate, EndDate, 0); 
        }

        private void lastSixMonths_Click_1(object sender, RoutedEventArgs e)
        {
            TimeRangeControl.IsOpen = false;
            EndDate = String.Format("{0:yyyy-MM-dd}", DateTime.Now.AddDays(-1));
            StartDate = String.Format("{0:yyyy-MM-dd}", DateTime.Now.AddDays(-1).AddMonths(-6));
            localSettings.Values["StartDate"] = StartDate;
            localSettings.Values["EndDate"] = EndDate;
            DataSource.clearChartData();
            loadData(actualMetricsIndex, StartDate, EndDate, 0); 
        }

        private void CompareToggleButton_Click_1(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Compare Toggle");
            if (flipView1.SelectedIndex > 7) { return; } // do nothing for events - there's no compare
            RadCartesianChart targetChart;
            RadCartesianChart[] targetCharts = { radChart1, radChart2, radChart3, radChart4, radChart5, radChart6, radChart7, radChart8 };
            RadCustomHubTile[] t1s = { tile1Text1, tile1Text2, tile1Text3, tile1Text4, tile1Text5, tile1Text6, tile1Text7, tile1Text8 };
            RadCustomHubTile[] t2s = { tile2Text1, tile2Text2, tile2Text3, tile2Text4, tile2Text5, tile2Text6, tile2Text7, tile2Text8 };
            RadCustomHubTile[] t3s = { tile3Text1, tile3Text2, tile3Text3, tile3Text4, tile3Text5, tile3Text6, tile3Text7, tile3Text8 };
            TextBlock[] d1s = { info3Text1, info3Text2, info3Text3, info3Text4, info3Text5, info3Text6, info3Text7, info3Text8 };
            TextBlock[] d2s = { info5Text1, info5Text2, info5Text3, info5Text4, info5Text5, info5Text6, info5Text7, info5Text8 };
            TextBlock[] c1s = { tile1Number2Text1, tile1Number2Text2, tile1Number2Text3, tile1Number2Text4, tile1Number2Text5, tile1Number2Text6, tile1Number2Text7, tile1Number2Text8 };
            TextBlock[] c2s = { tile2Number2Text1, tile2Number2Text2, tile2Number2Text3, tile2Number2Text4, tile2Number2Text5, tile2Number2Text6, tile2Number2Text7, tile2Number2Text8 };
            TextBlock[] c3s = { tile3Number2Text1, tile3Number2Text2, tile3Number2Text3, tile3Number2Text4, tile3Number2Text5, tile3Number2Text6, tile3Number2Text7, tile3Number2Text8 };
            TextBlock[] totals = { info4Text1, info4Text2, info4Text3, info4Text4, info4Text5, info4Text6, info4Text7, info4Text8 };

            int s = flipView1.SelectedIndex;

            targetChart = targetCharts[s];

            TimeSpan timeRange = DateTime.Parse(EndDate) - DateTime.Parse(StartDate);

            string StartDate2 = String.Format("{0:yyyy-MM-dd}", DateTime.Parse(StartDate).AddDays(-timeRange.TotalDays));
            string EndDate2 = String.Format("{0:yyyy-MM-dd}", DateTime.Parse(EndDate).AddDays(-timeRange.TotalDays));

            if (targetChart.Series[1].ItemsSource == null)
            {
                loadData(actualMetricsIndex, StartDate2, EndDate2, 1);     
            }
            else
            {
                targetChart.Series[1].ItemsSource = null;
                totals[s].Visibility = Visibility.Collapsed;
                d2s[s].Visibility = Visibility.Collapsed;
                t1s[s].IsFlipped = false;
                t2s[s].IsFlipped = false;
                t3s[s].IsFlipped = false;
                t1s[s].IsFrozen = true;
                t2s[s].IsFrozen = true;
                t3s[s].IsFrozen = true;
            }
        }

        private void EventsMetricsListPicker_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (EventsMetricsListPicker != null)
            {
                LoadUpXMLEvents(StartDate, EndDate, EventsMetricsListPicker.SelectedIndex);
            }
        }

        private void EventsListBox_ItemClick_1(object sender, ItemClickEventArgs e)
        {
            // jump to EventMetrics
            CallApp what = new CallApp();
            what.AppApiKey = appApiKey;
            what.ApiKey = apiKey;
            what.Name = appName;
            what.Platform = appPlatform;
            what.Event = ((EventItem)e.ClickedItem).eventName;
            this.Frame.Navigate(typeof(EventMetrics), what);
        }

        private void ZoomToggleButton_Click_1(object sender, RoutedEventArgs e)
        {

        }

    }
}
