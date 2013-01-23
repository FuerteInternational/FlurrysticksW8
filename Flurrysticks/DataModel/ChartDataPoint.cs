using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flurrysticks.DataModel
{
    public class ChartDataPoint
    {
        public string Label { get; set; }
        public double Value { get; set; }  // appmetrics
        public double Value1 { get; set; } // Eventmetrics
        public double Value2 { get; set; } // Eventmetrics
        public double Value3 { get; set; } // Eventmetrics
    }
}
