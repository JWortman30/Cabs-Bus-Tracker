using CABSBusTracker.Model;
using Coding4Fun.Toolkit.Controls;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Maps.Controls;
using Microsoft.Phone.Maps.Toolkit;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Device.Location;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
//using System.Device.Location;

namespace CABSBusTracker
{
    public partial class BusTimesResults : PhoneApplicationPage
    {

        //*************************************************************************************************
        //Global Variables: This section contains all of the variables that are accessed in multiple places
        //*************************************************************************************************

        //URL for the predictions API request
        String timesURL = "http://trip.osu.edu/bustime/api/v1/getpredictions?key=8GYvqZDsLLPgiAYM6SVjV3S7X&stpid=";

        //URL for Route Map Waypoints API request
        String routeMapURL = "http://trip.osu.edu/bustime/api/v1/getpatterns?key=8GYvqZDsLLPgiAYM6SVjV3S7X&pid=";

        //URL for Stop API request
        String routeStopsURL = "http://trip.osu.edu/bustime/api/v1/getstops?key=8GYvqZDsLLPgiAYM6SVjV3S7X&rt=";

        //URL for Bus GPS Location API request
        String routeBusLocationURL = "http://trip.osu.edu/bustime/api/v1/getvehicles?key=8GYvqZDsLLPgiAYM6SVjV3S7X&rt=";

        //Store the current stop that is being requested
        int currentStop = 0;

        //Stores whether or not the user has manually selected a stop
        Boolean stopSelected = false;

        //Collection of retreived bus arrival times
        ObservableCollection<ArrivalTime> ArrivalTimes = new ObservableCollection<ArrivalTime>();

        //Collection of Bus Stops for the Route Map
        ObservableCollection<BusStop> stopPushpins = new ObservableCollection<BusStop>();

        //Collection of Bus Stops for the Route Map
        ObservableCollection<Bus> busPushpins = new ObservableCollection<Bus>();

        //list of geocoordinates for the bus map
        List<GeoCoordinate> MyCoordinates = new List<GeoCoordinate>();

        //Variable for the source of the two listpickers
        List<BusRoute> source = new List<BusRoute>();
        List<BusStop> stopSource = new List<BusStop>();

        //Creates a timer to allow auto refreshing of bus prediction times
        System.Windows.Threading.DispatcherTimer dt = new System.Windows.Threading.DispatcherTimer();

        //Loads the settings from storage
        AppSettings settings = new AppSettings(); 

        //Variable for the map line
        MapPolyline Polyline = new MapPolyline();

        //Sets the color of the map line, changes with each route
        Color mapLineColor = new Color();

        //String of the selected route Map Dot and Pushpin color
        String stopDotColor = "";

        //determine whether timer was running on previous exit of page
        Boolean timerRunning = false;

        ////determing whether refresh indicator is running before indicator is created
        //Boolean refreshRunning = false;

        MapLayer prevUserMarker = new MapLayer();

        //****************************************************************************
        //Constructor: Inializes the page
        //****************************************************************************
        public BusTimesResults()
        {
            InitializeComponent();

            this.DataContext = App.ViewModel;

            //Clear previous arrival times that may be in the database
            App.ViewModel.ClearArrivalTimes();
            ArrivalTimes.Clear();

            //set up timer
            dt.Interval = new TimeSpan(0, 0, 0, 0, 30000); // 30 seconds
            dt.Tick += new EventHandler(dt_Tick);

            //Minimize the application bar by default
            ApplicationBar.Mode = ApplicationBarMode.Minimized;

            //Hide the last refreshed time textblock
            refreshTime.Visibility = Visibility.Collapsed;



            //If running on a 15:9 device, adjust size of the longlistselector
            double ScreenHeight = Application.Current.Host.Content.ActualHeight;
            if (ScreenHeight == 800)
            {
                retreivedBusTimes.Height = 475;
                MyMap.Height = 440;
            }

            
            //Set route info for default selected route list            
            source.Add(new BusRoute() { Name = "Buckeye Village (BV)" });
            source.Add(new BusRoute() { Name = "Central Connector (CC)" });
            source.Add(new BusRoute() { Name = "Campus Loop North (CLN)" });
            source.Add(new BusRoute() { Name = "Campus Loop South (CLS)" });
            source.Add(new BusRoute() { Name = "East Residential (ER)" });
            source.Add(new BusRoute() { Name = "Med Center Express (MC)" });
            source.Add(new BusRoute() { Name = "North Express (NE)" });

            stopSource.Add(new BusStop() { Name = "17th Ave & College", StopNum = 53 });
            stopSource.Add(new BusStop() { Name = "Ackerman & College", StopNum = 81 });
            stopSource.Add(new BusStop() { Name = "AG Campus (EB)", StopNum = 20 });
            stopSource.Add(new BusStop() { Name = "AG Campus (WB)", StopNum = 25 });
            stopSource.Add(new BusStop() { Name = "Arps Hall", StopNum = 54 });
            stopSource.Add(new BusStop() { Name = "Blackburn House", StopNum = 55 });
            stopSource.Add(new BusStop() { Name = "Bood Depository", StopNum = 72 });
            stopSource.Add(new BusStop() { Name = "Buckeye Lot Loop", StopNum = 75 });
            stopSource.Add(new BusStop() { Name = "Buckeye Village North", StopNum = 77 });
            stopSource.Add(new BusStop() { Name = "Buckeye Village South", StopNum = 76 });
            stopSource.Add(new BusStop() { Name = "Child Care Center", StopNum = 74 });
            stopSource.Add(new BusStop() { Name = "Fisher Commons", StopNum = 70 });
            stopSource.Add(new BusStop() { Name = "Kenny & Ackerman", StopNum = 73 });
            stopSource.Add(new BusStop() { Name = "Mason Hall", StopNum = 56 });
            stopSource.Add(new BusStop() { Name = "Olentangy River Rd. & Borror Dr.", StopNum = 79 });
            stopSource.Add(new BusStop() { Name = "RPAC Plaza", StopNum = 51 });
            stopSource.Add(new BusStop() { Name = "Service Annex", StopNum = 71 });
            stopSource.Add(new BusStop() { Name = "St. John Arena (EB)", StopNum = 21 });
            stopSource.Add(new BusStop() { Name = "St. John Arena (WB)", StopNum = 24 });
            stopSource.Add(new BusStop() { Name = "University Hall", StopNum = 52 });
            stopSource.Add(new BusStop() { Name = "Woody Hayes Athletic Center", StopNum = 78 });

            //Set source of the two list pickers
            this.routePicker.ItemsSource = source;
            this.stopPicker.ItemsSource = stopSource;

        }


        //******************************************************************
        //Navigation: This section contains code for the navigation to and from the page
        //******************************************************************

        //Stops the timer when the page is navigated away from
        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {

            dt.Stop();
            Debug.WriteLine("Stopping timer of navigation from");
            base.OnNavigatedFrom(e);
            
        }


        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if(timerRunning)
            {
                Debug.WriteLine("Starting timer on navigation to after timer was already running");
                dt.Start();
            }
            Debug.WriteLine("Timer Running on Navigated to: " + dt.IsEnabled);
            //retreive the ID of the stop to get times of if one was specified by a different page
            
            var k = PhoneApplicationService.Current.State["selStopID"];
            int selStopID = (int)k;

            //Retrieve times based on specified ID (If ID is -1, that means that a stop was not requested by a different page)
            if (selStopID != -1)
            {
                //Retrieve times based on the passed stop ID
                currentStop = selStopID;
                stopSelected = true;
                String stop = timesURL + currentStop;
                RetreiveCABSInfo(stop);

                //If auto refresh setting is turned on, enable app to run under lockscreen, and start auto refresh timer, start indeterminate progress bar
                if (settings.RefreshCheckBoxSetting)
                {
                    //Set interval of timer to 30 secongs, and set the tick event
                    dt.Start();
                    timerRunning = true;

                    //Progress indicator for refreshing
                    SystemTray.ProgressIndicator = new ProgressIndicator();
                    SystemTray.IsVisible = true;
                    SystemTray.ProgressIndicator.Text = "Refreshing...";
                    SystemTray.ProgressIndicator.IsIndeterminate = true;
                    SystemTray.ProgressIndicator.IsVisible = true;

                    PhoneApplicationService.Current.ApplicationIdleDetectionMode = IdleDetectionMode.Disabled;
                }
                
                //Navigate pivot to results pivot item, set refresh time, and temporary loading stop time.
                PivotControl.SelectedIndex = 1;
                stpName.Text = "Loading Buses...";
                refreshTime.Visibility = Visibility.Visible;
                refreshTime.Text = "Last Refreshed: " + Convert.ToString(DateTime.Now);

                //Set application selStopIF state variable to -1, to prevent reloading of times upon page navigation
                PhoneApplicationService.Current.State["selStopID"] = -1;
            }
        }


        //***************************************************************
        //Pivot Control: This section contains the code that managages the Pivot Control
        //***************************************************************

        //Hides or shows the actionbar in the results page, based on the auto refresh settings, and whether or not a stop was selected
        private void pivotActiveMenuChanged(object sender, SelectionChangedEventArgs e)
        {
            AppSettings setting = new AppSettings();

            ApplicationBarIconButton startRefreshing = new ApplicationBarIconButton();
            EventHandler startRefreshingClick = new EventHandler(startRefreshing_Click);
            startRefreshing.Text = "start refreshing";
            startRefreshing.Click += startRefreshingClick;
            startRefreshing.IconUri = new Uri("/Icons/refresh.png", UriKind.Relative);

            ApplicationBarIconButton stopRefreshing = new ApplicationBarIconButton();
            EventHandler stopRefreshingClick = new EventHandler(stopRefreshing_Click);
            stopRefreshing.Text = "stop refreshing";
            stopRefreshing.Click += stopRefreshingClick;
            stopRefreshing.IconUri = new Uri("/Icons/clear.png", UriKind.Relative);

            ApplicationBarIconButton refresh = new ApplicationBarIconButton();
            EventHandler refreshClick = new EventHandler(refresh_Click);
            refresh.Text = "refresh";
            refresh.Click += refreshClick;
            refresh.IconUri = new Uri("/Icons/refresh.png", UriKind.Relative);

            ApplicationBarIconButton saveStop = new ApplicationBarIconButton();
            EventHandler saveStopClick = new EventHandler(saveStop_Click);
            saveStop.Text = "save stop";
            saveStop.Click += saveStopClick;
            saveStop.IconUri = new Uri("/Icons/save.png", UriKind.Relative);

            if (ApplicationBar.Buttons.Count == 2)
            {
                ApplicationBar.Buttons.RemoveAt(0);
                ApplicationBar.Buttons.RemoveAt(0);
                ApplicationBar.Mode = ApplicationBarMode.Minimized;
            }
            else if (ApplicationBar.Buttons.Count == 1)
            {
                ApplicationBar.Buttons.RemoveAt(0);
                ApplicationBar.Mode = ApplicationBarMode.Minimized;
            }

            if (PivotControl.SelectedIndex == 1 && stopSelected == true && settings.RefreshCheckBoxSetting && timerRunning)
            {
                ApplicationBar.Mode = ApplicationBarMode.Default;
                ApplicationBar.Buttons.Add(saveStop);
                ApplicationBar.Buttons.Add(stopRefreshing);
            }
            else if (PivotControl.SelectedIndex == 1 && stopSelected == true && settings.RefreshCheckBoxSetting && !(timerRunning))
            {
                ApplicationBar.Mode = ApplicationBarMode.Default;
                ApplicationBar.Buttons.Add(saveStop);
                ApplicationBar.Buttons.Add(startRefreshing);
            }
            else if (PivotControl.SelectedIndex == 1 && stopSelected == true)
            {
                ApplicationBar.Mode = ApplicationBarMode.Default;
                ApplicationBar.Buttons.Add(saveStop);
                ApplicationBar.Buttons.Add(refresh);
            }


            if (PivotControl.SelectedIndex == 2 && settings.RefreshCheckBoxSetting && timerRunning)
            {
                ApplicationBar.Mode = ApplicationBarMode.Default;
                ApplicationBar.Buttons.Add(stopRefreshing);
            }
            else if (PivotControl.SelectedIndex == 2 && settings.RefreshCheckBoxSetting && !(timerRunning))
            {
                ApplicationBar.Mode = ApplicationBarMode.Default;
                ApplicationBar.Buttons.Add(startRefreshing);
            }
            else if (PivotControl.SelectedIndex == 2 && !(settings.RefreshCheckBoxSetting))
            {
                ApplicationBar.Mode = ApplicationBarMode.Default;
                ApplicationBar.Buttons.Add(refresh);
            }

            if (PivotControl.SelectedIndex == 2)
            {
                
                if (this.mapRoutePicker.ItemsSource == null)
                {
                    this.mapRoutePicker.ItemsSource = source;
                    //Debug.WriteLine("Added source to list picker");
                }

            }
        }


        //**************************************************************
        //Timer: This section contains the timer code
        //**************************************************************

        //retrieve bus times every 30 seconds, and update refreshed time
        void dt_Tick(object sender, EventArgs e)
        {
            dt.Stop();
            timerRunning = false;
            Debug.WriteLine("DT_Tick Run");
            if (stopSelected)
            {
                RetreiveCABSInfo(timesURL + currentStop);
                refreshTime.Text = "Last Refreshed: " + Convert.ToString(DateTime.Now);
            }
            if (this.mapRoutePicker.ItemsSource != null)
            {
                Task refreshBusPins = GetBusPushpins();
            }
            ShowMyLocationOnTheMap();            
            dt.Start();
            timerRunning = true;

        }

        //****************************************************************
        //Pivot Page 0: This section has code for the stop selection page
        //****************************************************************

        //Loads the proper stops based on interaction with the route listPicker
        private void listPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //BusRoute temp = new BusRoute();
            Debug.WriteLine("THIS IS A TEST!");
            BusRoute temp = (BusRoute)routePicker.SelectedItem;
            if (temp.Name.Equals("Campus Loop North (CLN)"))
            {
                List<BusStop> source = new List<BusStop>();
                source.Clear();
                source.Add(new BusStop() { Name = "AG Campus (EB)", StopNum = 20 });
                source.Add(new BusStop() { Name = "AG Campus (WB)", StopNum = 25 });
                source.Add(new BusStop() { Name = "Archer House", StopNum = 90 });
                source.Add(new BusStop() { Name = "Bevis Hall", StopNum = 10 });
                source.Add(new BusStop() { Name = "Blankenship Hall", StopNum = 14 });
                source.Add(new BusStop() { Name = "Cannon Dr. & 12th Ave.", StopNum = 36 });
                source.Add(new BusStop() { Name = "Carmack 1", StopNum = 13 });
                source.Add(new BusStop() { Name = "Carmack 4", StopNum = 12 });
                source.Add(new BusStop() { Name = "Carmack 5", StopNum = 94 });
                source.Add(new BusStop() { Name = "Carmack Corner", StopNum = 11 });
                source.Add(new BusStop() { Name = "Hamilton Hall", StopNum = 33 });
                source.Add(new BusStop() { Name = "Honors House", StopNum = 45 });
                source.Add(new BusStop() { Name = "Knowlton Hall", StopNum = 57 });
                source.Add(new BusStop() { Name = "Koffolt Lab", StopNum = 58 });
                source.Add(new BusStop() { Name = "Med Center & Cannon Dr. (WB)", StopNum = 35 });
                source.Add(new BusStop() { Name = "Meiling Hall", StopNum = 34 });
                source.Add(new BusStop() { Name = "Mid Towers", StopNum = 23 });
                source.Add(new BusStop() { Name = "Ohio Union (SB)", StopNum = 44 });
                source.Add(new BusStop() { Name = "St. John Arena (EB)", StopNum = 21 });
                source.Add(new BusStop() { Name = "St. John Arena (WB)", StopNum = 24 });
                source.Add(new BusStop() { Name = "Stillman Hall", StopNum = 59 });
                source.Add(new BusStop() { Name = "Taylor Tower", StopNum = 91 });
                source.Add(new BusStop() { Name = "Watts Hall", StopNum = 92 });
                this.stopPicker.ItemsSource = source;
            }
            else if (temp.Name.Equals("Buckeye Village (BV)"))
            {
                List<BusStop> source = new List<BusStop>();
                source.Clear();
                source.Add(new BusStop() { Name = "17th Ave & College", StopNum = 53 });
                source.Add(new BusStop() { Name = "Ackerman & College", StopNum = 81 });
                source.Add(new BusStop() { Name = "AG Campus (EB)", StopNum = 20 });
                source.Add(new BusStop() { Name = "AG Campus (WB)", StopNum = 25 });
                source.Add(new BusStop() { Name = "Arps Hall", StopNum = 54 });
                source.Add(new BusStop() { Name = "Blackburn House", StopNum = 55 });
                source.Add(new BusStop() { Name = "Book Depository", StopNum = 72 });
                source.Add(new BusStop() { Name = "Buckeye Lot Loop", StopNum = 75 });
                source.Add(new BusStop() { Name = "Buckeye Village North", StopNum = 77 });
                source.Add(new BusStop() { Name = "Buckeye Village South", StopNum = 76 });
                source.Add(new BusStop() { Name = "Child Care Center", StopNum = 74 });
                source.Add(new BusStop() { Name = "Fisher Commons", StopNum = 70 });
                source.Add(new BusStop() { Name = "Kenny & Ackerman", StopNum = 73 });
                source.Add(new BusStop() { Name = "Mason Hall", StopNum = 56 });
                source.Add(new BusStop() { Name = "Olentangy River Rd. & Borror Dr.", StopNum = 79 });
                source.Add(new BusStop() { Name = "RPAC Plaza", StopNum = 51 });
                source.Add(new BusStop() { Name = "Service Annex", StopNum = 71 });
                source.Add(new BusStop() { Name = "St. John Arena (EB)", StopNum = 21 });
                source.Add(new BusStop() { Name = "St. John Arena (WB)", StopNum = 24 });
                source.Add(new BusStop() { Name = "University Hall", StopNum = 52 });
                source.Add(new BusStop() { Name = "Woody Hayes Athletic Center", StopNum = 78 });
                this.stopPicker.ItemsSource = source;
            }
            else if (temp.Name.Equals("Central Connector (CC)"))
            {
                List<BusStop> source = new List<BusStop>();
                source.Clear();
                source.Add(new BusStop() { Name = "5th & Michigan Ave.", StopNum = 213 });
                source.Add(new BusStop() { Name = "Arps Hall", StopNum = 54 });
                source.Add(new BusStop() { Name = "Blackburn House", StopNum = 55 });
                source.Add(new BusStop() { Name = "Drake Union", StopNum = 22 });
                source.Add(new BusStop() { Name = "Hale Hall", StopNum = 42 });
                source.Add(new BusStop() { Name = "Hamilton Hall", StopNum = 33 });
                source.Add(new BusStop() { Name = "Honors House", StopNum = 45 });
                source.Add(new BusStop() { Name = "King & Michigan Ave.", StopNum = 215 });
                source.Add(new BusStop() { Name = "Knowlton Hall", StopNum = 57 });
                source.Add(new BusStop() { Name = "Koffolt Lab", StopNum = 58 });
                source.Add(new BusStop() { Name = "Mack Hall", StopNum = 51 });
                source.Add(new BusStop() { Name = "Mason Hall", StopNum = 56 });
                source.Add(new BusStop() { Name = "Mid Towers", StopNum = 23 });
                source.Add(new BusStop() { Name = "Neil & 10th Ave.", StopNum = 40 });
                source.Add(new BusStop() { Name = "Neil & 5th Ave.", StopNum = 212 });
                source.Add(new BusStop() { Name = "Neil & 7th Ave.", StopNum = 211 });
                source.Add(new BusStop() { Name = "Neil & 8th Ave. (NB)", StopNum = 216 });
                source.Add(new BusStop() { Name = "Neil & 8th Ave. (SB)", StopNum = 210 });
                source.Add(new BusStop() { Name = "Ohio Union (NB)", StopNum = 43 });
                source.Add(new BusStop() { Name = "Ohio Union (SB)", StopNum = 44 });
                source.Add(new BusStop() { Name = "Stillman Hall", StopNum = 59 });

                this.stopPicker.ItemsSource = source;

            }
            else if (temp.Name.Equals("Campus Loop South (CLS)"))
            {
                List<BusStop> source = new List<BusStop>();
                source.Clear();
                source.Add(new BusStop() { Name = "AG Campus (EB)", StopNum = 20 });
                source.Add(new BusStop() { Name = "AG Campus (WB)", StopNum = 25 });
                source.Add(new BusStop() { Name = "Arps Hall", StopNum = 54 });
                source.Add(new BusStop() { Name = "Bevis Hall", StopNum = 10 });
                source.Add(new BusStop() { Name = "Blackburn House", StopNum = 55 });
                source.Add(new BusStop() { Name = "Blankenship Hall", StopNum = 14 });
                source.Add(new BusStop() { Name = "Cannon Dr. & 12th Ave.", StopNum = 36 });
                source.Add(new BusStop() { Name = "Carmack 1", StopNum = 13 });
                source.Add(new BusStop() { Name = "Carmack 4", StopNum = 12 });
                source.Add(new BusStop() { Name = "Carmack 5", StopNum = 94 });
                source.Add(new BusStop() { Name = "Carmack Corner", StopNum = 11 });
                source.Add(new BusStop() { Name = "Drake Union", StopNum = 22 });
                source.Add(new BusStop() { Name = "Hale Hall", StopNum = 42 });
                source.Add(new BusStop() { Name = "Mack Hall", StopNum = 51 });
                source.Add(new BusStop() { Name = "Mason Hall", StopNum = 56 });
                source.Add(new BusStop() { Name = "Med Center & Cannon Dr. (EB)", StopNum = 31 });
                source.Add(new BusStop() { Name = "Med Center Dr. & 9th Ave.", StopNum = 32 });
                source.Add(new BusStop() { Name = "Neil & 10th Ave.", StopNum = 40 });
                source.Add(new BusStop() { Name = "Ohio Union (NB)", StopNum = 43 });
                source.Add(new BusStop() { Name = "St. John Arena (EB)", StopNum = 21 });
                source.Add(new BusStop() { Name = "St. John Arena (WB)", StopNum = 24 });
                this.stopPicker.ItemsSource = source;
            }
            else if (temp.Name.Equals("East Residential (ER)"))
            {
                List<BusStop> source = new List<BusStop>();
                source.Clear();
                source.Add(new BusStop() { Name = "11th Ave. & Indianola (EB)", StopNum = 111 });
                source.Add(new BusStop() { Name = "11th Ave. & Indianola (WB)", StopNum = 128 });
                source.Add(new BusStop() { Name = "11th Ave. & High St. (EB)", StopNum = 110 });
                source.Add(new BusStop() { Name = "11th Ave. & High St. (WB)", StopNum = 129 });
                source.Add(new BusStop() { Name = "13th Ave. (4th)", StopNum = 113 });
                source.Add(new BusStop() { Name = "13th Ave. (Summit)", StopNum = 126 });
                source.Add(new BusStop() { Name = "15th Ave. (4th)", StopNum = 114 });
                source.Add(new BusStop() { Name = "15th Ave. (Summit)", StopNum = 125 });
                source.Add(new BusStop() { Name = "13th Ave. (Summit)", StopNum = 126 });
                source.Add(new BusStop() { Name = "17th Ave. & College Rd.", StopNum = 53 });
                source.Add(new BusStop() { Name = "17th Ave. (4th)", StopNum = 115 });
                source.Add(new BusStop() { Name = "17th Ave. (Summit)", StopNum = 124 });
                source.Add(new BusStop() { Name = "19th Ave. (Summit)", StopNum = 123 });
                source.Add(new BusStop() { Name = "Arps Hall", StopNum = 54 });
                source.Add(new BusStop() { Name = "Chittenden Ave. (4th)", StopNum = 112 });
                source.Add(new BusStop() { Name = "Chittenden Ave. (Summit)", StopNum = 127 });
                source.Add(new BusStop() { Name = "Clinton St. (Summit)", StopNum = 119 });
                source.Add(new BusStop() { Name = "Lane Ave. (4th)", StopNum = 116 });
                source.Add(new BusStop() { Name = "Lane Ave. (Summit)", StopNum = 122 });
                source.Add(new BusStop() { Name = "Maynard Ave. (4th)", StopNum = 118 });
                source.Add(new BusStop() { Name = "Maynard Ave. (Summit)", StopNum = 120 });
                source.Add(new BusStop() { Name = "Neil & 17th Ave.", StopNum = 61 });
                source.Add(new BusStop() { Name = "Northwood Ave. (4th)", StopNum = 117 });
                source.Add(new BusStop() { Name = "Oakland Ave. (Summit)", StopNum = 121 });
                source.Add(new BusStop() { Name = "Ohio Union (NB)", StopNum = 43 });
                source.Add(new BusStop() { Name = "Ohio Union (SB)", StopNum = 44 });
                source.Add(new BusStop() { Name = "Physics Reasearch", StopNum = 60 });
                source.Add(new BusStop() { Name = "University Hall", StopNum = 52 });
                this.stopPicker.ItemsSource = source;
            }
            else if (temp.Name.Equals("Med Center Express (MC)"))
            {
                List<BusStop> source = new List<BusStop>();
                source.Clear();
                source.Add(new BusStop() { Name = "Ackerman Complex Loop", StopNum = 80 });
                source.Add(new BusStop() { Name = "Buckeye Lot Loop", StopNum = 75 });
                source.Add(new BusStop() { Name = "Cannon & 12th Ave. (NB)", StopNum = 36 });
                source.Add(new BusStop() { Name = "Child Care Center", StopNum = 74 });
                source.Add(new BusStop() { Name = "Doan Hall", StopNum = 37 });
                source.Add(new BusStop() { Name = "Hamilton Hall", StopNum = 33 });
                source.Add(new BusStop() { Name = "Med Center & Cannon Dr. (WB)", StopNum = 35 });
                source.Add(new BusStop() { Name = "Meiling Hall", StopNum = 34 });
                source.Add(new BusStop() { Name = "Vet Med Academic Building (EB)", StopNum = 26 });
                source.Add(new BusStop() { Name = "Vet Med Academic Building (WB)", StopNum = 27 });
                this.stopPicker.ItemsSource = source;
            }
            else if (temp.Name.Equals("North Express (NE)"))
            {
                List<BusStop> source = new List<BusStop>();
                source.Clear();
                source.Add(new BusStop() { Name = "17th Ave & College", StopNum = 53 });
                source.Add(new BusStop() { Name = "AG Campus (EB)", StopNum = 20 });
                source.Add(new BusStop() { Name = "AG Campus (WB)", StopNum = 25 });
                source.Add(new BusStop() { Name = "Arps Hall", StopNum = 54 });
                source.Add(new BusStop() { Name = "Bevis Hall", StopNum = 10 });
                source.Add(new BusStop() { Name = "Blackburn House", StopNum = 55 });
                source.Add(new BusStop() { Name = "Blankenship Hall", StopNum = 14 });
                source.Add(new BusStop() { Name = "Carmack 1", StopNum = 13 });
                source.Add(new BusStop() { Name = "Carmack 4", StopNum = 12 });
                source.Add(new BusStop() { Name = "Carmack Corner", StopNum = 11 });
                source.Add(new BusStop() { Name = "Mason Hall", StopNum = 56 });
                source.Add(new BusStop() { Name = "RPAC Plaza", StopNum = 51 });
                source.Add(new BusStop() { Name = "St. John Arena (EB)", StopNum = 21 });
                source.Add(new BusStop() { Name = "St. John Arena (WB)", StopNum = 24 });
                source.Add(new BusStop() { Name = "University Hall", StopNum = 52 });
                this.stopPicker.ItemsSource = source;
            }
            else
            {
                List<BusStop> source = new List<BusStop>();
                source.Clear();
                source.Add(new BusStop() { Name = "Stop Info Not Avaiable", StopNum = 0 });
                this.stopPicker.ItemsSource = source;
            }
        }


        //If user manually enters stop, prevent usage of listpickers to avoid confusion
        private void manualStop_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!manualStop.Text.Equals(""))
            {
                routePicker.IsEnabled = false;
                stopPicker.IsEnabled = false;
            }
            else
            {
                routePicker.IsEnabled = true;
                stopPicker.IsEnabled = true;
            }
        }


        //Click event for get bus times button
        private void GetBusTimes_Click(object sender, RoutedEventArgs e)
        {

            //If manual stop was not entered refreive stop ID from list picker, else retieve bus ID from textbox
            if (manualStop.Text != "")
            {
                String stop = timesURL + manualStop.Text;
                RetreiveCABSInfo(stop);
                currentStop = Convert.ToInt32(manualStop.Text);
                stopSelected = true;
            }
            else
            {
                BusStop temp = new BusStop();
                temp = (BusStop)this.stopPicker.SelectedItem;
                String stop = timesURL + temp.StopNum;
                RetreiveCABSInfo(stop);
                currentStop = temp.StopNum;
                stopSelected = true;
            }

            //Change stop name to loading, and set refreshed time
            stpName.Text = "Loading Buses...";
            refreshTime.Visibility = Visibility.Visible;
            refreshTime.Text = "Last Refreshed: " + Convert.ToString(DateTime.Now);

            //If auto refresh setting is on, start timer, and enable app to run under lockscreen.
            if (settings.RefreshCheckBoxSetting)
            {
                //Progress indicator for refreshing
                SystemTray.ProgressIndicator = new ProgressIndicator();
                SystemTray.IsVisible = true;
                SystemTray.ProgressIndicator.Text = "Refreshing...";
                SystemTray.ProgressIndicator.IsIndeterminate = true;
                SystemTray.ProgressIndicator.IsVisible = true;

                if (!dt.IsEnabled)
                {
                    dt.Start();
                    timerRunning = true;
                }                
                PhoneApplicationService.Current.ApplicationIdleDetectionMode = IdleDetectionMode.Disabled;
            }

            //Navigate to results page
            PivotControl.SelectedIndex = 1;
        }

        //****************************************************************************
        //Pivot Page 1: This section contains the code for the preduction results page
        //****************************************************************************

        //Download the xml of the times request
        private void RetreiveCABSInfo(String requestURL)
        {
            WebClient webClient = new WebClient();

            webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(webClient_DownloadStringCompleted);

            webClient.DownloadStringAsync(new System.Uri(requestURL + "&nocache=" + Environment.TickCount));
        }

        //Sends the resulting xml to be parsed and update the bus arrival times
        private void webClient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    //MessageBox.Show("There was an error downloading the bus times. Check your network connection.");
                    var color = new Color { A = 0xFF, R = 0xA2, G = 0x2B, B = 0x2B };
                    ToastPrompt toast = new ToastPrompt();
                    toast.Title = "ERROR";
                    toast.Message = "Download Failed. Check Connection.";
                    toast.Background = new SolidColorBrush(color);
                    toast.ImageSource = new BitmapImage(new Uri("/AppIconSmall.png", UriKind.RelativeOrAbsolute));
                    toast.Show();
                    stpName.Text = "Error Retrieving Stops";
                });
            }
            else
            {
                //Save the bus times into the State property in case the application is paused.
                this.State["busPredictions"] = e.Result;

                UpdateBusList(e.Result);
            }
        }



        //This method sets up the feed of BusTime and adds it to the database of stored times
        private void UpdateBusList(string busXML)
        {
            //Clear previously stored arrival times
            App.ViewModel.ClearArrivalTimes();
            ArrivalTimes.Clear();

            //Load bus times into xmlReader
            StringReader stringReader = new StringReader(busXML);
            XmlReader xmlReader = XmlReader.Create(stringReader);

            //Prevent work from being done on UI thread
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                //Parses the xml and adds the arrival times to the ObservableCollection
                String routeName = "", arrivalTime = "", timeToArrive = "", RtDestination = "", stopName = "", busNumID = "", busColor = "";
                int minutes = 0;
                if (xmlReader.ReadToFollowing("stpnm"))
                {
                    stopName = xmlReader.ReadElementContentAsString();
                    stpName.Text = stopName;
                }
                else
                {
                    stpName.Text = "No Times Found";
                }


                while (xmlReader.ReadToFollowing("vid"))
                {
                    busNumID = xmlReader.ReadElementContentAsString();

                    if (xmlReader.ReadToFollowing("rt"))
                    {
                        routeName = xmlReader.ReadElementContentAsString();
                    }

                    if (xmlReader.ReadToFollowing("des"))
                    {
                        RtDestination = "Destination: " + xmlReader.ReadElementContentAsString();
                    }

                    if (xmlReader.ReadToFollowing("prdtm"))
                    {
                        arrivalTime = xmlReader.ReadElementContentAsString();
                        String tempDispTime = minutesToArrival(arrivalTime);
                        if (tempDispTime.Contains("Arriving Soon"))
                        {
                            minutes = 0;
                        }
                        else
                        {
                            minutes = Convert.ToInt16(tempDispTime.Substring(0, 2));
                        }
                        timeToArrive = "ETA: " + tempDispTime;                        
                    }

                    switch (routeName)
                    {
                        case "BV":
                            busColor = "#FF00803F";
                            break;
                        case "CC":
                            busColor = "#FF4680D3";
                            break;
                        case "CLN":
                            busColor = "#FF77787A";
                            break;
                        case "CLS":
                            busColor = "#FFC0262C";
                            break;
                        case "ER":
                            busColor = "#FF113580";
                            break;
                        case "MC":
                            busColor = "#FFF05994";
                            break;
                        case "NE":
                            busColor = "#FFF26021";
                            break;
                    }
                    //Debug.WriteLine("Minutes: " + minutes);
                    ArrivalTime newArrivalTime = new ArrivalTime
                    {
                        RouteName = routeName,
                        Destination = RtDestination,
                        ExpectedTime = timeToArrive,
                        busID = busNumID,
                        routeColor = busColor,
                        expectedTimeMinutes = minutes
                    };

                    ArrivalTimes.Add(newArrivalTime);
                }

                //GetCoordinates();
                App.ViewModel.InsertBusTimes(ArrivalTimes);

            });
        }


        //Manually refresh times
        private void refresh_Click(object sender, EventArgs e)
        {
            if (stopSelected == true)
            {
                RetreiveCABSInfo(timesURL + currentStop);
                refreshTime.Text = "Last Refreshed: " + Convert.ToString(DateTime.Now);
                if (this.mapRoutePicker.ItemsSource != null)
                {
                    Task refreshBusPins = GetBusPushpins();
                }
            }
            else
            {
                if (this.mapRoutePicker.ItemsSource != null)
                {
                    Task refreshBusPins = GetBusPushpins();
                }
            }

        }

        //If auto-refresh is on start refreshing times, and replace refresh button with stop button
        private void startRefreshing_Click(object sender, EventArgs e)
        {
            ApplicationBarIconButton stopRefreshing = new ApplicationBarIconButton();
            EventHandler stopRefreshingClick = new EventHandler(stopRefreshing_Click);
            PhoneApplicationService.Current.ApplicationIdleDetectionMode = IdleDetectionMode.Disabled;
            stopRefreshing.Text = "stop refreshing";
            stopRefreshing.Click += stopRefreshingClick;
            stopRefreshing.IconUri = new Uri("/Icons/clear.png", UriKind.Relative);

            if (stopSelected == true)
            {
                dt.Start();
                timerRunning = true;
                //Progress indicator for refreshing
                SystemTray.ProgressIndicator = new ProgressIndicator();
                SystemTray.IsVisible = true;
                SystemTray.ProgressIndicator.Text = "Refreshing...";
                SystemTray.ProgressIndicator.IsIndeterminate = true;
                SystemTray.ProgressIndicator.IsVisible = true;


                if (ApplicationBar.Buttons.Count == 2)
                {
                    ApplicationBar.Buttons.RemoveAt(1);
                }
                else
                {
                    ApplicationBar.Buttons.RemoveAt(0);
                }
                ApplicationBar.Buttons.Add(stopRefreshing);
            }
            else
            {
                dt.Start();
                timerRunning = true;

                //Progress indicator for refreshing
                SystemTray.ProgressIndicator = new ProgressIndicator();
                SystemTray.IsVisible = true;
                SystemTray.ProgressIndicator.Text = "Refreshing...";
                SystemTray.ProgressIndicator.IsIndeterminate = true;
                SystemTray.ProgressIndicator.IsVisible = true;

                ApplicationBar.Buttons.RemoveAt(0);
                ApplicationBar.Buttons.Add(stopRefreshing);
            }

        }

        //If auto-refresh is on stop refreshing times, and replace stop button with refresh button
        private void stopRefreshing_Click(object sender, EventArgs e)
        {
            ApplicationBarIconButton startRefreshing = new ApplicationBarIconButton();
            EventHandler startRefreshingClick = new EventHandler(startRefreshing_Click);
            startRefreshing.Text = "start refreshing";
            startRefreshing.Click += startRefreshingClick;
            startRefreshing.IconUri = new Uri("/Icons/refresh.png", UriKind.Relative);

            if (stopSelected == true)
            {
                dt.Stop();
                timerRunning = false;
                //Progress indicator for refreshing
                SystemTray.ProgressIndicator = new ProgressIndicator();
                SystemTray.IsVisible = true;
                SystemTray.ProgressIndicator.Text = "Refreshing...";
                SystemTray.ProgressIndicator.IsIndeterminate = false;
                SystemTray.ProgressIndicator.IsVisible = false;
                if(ApplicationBar.Buttons.Count == 2)
                {
                    ApplicationBar.Buttons.RemoveAt(1);
                }
                else
                {
                    ApplicationBar.Buttons.RemoveAt(0);
                }
                
                ApplicationBar.Buttons.Add(startRefreshing);
            }
            else
            {
                dt.Stop();
                timerRunning = false;
                //Progress indicator for refreshing
                SystemTray.ProgressIndicator = new ProgressIndicator();
                SystemTray.IsVisible = true;
                SystemTray.ProgressIndicator.Text = "Refreshing...";
                SystemTray.ProgressIndicator.IsIndeterminate = false;
                SystemTray.ProgressIndicator.IsVisible = false;
                ApplicationBar.Buttons.RemoveAt(0);
                ApplicationBar.Buttons.Add(startRefreshing);
            }

        }

        //Save stop to favorites if the stop does not currently exist in favorites list
        private void saveStop_Click(object sender, EventArgs e)
        {
            if (!(stpName.Text.Equals("Loading Buses...") || stpName.Text.Equals("No Times Found")))
            {
                FavoriteItem newFavoriteItem = new FavoriteItem
                {
                    StopID = currentStop,
                    StopIDName = "Stop #: " + Convert.ToString(currentStop),
                    StopName = stpName.Text
                };

                if (App.ViewModel2.AddFavoriteItem(newFavoriteItem))
                {
                    MessageBox.Show("Stop: " + stpName.Text + ", with Stop #: " + currentStop + " was added to favorites.");
                }
                else
                {
                    MessageBox.Show("This stop is already in your favorites.");
                }
            }
            else
            {
                MessageBox.Show("Stop data not loaded, cannot save stop.");
            }
        }

        //Calcualtes the number of minutes from the current time till the estimated arrival time, simplifies it by only working if time is less than two hours away
        //This should never be a problem because bus times are not predicted that far out
        private String minutesToArrival(String preTime)
        {
            String temp = "";
            DateTime now = DateTime.Now;
            int curHours = now.Hour;
            int curMin = now.Minute;

            String preHourString = preTime.Substring(9, 2);

            int preHourInt = Convert.ToInt32(preHourString);
            String preMinString = preTime.Substring(12);

            int preMinInt = Convert.ToInt32(preMinString);

            if (preHourInt == curHours)
            {
                temp = Convert.ToString(preMinInt - curMin);
            }
            else
            {
                //Will only work if bus time is within the next two hours, times should never be this long
                temp = Convert.ToString((60 - curMin) + preMinInt);
            }
            if (preHourInt > 12)
            {
                preHourInt = preHourInt - 12;
                preHourString = Convert.ToString(preHourInt);
                if (Convert.ToInt16(temp) < 2)
                {
                    temp = "Arriving Soon" + " (" + preHourString + ":" + preMinString + "pm)";
                }
                else
                {
                    temp = temp + " Minutes" + " (" + preHourString + ":" + preMinString + "pm)";
                }
            }
            else
            {
                if (Convert.ToInt16(temp) < 2)
                {
                    temp = "Arriving Soon" + " (" + preHourString + ":" + preMinString + "pm)";

                }
                else
                {
                    temp = temp + " Minutes" + " (" + preHourString + ":" + preMinString + "am)";
                }
            }
            return temp;
        }

        //******************************************
        //Pivot Page 2: This portion starts the code for the map page
        //******************************************

        private async void ShowMyLocationOnTheMap()
        {

            GeoCoordinateWatcher geo = new GeoCoordinateWatcher(GeoPositionAccuracy.High);
            geo.Start();

            Grid MyGrid = new Grid();
            MyGrid.RowDefinitions.Add(new RowDefinition());
            MyGrid.RowDefinitions.Add(new RowDefinition());
            MyGrid.Background = new SolidColorBrush(Colors.Transparent);

            UserLocationMarker test = new UserLocationMarker();
            test.Foreground = new SolidColorBrush(Color.FromArgb(255, 162, 43, 43));
            test.Background = new SolidColorBrush(Colors.DarkGray);

            //Adding the marker to the Grid
            MyGrid.Children.Add(test);

            MapOverlay MyOverlay = new MapOverlay();
            MyOverlay.Content = MyGrid;
            MyOverlay.PositionOrigin = new Point(0.5, 0.5);
            MyOverlay.GeoCoordinate = geo.Position.Location;

            MapLayer MyLayer = new MapLayer();
            MyLayer.Add(MyOverlay);

            if(MyMap.Layers.Contains(prevUserMarker))
            {
                MyMap.Layers.Remove(prevUserMarker);
            }
            //Debug.WriteLine("Layer Count: " + MyMap.Layers.Count);
            prevUserMarker = MyLayer;
            MyMap.Layers.Add(MyLayer);

        }

        //Runs when the selected item of the listpicker is changed, and when the item source of the list picker is defined
        private void mapRouteChanged(object sender, SelectionChangedEventArgs e)
        {
            MyMap.MapElements.Clear();
            stopPushpins.Clear();
            busPushpins.Clear();
            Task drawMap = GetCoordinates();
            ShowMyLocationOnTheMap();
        }

       
        private async Task GetCoordinates()
        {
            HttpClient client = new HttpClient();

            // Create and start the tasks. As each task finishes, DisplayResults  
            // displays its length.
            BusRoute temp = (BusRoute)mapRoutePicker.SelectedItem;

            if (temp != null)
            {
                switch (temp.Name)
                {
                    case "Buckeye Village (BV)":
                        Task downloadMapRoute1 = ProcessMapURLAsync(routeMapURL + "143,312" + "&nocache=" + Environment.TickCount, client);
                        await downloadMapRoute1;
                        mapLineColor = Color.FromArgb(255, 0, 128, 63);
                        MyMap.ZoomLevel = 14;
                        MyMap.Center = new GeoCoordinate(40.0084055028856, -83.0248292069882);
                        break;
                    case "Central Connector (CC)":
                        Task downloadMapRoute2 = ProcessMapURLAsync(routeMapURL + "253,255,252" + "&nocache=" + Environment.TickCount, client);
                        await downloadMapRoute2;
                        mapLineColor = Color.FromArgb(255, 70, 128, 211);
                        MyMap.ZoomLevel = 14;
                        MyMap.Center = new GeoCoordinate(39.9956571310759, -83.0154608376324);
                        break;
                    case "Campus Loop North (CLN)":
                        Task downloadMapRoute3 = ProcessMapURLAsync(routeMapURL + "319,314,320" + "&nocache=" + Environment.TickCount, client);
                        await downloadMapRoute3;
                        mapLineColor = Color.FromArgb(255, 119, 120, 122);
                        MyMap.ZoomLevel = 14;
                        MyMap.Center = new GeoCoordinate(39.9992496147752, -83.0249323043972);
                        break;
                    case "Campus Loop South (CLS)":
                        Task downloadMapRoute4 = ProcessMapURLAsync(routeMapURL + "321,324,151" + "&nocache=" + Environment.TickCount, client);
                        await downloadMapRoute4;
                        mapLineColor = Color.FromArgb(255, 192, 38, 44);
                        MyMap.ZoomLevel = 14;
                        MyMap.Center = new GeoCoordinate(39.9992496147752, -83.0249323043972);
                        break;
                    case "East Residential (ER)":
                        Task downloadMapRoute5 = ProcessMapURLAsync(routeMapURL + "327,155" + "&nocache=" + Environment.TickCount, client);
                        await downloadMapRoute5;
                        mapLineColor = Color.FromArgb(255, 17, 53, 128);
                        MyMap.ZoomLevel = 14;
                        MyMap.Center = new GeoCoordinate(40.0039741583169, -83.006053827703);
                        break;
                    case "Med Center Express (MC)":
                        Task downloadMapRoute6 = ProcessMapURLAsync(routeMapURL + "276,283" + "&nocache=" + Environment.TickCount, client);
                        await downloadMapRoute6;
                        mapLineColor = Color.FromArgb(255, 240, 89, 148);
                        MyMap.ZoomLevel = 14;
                        MyMap.Center = new GeoCoordinate(40.0053485389799, -83.02444816567);
                        break;
                    case "North Express (NE)":
                        Task downloadMapRoute7 = ProcessMapURLAsync(routeMapURL + "184,237" + "&nocache=" + Environment.TickCount, client);
                        await downloadMapRoute7;
                        mapLineColor = Color.FromArgb(255, 241, 96, 33);
                        MyMap.ZoomLevel = 14;
                        MyMap.Center = new GeoCoordinate(40.0020967796445, -83.0253587756306);
                        break;

                }
            }

        }

        async Task ProcessMapURLAsync(string url, HttpClient client)
        {
            try
            {
                string results = await client.GetStringAsync(url);
                //Debug.WriteLine(results);

                ParseRouteWaypointList(results);
            }
            catch
            {
                //MessageBox.Show("There was an error retrieving the waypoints for the stop map. Please check your internect connection.");
                var color = new Color { A = 0xFF, R = 0xA2, G = 0x2B, B = 0x2B };
                ToastPrompt toast = new ToastPrompt();
                toast.Title = "ERROR";
                toast.Message = "Download Failed. Check Connection.";
                toast.Background = new SolidColorBrush(color);
                toast.ImageSource = new BitmapImage(new Uri("/AppIconSmall.png", UriKind.RelativeOrAbsolute));
                toast.Show();
                stpName.Text = "Error Retrieving Stops";
            }

        }

        //This method sets up the feed of BusTime and adds it to the database of stored times
        private void ParseRouteWaypointList(string stopXML)
        {
            StringReader stringReader = new StringReader(stopXML);
            XmlReader xmlReader = XmlReader.Create(stringReader);
            Double lat = 0, lon = 0;

            //Prevent work from being done on UI thread
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                MyCoordinates.Clear();
                while (xmlReader.ReadToFollowing("lat"))
                {
                    lat = xmlReader.ReadElementContentAsDouble();

                    if (xmlReader.ReadToFollowing("lon"))
                    {
                        lon = xmlReader.ReadElementContentAsDouble();
                    }
                    
                    GeoCoordinate wayPointGeoCoor = new GeoCoordinate(lat, lon);  

                    MyCoordinates.Add(wayPointGeoCoor);
                }

                var gc = new GeoCoordinateCollection();
                foreach (var geo in MyCoordinates)
                {
                    gc.Add(geo);
                }

                Polyline = new MapPolyline();
                Polyline.Path = gc;

                Polyline.StrokeColor = mapLineColor;
                Polyline.StrokeThickness = 5;
                MyMap.MapElements.Add(Polyline);

                Task placePins = GetStopPushpins();
            });

        }


        private async Task GetStopPushpins()
        {

            HttpClient client = new HttpClient();

            BusRoute temp = (BusRoute)mapRoutePicker.SelectedItem;
            String direction = "&dir=Circular";
            switch (temp.Name)
            {
                case "Buckeye Village (BV)":
                    Task downloadMapRoute1 = ProcessStopMapURLAsync(routeStopsURL + "BV" + direction + "&nocache=" + Environment.TickCount, client);
                    stopDotColor = "#FF00803F";
                    await downloadMapRoute1;
                    break;
                case "Central Connector (CC)":
                    Task downloadMapRoute2 = ProcessStopMapURLAsync(routeStopsURL + "CC" + direction + "&nocache=" + Environment.TickCount, client);
                    stopDotColor = "#FF4680D3";
                    await downloadMapRoute2;
                    break;
                case "Campus Loop North (CLN)":
                    Task downloadMapRoute3 = ProcessStopMapURLAsync(routeStopsURL + "CLN" + direction + "&nocache=" + Environment.TickCount, client);
                    stopDotColor = "#FF77787A";
                    await downloadMapRoute3;
                    break;
                case "Campus Loop South (CLS)":
                    Task downloadMapRoute4 = ProcessStopMapURLAsync(routeStopsURL + "CLS" + direction + "&nocache=" + Environment.TickCount, client);
                    stopDotColor = "#FFC0262C";
                    await downloadMapRoute4;
                    break;
                case "East Residential (ER)":
                    Task downloadMapRoute5 = ProcessStopMapURLAsync(routeStopsURL + "ER" + direction + "&nocache=" + Environment.TickCount, client);
                    stopDotColor = "#FF113580";
                    await downloadMapRoute5;
                    break;
                case "Med Center Express (MC)":
                    Task downloadMapRoute6 = ProcessStopMapURLAsync(routeStopsURL + "MC" + direction + "&nocache=" + Environment.TickCount, client);
                    stopDotColor = "#FFF05994";
                    await downloadMapRoute6;
                    break;
                case "North Express (NE)":
                    Task downloadMapRoute7 = ProcessStopMapURLAsync(routeStopsURL + "NE" + direction + "&nocache=" + Environment.TickCount, client);
                    stopDotColor = "#FFF26021";
                    await downloadMapRoute7;
                    break;
            }
        }

        async Task ProcessStopMapURLAsync(string url, HttpClient client)
        {
            try
            {
                string results = await client.GetStringAsync(url);
                //Debug.WriteLine(results);
                ParseStopMapList(results);
            }
            catch
            {
                //MessageBox.Show("There was an error retrieving the stops for the route map. Please check your internet connection.");
                var color = new Color { A = 0xFF, R = 0xA2, G = 0x2B, B = 0x2B };
                ToastPrompt toast = new ToastPrompt();
                toast.Title = "ERROR";
                toast.Message = "Download Failed. Check Connection.";
                toast.Background = new SolidColorBrush(color);
                toast.ImageSource = new BitmapImage(new Uri("/AppIconSmall.png", UriKind.RelativeOrAbsolute));
                toast.Show();
                stpName.Text = "Error Retrieving Stops";
            }

        }

        //This method sets up the feed of BusTime and adds it to the database of stored times
        private void ParseStopMapList(string stopXML)
        {
            StringReader stringReader = new StringReader(stopXML);
            XmlReader xmlReader = XmlReader.Create(stringReader);
            string stopName = "";
            Double lat = 0, lon = 0;
            int stopID = 0;


            //Prevent work from being done on UI thread
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                stopPushpins.Clear();
                while (xmlReader.ReadToFollowing("stpid"))
                {
                    stopID = Convert.ToInt16(xmlReader.ReadElementContentAsString());
                    //Debug.WriteLine("Stop Num is: " + stopID);
                    if (xmlReader.ReadToFollowing("stpnm"))
                    {
                        stopName = xmlReader.ReadElementContentAsString();
                    }
                    if (xmlReader.ReadToFollowing("lat"))
                    {
                        lat = xmlReader.ReadElementContentAsDouble();
                    }
                    if (xmlReader.ReadToFollowing("lon"))
                    {
                        lon = xmlReader.ReadElementContentAsDouble();
                    }

                    GeoCoordinate stopCoordinate = new GeoCoordinate(lat, lon);
                    if (!(stopID == 94 || stopID == 95 || stopID == 90 || stopID == 91))
                    {
                        stopPushpins.Add(new BusStop() { Name = stopName, StopNum = stopID, location = stopCoordinate, stopColor = stopDotColor });
                    }
                }
            });


            ObservableCollection<DependencyObject> children = MapExtensions.GetChildren(MyMap);
            var obj =
                 children.FirstOrDefault(x => x.GetType() == typeof(MapItemsControl)) as MapItemsControl;

            obj.ItemsSource = stopPushpins;

            Task loadBusPushpins = GetBusPushpins();

        }

        private async Task GetBusPushpins()
        {

            HttpClient client = new HttpClient();

            BusRoute temp = (BusRoute)mapRoutePicker.SelectedItem;
            switch (temp.Name)
            {
                case "Buckeye Village (BV)":
                    Task downloadMapRoute1 = ProcessBusLocationURLAsync(routeBusLocationURL + "BV" + "&nocache=" + Environment.TickCount, client);
                    await downloadMapRoute1;
                    break;
                case "Central Connector (CC)":
                    Task downloadMapRoute2 = ProcessBusLocationURLAsync(routeBusLocationURL + "CC" + "&nocache=" + Environment.TickCount, client);
                    await downloadMapRoute2;
                    break;
                case "Campus Loop North (CLN)":
                    Task downloadMapRoute3 = ProcessBusLocationURLAsync(routeBusLocationURL + "CLN" + "&nocache=" + Environment.TickCount, client);
                    await downloadMapRoute3;
                    break;
                case "Campus Loop South (CLS)":
                    Task downloadMapRoute4 = ProcessBusLocationURLAsync(routeBusLocationURL + "CLS" + "&nocache=" + Environment.TickCount, client);
                    await downloadMapRoute4;
                    break;
                case "East Residential (ER)":
                    Task downloadMapRoute5 = ProcessBusLocationURLAsync(routeBusLocationURL + "ER" + "&nocache=" + Environment.TickCount, client);
                    await downloadMapRoute5;
                    break;
                case "Med Center Express (MC)":
                    Task downloadMapRoute6 = ProcessBusLocationURLAsync(routeBusLocationURL + "MC" + "&nocache=" + Environment.TickCount, client);
                    await downloadMapRoute6;
                    break;
                case "North Express (NE)":
                    Task downloadMapRoute7 = ProcessBusLocationURLAsync(routeBusLocationURL + "NE" + "&nocache=" + Environment.TickCount, client);
                    await downloadMapRoute7;
                    break;
            }
        }

        async Task ProcessBusLocationURLAsync(string url, HttpClient client)
        {
            try
            {
                string results = await client.GetStringAsync(url);
                //Debug.WriteLine(results);
                ParseBusLocationList(results);
            }
            catch
            {
                //MessageBox.Show("There was an error retrieving the bus locations. Please Check your internet connection.");
                var color = new Color { A = 0xFF, R = 0xA2, G = 0x2B, B = 0x2B };
                ToastPrompt toast = new ToastPrompt();
                toast.Title = "ERROR";
                toast.Message = "Download Failed. Check Connection.";
                toast.Background = new SolidColorBrush(color);
                toast.ImageSource = new BitmapImage(new Uri("/AppIconSmall.png", UriKind.RelativeOrAbsolute));
                toast.Show();
                stpName.Text = "Error Retrieving Stops";
            }

        }

        //This method sets up the feed of BusTime and adds it to the database of stored times
        private void ParseBusLocationList(string stopXML)
        {
            StringReader stringReader = new StringReader(stopXML);
            XmlReader xmlReader = XmlReader.Create(stringReader);
            string rtName = "";
            Double lat = 0, lon = 0;
            int busID = 0;

            //Prevent work from being done on UI thread
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                busPushpins.Clear();
                while (xmlReader.ReadToFollowing("vid"))
                {
                    busID = Convert.ToInt16(xmlReader.ReadElementContentAsString());
                    //Debug.WriteLine("Stop Num is: " + stopID);
                    if (xmlReader.ReadToFollowing("lat"))
                    {
                        lat = xmlReader.ReadElementContentAsDouble();
                    }
                    if (xmlReader.ReadToFollowing("lon"))
                    {
                        lon = xmlReader.ReadElementContentAsDouble();
                    }
                    if (xmlReader.ReadToFollowing("rt"))
                    {
                        rtName = xmlReader.ReadElementContentAsString();
                    }


                    GeoCoordinate stopCoordinate = new GeoCoordinate(lat, lon);

                    busPushpins.Add(new Bus() { BusNum = busID, BusRoute = rtName, location = stopCoordinate, stopColor = stopDotColor });

                }
                //Debug.WriteLine("Bus To Be Refreshed: ");
                //foreach(Bus temp in busPushpins)
                //{
                //    Debug.WriteLine("Bus ID: " + temp.BusNum + " Bus Route: " + rtName + " Location: " + temp.location);
                //}
            });




            ObservableCollection<DependencyObject> children = MapExtensions.GetChildren(MyMap);

            var obj =
                    children.Last(x => x.GetType() == typeof(MapItemsControl)) as MapItemsControl;

            obj.ItemsSource = busPushpins;
        }

        private void StopPushpin_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            BusStop temp = ((FrameworkElement)e.OriginalSource).DataContext
                                                             as BusStop;
            int stopID = temp.StopNum;
            //MessageBox.Show("ID: " + stopID);

            //Retrieve times based on the passed stop ID
            currentStop = stopID;
            stopSelected = true;
            String stop = timesURL + currentStop;
            RetreiveCABSInfo(stop);

            //If auto refresh setting is turned on, and it is not currently refreshing,
            //enable app to run under lockscreen, and start auto refresh timer, start indeterminate progress bar
            if (settings.RefreshCheckBoxSetting && !(dt.IsEnabled))
            {
                dt.Start();
                timerRunning = true;
                //Progress indicator for refreshing
                SystemTray.ProgressIndicator = new ProgressIndicator();
                SystemTray.IsVisible = true;
                SystemTray.ProgressIndicator.Text = "Refreshing...";
                SystemTray.ProgressIndicator.IsIndeterminate = true;
                SystemTray.ProgressIndicator.IsVisible = true;
                PhoneApplicationService.Current.ApplicationIdleDetectionMode = IdleDetectionMode.Disabled;
            }

            //Navigate pivot to results pivot item, set refresh time, and temporary loading stop time.
            PivotControl.SelectedIndex = 1;

            stpName.Text = "Loading Buses...";
            refreshTime.Visibility = Visibility.Visible;
            refreshTime.Text = "Last Refreshed: " + Convert.ToString(DateTime.Now);
        }

        private void indvBusPushpins_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Bus temp = ((FrameworkElement)e.OriginalSource).DataContext
                                                             as Bus;

            PhoneApplicationService.Current.State["selBusID"] = temp.BusNum;

            NavigationService.Navigate(new Uri("/View/SingleBusTimes.xaml", UriKind.Relative));
        }


    }
}