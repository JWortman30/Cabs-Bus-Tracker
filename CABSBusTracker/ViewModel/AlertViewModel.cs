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
    public class AlertViewModel : INotifyPropertyChanged
    {
        public const string AlertsPropertyName = "AlertResults";

        private ObservableCollection<Alert> _AlertResults = new ObservableCollection<Alert>();

        /// <summary>
        /// Gets the GroupedContacts property.
        /// 
        /// Changes to that property's value raise the PropertyChanged event. 
        /// This property's value is broadcasted by the Messenger's default instance when it changes.
        /// </summary>
        public ObservableCollection<Alert> AlertResults
        {
            get
            {
                return _AlertResults;
            }
            set
            {
                if (_AlertResults == value)
                {
                    return;
                }
                var oldValue = _AlertResults;
                _AlertResults = value;
                // Update bindings, no broadcast
                NotifyPropertyChanged("AlertResults");
            }
        }

        public void AddAlert(Alert tempAlert)
        {
            var query = from alert in AlertResults

                        where alert.Title.Contains(tempAlert.Title)

                        select alert;

            if (!query.Any())
            {
                AlertResults.Add(tempAlert);
            }
            
        }
        public void ClearAlerts()
        {

            // Add a to-do item to the "all" observable collection.
            AlertResults.Clear();


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
