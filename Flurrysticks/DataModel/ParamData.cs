using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Flurrysticks.DataModel
{
    public class ParamData // all parameters w/ keys
    {
        public string key { get; set; }
        public IEnumerable<XElement> content { get; set; }
        public System.Collections.IEnumerable children { get; set; }
    }
}
