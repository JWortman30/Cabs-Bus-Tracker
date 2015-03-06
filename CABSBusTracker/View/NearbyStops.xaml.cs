using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Threading.Tasks;
using System.Net.Http;
using System.Diagnostics;
using System.Xml;
using System.IO;
using CABSBusTracker.Model;
using CABSBusTracker.Functions;
using System.Collections.ObjectModel;
using System.Device.Location;
using Windows.Devices.Geolocation;
using Microsoft.Phone.Tasks;
using Coding4Fun.Toolkit.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;


namespace CABSBusTracker
{
    public partial class NearbyStops : PhoneApplicationPage
    {
        private static string requestURL = "http://trip.osu.edu/bustime/api/v1/getstops?key=8GYvqZDsLLPgiAYM6SVjV3S7X&rt=";
        private static string direction = "&dir=Circular";
        private ObservableCollection<BusStop> NearByStops = new ObservableCollection<BusStop>();
        public NearbyStops()
        {
            InitializeComponent();
            this.DataContext = App.ViewModel4;
            double ScreenHeight = Application.Current.Host.Content.ActualHeight;
            if (ScreenHeight == 800)
            {
                nearbyStops.Height = 610;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

            //Progress indicator for refreshing
            SystemTray.ProgressIndicator = new ProgressIndicator();
            SystemTray.IsVisible = true;
            SystemTray.ProgressIndicator.Text = "Loading Nearby Stops...";
            SystemTray.ProgressIndicator.IsIndeterminate = false;
            SystemTray.ProgressIndicator.IsVisible = false;

            base.OnNavigatedTo(e);

            if (!(e.NavigationMode == System.Windows.Navigation.NavigationMode.Back))
            {
                App.ViewModel4.ClearStops();
                Task downloadNot = FindNearbyStops();
            }

        }

        private async Task FindNearbyStops()
        {
            // Declare an HttpClient object, and increase the buffer size. The 
            // default buffer size is 65,536.
            HttpClient client = new HttpClient();

            //Progress indicator for refreshing
            SystemTray.ProgressIndicator = new ProgressIndicator();
            SystemTray.IsVisible = true;
            SystemTray.ProgressIndicator.Text = "Loading Nearby Stops...";
            SystemTray.ProgressIndicator.IsIndeterminate = true;
            SystemTray.ProgressIndicator.IsVisible = true;

            // Create and start the tasks. As each task finishes, DisplayResults  
            // displays its length.
            Task download1 = ProcessURLAsync(requestURL + "BV" + direction + "&nocache=" + Environment.TickCount, client);

            Task download2 = ProcessURLAsync(requestURL + "CC" + direction + "&nocache=" + Environment.TickCount, client);

            Task download3 = ProcessURLAsync(requestURL + "CLN" + direction + "&nocache=" + Environment.TickCount, client);

            Task download4 = ProcessURLAsync(requestURL + "CLS" + direction + "&nocache=" + Environment.TickCount, client);

            Task download5 = ProcessURLAsync(requestURL + "ER" + direction + "&nocache=" + Environment.TickCount, client);

            Task download6 = ProcessURLAsync(requestURL + "MC" + direction + "&nocache=" + Environment.TickCount, client);

            Task download7 = ProcessURLAsync(requestURL + "NE" + direction + "&nocache=" + Environment.TickCount, client);
            await download1;
            await download2;
            await download3;
            await download4;
            await download5;
            await download6;
            await download7;


            var SortedStops = (from stop in NearByStops
                              orderby stop.numDistanceFromLocation
                              select stop).ToList<BusStop>();

            //Populate the GroupedOC
            foreach (BusStop stop in SortedStops)
            {

                    App.ViewModel4.AddStop(stop);

            }

            //Progress indicator for refreshing
            SystemTray.ProgressIndicator = new ProgressIndicator();
            SystemTray.IsVisible = true;
            SystemTray.ProgressIndicator.Text = "Loading Nearby Stops...";
            SystemTray.ProgressIndicator.IsIndeterminate = false;
            SystemTray.ProgressIndicator.IsVisible = false;

            if(SortedStops.Count == 0)
            {
                MessageBox.Show("There were no nearby stops found.");
            }
        }

        void LoadingBusStops_Completed(object sender, EventArgs e)
        {
            //Progress indicator for refreshing
            SystemTray.ProgressIndicator = new ProgressIndicator();
            SystemTray.IsVisible = true;
            SystemTray.ProgressIndicator.Text = "Loading Nearby Stops...";
            SystemTray.ProgressIndicator.IsIndeterminate = false;
            SystemTray.ProgressIndicator.IsVisible = false;
        }

        async Task ProcessURLAsync(string url, HttpClient client)
        {
            try
            {
                string results = await client.GetStringAsync(url);
                //Debug.WriteLine(results);
                ParseStopList(results);
            }
            catch
            {
                //MessageBox.Show("There was an error retrieving the some or all of the Bus Stops. Please check your network connection.");
                var color = new Color { A = 0xFF, R = 0xA2, G = 0x2B, B = 0x2B };
                ToastPrompt toast = new ToastPrompt();
                toast.Title = "ERROR";
                toast.Message = "Download Failed. Check Connection.";
                toast.Background = new SolidColorBrush(color);
                toast.ImageSource = new BitmapImage(new Uri("/AppIconSmall.png", UriKind.RelativeOrAbsolute));
                toast.Show();
            }

        }

        //This method sets up the feed of BusTime and adds it to the database of stored times
        private void ParseStopList(string stopXML)
        {
            //Load bus times into xmlReader
            StringReader stringReader = new StringReader(stopXML);
            XmlReader xmlReader = XmlReader.Create(stringReader);
            string curTitle = "", curContent = "";
            Double lat = 0, lon = 0;


            //Prevent work from being done on UI thread
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                while (xmlReader.ReadToFollowing("stpid"))
                {
                    curTitle = xmlReader.ReadElementContentAsString();
                    //Debug.WriteLine("Detected stopid is " + curTitle);

                    if (xmlReader.ReadToFollowing("stpnm"))
                    {
                        curContent = xmlReader.ReadElementContentAsString();
                        //Debug.WriteLine("And its content is: " + curContent);
                    }
                    if (xmlReader.ReadToFollowing("lat"))
                    {
                        lat = Convert.ToDouble(xmlReader.ReadElementContentAsString());
                        //Debug.WriteLine("And its lat is: " + lat);
                    }
                    if (xmlReader.ReadToFollowing("lon"))
                    {
                        lon = Convert.ToDouble(xmlReader.ReadElementContentAsString());
                        //Debug.WriteLine("And its lon is: " + lon);
                    }
                    GeoCoordinate stopGeoCoor = new GeoCoordinate(lat, lon);
                    GeoCoordinateWatcher curLocFinder = new GeoCoordinateWatcher(GeoPositionAccuracy.High);
                    curLocFinder.Start();
                    GeoCoordinate curLoc = curLocFinder.Position.Location;
                    //Debug.WriteLine("Current Lat = " + curLoc.Latitude + " Current lon = " + curLoc.Longitude);
                    Double distance = DistanceCalculator.CalcDistance(curLoc, stopGeoCoor);
                    distance = Math.Round(distance, 2);
                    if (distance < 1000)
                    {
                        NearByStops.Add(new BusStop() { Name = curContent, 
                            StopNum = Convert.ToInt32(curTitle), 
                            distanceFromLocation = "Distance: " + Convert.ToString(distance) + " Meters", 
                            numDistanceFromLocation = distance,
                            location = stopGeoCoor});
                    }
                }


                Debug.WriteLine("Count of BusStops is: " + NearByStops.Count);

            });


        }
        private void NearbyStop_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            BusStop temp = ((FrameworkElement)e.OriginalSource).DataContext
                                                             as BusStop;
            int stopID = temp.StopNum;
            //MessageBox.Show("ID: " + stopID);

            PhoneApplicationService.Current.State["selStopID"] = stopID;
            NavigationService.Navigate(new Uri("/View/BusTimesResults.xaml", UriKind.Relative));
        }

        private void NavigateToStop_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            BusStop temp = ((FrameworkElement)e.OriginalSource).DataContext 
                                                             as BusStop;
            MapsDirectionsTask mapsDirectionsTask = new MapsDirectionsTask();
            LabeledMapLocation busStopLocation = new LabeledMapLocation(temp.Name, temp.location);
            mapsDirectionsTask.End = busStopLocation;
            mapsDirectionsTask.Show();
            //MessageBox.Show("Clicking this will eventually open maps app and provide navigation instructions to the selected stop");
            //NavigationService.Navigate(new Uri("/BusTimesResults.xaml", UriKind.Relative));
        }
    }
}