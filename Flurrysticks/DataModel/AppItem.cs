using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flurrysticks.DataModel
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
}
