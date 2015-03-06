using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using CABSBusTracker.Resources;
using System.Xml;
using System.IO;
using Microsoft.Phone.Tasks;
using System.Collections.ObjectModel;
using CABSBusTracker.Model;
using CABSBusTracker.Functions;
using System.Windows.Threading;
using System.Windows.Media;
using System.Diagnostics;
using Microsoft.Phone.Maps.Toolkit;
using System.Threading.Tasks;
using System.Net.Http;
using Coding4Fun.Toolkit.Controls;
using System.Windows.Media.Imaging;


namespace CABSBusTracker
{
    public partial class SingleBusTimes : PhoneApplicationPage
    {
        private int BusID = -1;
        //URL for Bus GPS Location API request
        private String routeBusLocationsURL = "http://trip.osu.edu/bustime/api/v1/getpredictions?key=8GYvqZDsLLPgiAYM6SVjV3S7X&vid=";
        //Collection of Bus Stops for the Route Map
        ObservableCollection<ArrivalTime> busFutureStops = new ObservableCollection<ArrivalTime>();

        //Creates a timer to allow auto refreshing of bus prediction times
        System.Windows.Threading.DispatcherTimer dt = new System.Windows.Threading.DispatcherTimer();

        //Loads the settings from storage
        AppSettings settings = new AppSettings();

        

        public SingleBusTimes()
        {
            InitializeComponent();
            this.DataContext = App.ViewModel5;

            //set up timer
            dt.Interval = new TimeSpan(0, 0, 0, 0, 30000); // 30 seconds
            dt.Tick += new EventHandler(dt_Tick);

            //If running on a 15:9 device, adjust size of the longlistselector
            double ScreenHeight = Application.Current.Host.Content.ActualHeight;
            if (ScreenHeight == 800)
            {
                retreivedSingleBusTimes.Height = 475;
            }


        }

        //retrieve bus times every 30 seconds, and update refreshed time
        void dt_Tick(object sender, EventArgs e)
        {
            dt.Stop();
            Debug.WriteLine("DT_Tick Run");
            busFutureStops.Clear();
            //App.ViewModel5.ClearArrivalTimes();
            GetBusFutureStops();
            refreshTime.Text = "Last Refreshed: " + Convert.ToString(DateTime.Now);
            dt.Start();
        }
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            busFutureStops.Clear();
            App.ViewModel5.ClearArrivalTimes();
            var k = PhoneApplicationService.Current.State["selBusID"];
            BusID = (int)k;
            Debug.WriteLine("Bus ID to reteive: " + BusID);
            PhoneApplicationService.Current.State["selBusID"] = -1;
            if (BusID != -1)
            {
                stpName.Text = "Loading...";
                GetBusFutureStops();                
                refreshTime.Text = "Last Refreshed: " + Convert.ToString(DateTime.Now);
            }

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

            if (settings.RefreshCheckBoxSetting)
            {
                dt.Start();

                //Progress indicator for refreshing
                SystemTray.ProgressIndicator = new ProgressIndicator();
                SystemTray.IsVisible = true;
                SystemTray.ProgressIndicator.Text = "Refreshing...";
                SystemTray.ProgressIndicator.IsIndeterminate = true;
                SystemTray.ProgressIndicator.IsVisible = true;
                

                ApplicationBar.Buttons.Add(stopRefreshing);
            }
            else
            {
                ApplicationBar.Buttons.Add(refresh);
            }
        }

        //Stops the timer when the page is navigated away from
        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {

            dt.Stop();
            Debug.WriteLine("Stopping timer of navigation from");
            base.OnNavigatedFrom(e);

        }

        //Manually refresh times
        private void refresh_Click(object sender, EventArgs e)
        {
            stpName.Text = "Loading...";
            busFutureStops.Clear();
            App.ViewModel5.ClearArrivalTimes();
            GetBusFutureStops();
            refreshTime.Text = "Last Refreshed: " + Convert.ToString(DateTime.Now);
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

           dt.Start();

           SystemTray.ProgressIndicator.IsVisible = true;

           ApplicationBar.Buttons.RemoveAt(0);

           ApplicationBar.Buttons.Add(stopRefreshing);
        }

        //If auto-refresh is on stop refreshing times, and replace stop button with refresh button
        private void stopRefreshing_Click(object sender, EventArgs e)
        {
            ApplicationBarIconButton startRefreshing = new ApplicationBarIconButton();
            EventHandler startRefreshingClick = new EventHandler(startRefreshing_Click);
            startRefreshing.Text = "start refreshing";
            startRefreshing.Click += startRefreshingClick;
            startRefreshing.IconUri = new Uri("/Icons/refresh.png", UriKind.Relative);

            dt.Stop();

            SystemTray.ProgressIndicator.IsVisible = false;

            ApplicationBar.Buttons.RemoveAt(0);

            ApplicationBar.Buttons.Add(startRefreshing);
        }

        private async Task GetBusFutureStops()
        {

            HttpClient client = new HttpClient();

            Task downloadMapRoute1 = ProcessBusFutureStopsURLAsync(routeBusLocationsURL + BusID + "&nocache=" + Environment.TickCount, client);
            await downloadMapRoute1;

        }

        async Task ProcessBusFutureStopsURLAsync(string url, HttpClient client)
        {
            try
            {
                string results = await client.GetStringAsync(url);
                //Debug.WriteLine(results);
                ParseFutureBusStopList(results);
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
        private void ParseFutureBusStopList(string busXML)
        {
            StringReader stringReader = new StringReader(busXML);
            XmlReader xmlReader = XmlReader.Create(stringReader);
            string stopName = "", routeName = "", arrivalTime = "", busColor = "", timeToArrive = "";
            int stopID = 0, minutes = 0, fakeBusID = 1;


            //Prevent work from being done on UI thread
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                while (xmlReader.ReadToFollowing("stpnm"))
                {
                    stopName = xmlReader.ReadElementContentAsString();
                    

                    //Debug.WriteLine("Stop Num is: " + stopID);
                    if (xmlReader.ReadToFollowing("stpid"))
                    {
                        stopID = Convert.ToInt16(xmlReader.ReadElementContentAsString());
                    }
                    if (xmlReader.ReadToFollowing("rt"))
                    {
                        routeName = xmlReader.ReadElementContentAsString();
                        //Debug.WriteLine(routeName);
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
                    busFutureStops.Add(
                        new ArrivalTime { RouteName = routeName, 
                        ExpectedTime = timeToArrive, 
                        routeColor = busColor, 
                        expectedTimeMinutes = minutes, 
                        Destination = stopName, 
                        busID = Convert.ToString(fakeBusID)});

                    fakeBusID++;
                }
                App.ViewModel5.InsertBusTimes(busFutureStops);
                stpName.Text = "Bus ID: " + BusID;
            });
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
    }
}