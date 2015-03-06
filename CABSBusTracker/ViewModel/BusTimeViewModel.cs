using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;

using CABSBusTracker.Model;


namespace CABSBusTracker.ViewModel
{
    public class BusTimeViewModel : INotifyPropertyChanged
    {
        // LINQ to SQL data context for the local database.
        private BusTimeDataContext busTimeDB;

        // Class constructor, create the data context object.
        public BusTimeViewModel(string busTimeDBConnectionString)
        {
            busTimeDB = new BusTimeDataContext(busTimeDBConnectionString);
        }

        //A list of all the bus time items.
        private ObservableCollection<BusTimeItem> _busTimeItems;
        public ObservableCollection<BusTimeItem> BusTimeItems
        {
            get { return _busTimeItems; }
            set
            {
                _busTimeItems = value;
                NotifyPropertyChanged("BusTimeItems");
            }
        }

        // Query database and load the collections and list used by the pivot pages.
        public void LoadBusTimesFromDatabase()
        {

            // Specify the query for all to-do items in the database.
            var busTimeItemsInDB = from BusTimeItem bustime in busTimeDB.Items
                                select bustime;

            // Query the database and load all to-do items.
            BusTimeItems = new ObservableCollection<BusTimeItem>(busTimeItemsInDB);
        }

        // Add a to-do item to the database and collections.
        public void AddBusTimeItem(BusTimeItem newBusTimeItem)
        {
            // Add a to-do item to the data context.
            busTimeDB.Items.InsertOnSubmit(newBusTimeItem);

            // Save changes to the database.
            busTimeDB.SubmitChanges();

            // Add a to-do item to the "all" observable collection.
            BusTimeItems.Add(newBusTimeItem);

        }

        public void ClearBusTimeItems()
        {
            busTimeDB.Items.DeleteAllOnSubmit(busTimeDB.Items);

            busTimeDB.SubmitChanges();

            BusTimeItems.Clear();
        }

        // Write changes in the data context to the database.
        public void SaveChangesToDB()
        {
            busTimeDB.SubmitChanges();
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
