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
using Flurrysticks.DataModel;

// The data model defined by this file serves as a representative example of a strongly-typed
// model that supports notification when members are added, removed, or modified.  The property
// names chosen coincide with data bindings in the standard item templates.

namespace Flurrystics.Data 
{

    /// <summary>
    /// Generic data model.
    /// </summary>
    /// 

    public sealed class DataSource
    {
        private static DataSource _DataSource = new DataSource();
        public static XDocument dataEventsXML = null;

        public static ObservableCollection<AppItem> getApps()
        {
            return _DataSource.sampleApps;
        }

        public static void setApps(ObservableCollection<AppItem> what)
        {
            _DataSource.sampleApps = what;
        }

        private ObservableCollection<AppItem> _sampleApps = new ObservableCollection<AppItem>();
        public ObservableCollection<AppItem> sampleApps
        {
            get { return this._sampleApps; }
            set { _sampleApps = value; }
        }



        public static ObservableCollection<Account> getAccounts()
        {
            return _DataSource.sampleAccounts;
        }

        public static void setAccounts(ObservableCollection<Account> what)
        {
            _DataSource.sampleAccounts = what;
        }

        private ObservableCollection<Account> _sampleAccounts = new ObservableCollection<Account>();
        public ObservableCollection<Account> sampleAccounts
        {
            get { return this._sampleAccounts; }
            set { _sampleAccounts = value; }
        }

        public static IEnumerable<ChartDataPoint>[,] getChartData()
        {
            return _DataSource.ChartData;
        }

        public static void setChartData(IEnumerable<ChartDataPoint> what, int index, int series)
        {
            _DataSource.ChartData[index,series] = what;
        }

        public static void clearChartData()
        {
            Array.Clear(_DataSource.ChartData,0,_DataSource.ChartData.Length);
            dataEventsXML = null;
        }

        private IEnumerable<ChartDataPoint>[,] _ChartData = new IEnumerable<ChartDataPoint>[8,2];
        public IEnumerable<ChartDataPoint>[,] ChartData
        {
            get { return this._ChartData; }
            set { _ChartData = value; }
        }

    }

    /*
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
