using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.IO.IsolatedStorage;
using Microsoft.Phone.Tasks;

namespace HDStream
{
    public partial class Privarcy : PhoneApplicationPage
    {
        IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
        public Privarcy()
        {
            InitializeComponent();
        }
        bool pageAlreadyVisited = false;

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (pageAlreadyVisited)
            {
                if (NavigationService.CanGoBack == true)
                    NavigationService.GoBack();
            }

            base.OnNavigatedTo(e);

            pageAlreadyVisited = true;
            // Anything else you want to do
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            settings["locchk"] = 1;
            settings.Save();
            this.NavigationService.Navigate(new Uri("/MainMenu.xaml", UriKind.RelativeOrAbsolute));
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/MainMenu.xaml", UriKind.RelativeOrAbsolute));
        }

        private void TextBlock_GotFocus(object sender, RoutedEventArgs e)
        {

        }

        private void TextBlock_MouseEnter(object sender, MouseEventArgs e)
        {
            WebBrowserTask task = new WebBrowserTask();
            task.URL = HttpUtility.UrlEncode("http://lhd1413.sshel.com/privarcy.html");
            task.Show();

        }
    }
}