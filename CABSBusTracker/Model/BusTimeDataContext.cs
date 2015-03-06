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
    public class BusTimeDataContext : DataContext
    {
        // Pass the connection string to the base class.
        public BusTimeDataContext(string connectionString)
            : base(connectionString)
        { }

        // Specify a table for the to-do items.
        public Table<BusTimeItem> Items;

    }

    [Table]
    public class BusTimeItem : INotifyPropertyChanged, INotifyPropertyChanging
    {

        // Define ID: private field, public property, and database column.
        private int _busTimeItemId;

        [Column(IsPrimaryKey = true, IsDbGenerated = true, DbType = "INT NOT NULL Identity", CanBeNull = false, AutoSync = AutoSync.OnInsert)]
        public int BusTimeItemId
        {
            get { return _busTimeItemId; }
            set
            {
                if (_busTimeItemId != value)
                {
                    NotifyPropertyChanging("BusTimeItemId");
                    _busTimeItemId = value;
                    NotifyPropertyChanged("BusTimeItemId");
                }
            }
        }

        // Define item name: private field, public property, and database column.
        private string _route;

        [Column]
        public string Route
        {
            get { return _route; }
            set
            {
                if (_route != value)
                {
                    NotifyPropertyChanging("Route");
                    _route = value;
                    NotifyPropertyChanged("Route");
                }
            }
        }

        // Define item name: private field, public property, and database column.
        private string _time;

        [Column]
        public string Time
        {
            get { return _time; }
            set
            {
                if (_time != value)
                {
                    NotifyPropertyChanging("Time");
                    _time = value;
                    NotifyPropertyChanged("Time");
                }
            }
        }

        // Define item name: private field, public property, and database column.
        private string _destination;

        [Column]
        public string Destination
        {
            get { return _destination; }
            set
            {
                if (_destination != value)
                {
                    NotifyPropertyChanging("Destination");
                    _destination = value;
                    NotifyPropertyChanged("Destination");
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
