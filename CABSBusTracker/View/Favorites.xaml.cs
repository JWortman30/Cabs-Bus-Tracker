using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using CABSBusTracker.Model;

namespace CABSBusTracker
{
    public partial class Favorites : PhoneApplicationPage
    {
            
        public Favorites()
        {
            InitializeComponent();
            this.DataContext = App.ViewModel2;
            ApplicationBar.Mode = ApplicationBarMode.Minimized;

            //If running on a 15:9 device, adjust size of the longlistselector
            double ScreenHeight = Application.Current.Host.Content.ActualHeight;
            if (ScreenHeight == 800)
            {
                favoriteStops.Height = 580;
            }
        }

        //Clears all stored favorites and notifies the user
        private void ClearAllFavorites_Click(object sender, EventArgs e)
        {            
            App.ViewModel2.ClearFavoriteItems();
            MessageBox.Show("Your favorite stops have been cleared.");
        }

        //Deletes the favorite that was long clicked
        private void DeleteFavorite_Click(object sender, RoutedEventArgs e)
        {

            var item = (sender as MenuItem).DataContext;
            CABSBusTracker.Model.FavoriteItem selFavorite = (CABSBusTracker.Model.FavoriteItem)item;
            if (selFavorite != null)
            {
                //Remove the to-do item from the observable collection.
                App.ViewModel2.FavoriteStopsItems.Remove(selFavorite);

                App.ViewModel2.RemoveStopItem(selFavorite);

                this.Focus();
            }
        }

        //Navigates to the results page of the stop that was tapped.
        private void FavoriteStop_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            //CABSBusTracker.Model.FavoriteItem temp = (CABSBusTracker.Model.FavoriteItem)favoriteStops.SelectedItem;
            FavoriteItem temp = ((FrameworkElement)e.OriginalSource).DataContext
                                                             as FavoriteItem;
            int stopID = temp.StopID;
            //MessageBox.Show("ID: " + stopID);

            PhoneApplicationService.Current.State["selStopID"] = stopID;
            NavigationService.Navigate(new Uri("/View/BusTimesResults.xaml", UriKind.Relative));
        }
    }
}