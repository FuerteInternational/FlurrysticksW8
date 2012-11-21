using System;
using System.Runtime.Serialization;

namespace Flurrysticks.DataModel
{

    [KnownType(typeof(Flurrysticks.DataModel.AccountItem))]
    [DataContractAttribute]
    class AccountItem
    {
        [DataMember()]
        public String Name { get; set; }

        [DataMember()]
        public String ApiKey { get; set; }
    }

}
