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
using System.IO.IsolatedStorage;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using TweetSharp;
using System.Net.NetworkInformation;

namespace HDStream
{
    public partial class TwitterMessageWrite : PhoneApplicationPage
    {
        private TwitterService service;
        private IsolatedStorageSettings settings;
        private string emptystr;
        private string emptystr2;

        public TwitterMessageWrite()
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                MessageBox.Show("This application must require for internet connection. Please check your internet connection status", "Sorry", MessageBoxButton.OK);
                this.NavigationService.GoBack();
            }
            InitializeComponent();
            settings = IsolatedStorageSettings.ApplicationSettings;
            service = new TwitterService("g8F2KdKH40gGp9BXemw13Q", "OyFRFsI05agcJtURtLv8lpYbYRwZAIL5gr5xQNPW0Q");
            service.AuthenticateWith((string)settings["twitter_token"], (string)settings["twitter_tokensecret"]);
            emptystr = "User ID";
            emptystr2 = "What's on your mind?";
        }

        private void ApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            if (id.Text == emptystr)
            {
                MessageBox.Show("Please input user id", "Sorry", MessageBoxButton.OK);
                return;
            }

            if (WatermarkTB.Text == emptystr2)
            {
                MessageBox.Show("Please Input your mind :)", "Sorry", MessageBoxButton.OK);
                return;
            }

            service.SendDirectMessage(id.Text, WatermarkTB.Text,
                (tweets, response) =>
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        
                    }
                    Dispatcher.BeginInvoke(delegate()
                    {
                        MessageBox.Show("Send successfuly", "Thanks", MessageBoxButton.OK);
                        this.NavigationService.GoBack();
                    });
                });


        }

        private void keyboard_ContentChanged(object sender, KoreanKeyboard.ContentChangedEventArgs e)
        {
            if (keyboard.txt != "")
            {
                SolidColorBrush Brush1 = new SolidColorBrush();
                Brush1.Color = Colors.Black;
                WatermarkTB.Foreground = Brush1;
                WatermarkTB.Text = keyboard.txt;
            }
            else
            {
                SolidColorBrush Brush1 = new SolidColorBrush();
                Brush1.Color = Colors.Gray;
                WatermarkTB.Foreground = Brush1;
                WatermarkTB.Text = emptystr2;
            }
        }

        private void Border_MouseEnter(object sender, MouseEventArgs e)
        {
            keyboard.Visibility = Visibility.Visible;
        }

        private void id_MouseEnter(object sender, MouseEventArgs e)
        {
            keyboard.Visibility = Visibility.Collapsed;
        
        }

        private void id_GotFocus(object sender, RoutedEventArgs e)
        {
            if (id.Text == emptystr)
            {
                id.Text = "";
                SolidColorBrush Brush1 = new SolidColorBrush();
                Brush1.Color = Colors.Black;
                id.Foreground = Brush1;
            }
        }

        private void id_LostFocus(object sender, RoutedEventArgs e)
        {
            if (id.Text == String.Empty)
            {
                id.Text = emptystr;
                SolidColorBrush Brush2 = new SolidColorBrush();
                Brush2.Color = Colors.Gray;
                id.Foreground = Brush2;
            }
        }
    }
}