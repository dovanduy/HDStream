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
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using System.IO;
using Hammock;
using Hammock.Serialization;
using Hammock.Web;
using Hammock.Authentication;
using Hammock.Authentication.Basic;
using Hammock.Authentication.OAuth;
using Microsoft.Xna.Framework.GamerServices;
using System.Net.NetworkInformation;

namespace HDStream
{
    public partial class FacebookWrite : PhoneApplicationPage
    {
        private Stream imgstream;
        private IsolatedStorageSettings settings;
        private string emptystr;


        public FacebookWrite()
        {
            settings = IsolatedStorageSettings.ApplicationSettings;
            emptystr = "What's on your mind?";
            imgstream = null;
            Loaded += new RoutedEventHandler(MainPage_Loaded);
            InitializeComponent();
        }

        // Load data for the ViewModel Items
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                MessageBox.Show("This application must require for internet connection. Please check your internet connection status", "Sorry", MessageBoxButton.OK);
                this.NavigationService.GoBack();
            }
            if (!settings.Contains("facebook_name"))
            {
                MessageBox.Show("Facebook account doesn't set-up. Please check your Facebook acount", "Sorry", MessageBoxButton.OK);
                this.NavigationService.GoBack();
                return;
            }
            title.Text = "FACEBOOK - " + settings["facebook_name"];


        }

        private void Border_MouseEnter(object sender, MouseEventArgs e)
        {
            keyboard.Visibility = Visibility.Visible;
        }

        private void Border_MouseLeave(object sender, MouseEventArgs e)
        {
            
        }

        private void keyboard_ContentChanged(object sender, KoreanKeyboard.ContentChangedEventArgs e)
        {
            if (keyboard.txt != "")
            {
                SolidColorBrush Brush1 = new SolidColorBrush();
                Brush1.Color = Colors.Black;
                WatermarkTB.Foreground = Brush1;
            }
            else
            {
                SolidColorBrush Brush1 = new SolidColorBrush();
                Brush1.Color = Colors.Gray;
                WatermarkTB.Foreground = Brush1;
                WatermarkTB.Text = emptystr;
            }

            WatermarkTB.Text = keyboard.txt;
        }

        private void ApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            RestClient client2 = new RestClient
            {
                Authority = "https://graph.facebook.com/",
            };

            RestRequest request2 = new RestRequest
            {
                Path = "/me/feed?message=" + WatermarkTB.Text
            };
            if (imgstream != null)
            {
                string albumId = (string)settings["facebook_photo"];
                request2 = new RestRequest
                {
                    Path = albumId + "/photos?message=" + WatermarkTB.Text
                };
                request2.AddFile("photo", "image.jpg", imgstream);
            }
            request2.AddField("access_token", (string)settings["facebook_token"]);
            var callback = new RestCallback(
                (restRequest, restResponse, userState) =>
                {
                    // Callback when signalled
                }
                );
            client2.BeginRequest(request2, callback);

            
            MessageBox.Show("Share successfully.", "Thanks", MessageBoxButton.OK);
            this.NavigationService.GoBack();
        }

        private void ApplicationBarIconButton_Click_1(object sender, EventArgs e)
        {
            this.NavigationService.GoBack();
        }

        private void ApplicationBarIconButton_Click_2(object sender, EventArgs e)
        {
            Guide.BeginShowMessageBox("Notice", "Please choose getting photo storage",
                new List<string> { "Camera", "Picture" }, 0, MessageBoxIcon.None,
                asyncResult =>
                {
                    int? returned = Guide.EndShowMessageBox(asyncResult);
                    switch (returned)
                    {
                        case 0:
                            CameraCaptureTask cimg = new CameraCaptureTask();
                            cimg.Completed += new EventHandler<PhotoResult>(photoChooserTask_Completed);
                            cimg.Show();
                            break;
                        case 1:
                            PhotoChooserTask pimg = new PhotoChooserTask();
                            pimg.Completed += new EventHandler<PhotoResult>(photoChooserTask_Completed);
                            pimg.Show();
                            break;
                        default:
                            break;
                    }
                }, null);
        }

        private void photoChooserTask_Completed(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                imgstream = e.ChosenPhoto;                    
            }
        }
        
    }
}