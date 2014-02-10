using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.IO.IsolatedStorage;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Net.NetworkInformation;
using TweetSharp;

namespace HDStream
{
    public partial class TweetView : PhoneApplicationPage
    {
        private TwitterService service;
        private String id;
        private IsolatedStorageSettings settings;
        private string emptystr;
        public TweetView()
        {
            InitializeComponent();
            emptystr = "What's on your mind?";

            settings = IsolatedStorageSettings.ApplicationSettings;
            this.Loaded += new RoutedEventHandler(ListPage_Loaded);
            service = new TwitterService("g8F2KdKH40gGp9BXemw13Q", "OyFRFsI05agcJtURtLv8lpYbYRwZAIL5gr5xQNPW0Q");
            service.AuthenticateWith((string)settings["twitter_token"], (string)settings["twitter_tokensecret"]);
        }
        private void ListPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                MessageBox.Show("This application must require for internet connection. Please check your internet connection status", "Sorry", MessageBoxButton.OK);
                this.NavigationService.GoBack();
            }
            String thumb_img, name, text, time;
            NavigationContext.QueryString.TryGetValue("id", out id);
            NavigationContext.QueryString.TryGetValue("thumb_img", out thumb_img);
            NavigationContext.QueryString.TryGetValue("name", out name);
            NavigationContext.QueryString.TryGetValue("text", out text);
            NavigationContext.QueryString.TryGetValue("time", out time);
            ApplicationTitle.Text = String.Format("@{0}", name);            
            hour.Text = time;
            txt.Text = text;
            img.Source = new BitmapImage(new Uri(thumb_img));
            
        }

        private void keyboard_ContentChanged(object sender, KoreanKeyboard.ContentChangedEventArgs e)
        {
            if (keyboard.txt != "")
            {
                SolidColorBrush Brush1 = new SolidColorBrush();
                Brush1.Color = Colors.Black;
                WatermarkTB.Foreground = Brush1;
                WatermarkTB.Text = ApplicationTitle.Text + " " + keyboard.txt;
            }
            else
            {
                SolidColorBrush Brush1 = new SolidColorBrush();
                Brush1.Color = Colors.Gray;
                WatermarkTB.Foreground = Brush1;
                WatermarkTB.Text = emptystr;
            }

            
        }

        private void Border_MouseEnter(object sender, MouseEventArgs e)
        {
            ScrollViewer.Height = 280;            
            ScrollViewer.UpdateLayout();
            ScrollViewer.ScrollToVerticalOffset(double.MaxValue);            
            KeyboardPanel.Visibility = Visibility.Visible;
        }

        private void Border_MouseLeave(object sender, MouseEventArgs e)
        {
            
        }

        private void ApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            if(keyboard.txt != "")
            {
                string tweet = WatermarkTB.Text;
                long lid = System.Convert.ToInt64(id);
                service.SendTweet(tweet, lid, (tweets, response) =>
                {

                });
                MessageBox.Show("Share successfully.", "Thanks", MessageBoxButton.OK);
                this.NavigationService.GoBack();
            }
        }

        private void ApplicationBarIconButton_Click_1(object sender, EventArgs e)
        {

        }
    }
}