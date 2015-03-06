using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Device.Location;
using System.Windows.Media;

namespace CABSBusTracker.Model
{
    public class Bus
    {

        public int BusNum
        {
            get;
            set;
        }
        public string BusRoute
        {
            get;
            set;
        }

        public GeoCoordinate location
        {
            get;
            set;
        }

        public String stopColor
        {
            get;
            set;
        }

        public String StopName
        {
            get;
            set;
        }
    }
}
