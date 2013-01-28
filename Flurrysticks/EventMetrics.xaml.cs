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
using Windows.UI.StartScreen;

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

        public EventMetrics()
        {
            this.InitializeComponent();
        }

        private void LoadUpXMLEventParameters(XDocument loadedData, int selectedIndex, bool addParam)
        {
            // parse data for parameters
            var dataParam = from query in loadedData.Descendants("key")
                            select new ParamData
                            {
                                key = (string)query.Attribute("name"),
                                content = (IEnumerable<XElement>)query.Descendants("value"),
                            };
            IEnumerator<ParamData> enumerator = dataParam.GetEnumerator();
            int index = 0;
            IEnumerable<EventItem> dataParams = null;
            while (enumerator.MoveNext())
            {
                ParamData dataParamValues = enumerator.Current;
                if (addParam)
                {
                    DataSource.ParamKeys.Add(new EventItem { eventName = dataParamValues.key });
                }
                dataParams = from query in dataParamValues.content
                             orderby (int)query.Attribute("totalCount") descending
                             select new EventItem
                             {
                                 eventName = (string)query.Attribute("name"),
                                 eventValue = (string)query.Attribute("totalCount")
                             };
                if (index == selectedIndex)
                {
                    Debug.WriteLine("Setting parameter values list");
                    ParametersListBox.ItemsSource = dataParams; // dataParamValues.children;
                    ParametersListBoxShrinked.ItemsSource = dataParams; // dataParamValues.children;
                }
                Debug.WriteLine("Processing line: " + index);
                index = index + 1;
            }

            Debug.WriteLine("ParamKeys.Count=" + DataSource.ParamKeys.Count);

            if (DataSource.ParamKeys.Count > 0)
            {
                ParametersMetricsListPicker.ItemsSource = DataSource.ParamKeys;
                ParametersMetricsListPicker.IsEnabled = true;
                ParametersMetricsListPicker.SelectedIndex = selectedIndex;
                //NoParameters.Visibility = System.Windows.Visibility.Collapsed;

                List<EventItem> check = dataParams.ToList();
                if (check.Count > 0)
                {
                    //NoParameters.Visibility = System.Windows.Visibility.Collapsed;
                    ParametersMetricsListPicker.IsEnabled = true;
                }
                else // show no events available
                {
                    //NoParameters.Visibility = System.Windows.Visibility.Visible;
                    ParametersMetricsListPicker.IsEnabled = false;
                }

            }
            else
            {
                ParametersMetricsListPicker.IsEnabled = false;
                //NoParameters.Visibility = System.Windows.Visibility.Visible;
            }

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
                string callURL = "http://api.flurry.com/eventMetrics/Event?apiAccessCode=" + apiKey + "&apiKey=" + appApiKey + "&startDate=" + SDate + "&endDate=" + EDate + "&eventName=" + eventName;
                Debug.WriteLine(callURL);

                try
                {
                    DataSource.dataParametersXML = await dh.DownloadXML(callURL); // load it   
                    success = true;
                }
                catch (System.Net.Http.HttpRequestException)
                {   // load failed
                    success = false;
                }

                Debug.WriteLine("Success:" + success);
                if (success)
                { // --------------------------------------------------------------------------------------------------------------------- now parse and bind data
                    DataSource.getChartData()[8, targetSeries] = from query in DataSource.dataParametersXML.Descendants("day")
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
                        LoadUpXMLEventParameters(DataSource.dataParametersXML, 0, true);
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
            CompareToggleSetVisibility(flipView1.SelectedIndex);
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

            datePicker1.Value = DateTime.Parse(StartDate);
            datePicker2.Value = DateTime.Parse(EndDate);
            updateButtons();

            ToggleAppBarButton(!SecondaryTile.Exists(appApiKey));

            LoadUpXMLEventMetrics(StartDate, EndDate, 0);

        }

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

        private void ZoomToggleSetVisibility(int position)
        {
            RadCartesianChart[] targetCharts = { radChart1, radChart2, radChart3};
            if (position < 8)
            {
                if (targetCharts[position].Behaviors.Count > 0)
                { // show disable
                    ZoomToggleButton.Visibility = Visibility.Collapsed;
                    UnZoomToggleButton.Visibility = Visibility.Visible;
                }
                else
                { // show enable
                    ZoomToggleButton.Visibility = Visibility.Visible;
                    UnZoomToggleButton.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                ZoomToggleButton.Visibility = Visibility.Collapsed;
                UnZoomToggleButton.Visibility = Visibility.Collapsed;
            }
        }

        private void flipView1_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            Debug.WriteLine("flipView1 SelectionChanged");
            if (flipView1 != null)
            {
                changeMetrics(((FlipView)sender).SelectedIndex);
                //loadData(actualMetricsIndex, StartDate, EndDate, 0);
                ZoomToggleSetVisibility(flipView1.SelectedIndex);
                CompareToggleSetVisibility(flipView1.SelectedIndex);
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
            StartDate = String.Format("{0:yyyy-MM-dd}", datePicker1.Value);
            Debug.WriteLine("StartDate:" + StartDate);
            EndDate = String.Format("{0:yyyy-MM-dd}", datePicker2.Value);
            Debug.WriteLine("EndDate:" + EndDate);
            updateButtons();
            pageRoot.BottomAppBar.IsOpen = false;
            LoadUpXMLEventMetrics(StartDate, EndDate, 0); 
        }

        private void ToggleAppBarButton(bool showPinButton)
        {
            if (pinButton != null)
            {

                if (showPinButton)
                {
                    pinButton.Style = App.Current.Resources.MergedDictionaries[0]["PinAppBarButtonStyle"] as Style;
                }
                else
                {
                    pinButton.Style = App.Current.Resources.MergedDictionaries[0]["UnPinAppBarButtonStyle"] as Style;
                }

                //pinButton.Style = (showPinButton) ? (this.Resources["PinAppBarButtonStyle"] as Style) : (this.Resources["UnPinAppBarButtonStyle"] as Style);
            }
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
            datePicker1.Value = DateTime.Parse(StartDate);
            datePicker2.Value = DateTime.Parse(EndDate);
            updateButtons();
            pageRoot.BottomAppBar.IsOpen = false;
            LoadUpXMLEventMetrics(StartDate, EndDate, 0); 
        }

        private void lastMonth_Click_1(object sender, RoutedEventArgs e)
        {
            TimeRangeControl.IsOpen = false;
            EndDate = String.Format("{0:yyyy-MM-dd}", DateTime.Now.AddDays(-1));
            StartDate = String.Format("{0:yyyy-MM-dd}", DateTime.Now.AddDays(-1).AddMonths(-1));
            datePicker1.Value = DateTime.Parse(StartDate);
            datePicker2.Value = DateTime.Parse(EndDate);
            updateButtons();
            pageRoot.BottomAppBar.IsOpen = false;
            LoadUpXMLEventMetrics(StartDate, EndDate, 0); 
        }

        private void lastQuarter_Click_1(object sender, RoutedEventArgs e)
        {
            TimeRangeControl.IsOpen = false;
            EndDate = String.Format("{0:yyyy-MM-dd}", DateTime.Now.AddDays(-1));
            StartDate = String.Format("{0:yyyy-MM-dd}", DateTime.Now.AddDays(-1).AddMonths(-3));
            datePicker1.Value = DateTime.Parse(StartDate);
            datePicker2.Value = DateTime.Parse(EndDate);
            updateButtons();
            pageRoot.BottomAppBar.IsOpen = false;
            LoadUpXMLEventMetrics(StartDate, EndDate, 0); 
        }

        private void lastSixMonths_Click_1(object sender, RoutedEventArgs e)
        {
            TimeRangeControl.IsOpen = false;
            EndDate = String.Format("{0:yyyy-MM-dd}", DateTime.Now.AddDays(-1));
            StartDate = String.Format("{0:yyyy-MM-dd}", DateTime.Now.AddDays(-1).AddMonths(-6));
            datePicker1.Value = DateTime.Parse(StartDate);
            datePicker2.Value = DateTime.Parse(EndDate);
            updateButtons();
            pageRoot.BottomAppBar.IsOpen = false;
            LoadUpXMLEventMetrics(StartDate, EndDate, 0); 
        }

        private void CompareToggleSetVisibility(int position)
        {
            RadCartesianChart[] targetCharts = { radChart1, radChart2, radChart3};
            if (position < 3)
            {
                if (targetCharts[flipView1.SelectedIndex].Series[1].ItemsSource != null)
                { // show disable
                    CompareToggleButton.Visibility = Visibility.Collapsed;
                    UnCompareToggleButton.Visibility = Visibility.Visible;
                }
                else
                { // show enable
                    CompareToggleButton.Visibility = Visibility.Visible;
                    UnCompareToggleButton.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                CompareToggleButton.Visibility = Visibility.Collapsed;
                UnCompareToggleButton.Visibility = Visibility.Collapsed;
            }
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

            CompareToggleSetVisibility(flipView1.SelectedIndex);
            pageRoot.BottomAppBar.IsOpen = false;

        }

        private void ParametersMetricsListPicker_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {

            if (ParametersMetricsListPicker != null)
            {
                LoadUpXMLEventParameters(DataSource.dataParametersXML, ParametersMetricsListPicker.SelectedIndex, false);
            }

        }

        private void ZoomToggleButton_Click_1(object sender, RoutedEventArgs e)
        {
            RadCartesianChart[] targetCharts = { radChart1, radChart2, radChart3 };
            if (flipView1.SelectedIndex < 8)
            {
                RadCartesianChart targetChart = targetCharts[flipView1.SelectedIndex];
                //foreach (RadCartesianChart targetChart in targetCharts)
                // {
                if (targetChart.Behaviors.Count > 0)
                { // disable chart behaviour
                    Debug.WriteLine("Disable chart behaviour: " + targetChart.Behaviors.Count);
                    //ChartPanAndZoomBehavior item1 = targetChart.Behaviors[0] as ChartPanAndZoomBehavior;
                    //ChartTrackBallBehavior item2 = targetChart.Behaviors[1] as ChartTrackBallBehavior;
                    try
                    {
                        targetChart.Behaviors.Clear();
                    }
                    catch (System.ArgumentException)
                    {
                        Debug.WriteLine("ArgumentException");
                    }
                    //if (item2 != null) { targetChart.Behaviors.Remove(item2); }
                    //if (item1 != null) { targetChart.Behaviors.Remove(item1); }                  
                    targetChart.IsHitTestVisible = false;
                    targetChart.Zoom = new Size(1, 1);
                }
                else
                { // enable
                    Debug.WriteLine("Enable chart behaviour: " + targetChart.Behaviors.Count);
                    ChartPanAndZoomBehavior item1 = new ChartPanAndZoomBehavior();
                    item1.ZoomMode = ChartPanZoomMode.Horizontal;
                    item1.PanMode = ChartPanZoomMode.Horizontal;
                    targetChart.Behaviors.Add(item1);
                    ChartTrackBallBehavior item2 = new ChartTrackBallBehavior();
                    item2.ShowIntersectionPoints = true;
                    item2.InfoMode = TrackInfoMode.Multiple;
                    item2.TrackInfoUpdated += ChartTrackBallBehavior_TrackInfoUpdated_1;
                    item2.ShowInfo = true;
                    targetChart.Behaviors.Add(item2);
                } // enabling chart behaviours
                //}
                pageRoot.BottomAppBar.IsOpen = false; // close it
                ZoomToggleSetVisibility(flipView1.SelectedIndex);
            }
        }

        private void updateButtons()
        {

            localSettings.Values["StartDate"] = StartDate;
            localSettings.Values["EndDate"] = EndDate;

            datePicker1.MaxValue = DateTime.Parse(EndDate).AddDays(-1);
            datePicker1.MinValue = DateTime.Parse(EndDate).AddDays(-1).AddYears(-1);
            datePicker2.MinValue = DateTime.Parse(StartDate);
            datePicker2.MaxValue = DateTime.Now;

            DataSource.clearChartData(); // after setting new timerange clear data to force new loadUp

        }

        private void datePicker2_ValueChanged_1(object sender, EventArgs e)
        {
            EndDate = String.Format("{0:yyyy-MM-dd}", datePicker2.Value);
            updateButtons();
        }

        private void datePicker1_ValueChanged_1(object sender, EventArgs e)
        {
            StartDate = String.Format("{0:yyyy-MM-dd}", datePicker1.Value);
            updateButtons();
        }

        private async void pinButton_Click_1(object sender, RoutedEventArgs e)
        {

            pageRoot.BottomAppBar.IsSticky = true;
            string shortName = appName;
            string displayName = appName;
            string tileActivationArguments = appName + "|" + appPlatform + "|" + apiKey + "|" + appApiKey;
            Uri logo = new Uri("ms-appx:///Assets/Logo.png");


            SecondaryTile secondaryTile = new SecondaryTile(appApiKey,
                                                            shortName,
                                                            displayName,
                                                            tileActivationArguments,
                                                            TileOptions.ShowNameOnLogo,
                                                            logo);

            secondaryTile.ForegroundText = ForegroundText.Dark;
            secondaryTile.SmallLogo = new Uri("ms-appx:///Assets/SmallLogo.png");

            if (!SecondaryTile.Exists(appApiKey))
            { // add
                bool x = await secondaryTile.RequestCreateAsync();
                Debug.WriteLine("secondaryTile creation: " + x);

            }
            else
            { // remove
                bool x = await secondaryTile.RequestDeleteAsync();
                Debug.WriteLine("secondaryTile removal: " + x);
            }
            pageRoot.BottomAppBar.IsSticky = false;
            pageRoot.BottomAppBar.IsOpen = false;
            ToggleAppBarButton(!SecondaryTile.Exists(appApiKey));

            /*
            XmlDocument tileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquareText02);
            XmlNodeList tileTextAttributes = tileXml.GetElementsByTagName("text");
            tileTextAttributes[0].InnerText = appName;
            tileTextAttributes[1].InnerText = appPlatform;

            // create a tile notification
            TileNotification tile = new TileNotification(tileXml);

            TileUpdateManager.CreateTileUpdaterForSecondaryTile("1").Update(tile);
           */

        }

        private void Grid_KeyUp_1(object sender, KeyRoutedEventArgs e)
        {
            if ((e.Key == Windows.System.VirtualKey.Escape) || (e.Key == Windows.System.VirtualKey.Back))
            {
                this.GoBack(sender, e);
            }
        }

    }
}
