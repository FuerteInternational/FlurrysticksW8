using Flurrystics.Data;
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
using Flurrystics.DataModel;
using Windows.UI.Popups;
using Flurrysticks.DataModel;
using Windows.UI.Input;

// The Items Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234233

namespace Flurrystics
{
    /// <summary>
    /// A page that displays a collection of item previews.  In the Split Application this page
    /// is used to display and select one of the available groups.
    /// </summary>
    public sealed partial class AccountApplicationsPage : Flurrystics.Common.LayoutAwarePage
    {
        int currentAccount;
        DownloadHelper dh = new DownloadHelper();
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        String settingsOrderBy = "name1";
        static readonly string ApiFileName = "apikeys.xml";
        /*
        int fingerCount = 0;
        int currentOffset;
        PointerPoint fingerStart, fingerMove;
        bool isTwoFinger = false;
        ScrollViewer scrollViewer;
        */
          
        public AccountApplicationsPage()
        {
            this.InitializeComponent();
        }

        private async static Task<StorageFile> GetFile(string path)
        {
            var folder = ApplicationData.Current.RoamingFolder;
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
            var folder = ApplicationData.Current.RoamingFolder;
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
            var folder = ApplicationData.Current.RoamingFolder;
            return await folder.OpenStreamForWriteAsync(path, CreationCollisionOption.ReplaceExisting);
        }

        private void OpenInsertAccountFlyout()
        {
            if (!logincontrol1.IsOpen) // if not open - start anim
            {
                RootPopupBorder.Width = 320;
                logincontrol1.HorizontalOffset = Window.Current.Bounds.Width - 320;
                logincontrol1.VerticalOffset = Window.Current.Bounds.Height - 420;
                logincontrol1.IsOpen = true;
            }    
        }

        private void noAccountData()
        {
            Debug.WriteLine("noAccountData");
            if (!(DataSource.getApps().Count > 0))
            {
                ProgressBar1.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                //pageTitle2.IsTapEnabled = false;
                pageTitle2.Text = "no account";
                //pageDropDown.IsTapEnabled = false;
                // no account defined - let's open appbar and entry new api dialog
                bottomAppBar.IsOpen = false;
                OpenInsertAccountFlyout();
            }
        }

        public async void LoadApiKeyData()
        {

            if (DataSource.getAccounts().Count() == 0)
            { // if empty - try...

                //List<AccountItem> Accounts = new List<AccountItem>();
                Debug.WriteLine("LoadApiKeyData()");
                //AccountItem[] AccountsArray;
                StorageFile x = await GetFile(ApiFileName);

                var file = x;

                if (file == null)
                {
                    Debug.WriteLine("Empty XML");
                    noAccountData();
                    return;
                }

                var folder = ApplicationData.Current.RoamingFolder;
                Stream filevalue = await folder.OpenStreamForReadAsync(ApiFileName);

                Debug.WriteLine("OpenStreamForReadAsync()");
                using (var stream = filevalue)
                {
                    Debug.WriteLine("filevalue.Result");
                    DataContractSerializer serializer = new DataContractSerializer(typeof(AccountItem[]));
                    var localCats = serializer.ReadObject(stream) as AccountItem[];
                    Debug.WriteLine("localCats length:" + localCats.Length);
                    if (localCats == null || localCats.Length == 0)
                    {
                        Debug.WriteLine("Empty XML");
                        noAccountData();
                        return;
                    }
                    //AccountsArray = localCats;
                    //sampleAccounts = new ObservableCollection<Account>();
                    DataSource.getAccounts().Clear();
                    foreach (AccountItem OneAccount in localCats)
                    {
                        DataSource.getAccounts().Add(
                            //sampleAccounts.Add( 
                            new Account(
                                OneAccount.Name,
                                false,
                                OneAccount.ApiKey
                                )
                            );
                    }
                }

                Debug.WriteLine("After await");

            }

            try
            {
                switchData(DataSource.getAccounts().ElementAt<Account>(currentAccount).Name);
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
            if (DataSource.getAccounts() == null) { return; }
            IEnumerator<Account> MyEnumerator = DataSource.getAccounts().GetEnumerator();
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
/*
        protected override void OnPointerPressed(PointerRoutedEventArgs e)
        {
            Debug.WriteLine("OnPointerPressed");
            itemGridView.CapturePointer(e.Pointer);
            Debug.WriteLine("PointerID:" + e.Pointer.PointerId);
            fingerCount++;

            isTwoFinger = fingerCount == 1;

            
            if (!isTwoFinger)
            {
                Debug.WriteLine("NOT two finger");
                return;
            }
            

            Debug.WriteLine("Is two finger");

            fingerStart = e.GetCurrentPoint(itemGridView);
            scrollViewer = GetVisualChild<ScrollViewer>(itemGridView);
        }

        private T GetVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            T child = default(T);
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                DependencyObject v = (DependencyObject)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                    child = GetVisualChild<T>(v);
                if (child != null)
                    break;
            }
            return child;
        } 

        protected override void OnPointerMoved(PointerRoutedEventArgs e)
        {
            if (!isTwoFinger) return;

            fingerMove = e.GetCurrentPoint(itemGridView);

            double fingerMovement = fingerStart.Position.X - fingerMove.Position.Y;

            Debug.WriteLine("fingerMovement is " + fingerMovement);

            scrollViewer.UpdateLayout();
            scrollViewer.ScrollToVerticalOffset(currentOffset + fingerMovement);

        }

        protected override void OnPointerReleased(PointerRoutedEventArgs e)
        {
            Debug.WriteLine("OnPointerReleased");
            fingerCount = 0;
        }

        protected override void OnPointerCanceled(PointerRoutedEventArgs e)
        {
            OnPointerReleased(e);
        }

        protected override void OnPointerCaptureLost(PointerRoutedEventArgs e)
        {
            OnPointerReleased(e);
        }
        */
        private void initApp()
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
                currentAccount = (int)localSettings.Values["currentAccount"];  // otherwise get it from stored isolated storage
            }
            catch (System.NullReferenceException)
            {
                currentAccount = 0;
            }

            //if (DataSource.getAccounts().Count() == 0)
            //{ // if no account then try to deserialize
                LoadApiKeyData();
            //}
        
        }   
        
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            Debug.WriteLine("AccountApplicationsPage - LoadState");
            initApp();           
        }
         /*
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            
            base.OnNavigatedTo(e);
            Debug.WriteLine("AccountApplicationsPage - OnNavigatedTo");
            initApp();
        }
       
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            //initApp();
            base.OnNavigatedFrom(e);

        }
          * */

        private string getIconFileForPlatform(string input)
        {
            string output = "Assets/flu_device_8_ios.png";
            switch (input)
            {
                case "iPhone":
                case "iPad":
                    output = "Assets/flu_device_8_ios.png"; 
                    break;
                case "Android":
                    output = "Assets/flu_device_8_android.png";
                    break;
                case "WindowsPhone":
                    output = "Assets/flurryst_icon_windows.png";
                    break;
                case "BlackberrySDK":
                    output = "Assets/flurryst_icon_blackberry.png";
                    break;
                case "JavaMESDK":
                    output = "Assets/flurryst_icon_java.png";
                    break;

            }
            return output;
        }

        private void headerMenuClicked(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("headerMenuClicked");
            // Create a menu containing two items
            if (DataSource.getAccounts().Count > 0)
            { // if any account is present
                var menu = new Menu();
                IEnumerator<Account> MyEnumerator = DataSource.getAccounts().GetEnumerator();
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
            else
            { // no account present - display flyout instead
                OpenInsertAccountFlyout();
            }
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
                                appapi,
                                getIconFileForPlatform(platform)
                                ));
                }
                pageTitle2.Text = what.Name;
                result = true;
            }
            return result;
        } // ParseXML

        private async void switchData(String title) {

            if (DataSource.getAccounts().ElementAt<Account>(currentAccount) == null) { return; }

            Debug.WriteLine("switching to currentAccount:" + currentAccount);
            Debug.WriteLine("switching to ApiKey:" + DataSource.getAccounts().ElementAt<Account>(currentAccount).ApiKey);

            ProgressBar1.Visibility = Windows.UI.Xaml.Visibility.Visible;
            // check if it's loaded, if not - load it up
            bool success;
            bool retry = true;
            XDocument result = null;

            while (retry)
            {

                if (!DataSource.getAccounts().ElementAt<Account>(currentAccount).IsLoaded) // if not loaded
                {
                    string callURL = "http://api.flurry.com/appInfo/getAllApplications?apiAccessCode=" + DataSource.getAccounts().ElementAt<Account>(currentAccount).ApiKey;
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
                        DataSource.getAccounts().ElementAt<Account>(currentAccount).xdoc = result; // if we will need it in future
                        if (ParseXML(DataSource.getAccounts().ElementAt<Account>(currentAccount)))
                        {
                            Debug.WriteLine("Data parsed OK");
                            DataSource.getAccounts().ElementAt<Account>(currentAccount).IsLoaded = true;
                            logincontrol1.IsOpen = false;
                            flurry_api_access.Text = "";
                        }
                        else success = false;
                    }

                    if (!success)  // data not OK
                    {
                        var messageDialog = new Windows.UI.Popups.MessageDialog("Unable to fetch data; either API access key is incorrect or something's wrong with your internet connection.", "Load data fail");
                        retry = false;
                        // clear all account, which weren't loaded EVER
                        if (DataSource.getAccounts().ElementAt<Account>(currentAccount).Name == "Loading...")
                        {
                            DataSource.getAccounts().RemoveAt(currentAccount);
                            retry = true;
                            if (currentAccount > DataSource.getAccounts().ToList().Count - 1)
                            { // if currentAccount pointer is after the last item
                                currentAccount = currentAccount - 1; 
                                localSettings.Values["currentAccount"] = currentAccount;
                                retry = true;
                            }
                            try
                            {
                                title = DataSource.getAccounts().ElementAt<Account>(currentAccount).Name; // update title for next round (retry=true) 
                            }
                            catch (System.ArgumentOutOfRangeException)
                            {
                                Debug.WriteLine("no account");
                                retry = false;
                            }

                            SaveApiKeyData(); // we better save it if next account data is cached
                        }
                        await messageDialog.ShowAsync(); 
                    }
                    else retry = false;

                }
                else
                {
                    pageTitle2.Text = title;
                    retry = false;
                }

            } // retry

            // sampleApps = SampleDataSource.GetAppItems(SampleDataSource.GetAccountByIndex(SampleDataSource.currentAccount).ApiKey);
            try
            {
                DataSource.setApps(DataSource.getAccounts().ElementAt<Account>(currentAccount).Apps);
            }
            catch
            {
                Debug.WriteLine("no account");
            }

            this.DefaultViewModel["Items"] = DataSource.getApps();
            
            /*
            List<GroupInfoList<object>> dataLetter = GetGroupsByCategory();
            this.DefaultViewModel["Items"] = dataLetter;
            */
              
            ProgressBar1.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            noAccountData();
            
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
                if (DataSource.getAccounts().ToList().Count > 0)
                {
                    ParseXML(DataSource.getAccounts().ElementAt<Account>(currentAccount)); // -> REORDER
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

        internal List<GroupInfoList<object>> GetGroupsByCategory()
        {
            List<GroupInfoList<object>> groups = new List<GroupInfoList<object>>();
            var query = from item in DataSource.getApps()
                        orderby ((AppItem)item).Platform
                        group item by ((AppItem)item).Platform into g
                        select new { GroupName = g.Key, Items = g };
            // reorder according to most items in group
            /*
            var newquery = from item2 in query
                           orderby item2.Items.Count() descending
                           select new { GroupName = item2.GroupName, Items = item2.Items };
            */
            foreach (var g in query)
            {
                GroupInfoList<object> info = new GroupInfoList<object>();

                    info.Key = g.GroupName + " (" + g.Items.Count() + ")";
                    foreach (var item in g.Items)
                    {
                        info.Add(item);
                    }
                    groups.Add(info);

            }
            return groups;
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
            DataSource.flipViewPosition = 0;
            Debug.WriteLine(((AppItem)e.ClickedItem).AppApiKey);
            CallApp what = new CallApp();
            what.AppApiKey = ((AppItem)e.ClickedItem).AppApiKey;
            what.ApiKey = DataSource.getAccounts().ElementAt<Account>(currentAccount).ApiKey;
            what.Name = ((AppItem)e.ClickedItem).Name;
            what.Platform = ((AppItem)e.ClickedItem).Platform;
            this.Frame.Navigate(typeof(AppMetrics), what);
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {

            if (DataSource.getAccounts().Count > 0) // only deal w/ when some accounts exists!
            {

                var messageDialog = new Windows.UI.Popups.MessageDialog("Are you Sure you want to remove account \"" + DataSource.getAccounts().ElementAt<Account>(currentAccount).Name + "\" ?", "Please confirm");

                // Add commands and set their callbacks

                messageDialog.Commands.Add(new UICommand("Yes", (command) =>
                {
                    // what happens when Yes is selected
                    // remove account
                    int removeAccount = currentAccount;
                    if (currentAccount > DataSource.getAccounts().ToList().Count - 2)
                    { // if currentAccount pointer is after the last item
                        currentAccount = currentAccount - 1;
                        localSettings.Values["currentAccount"] = currentAccount;
                    }
                    DataSource.getAccounts().RemoveAt(removeAccount);
                    switchData(DataSource.getAccounts().ElementAt<Account>(currentAccount).Name);

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
        }

        private void Button_Click_2(object sender, RoutedEventArgs e) // hitting adding new account
        {
            OpenInsertAccountFlyout();  
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
                    noAccountData();
                }
                else // it's 20-char string - PROCEEED
                {

                    Debug.WriteLine("Adding new API key");
                    DataSource.getAccounts().Add(
                        new Account(
                            "Loading...",
                            false,
                            flurry_api_access.Text
                            )
                        );
                    
                    currentAccount = DataSource.getAccounts().ToList<Account>().Count - 1;
                    localSettings.Values["currentAccount"] = currentAccount;
                    switchData(DataSource.getAccounts().ElementAt<Account>(currentAccount).Name);
                }
            } // logincontrol1.IsOpen
        }

        private void bottomAppBar_Opened_1(object sender, object e)
        {
            if (!(DataSource.getAccounts().Count() > 0))
            {
                RemoveAppBarButton.Visibility = Visibility.Collapsed;
                sortButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                RemoveAppBarButton.Visibility = Visibility.Visible;
                sortButton.Visibility = Visibility.Visible;
            }
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

    public class GroupInfoList<T> : List<object>
    {
        public object Key { get; set; }
        public new IEnumerator<object> GetEnumerator()
        {
            return (System.Collections.Generic.IEnumerator<object>)base.GetEnumerator();
        }

    }

}
