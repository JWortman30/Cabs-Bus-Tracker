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
    public class ArrivalTimeGroupedViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// The <see cref="GroupedContacts" /> property's name.
        /// </summary>
        public const string GroupedContactsPropertyName = "GroupedArrivalTimes";

        private ObservableCollection<GroupedOC<ArrivalTime>> _GroupedArrivalTimes = new ObservableCollection<GroupedOC<ArrivalTime>>();

        /// <summary>
        /// Gets the GroupedContacts property.
        /// 
        /// Changes to that property's value raise the PropertyChanged event. 
        /// This property's value is broadcasted by the Messenger's default instance when it changes.
        /// </summary>
        public ObservableCollection<GroupedOC<ArrivalTime>> GroupedArrivalTimes
        {
            get
            {
                return _GroupedArrivalTimes;
            }
            set
            {
                if (_GroupedArrivalTimes == value)
                {
                    return;
                }
                var oldValue = _GroupedArrivalTimes;
                _GroupedArrivalTimes = value;
                // Update bindings, no broadcast
                NotifyPropertyChanged("GroupedArrivalTimes");
            }
        }

        public void InsertBusTimes(ObservableCollection<ArrivalTime> tempBusTimes)
        {

            // Add a to-do item to the "all" observable collection.
            GroupedArrivalTimes = CollectionHelpers.CreateGroupedOC(tempBusTimes);


        }
        public void ClearArrivalTimes()
        {

            // Add a to-do item to the "all" observable collection.
            GroupedArrivalTimes.Clear();


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
