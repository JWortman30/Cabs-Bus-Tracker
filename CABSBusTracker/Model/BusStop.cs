using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Device.Location;
using System.Windows.Media;

namespace CABSBusTracker.Model
{
    public class BusStop
    {
        public string Name
        {
            get;
            set;
        }
        public int StopNum
        {
            get;
            set;
        }

        public String distanceFromLocation
        {
            get;
            set;
        }

        public Double numDistanceFromLocation
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
    }
    
}
