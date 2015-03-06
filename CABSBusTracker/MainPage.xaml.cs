using CABSBusTracker.Model;
using Coding4Fun.Toolkit.Controls;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Marketplace;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Xml;

namespace CABSBusTracker
{
    public partial class MainPage : PhoneApplicationPage
    {

        private static string requestURL = "http://trip.osu.edu/bustime/api/v1/getservicebulletins?key=8GYvqZDsLLPgiAYM6SVjV3S7X&rt=";
        private static ObservableCollection<Alert> Alerts = new ObservableCollection<Alert>();
        private Boolean retreivedNotifications = false;
        private Boolean failed = false;
        private Boolean trialIncrease = false;
        private static LicenseInformation _licenseInfo = new LicenseInformation();
        MarketplaceDetailTask _marketPlaceDetailTask = new MarketplaceDetailTask();
               
        // Constructor
        public MainPage()
        {
            InitializeComponent();
            
            this.DataContext = App.ViewModel3;
            ApplicationBar.Mode = ApplicationBarMode.Minimized;

            PhoneApplicationService.Current.State["selStopID"] = -1;            
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //Determine if the page was navigated to via alert pop-up, and if it was, slide to pivot item 1
            string strItemIndex;

            Boolean doubleCheck = (Boolean)PhoneApplicationService.Current.State["needToNavigateToPage1"];


            if (NavigationContext.QueryString.TryGetValue("goto", out strItemIndex) && doubleCheck)
            {
                PivotControl.SelectedIndex = Convert.ToInt32(strItemIndex);
                retreivedNotifications = true;
                PhoneApplicationService.Current.State["needToNavigateToPage1"] = false;
                Debug.WriteLine("Navigating to page 1.");
            }


            base.OnNavigatedTo(e);


            //Read application settings, and check for alerts if settings is turned on
            AppSettings settings = new AppSettings();

            if (settings.NotificationsCheckBoxSetting && retreivedNotifications == false)
            {

                App.ViewModel3.ClearAlerts();
                Task downloadNot = FetchNotifications();
                if (PivotControl.Items.Count != 2)
                {
                    PivotControl.Items.Add(alertsPage);
                }

            }
            
            //If alerts are not turned on, remove alert pivot item from apge
            if (!settings.NotificationsCheckBoxSetting)
            {
                PivotControl.Items.Remove(alertsPage);
            }
            //MessageBox.Show("Test: " + settings.TrialDisplayDays);
            if((Application.Current as App).IsTrial && (settings.TrialDisplayDays == -1 || settings.TrialDisplayDays == 60))
            {
                SystemTray.Opacity = 1;
                CustomMessageBox custMessageBox = new CustomMessageBox()
                {
                    Caption = "Enjoying the App?",
                    Message = "Thanks for trying CABS Bus Tracker! The only difference between the trial version and the paid version of this application, is that this message will display once every 60 uses. If you find this application useful, please consider purchasing the application to support the developer. Thanks!",
                    LeftButtonContent = "Continue",
                    RightButtonContent = "Buy"
                };

                custMessageBox.Dismissed += (s1, e1) =>
                {
                    switch (e1.Result)
                    {
                        case CustomMessageBoxResult.LeftButton:
                            SystemTray.Opacity = 0;
                            break;
                        case CustomMessageBoxResult.RightButton:
                            SystemTray.Opacity = 0;
                            _marketPlaceDetailTask.Show();
                            break;
                        case CustomMessageBoxResult.None:
                            SystemTray.Opacity = 0;
                            break;
                        default:
                            SystemTray.Opacity = 0;
                            break;
                    }
                };
                custMessageBox.Show();
                settings.TrialDisplayDays = 0;
            }
            else if(trialIncrease == false)
            {
                settings.TrialDisplayDays = settings.TrialDisplayDays + 1;
                trialIncrease = true;
            }

            //ApplicationBar.BackgroundColor = new Color { A = 0xFF, R = 0xA2, G = 0x2B, B = 0x2B };
        }

        //Launch the stop selector page
        private void chooseStopClick(object sender, RoutedEventArgs e)
        {            
            NavigationService.Navigate(new Uri("/View/BusTimesResults.xaml", UriKind.Relative));
        }

        //Launch the about page
        private void About_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/View/About.xaml", UriKind.Relative));
        }

        //Launch the favorite stops page
        private void favoriteStopsClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/View/Favorites.xaml", UriKind.Relative));
        }

        //Launch the nearby stops page
        private void nearbyStopsClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/View/NearbyStops.xaml", UriKind.Relative));
        }

        private void displayNotifications()
        {
            if (App.ViewModel3.AlertResults.Count > 1)
            {
                SystemTray.IsVisible = false;
                //Create pop-up notification for multiple service alerts, tells user how many alerts there are
                var color = new Color { A = 0xFF, R = 0xA2, G = 0x2B, B = 0x2B };
                ToastPrompt toast = new ToastPrompt();
                toast.Title = "SERVICE ALERTS";
                toast.Message = App.ViewModel3.AlertResults.Count + " service alerts";
                toast.Background = new SolidColorBrush(color);
                toast.ImageSource = new BitmapImage(new Uri("AppIconSmall.png", UriKind.RelativeOrAbsolute));
                toast.Completed += toast_Completed;
                toast.Show();
            }
            else if (App.ViewModel3.AlertResults.Count == 1)
            {
                SystemTray.IsVisible = false;
                //Create pop-up notification for a single service alert, includes title of alert in notification.
                var color = new Color { A = 0xFF, R = 0xA2, G = 0x2B, B = 0x2B };
                ToastPrompt toast = new ToastPrompt();
                toast.Title = "ALERT";
                if (App.ViewModel3.AlertResults[0].Title.Length > 20)
                {
                    toast.Message = App.ViewModel3.AlertResults[0].Title.Substring(0, 20) + "...";
                }
                else
                {
                    toast.Message = App.ViewModel3.AlertResults[0].Title;
                }
               
                toast.Background = new SolidColorBrush(color);
                toast.ImageSource = new BitmapImage(new Uri("AppIconSmall.png", UriKind.RelativeOrAbsolute));
                toast.Completed += toast_Completed;
                toast.Show();
            }

        }


        void toast_Completed(object sender, PopUpEventArgs<string, PopUpResult> e)
        {
            
            //If the toast was interacted with do one of the following
            if (e.PopUpResult.Equals(PopUpResult.Ok))
            {
                //If toast was clicked on the mainpage, open the alerts page, otherwise navigate to mainpage, and open the alerts page
                var currentPage = ((PhoneApplicationFrame)Application.Current.RootVisual).Content;
                Uri thisPage = new Uri("/MainPage.xaml", UriKind.Relative);
                if (NavigationService.CurrentSource.Equals(thisPage))
                {
                    PivotControl.SelectedIndex = 1;
                }
                else
                {
                    NavigationService.Navigate(new Uri("/MainPage.xaml?goto=1", UriKind.Relative));
                    PhoneApplicationService.Current.State["needToNavigateToPage1"] = true;
                }
            }
            SystemTray.IsVisible = true;
            
        }

        //Open the settings page
        private void Settings_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/View/SettingsPage.xaml", UriKind.Relative));
        }



        private async Task FetchNotifications()
        {
            if (failed == false)
            {
                //Create HttpClient to request alerts
                HttpClient client = new HttpClient();

                //Create and run alert download tasks, set retreived notifications to true, so it only runs once, display notifications.
                if (failed == false)
                {
                    Task download1 = ProcessURLAsync(requestURL + "BV" + "&nocache=" + Environment.TickCount, client);
                    await download1;
                }
                if (failed == false)
                {
                    Task download2 = ProcessURLAsync(requestURL + "CC" + "&nocache=" + Environment.TickCount, client);
                    await download2;
                }

                if (failed == false)
                {
                    Task download3 = ProcessURLAsync(requestURL + "CLN" + "&nocache=" + Environment.TickCount, client);
                    await download3;
                }

                if (failed == false)
                {
                    Task download4 = ProcessURLAsync(requestURL + "CLS" + "&nocache=" + Environment.TickCount, client);
                    await download4;
                }

                if (failed == false)
                {
                    Task download5 = ProcessURLAsync(requestURL + "ER" + "&nocache=" + Environment.TickCount, client);
                    await download5;
                }

                if (failed == false)
                {
                    Task download6 = ProcessURLAsync(requestURL + "MC" + "&nocache=" + Environment.TickCount, client);
                    await download6;
                }

                if (failed == false)
                {
                    Task download7 = ProcessURLAsync(requestURL + "NE" + "&nocache=" + Environment.TickCount, client);
                    await download7;
                    retreivedNotifications = true;

                    displayNotifications();
                }


            }

        }

        //Downloads the xml of the alerts, if error occurs, notifies user.
        async Task ProcessURLAsync(string url, HttpClient client)
        {
            try
            {
                string results = await client.GetStringAsync(url);
                ParseAlertList(results);
            }
            catch
            {
                //MessageBox.Show("There was an error retrieving the notifications");
                failed = true;
                var color = new Color { A = 0xFF, R = 0xA2, G = 0x2B, B = 0x2B };
                ToastPrompt toast = new ToastPrompt();
                toast.Title = "ERROR";
                toast.Message = "Download Failed. Check Connection.";
                toast.Background = new SolidColorBrush(color);
                toast.ImageSource = new BitmapImage(new Uri("/AppIconSmall.png", UriKind.RelativeOrAbsolute));
                toast.Show();
            }
            
        }


        //This method sets up the feed of alerts, and adds new alerts to the observable collection
        private void ParseAlertList(string alertXML)
        {
            //Load bus alerts into xmlReader
            StringReader stringReader = new StringReader(alertXML);
            XmlReader xmlReader = XmlReader.Create(stringReader);
            string curTitle = "", curContent = "";


            //Prevent work from being done on UI thread
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                //Debug.WriteLine("I am starting to parse the string" + alertXML);
                while (xmlReader.ReadToFollowing("sbj"))
                {
                    curTitle = xmlReader.ReadElementContentAsString();
                    Debug.WriteLine("Detected subject is " + curTitle);

                    if (xmlReader.ReadToFollowing("brf"))
                    {
                        curContent = xmlReader.ReadElementContentAsString();
                        Debug.WriteLine("And its content is: " + curContent);
                    }
                    App.ViewModel3.AddAlert(new Alert() { Title = curTitle, Content = curContent });
                }

                Debug.WriteLine("Alert Count: " + Alerts.Count);
                

            });

            
        }

        private void Help_Click(object sender, EventArgs e)
        {
            try
            {
                Microsoft.Phone.Tasks.WebBrowserTask wbt = new Microsoft.Phone.Tasks.WebBrowserTask();
                wbt.Uri = new Uri("http://cabsbustrackerwp.uservoice.com");
                wbt.Show();
            }
            catch { }
        }


    }
}