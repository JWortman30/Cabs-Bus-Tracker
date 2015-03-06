using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using CABSBusTracker;

namespace CABSBusTracker
{
    public partial class SettingsPage : PhoneApplicationPage
    {
        AppSettings setting = new AppSettings();
        public SettingsPage()
        {
            InitializeComponent();
        }

        private void autoRefreshEnabled_Click(object sender, RoutedEventArgs e)
        {
            
            if(setting.RefreshCheckBoxSetting){
                MessageBox.Show("WARNING! Enabling of auto refresh will also enable the application to run under the lockscreen once times have been requested. This allows the application to continue to refresh the times even if the screen shuts off, which can result in increased battery and data usage if left running.");
            }
            
        }

        private void notificationsSetting_Click(object sender, RoutedEventArgs e)
        {
            if (setting.NotificationsCheckBoxSetting)
            {
                MessageBox.Show("You will now receive in-app notifications on the main screen in the event of route changes or other service modifications and notices.");
            }
        }


    }
}