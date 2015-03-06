using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Data.Linq;
using System.Data.Linq.Mapping;

namespace CABSBusTracker.Model
{
    public class FavoriteDataContext : DataContext
    {
        // Pass the connection string to the base class.
        public FavoriteDataContext(string connectionString)
            : base(connectionString)
        { }

        // Specify a table for the to-do items.
        public Table<FavoriteItem> Items;

    }

    [Table]
    public class FavoriteItem : INotifyPropertyChanged, INotifyPropertyChanging
    {

        // Define ID: private field, public property, and database column.
        private int _favoriteItemId;

        [Column(IsPrimaryKey = true, IsDbGenerated = true, DbType = "INT NOT NULL Identity", CanBeNull = false, AutoSync = AutoSync.OnInsert)]
        public int FavoriteItemId
        {
            get { return _favoriteItemId; }
            set
            {
                if (_favoriteItemId != value)
                {
                    NotifyPropertyChanging("FavoriteItemId");
                    _favoriteItemId = value;
                    NotifyPropertyChanged("FavoriteItemId");
                }
            }
        }

        // Define item name: private field, public property, and database column.
        private string _stopIDName;

        [Column]
        public string StopIDName
        {
            get { return _stopIDName; }
            set
            {
                if (_stopIDName != value)
                {
                    NotifyPropertyChanging("StopIDName");
                    _stopIDName = value;
                    NotifyPropertyChanged("StopIDName");
                }
            }
        }

        // Define item name: private field, public property, and database column.
        private int _stopID;

        [Column]
        public int StopID
        {
            get { return _stopID; }
            set
            {
                if (_stopID != value)
                {
                    NotifyPropertyChanging("StopID");
                    _stopID = value;
                    NotifyPropertyChanged("StopID");
                }
            }
        }

        // Define item name: private field, public property, and database column.
        private string _stopName;

        [Column]
        public string StopName
        {
            get { return _stopName; }
            set
            {
                if (_stopName != value)
                {
                    NotifyPropertyChanging("StopName");
                    _stopName = value;
                    NotifyPropertyChanged("StopName");
                }
            }
        }


        // Version column aids update performance.
        [Column(IsVersion = true)]
        private Binary _version;

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        // Used to notify that a property changed
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region INotifyPropertyChanging Members

        public event PropertyChangingEventHandler PropertyChanging;

        // Used to notify that a property is about to change
        private void NotifyPropertyChanging(string propertyName)
        {
            if (PropertyChanging != null)
            {
                PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
            }
        }

        #endregion




    }

}

