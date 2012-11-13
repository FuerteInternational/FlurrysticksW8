using Flurrysticks.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Diagnostics;
using Callisto.Controls;
using System.Collections.ObjectModel;
using System.Xml.Linq;

// The Items Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234233

namespace Flurrysticks
{
    /// <summary>
    /// A page that displays a collection of item previews.  In the Split Application this page
    /// is used to display and select one of the available groups.
    /// </summary>
    public sealed partial class AccountApplicationsPage : Flurrysticks.Common.LayoutAwarePage
    {
        IEnumerable<AppItem> sampleApps;
        IEnumerable<Account> sampleAccounts;
        DownloadHelper dh = new DownloadHelper();

        public AccountApplicationsPage()
        {
            this.InitializeComponent();
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
            // TODO: Create an appropriate data model for your problem domain to replace the sample data
            SampleDataSource.currentAccount = 0; // otherwise get it from stored isolated storage
            sampleAccounts = SampleDataSource.GetAccounts();
            switchData(SampleDataSource.GetAccountByIndex(SampleDataSource.currentAccount).Name);

            //Debug.WriteLine(result.ToString());
        }

        private void headerMenuClicked(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("headerMenuClicked");
            // Create a menu containing two items
            var menu = new Menu();

            IEnumerator<Account> MyEnumerator = sampleAccounts.GetEnumerator();
            int i = 0;
            while (MyEnumerator.MoveNext())
            {
                Account processingAccount = MyEnumerator.Current;
                var newItem = new MenuItem { Text = processingAccount.Name, Tag = i /* processingAccount.ApiKey */ };
                newItem.Tapped += homeNavClicked;
                menu.Items.Add(newItem);
                i++;
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

        private void ParseXML(Account what)
        {
            Debug.WriteLine("Processing..."+what.xdoc.ToString());
            what.Name = what.xdoc.Root.Attribute("companyName").Value;
            var apps = from node in what.xdoc.Descendants("application") 
                       orderby node.Attribute("name").Value
                       select node;
            IEnumerator<XElement> myEnum = apps.GetEnumerator();
            while (myEnum.MoveNext())
            {
                XElement current = myEnum.Current;
                string name = current.Attribute("name").Value;
                string platform = current.Attribute("platform").Value;
                DateTime cdate = DateTime.Parse(current.Attribute("createdDate").Value);
                string appapi = current.Attribute("apiKey").Value;
                what.Apps.Add(new AppItem(
                            name,
                            platform,
                            cdate,
                            appapi
                            ));
            }
        }

        private async void switchData(String title) {
            Debug.WriteLine("switching to currentAccount:" + SampleDataSource.currentAccount);
            Debug.WriteLine("switching to ApiKey:" + SampleDataSource.GetAccountByIndex(SampleDataSource.currentAccount).ApiKey);          

            // check if it's loaded, if not - load it up

            if (!SampleDataSource.GetAccountByIndex(SampleDataSource.currentAccount).IsLoaded) // if not loaded
            {
                string callURL = "http://api.flurry.com/appInfo/getAllApplications?apiAccessCode=" + SampleDataSource.GetAccountByIndex(SampleDataSource.currentAccount).ApiKey;
                XDocument result = await dh.DownloadXML(callURL); // load it            
                //where node.Element("CountryRegion").Value.Contains("United States")
                //select node.Element("FormattedAddress").Value                           
                SampleDataSource.GetAccountByIndex(SampleDataSource.currentAccount).xdoc = result; // if we will need it in future
                ParseXML(SampleDataSource.GetAccountByIndex(SampleDataSource.currentAccount));
                SampleDataSource.GetAccountByIndex(SampleDataSource.currentAccount).IsLoaded = true;
                // SampleDataSource.GetAccountByIndex(SampleDataSource.currentAccount).Apps
            }
            sampleApps = SampleDataSource.GetAppItems(SampleDataSource.GetAccountByIndex(SampleDataSource.currentAccount).ApiKey);

            this.DefaultViewModel["Items"] = sampleApps;
            pageTitle.Text = title;
        }

        private void homeNavClicked(object sender, TappedRoutedEventArgs e)
        {
            MenuItem what = ((MenuItem)sender);
            SampleDataSource.currentAccount = (int)what.Tag;
            switchData(what.Text);
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Invoked when an item is clicked.
        /// </summary>
        /// <param name="sender">The GridView (or ListView when the application is snapped)
        /// displaying the item clicked.</param>
        /// <param name="e">Event data that describes the item clicked.</param>
        void ItemView_ItemClick(object sender, ItemClickEventArgs e)
        {
            // Navigate to the appropriate destination page, configuring the new page
            // by passing required information as a navigation parameter
            // var groupId = ((SampleDataGroup)e.ClickedItem).UniqueId;
            // this.Frame.Navigate(typeof(SplitPage), groupId);
        }
    }
}
