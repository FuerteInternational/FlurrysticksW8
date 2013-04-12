using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Flurrysticks.DataModel
{
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

}
