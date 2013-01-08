using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel.Resources.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using System.Collections.Specialized;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.IO;
using System.Runtime.Serialization;

// The data model defined by this file serves as a representative example of a strongly-typed
// model that supports notification when members are added, removed, or modified.  The property
// names chosen coincide with data bindings in the standard item templates.
//
// Applications may use this model as a starting point and build on it, or discard it entirely and
// replace it with something appropriate to their needs.

namespace Flurrystics.Data 
{
    /// <summary>
    /// Generic item data model.
    /// </summary>
    public class AppItem : Flurrystics.Common.BindableBase
    {
        public AppItem(String Name, String Platform, DateTime createdDate, String AppApiKey, String Image)
        {
            this._Name = Name;
            this._AppApiKey = AppApiKey;
            this._Platform = Platform;
            this._CreatedDate = CreatedDate; 
            this._Image = Image;
        }

        private string _Name = string.Empty;
        public string Name
        {
            get { return this._Name; }
            set { this.SetProperty(ref this._Name, value); }
        }

        private string _AppApiKey = string.Empty;
        public string AppApiKey
        {
            get { return this._AppApiKey; }
            set { this.SetProperty(ref this._AppApiKey, value); }
        }

        private string _Platform = string.Empty;
        public string Platform
        {
            get { return this._Platform; }
            set { this.SetProperty(ref this._Platform, value); }
        }

        private string _Image = string.Empty;
        public string Image
        {
            get { return this._Image; }
            set { this.SetProperty(ref this._Image, value); }
        }

        private DateTime _CreatedDate = DateTime.Now;
        public DateTime CreatedDate
        {
            get { return this._CreatedDate; }
            set { this.SetProperty(ref this._CreatedDate, value); }
        }
    }

    public class TimeRange
    {
        public DateTime StartTime
        {
            get;
            set;
        }

        public DateTime EndTime
        {
            get;
            set;
        }
    }


    /// <summary>
    /// Generic group data model.
    /// </summary>
    /// 
    public class Account : Flurrystics.Common.BindableBase
    {
        public Account(String Name, bool isLoaded, String ApiKey)
        {
            //Items.CollectionChanged += ItemsCollectionChanged;
            this._Name = Name;
            this._ApiKey = ApiKey;
            this._IsLoaded = isLoaded;
        }

        private string _Name = string.Empty;
        public string Name
        {
            get { return this._Name; }
            set { this.SetProperty(ref this._Name, value); }
        }

        private bool _IsLoaded = false;
        public bool IsLoaded
        {
            get { return this._IsLoaded; }
            set { this.SetProperty(ref this._IsLoaded, value); }
        }

        private string _ApiKey = string.Empty;
        public string ApiKey
        {
            get { return this._ApiKey; }
            set { this.SetProperty(ref this._ApiKey, value); }
        }

        private XDocument _xdoc = null;
        public XDocument xdoc
        {
            get { return this._xdoc; }
            set { this.SetProperty(ref this._xdoc, value); }
        }

        private ObservableCollection<AppItem> _Apps = new ObservableCollection<AppItem>();
        public ObservableCollection<AppItem> Apps
        {
            get { return this._Apps; }
        }
    }

    /// <summary>
    /// Creates a collection of groups and items with hard-coded content.
    /// 
    /// SampleDataSource initializes with placeholder data rather than live production
    /// data so that sample data is provided at both design-time and run-time.
    /// </summary>
    /// 
    /*
    public sealed class SampleDataSource
    {

        public static int currentAccount = 0;

        private static SampleDataSource _sampleDataSource = new SampleDataSource();

        private ObservableCollection<Account> _allAccounts = new ObservableCollection<Account>();
        public ObservableCollection<Account> AllAccounts
        {
            get { return this._allAccounts; }
        }

        public static IEnumerable<Account> GetAccounts()
        {           
            return _sampleDataSource.AllAccounts;
        }

        public static Account GetAccount(string ApiKey)
        {
            // Simple linear search is acceptable for small data sets
            var matches = _sampleDataSource.AllAccounts.Where((group) => group.ApiKey.Equals(ApiKey));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public static Account GetAccountByIndex(int i)
        {
            if (_sampleDataSource.AllAccounts.Count > 0)
            {
                return _sampleDataSource.AllAccounts.ElementAt<Account>(i);
            }
            else return null;
        }

        public static AppItem GetAppItem(string AppApiKey)
        {
            // Simple linear search is acceptable for small data sets
            var matches = _sampleDataSource.AllAccounts.SelectMany(group => group.Apps).Where((item) => item.AppApiKey.Equals(AppApiKey));
            if (matches.Count() == 1) return matches.First();
            return null;
        }


        public static ObservableCollection<AppItem> GetAppItems(string ApiKey)
        {
            Account requestedAccount = GetAccount(ApiKey);
            // Simple linear search is acceptable for small data sets
            // var matches = requestedAccount.Apps.SelectMany(group => group.Apps).Where((item) => item.AppApiKey.Equals(AppApiKey));
            return requestedAccount.Apps;
        }
        */

     //public SampleDataSource()
     //{
         /*
                  var account1 = new Account(
                          "loading...",
                          false,
                          "DJBUBP9NE5YBQB5CQKH3"
                          );
         
                  // String Name, String Platform, DateTime createdDate, String AppApiKey
                  account1.Apps.Add(
                      new AppItem(
                          "Test App 1",
                          "Android",
                          DateTime.Now,
                          "HXCWZ1L3CWMVGQM68JPI"));
                  account1.Apps.Add(
                      new AppItem(
                          "Test App 2",
                          "Android",
                          DateTime.Now,
                          "HXCWZ1L3CWMVGQM68JPI"));
                  account1.Apps.Add(
                      new AppItem(
                          "Test App 3",
                          "Android",
                          DateTime.Now,
                          "HXCWZ1L3CWMVGQM68JPI"));
                              account1.Apps.Add(
                      new AppItem(
                          "Test App 4",
                          "Android",
                          DateTime.Now,
                          "HXCWZ1L3CWMVGQM68JPI"));
                   * */
        /*
         this.AllAccounts.Add(account1);

         var account2 = new Account(
                 "loading...",
                 false,
                 "WSN22PRKRZH4B6RKFPZN"
                 );
         */
         /*
         // String Name, String Platform, DateTime createdDate, String AppApiKey
         account2.Apps.Add(
             new AppItem(
                 "Test2 App 1",
                 "Android",
                 DateTime.Now,
                 "HXCWZ1L3CWMVGQM68JPI"));
         account2.Apps.Add(
             new AppItem(
                 "Test2 App 2",
                 "Android",
                 DateTime.Now,
                 "HXCWZ1L3CWMVGQM68JPI"));
         account2.Apps.Add(
             new AppItem(
                 "Test2 App 3",
                 "Android",
                 DateTime.Now,
                 "HXCWZ1L3CWMVGQM68JPI"));
         account2.Apps.Add(
             new AppItem(
                 "Test2 App 4",
                 "Android",
                 DateTime.Now,
                 "HXCWZ1L3CWMVGQM68JPI"));
         account2.Apps.Add(
              new AppItem(
                  "Test2 App 5",
                  "Android",
                  DateTime.Now,
                  "HXCWZ1L3CWMVGQM68JPI"));
         account2.Apps.Add(
             new AppItem(
                 "Test2 App 6",
                 "Android",
                 DateTime.Now,
                 "HXCWZ1L3CWMVGQM68JPI"));
         account2.Apps.Add(
             new AppItem(
                 "Test2 App 7",
                 "Android",
                 DateTime.Now,
                 "HXCWZ1L3CWMVGQM68JPI"));
         account2.Apps.Add(
             new AppItem(
                 "Test2 App 8",
                 "Android",
                 DateTime.Now,
                 "HXCWZ1L3CWMVGQM68JPI"));
          * */
         //this.AllAccounts.Add(account2);

            

     //}
         
    //}
}
