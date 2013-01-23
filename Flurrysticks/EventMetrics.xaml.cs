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
    public sealed partial class EventMetrics : Flurrystics.Common.LayoutAwarePage
    {

        string apiKey;
        string appApiKey = ""; // initial apikey of the app
        string appName;
        string appPlatform;
        string eventName;
        string EndDate;
        string StartDate;
        string[] EventMetricsNames = { "uniqueUsers", "totalSessions", "totalCount" };
        string[] EventMetricsNamesFormatted = { "Unique Users", "Total Sessions", "Total Count", "Parameters" };
        int actualMetricsIndex = 0;
        DownloadHelper dh = new DownloadHelper();
        Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
        TimeRange myTimeRange = new TimeRange();

        public EventMetrics()
        {
            this.InitializeComponent();
        }

        private async void LoadUpXMLEventMetrics(String SDate, String EDate, int targetSeries)
        {
            ProgressBar1.Visibility = Windows.UI.Xaml.Visibility.Visible;

            RadCartesianChart[] targetCharts = { radChart1, radChart2, radChart3 };
            RadCustomHubTile[] t1s = { tile1Text1, tile2Text1, tile3Text1 };
            RadCustomHubTile[] t2s = { tile1Text2, tile2Text2, tile3Text2 };
            RadCustomHubTile[] t3s = { tile1Text3, tile2Text3, tile3Text3 };
            TextBlock[] n1s = { tile1Number1Text1, tile2Number1Text1, tile3Number1Text1 };
            TextBlock[] n2s = { tile1Number1Text2, tile2Number1Text2, tile3Number1Text2 };
            TextBlock[] n3s = { tile1Number1Text3, tile2Number1Text3, tile3Number1Text3 };
            TextBlock[] c1s = { tile1Number2Text1, tile2Number2Text1, tile3Number2Text1 };
            TextBlock[] c2s = { tile1Number2Text2, tile2Number2Text2, tile3Number2Text2 };
            TextBlock[] c3s = { tile1Number2Text3, tile2Number2Text3, tile3Number2Text3 };
            TextBlock[] totals = { info2Text1, info2Text2, info2Text3 };
            TextBlock[] totals2 = { info4Text1, info4Text2, info4Text3 };            
            TextBlock[] d1s = { info3Text1, info3Text2, info3Text3 };
            TextBlock[] d2s = { info5Text1, info5Text2, info5Text3 };
            //int s = MainPivot.SelectedIndex;
            String queryURL = SDate + " - " + EDate;

            if (targetSeries > 0)
            {
                // progressBar.Visibility = System.Windows.Visibility.Visible;
                for (int i = 0; i < 3; i++)
                {
                    t1s[i].IsFrozen = false;
                    t2s[i].IsFrozen = false;
                    t3s[i].IsFrozen = false;
                    totals2[i].Visibility = Visibility.Visible;
                    d2s[i].Text = "(" + SDate + " - " + EDate + ")";
                    d2s[i].Visibility = Visibility.Visible;
                }
            }
            else  // reset compare chart
            {
                for (int i = 0; i < 3; i++)
                {
                    totals2[i].Visibility = Visibility.Collapsed;
                    targetCharts[i].Series[1].ItemsSource = null;
                    d1s[i].Visibility = Visibility.Visible;
                    d2s[i].Visibility = Visibility.Collapsed;
                    d1s[i].Text = "(" + SDate + " - " + EDate + ")";
                    t1s[i].IsFrozen = true;
                    t2s[i].IsFrozen = true;
                    t3s[i].IsFrozen = true;
                    t1s[i].IsFlipped = false;
                    t2s[i].IsFlipped = false;
                    t3s[i].IsFlipped = false;
                }
            }

            if (DataSource.getChartData()[8, targetSeries] == null) // if no data present
            {

                bool success;
                XDocument result = null;
                string callURL = "http://api.flurry.com/eventMetrics/Event?apiAccessCode=" + apiKey + "&apiKey=" + appApiKey + "&startDate=" + SDate + "&endDate=" + EDate + "&eventName=" + eventName;
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
                if (success)
                { // --------------------------------------------------------------------------------------------------------------------- now parse and bind data
                    DataSource.getChartData()[8, targetSeries] = from query in result.Descendants("day")
                                                                     select new ChartDataPoint
                                                                     {
                                                                         // <day uniqueUsers="378" totalSessions="3152" totalCount="6092" duration="0" date="2012-09-13"/>
                                                                         Value1 = (double)query.Attribute("uniqueUsers"),
                                                                         Value2 = (double)query.Attribute("totalSessions"),
                                                                         Value3 = (double)query.Attribute("totalCount"),
                                                                         Label = (string)query.Attribute("date")
                                                                     };

                    if (!(targetSeries > 0))
                    { // process parameters only when not processing compare data
                        // LoadUpXMLEventParameters(loadedData, 0, true);
                    }

                    // count max,min,latest,total for display purposes
                    double latest = 0, minim = 9999999999999, maxim = 0, totalCount = 0;
                    double latest2 = 0, minim2 = 9999999999999, maxim2 = 0, totalCount2 = 0;
                    double latest3 = 0, minim3 = 9999999999999, maxim3 = 0, totalCount3 = 0;
                    IEnumerator<ChartDataPoint> enumerator = DataSource.getChartData()[8, targetSeries].GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        ChartDataPoint oneValue = enumerator.Current;

                        latest = oneValue.Value1;
                        minim = Math.Min(minim, oneValue.Value1);
                        maxim = Math.Max(maxim, oneValue.Value1);
                        totalCount = totalCount + oneValue.Value1;

                        latest2 = oneValue.Value2;
                        minim2 = Math.Min(minim2, oneValue.Value2);
                        maxim2 = Math.Max(maxim2, oneValue.Value2);
                        totalCount2 = totalCount2 + oneValue.Value2;

                        latest3 = oneValue.Value3;
                        minim3 = Math.Min(minim, oneValue.Value3);
                        maxim3 = Math.Max(maxim, oneValue.Value3);
                        totalCount3 = totalCount3 + oneValue.Value3;

                    }

                    if (!(targetSeries > 0))
                    {
                        n1s[0].Text = latest.ToString();
                        n2s[0].Text = minim.ToString();
                        n3s[0].Text = maxim.ToString();
                        totals[0].Text = totalCount.ToString();
                        n1s[1].Text = latest2.ToString();
                        n2s[1].Text = minim2.ToString();
                        n3s[1].Text = maxim2.ToString();
                        totals[1].Text = totalCount2.ToString();
                        n1s[2].Text = latest3.ToString();
                        n2s[2].Text = minim3.ToString();
                        n3s[2].Text = maxim3.ToString();
                        totals[2].Text = totalCount3.ToString();
                    }
                    else
                    {
                        c1s[0].Text = latest.ToString();
                        c2s[0].Text = minim.ToString();
                        c3s[0].Text = maxim.ToString();
                        totals2[0].Text = totalCount.ToString();
                        c1s[1].Text = latest2.ToString();
                        c2s[1].Text = minim2.ToString();
                        c3s[1].Text = maxim2.ToString();
                        totals2[1].Text = totalCount2.ToString();
                        c1s[2].Text = latest3.ToString();
                        c2s[2].Text = minim3.ToString();
                        c3s[2].Text = maxim3.ToString();
                        totals2[2].Text = totalCount3.ToString();
                    }

                    List<ChartDataPoint> count = DataSource.getChartData()[8, targetSeries].ToList();

                    int setInterval = 5; // default
                    if (count != null)
                    {
                        setInterval = Util.getLabelIntervalByCount(count.Count);
                    }
                    else setInterval = Util.getLabelInterval(DateTime.Parse(StartDate), DateTime.Parse(EndDate));

                    // re-assign time data if comparing

                    // for processed data for comparison
                    ObservableCollection<ChartDataPoint> newData = new ObservableCollection<ChartDataPoint>();

                    if (targetSeries > 0) // if it's compare we have to fake time
                    {
                        var previousData = targetCharts[0].Series[0].ItemsSource;
                        IEnumerator<ChartDataPoint> myenumerator = previousData.GetEnumerator() as System.Collections.Generic.IEnumerator<ChartDataPoint>;
                        int p = 0;

                        while (myenumerator.MoveNext())
                        {
                            ChartDataPoint c = myenumerator.Current;
                            ChartDataPoint n = DataSource.getChartData()[8, targetSeries].ElementAt(p) as ChartDataPoint;
                            n.Label = c.Label;
                            newData.Add(new ChartDataPoint { Value1 = n.Value1, Value2 = n.Value2, Value3 = n.Value3, Label = c.Label });
                            p++;
                        }

                    }
                    if (!(targetSeries > 0))
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            targetCharts[i].Series[0].ItemsSource = DataSource.getChartData()[8, targetSeries];
                            targetCharts[i].HorizontalAxis.LabelInterval = setInterval;
                        }
                    }
                    else
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            targetCharts[i].Series[1].ItemsSource = newData;
                            targetCharts[i].HorizontalAxis.LabelInterval = setInterval;
                        }
                    }

                } // if success

            } // if noDataPresent

            ProgressBar1.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
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
            Debug.WriteLine("EventMetrics - LoadState");
            CallApp what = (CallApp)navigationParameter;
            appName = what.Name;
            appPlatform = what.Platform;
            platformTitle.Text = appPlatform;
            apiKey = what.ApiKey;
            appApiKey = what.AppApiKey;
            eventName = what.Event;
            pageTitle.Text = appName+": "+eventName;

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

            DataSource.clearChartDataEvents();
            LoadUpXMLEventMetrics(StartDate, EndDate, 0);

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
                pageTitle2.Text = EventMetricsNamesFormatted[actualMetricsIndex];
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
                //loadData(actualMetricsIndex, StartDate, EndDate, 0);
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
            for (int i = 0; i <= EventMetricsNamesFormatted.Length-1; i++)
            {
                var newItem = new MenuItem { Text = EventMetricsNamesFormatted[i], Tag = i /* processingAccount.ApiKey */ };
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
            //loadData(actualMetricsIndex,StartDate,EndDate,0); 
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
            LoadUpXMLEventMetrics(StartDate, EndDate, 0);
        }

        private void lastMonth_Click_1(object sender, RoutedEventArgs e)
        {
            TimeRangeControl.IsOpen = false;
            EndDate = String.Format("{0:yyyy-MM-dd}", DateTime.Now.AddDays(-1));
            StartDate = String.Format("{0:yyyy-MM-dd}", DateTime.Now.AddDays(-1).AddMonths(-1));
            localSettings.Values["StartDate"] = StartDate;
            localSettings.Values["EndDate"] = EndDate;
            DataSource.clearChartData();
            LoadUpXMLEventMetrics(StartDate, EndDate, 0);  
        }

        private void lastQuarter_Click_1(object sender, RoutedEventArgs e)
        {
            TimeRangeControl.IsOpen = false;
            EndDate = String.Format("{0:yyyy-MM-dd}", DateTime.Now.AddDays(-1));
            StartDate = String.Format("{0:yyyy-MM-dd}", DateTime.Now.AddDays(-1).AddMonths(-3));
            localSettings.Values["StartDate"] = StartDate;
            localSettings.Values["EndDate"] = EndDate;
            DataSource.clearChartData();
            LoadUpXMLEventMetrics(StartDate, EndDate, 0); 
        }

        private void lastSixMonths_Click_1(object sender, RoutedEventArgs e)
        {
            TimeRangeControl.IsOpen = false;
            EndDate = String.Format("{0:yyyy-MM-dd}", DateTime.Now.AddDays(-1));
            StartDate = String.Format("{0:yyyy-MM-dd}", DateTime.Now.AddDays(-1).AddMonths(-6));
            localSettings.Values["StartDate"] = StartDate;
            localSettings.Values["EndDate"] = EndDate;
            DataSource.clearChartData();
            LoadUpXMLEventMetrics(StartDate, EndDate, 0);
        }

        private void CompareToggleButton_Click_1(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Compare Toggle");
            if (flipView1.SelectedIndex > 7) { return; } // do nothing for events - there's no compare
            RadCartesianChart targetChart;
            RadCartesianChart[] targetCharts = { radChart1, radChart2, radChart3 };
            RadCustomHubTile[] t1s = { tile1Text1, tile1Text2, tile1Text3 };
            RadCustomHubTile[] t2s = { tile2Text1, tile2Text2, tile2Text3 };
            RadCustomHubTile[] t3s = { tile3Text1, tile3Text2, tile3Text3 };
            TextBlock[] d1s = { info3Text1, info3Text2, info3Text3 };
            TextBlock[] d2s = { info5Text1, info5Text2, info5Text3 };
            TextBlock[] c1s = { tile1Number2Text1, tile1Number2Text2, tile1Number2Text3 };
            TextBlock[] c2s = { tile2Number2Text1, tile2Number2Text2, tile2Number2Text3 };
            TextBlock[] c3s = { tile3Number2Text1, tile3Number2Text2, tile3Number2Text3 };
            TextBlock[] totals = { info4Text1, info4Text2, info4Text3 };

            int s = flipView1.SelectedIndex;

            targetChart = targetCharts[s];

            TimeSpan timeRange = DateTime.Parse(EndDate) - DateTime.Parse(StartDate);

            string StartDate2 = String.Format("{0:yyyy-MM-dd}", DateTime.Parse(StartDate).AddDays(-timeRange.TotalDays));
            string EndDate2 = String.Format("{0:yyyy-MM-dd}", DateTime.Parse(EndDate).AddDays(-timeRange.TotalDays));

            if (targetChart.Series[1].ItemsSource == null)
            {
                LoadUpXMLEventMetrics(StartDate2, EndDate2, 1);
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

        private void EventsListBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e) // clicking on event
        {

        }

        private void EventsMetricsListPicker_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (EventsMetricsListPicker != null)
            {
                // LoadUpXMLEvents(StartDate, EndDate, EventsMetricsListPicker.SelectedIndex);
            }
        }

    }
}
