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
    public class FavoriteStopsViewModel : INotifyPropertyChanged
    {
        // LINQ to SQL data context for the local database.
        private FavoriteDataContext favoriteStopDB;

        // Class constructor, create the data context object.
        public FavoriteStopsViewModel(string favoritesDBConnectionString)
        {
            favoriteStopDB = new FavoriteDataContext(favoritesDBConnectionString);
        }

        //A list of all the bus time items.
        private ObservableCollection<FavoriteItem> _favoriteStopsItems;
        public ObservableCollection<FavoriteItem> FavoriteStopsItems
        {
            get { return _favoriteStopsItems; }
            set
            {
                _favoriteStopsItems = value;
                NotifyPropertyChanged("FavoriteStopItems");
            }
        }

        // Query database and load the collections and list used by the pivot pages.
        public void LoadFavoriteStopsFromDatabase()
        {

            // Specify the query for all to-do items in the database.
            var favoriteStopItemsInDB = from FavoriteItem favorite in favoriteStopDB.Items
                                   select favorite;

            // Query the database and load all to-do items.
            FavoriteStopsItems = new ObservableCollection<FavoriteItem>(favoriteStopItemsInDB);
        }

        // Add a to-do item to the database and collections.
        public bool AddFavoriteItem(FavoriteItem newFavoriteItem)
        {
            if (DoesDataExist(newFavoriteItem.StopName))
            {
                // Add a to-do item to the data context.
                favoriteStopDB.Items.InsertOnSubmit(newFavoriteItem);

                // Save changes to the database.
                favoriteStopDB.SubmitChanges();

                // Add a to-do item to the "all" observable collection.
                FavoriteStopsItems.Add(newFavoriteItem);

                return true;
            }
            else
            {
                return false;
            }

        }

        private bool DoesDataExist(string name)
        {
            var query = from stop in favoriteStopDB.Items

                        where stop.StopName.Contains(name)

                        select stop;


            //return false if a stop with the same name already exists

            if (query.Any())
            {
                return false;
            }
            else
            {
                return true;
            }
        }



        public void ClearFavoriteItems()
        {
            favoriteStopDB.Items.DeleteAllOnSubmit(favoriteStopDB.Items);

            favoriteStopDB.SubmitChanges();

            FavoriteStopsItems.Clear();
        }


        public void RemoveStopItem(FavoriteItem delFavoriteItem)
        {
            favoriteStopDB.Items.DeleteOnSubmit(delFavoriteItem);

            favoriteStopDB.SubmitChanges();
        }

        // Write changes in the data context to the database.
        public void SaveChangesToDB()
        {
            favoriteStopDB.SubmitChanges();
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

