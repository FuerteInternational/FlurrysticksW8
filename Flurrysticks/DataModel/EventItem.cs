using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Flurrysticks.DataModel
{
    public class EventItem
    {

        // "usersLastDay", "usersLastWeek", "usersLastMonth", "avgUsersLastDay", "avgUsersLastWeek", "avgUsersLastMonth", "totalSessions", "totalCount"
        public string eventName { get; set; }
        public string eventValue { get; set; }
        /*
        public string usersLastDay { get; set; }
        public string usersLastWeek { get; set; }
        public string usersLastMonth { get; set; }
        public string avgUsersLastDay { get; set; }
        public string avgUsersLastWeek { get; set; }
        public string avgUsersLastMonth { get; set; }
        public string totalSessions { get; set; }
        public string totalCount { get; set; }
        */
    }

}
