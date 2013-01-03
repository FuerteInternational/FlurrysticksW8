using System;
using System.Runtime.Serialization;

namespace Flurrystics.DataModel
{

    [KnownType(typeof(Flurrystics.DataModel.AccountItem))]
    [DataContractAttribute]
    class AccountItem
    {
        [DataMember()]
        public String Name { get; set; }

        [DataMember()]
        public String ApiKey { get; set; }
    }

}
