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
using System.Diagnostics;

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
        public static ObservableCollection<EventItem> ParamKeys = new ObservableCollection<EventItem>();
        public static XDocument dataParametersXML = null;
        public static int flipViewPosition = 0;

        public static ObservableCollection<AppItem> getApps()
        {
            return _DataSource.sampleApps; 
        }

        public static void setApps(ObservableCollection<AppItem> what)
        {
            _DataSource.sampleApps = what;
        }

        public static ObservableCollection<AppItem> getFavApps()
        {
            return _DataSource.sampleFavApps;
        }

        public static void setFavApps(ObservableCollection<AppItem> what)
        {
            _DataSource.sampleFavApps = what;
        }

        private ObservableCollection<AppItem> _sampleApps = new ObservableCollection<AppItem>();
        public ObservableCollection<AppItem> sampleApps
        {
            get { return this._sampleApps; }
            set { _sampleApps = value; }
        }

        private ObservableCollection<AppItem> _sampleFavApps = new ObservableCollection<AppItem>();
        public ObservableCollection<AppItem> sampleFavApps
        {
            get { return this._sampleFavApps; }
            set { _sampleFavApps = value; }
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
            //dataParametersXML = null;
            ParamKeys.Clear();
            dataEventsXML = null;
        }

        public static void clearChartDataEvents() // only clearing events-specific data
        {
            try
            {
                _DataSource.ChartData[8, 0] = null;
                _DataSource.ChartData[8, 1] = null;
            }
            catch (System.IndexOutOfRangeException)
            {
                Debug.WriteLine("nothing to clear");
            }
        }

        private IEnumerable<ChartDataPoint>[,] _ChartData = new IEnumerable<ChartDataPoint>[8+1, 2]; // first 8 = appmetrics; next 1 = eventmetrics Value1/Value2/Value3
        public IEnumerable<ChartDataPoint>[,] ChartData
        {
            get { return this._ChartData; }
            set { _ChartData = value; }
        }

    }

}
