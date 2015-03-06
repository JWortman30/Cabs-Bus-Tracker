using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CABSBusTracker.Model
{
    public class ArrivalTime
    {
        public string RouteName { get; set; }

        public string Destination { get; set; }

        public string ExpectedTime { get; set; }

        public string busID { get; set; }

        public string routeColor { get; set; }

        public int expectedTimeMinutes { get; set; }        

    }
}
