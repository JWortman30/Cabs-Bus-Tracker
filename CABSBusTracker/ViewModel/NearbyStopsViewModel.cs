using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using CABSBusTracker.Model;
using CABSBusTracker.Functions;

namespace CABSBusTracker.ViewModel
{
    public class NearbyStopsViewModel : INotifyPropertyChanged
    {
        public const string NearbyStopsPropertyName = "NearbyStopsResults";

        private ObservableCollection<BusStop> _NearbyStopsResults = new ObservableCollection<BusStop>();

        /// <summary>
        /// Gets the GroupedContacts property.
        /// 
        /// Changes to that property's value raise the PropertyChanged event. 
        /// This property's value is broadcasted by the Messenger's default instance when it changes.
        /// </summary>
        public ObservableCollection<BusStop> NearbyStopsResults
        {
            get
            {
                return _NearbyStopsResults;
            }
            set
            {
                if (_NearbyStopsResults == value)
                {
                    return;
                }
                var oldValue = _NearbyStopsResults;
                _NearbyStopsResults = value;
                // Update bindings, no broadcast
                NotifyPropertyChanged("NearbyStopsResults");
            }
        }

        public void AddStop(BusStop tempStop)
        {
            var query = from stop in NearbyStopsResults

                        where stop.StopNum == (tempStop.StopNum)

                        select stop;

            if (!query.Any())
            {
                NearbyStopsResults.Add(tempStop);
            }

        }
        public void ClearStops()
        {

            // Add a to-do item to the "all" observable collection.
            NearbyStopsResults.Clear();


        }

        public void sortStops()
        {
            var SortedList = (from stop in NearbyStopsResults
                              orderby stop.numDistanceFromLocation
                              select stop).ToList();
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        // Used to notify the app that a property has changed.
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

    }
}
