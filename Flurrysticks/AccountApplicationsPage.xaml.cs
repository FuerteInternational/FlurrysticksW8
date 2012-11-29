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
using System.Xml.Serialization;
using System.Threading.Tasks;
using Windows.Storage;
using System.Runtime.Serialization;
using Flurrysticks.DataModel;

// The Items Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234233

namespace Flurrysticks
{
    /// <summary>
    /// A page that displays a collection of item previews.  In the Split Application this page
    /// is used to display and select one of the available groups.
    /// </summary>
    public sealed partial class AccountApplicationsPage : Flurrysticks.Common.LayoutAwarePage
    {
        ObservableCollection<AppItem> sampleApps;
        ObservableCollection<Account> sampleAccounts;
        int currentAccount;
        DownloadHelper dh = new DownloadHelper();

        public AccountApplicationsPage()
        {
            this.InitializeComponent();
        }

        private async static Task<StorageFile> GetFile(string path)
        {
            var folder = ApplicationData.Current.LocalFolder;
            try
            {
                return await folder.GetFileAsync(path);
            }
            catch (FileNotFoundException)
            {
                return null;
            }
        }

        private async static Task<bool> FileExists(string path)
        {
            var folder = ApplicationData.Current.LocalFolder;
            try
            {
                var file = await folder.GetFileAsync(path);
                return true;
            }
            catch (FileNotFoundException)
            {
                return false;
            }
        }

        private async static Task<Stream> WriteFile(string path)
        {
            var folder = ApplicationData.Current.LocalFolder;
            return await folder.OpenStreamForWriteAsync(path, CreationCollisionOption.ReplaceExisting);
        }

        static readonly string ApiFileName = "apikeys.xml";

        public async void LoadApiKeyData()
        {
            //List<AccountItem> Accounts = new List<AccountItem>();
            Debug.WriteLine("LoadApiKeyData()");
            //AccountItem[] AccountsArray;
            await GetFile(ApiFileName).ContinueWith(value =>
            {
                var file = value.Result;

                if (file == null)
                {
                    Debug.WriteLine("Empty XML");
                    return;
                }

                var folder = ApplicationData.Current.LocalFolder;
                folder.OpenStreamForReadAsync(ApiFileName).ContinueWith(filevalue =>
                {
                    using (var stream = filevalue.Result)
                    {
                        DataContractSerializer serializer = new DataContractSerializer(typeof(AccountItem[]));
                        var localCats = serializer.ReadObject(stream) as AccountItem[];

                        if (localCats == null || localCats.Length == 0)
                        {
                            Debug.WriteLine("Empty XML");
                            return;
                        }
                        //AccountsArray = localCats;
                        sampleAccounts = new ObservableCollection<Account>();
                        foreach (AccountItem OneAccount in localCats)
                        {
                            sampleAccounts.Add(
                                new Account(
                                    OneAccount.Name,
                                    false,
                                    OneAccount.ApiKey
                                    )
                                );

                        }
                    }
                });
            });
                switchData(sampleAccounts.ElementAt<Account>(currentAccount).Name);
        }

        private void SaveApiKeyData()
        {
            List<AccountItem> Accounts = new List<AccountItem>();
            if (sampleAccounts == null) { return; }
            IEnumerator<Account> MyEnumerator = sampleAccounts.GetEnumerator();
            while (MyEnumerator.MoveNext())
            {
                Account processingAccount = MyEnumerator.Current;
                var newItem = new AccountItem { Name = processingAccount.Name, ApiKey = processingAccount.ApiKey };
                Accounts.Add(newItem);
            }

            WriteFile(ApiFileName).ContinueWith(opentask =>
            {
                using (var stream = opentask.Result)
                {
                    DataContractSerializer serializer = new DataContractSerializer(typeof(AccountItem[]));
                    serializer.WriteObject(stream, Accounts.ToArray());
                }
            });

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
            currentAccount = 0; // otherwise get it from stored isolated storage
            LoadApiKeyData();
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
            SaveApiKeyData(); // we got name and apikey - let's save it
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
            pageTitle.Text = what.Name;
        }

        private async void switchData(String title) {
            Debug.WriteLine("switching to currentAccount:" + currentAccount);
            Debug.WriteLine("switching to ApiKey:" + sampleAccounts.ElementAt<Account>(currentAccount).ApiKey);


            ProgressBar1.Visibility = Windows.UI.Xaml.Visibility.Visible;
            pageTitle.IsTapEnabled = false;
            pageDropDown.IsTapEnabled = false;
            // check if it's loaded, if not - load it up

            if (!sampleAccounts.ElementAt<Account>(currentAccount).IsLoaded) // if not loaded
            {
                string callURL = "http://api.flurry.com/appInfo/getAllApplications?apiAccessCode=" + sampleAccounts.ElementAt<Account>(currentAccount).ApiKey;
                XDocument result = await dh.DownloadXML(callURL); // load it                                     
                sampleAccounts.ElementAt<Account>(currentAccount).xdoc = result; // if we will need it in future
                ParseXML(sampleAccounts.ElementAt<Account>(currentAccount));
                sampleAccounts.ElementAt<Account>(currentAccount).IsLoaded = true;
            }
            else
            {
                pageTitle.Text = title;
            }

            // sampleApps = SampleDataSource.GetAppItems(SampleDataSource.GetAccountByIndex(SampleDataSource.currentAccount).ApiKey);
            sampleApps = sampleAccounts.ElementAt<Account>(currentAccount).Apps;

            this.DefaultViewModel["Items"] = sampleApps;
            ProgressBar1.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            pageTitle.IsTapEnabled = true;
            pageDropDown.IsTapEnabled = true;
            
        }

        private void homeNavClicked(object sender, TappedRoutedEventArgs e)
        {
            MenuItem what = ((MenuItem)sender);
            currentAccount = (int)what.Tag;
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

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (!logincontrol1.IsOpen)
            {

                RootPopupBorder.Width = 646;
                logincontrol1.HorizontalOffset = Window.Current.Bounds.Width - 650;
                logincontrol1.VerticalOffset = Window.Current.Bounds.Height - 400;
                logincontrol1.IsOpen = true;

            }    
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {

        }

        private void cancelClick_Click_1(object sender, RoutedEventArgs e)
        {
            if (logincontrol1.IsOpen)
            {
                logincontrol1.IsOpen = false;
            }
        }

        private void addClick_Click_1(object sender, RoutedEventArgs e)
        {

        }
    }

    [XmlRoot]
    public class ApiKeysContainer
    {
        private List<string> strings = new List<string>();
        public List<string> Strings { get { return strings; } set { strings = value; } }

        private List<string> names = new List<string>();
        public List<string> Names { get { return names; } set { names = value; } }
    }

}
