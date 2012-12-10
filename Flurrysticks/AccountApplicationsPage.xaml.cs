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
using Windows.UI.Popups;

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
        ObservableCollection<Account> sampleAccounts = new ObservableCollection<Account>();
        int currentAccount;
        DownloadHelper dh = new DownloadHelper();
        Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
        String settingsOrderBy = "name1";

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

        private void noAccountData()
        {
            ProgressBar1.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            pageTitle.IsTapEnabled = false;
            pageTitle.Text = "no account";
            pageDropDown.IsTapEnabled = false;

            // no account defined - let's open appbar and entry new api dialog

            bottomAppBar.IsOpen = true;

            if (!logincontrol1.IsOpen) // if not open - start anim
            {
                RootPopupBorder.Width = 646;
                logincontrol1.HorizontalOffset = Window.Current.Bounds.Width - 650;
                logincontrol1.VerticalOffset = Window.Current.Bounds.Height - 400;
                logincontrol1.IsOpen = true;
            }    

        }

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
                        //sampleAccounts = new ObservableCollection<Account>();
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

            try
            {
                switchData(sampleAccounts.ElementAt<Account>(currentAccount).Name);
            }
            catch (System.ArgumentOutOfRangeException)
            {
                Debug.WriteLine("No account");
                noAccountData();
            }

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
            try
            {
                settingsOrderBy = (String)localSettings.Values["settingsOrderBy"];
            }
            catch (System.NullReferenceException)
            {
                settingsOrderBy = "name1";
            }
            if (settingsOrderBy == null) { settingsOrderBy = "name1"; }
            try
            {
                currentAccount = (int)localSettings.Values["currentAccount"]; ; // otherwise get it from stored isolated storage
            }
            catch (System.NullReferenceException)
            {
                currentAccount = 0;
            }
            LoadApiKeyData();

            // if (!(sampleAccounts.ToList().Count>0)) { noAccountData(); }  // if load failed / no account data available
            
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

        private bool ParseXML(Account what)
        {
            Debug.WriteLine("Processing..."+what.xdoc.ToString());
            bool result = false;
            if (what.xdoc.Root.Element("error") == null)
            {
                what.Name = what.xdoc.Root.Attribute("companyName").Value;
                SaveApiKeyData(); // we got name and apikey - let's save it

                String orderValue = "name";
                if (settingsOrderBy.Contains("name"))
                {
                    orderValue = "name";
                }
                if (settingsOrderBy.Contains("createddate"))
                {
                    orderValue = "createdDate";
                }
                if (settingsOrderBy.Contains("platform"))
                {
                    orderValue = "platform";
                }
                Debug.WriteLine("orderValue:"+orderValue);
                IEnumerable<XElement> apps;
                if (settingsOrderBy.Contains("1")) // ascending
                {
                        Debug.WriteLine("order ASCENDING");
                        apps = from node in what.xdoc.Descendants("application")
                               orderby node.Attribute(orderValue).Value ascending
                               select node;

                }
                else // descending
                {
                        Debug.WriteLine("order DESCENDING");
                        apps = from node in what.xdoc.Descendants("application")
                               orderby node.Attribute(orderValue).Value descending
                               select node;
                }

                IEnumerator<XElement> myEnum = apps.GetEnumerator();
                what.Apps.Clear(); // reset before filling new data
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
                result = true;
            }
            return result;
        } // ParseXML

        private async void switchData(String title) {

            if (sampleAccounts.ElementAt<Account>(currentAccount) == null) { return; }

            Debug.WriteLine("switching to currentAccount:" + currentAccount);
            Debug.WriteLine("switching to ApiKey:" + sampleAccounts.ElementAt<Account>(currentAccount).ApiKey);

            ProgressBar1.Visibility = Windows.UI.Xaml.Visibility.Visible;
            pageTitle.IsTapEnabled = false;
            pageDropDown.IsTapEnabled = false;
            // check if it's loaded, if not - load it up
            bool success;
            bool retry = true;
            XDocument result = null;

            while (retry)
            {

                if (!sampleAccounts.ElementAt<Account>(currentAccount).IsLoaded) // if not loaded
                {
                    string callURL = "http://api.flurry.com/appInfo/getAllApplications?apiAccessCode=" + sampleAccounts.ElementAt<Account>(currentAccount).ApiKey;
                    try
                    {
                        result = await dh.DownloadXML(callURL); // load it   
                        success = true;
                    }
                    catch (System.Net.Http.HttpRequestException)
                    { // load failed
                        success = false;
                    }

                    if (success) // data OK
                    {
                        sampleAccounts.ElementAt<Account>(currentAccount).xdoc = result; // if we will need it in future
                        if (ParseXML(sampleAccounts.ElementAt<Account>(currentAccount)))
                        {
                            sampleAccounts.ElementAt<Account>(currentAccount).IsLoaded = true;
                        }
                        else success = false;
                    }

                    if (!success)  // data not OK
                    {
                        var messageDialog = new Windows.UI.Popups.MessageDialog("Unable to fetch data; either API access key is incorrect or something's wrong with internet connection.", "Load data fail");
                        retry = false;
                        // clear all account, which weren't loaded EVER
                        if (sampleAccounts.ElementAt<Account>(currentAccount).Name == "Loading...")
                        {
                            sampleAccounts.RemoveAt(currentAccount);
                            retry = true;
                            if (currentAccount > sampleAccounts.ToList().Count - 1)
                            { // if currentAccount pointer is after the last item
                                currentAccount = currentAccount - 1;
                                localSettings.Values["currentAccount"] = currentAccount;
                                retry = true;
                            }
                            title = sampleAccounts.ElementAt<Account>(currentAccount).Name; // update title for next round (retry=true) 
                            SaveApiKeyData(); // we better save it if next account data is cached
                        }
                        await messageDialog.ShowAsync();
                    }
                    else retry = false;

                }
                else
                {
                    pageTitle.Text = title;
                    retry = false;
                }

            } // retry

            // sampleApps = SampleDataSource.GetAppItems(SampleDataSource.GetAccountByIndex(SampleDataSource.currentAccount).ApiKey);
            sampleApps = sampleAccounts.ElementAt<Account>(currentAccount).Apps;

            this.DefaultViewModel["Items"] = sampleApps;
            ProgressBar1.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            pageTitle.IsTapEnabled = true;
            pageDropDown.IsTapEnabled = true;
            
        }

        private void sortNavClicked(object sender, TappedRoutedEventArgs e)
        {
            MenuItem what = ((MenuItem)sender);
            String sortModeClicked = (String)what.Tag;
            if (settingsOrderBy == sortModeClicked)
            { // if it is the same order just reverse direction
                if (sortModeClicked.Contains("1"))
                {
                    sortModeClicked = sortModeClicked.Replace("1", "2");
                }
                else
                {
                    sortModeClicked = sortModeClicked.Replace("2", "1");
                }
            }

            settingsOrderBy = sortModeClicked;
            localSettings.Values["settingsOrderBy"] = settingsOrderBy;

            try
            {
                // switchData(sampleAccounts.ElementAt<Account>(currentAccount).Name);
                if (sampleAccounts.ToList().Count > 0)
                {
                    ParseXML(sampleAccounts.ElementAt<Account>(currentAccount)); // -> REORDER
                }
            }
            catch (System.ArgumentOutOfRangeException)
            {
                Debug.WriteLine("No account");
                noAccountData();
            }

        }

        private void homeNavClicked(object sender, TappedRoutedEventArgs e)
        {
            MenuItem what = ((MenuItem)sender);
            currentAccount = (int)what.Tag;
            localSettings.Values["currentAccount"] = currentAccount;
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
            Debug.WriteLine(((AppItem)e.ClickedItem).AppApiKey);
            CallApp what = new CallApp();
            what.AppApiKey = ((AppItem)e.ClickedItem).AppApiKey;
            what.ApiKey = sampleAccounts.ElementAt<Account>(currentAccount).ApiKey;
            this.Frame.Navigate(typeof(AppMetrics), what);
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var messageDialog = new Windows.UI.Popups.MessageDialog("Are you Sure you want to remove account \"" + sampleAccounts.ElementAt<Account>(currentAccount).Name + "\" ?","Please confirm");

            // Add commands and set their callbacks

            messageDialog.Commands.Add(new UICommand("Yes", (command) =>
            {
                // what happens when Yes is selected
                // remove account
                int removeAccount = currentAccount;
                if (currentAccount > sampleAccounts.ToList().Count - 2)
                { // if currentAccount pointer is after the last item
                    currentAccount = currentAccount - 1;
                    localSettings.Values["currentAccount"] = currentAccount;
                }
                sampleAccounts.RemoveAt(removeAccount);
                switchData(sampleAccounts.ElementAt<Account>(currentAccount).Name);

            }));

            messageDialog.Commands.Add(new UICommand("No", (command) =>
            {
                // what happens when No is selected
                // - nothing
            }));

            // Set the command that will be invoked by default
            messageDialog.DefaultCommandIndex = 1;
            // Show the message dialog
            await messageDialog.ShowAsync();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e) // hitting adding new account
        {
            if (!logincontrol1.IsOpen) // if not open - start anim
            {
                RootPopupBorder.Width = 646;
                logincontrol1.HorizontalOffset = Window.Current.Bounds.Width - 650;
                logincontrol1.VerticalOffset = Window.Current.Bounds.Height - 400;
                logincontrol1.IsOpen = true;
            }    
        }

        private void Button_Click_3(object sender, RoutedEventArgs e) // filter
        {

            var menu = new Menu();
            MenuItem newItem;
            newItem = new MenuItem { Text = "Name", Tag = "name1" };
            newItem.Tapped += sortNavClicked;
            menu.Items.Add(newItem);
            newItem = new MenuItem { Text = "Platform", Tag = "platform1" };
            newItem.Tapped += sortNavClicked;
            menu.Items.Add(newItem);
            newItem = new MenuItem { Text = "Created Date", Tag = "createddate1" };
            newItem.Tapped += sortNavClicked;
            menu.Items.Add(newItem);
            // Show the menu in a flyout anchored to the header title
            var flyout = new Flyout();
            flyout.Placement = PlacementMode.Top;
            flyout.HorizontalAlignment = HorizontalAlignment.Right;
            flyout.HorizontalContentAlignment = HorizontalAlignment.Left;
            flyout.PlacementTarget = sortButton;
            flyout.Content = menu;
            flyout.IsOpen = true;

        }

        private void cancelClick_Click_1(object sender, RoutedEventArgs e)
        {
            if (logincontrol1.IsOpen)
            {
                Debug.WriteLine("Cancel removal new API key");
                logincontrol1.IsOpen = false;
            }
        }

        private async void addClick_Click_1(object sender, RoutedEventArgs e)
        {
            if (logincontrol1.IsOpen)
            {
                // first check if we have 20-chars (that's standard flurry API access key length)
                if (flurry_api_access.Text.Length != 20)
                {
                    var messageDialog = new Windows.UI.Popups.MessageDialog("Please enter valid Flurry API access key", "Flurry API access key incorrect");
                    await messageDialog.ShowAsync();
                }
                else // it's 20-char string - PROCEEED
                {

                    Debug.WriteLine("Adding new API key");
                    sampleAccounts.Add(
                        new Account(
                            "Loading...",
                            false,
                            flurry_api_access.Text
                            )
                        );
                    logincontrol1.IsOpen = false;
                    currentAccount = sampleAccounts.ToList<Account>().Count - 1;
                    localSettings.Values["currentAccount"] = currentAccount;
                    switchData(sampleAccounts.ElementAt<Account>(currentAccount).Name);
                }
            } // logincontrol1.IsOpen
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

    public class CallApp
    {
        public string AppApiKey;
        public string ApiKey;


    }

}
