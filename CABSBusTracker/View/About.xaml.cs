using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace CABSBusTracker
{
    public partial class About : PhoneApplicationPage
    {
        public About()
        {
            InitializeComponent();
        }

        private void BobHall_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Microsoft.Phone.Tasks.WebBrowserTask wbt = new Microsoft.Phone.Tasks.WebBrowserTask();
                wbt.Uri = new Uri("http://www.flickr.com/photos/houseofhall/");
                wbt.Show();
            }
            catch { }

        }

        private void License_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Microsoft.Phone.Tasks.WebBrowserTask wbt = new Microsoft.Phone.Tasks.WebBrowserTask();
                wbt.Uri = new Uri("http://creativecommons.org/licenses/by-sa/2.0/");
                wbt.Show();
            }
            catch { }
        }

        private void MSPublicLicense_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Microsoft.Phone.Tasks.WebBrowserTask wbt = new Microsoft.Phone.Tasks.WebBrowserTask();
                wbt.Uri = new Uri("http://www.microsoft.com/en-us/openness/licenses.aspx");
                wbt.Show();
            }
            catch { }
        }
    }
}