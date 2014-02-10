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
using Hammock;
using Hammock.Serialization;
using Hammock.Web;
using Hammock.Authentication;
using Hammock.Authentication.Basic;
using Hammock.Authentication.OAuth;
using Microsoft.Xna.Framework.GamerServices;
using System.Net.NetworkInformation;

using TweetSharp;

namespace HDStream
{
    public partial class TwitterWrite : PhoneApplicationPage
    {
        private Boolean img_bool;
        private string twit_pic;
        private IsolatedStorageSettings settings;
        private string emptystr;

        public TwitterWrite()
        {
            settings = IsolatedStorageSettings.ApplicationSettings;
            emptystr = "What's on your mind?";
            twit_pic = "";
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
            if (!settings.Contains("twitter_screenname"))
            {
                MessageBox.Show("Twtter account doesn't set-up. Please check your twitter acount", "Sorry", MessageBoxButton.OK);
                this.NavigationService.GoBack();
                return;
            }
            title.Text = "TWITTER - @" + settings["twitter_screenname"];

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
            if (img_bool == true && twit_pic == "")
            {
                MessageBox.Show("Image upload in progress. please wait.", "Sorry", MessageBoxButton.OK);
                return;
            }

            if (WatermarkTB.Text == emptystr)
            {
                MessageBox.Show("Please input your mind :)", "Sorry", MessageBoxButton.OK);
                return;
            }

            TwitterService service = new TwitterService("g8F2KdKH40gGp9BXemw13Q", "OyFRFsI05agcJtURtLv8lpYbYRwZAIL5gr5xQNPW0Q");
            service.AuthenticateWith((string)settings["twitter_token"], (string)settings["twitter_tokensecret"]);
            string tweet = WatermarkTB.Text;
            if (img_bool == true)
                tweet += " " + twit_pic;

            service.SendTweet(tweet,
                (tweets, response) =>
                {

                });
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
                img_bool = true;
                var client = new RestClient
                {
                    Authority = "http://api.twipl.net/",
                    VersionPath = "1",
                };

                var request = new RestRequest
                {
                    Path = "upload.json" // can be upload.xml or whatever, but you have to parse it accordingly
                };

                request.AddFile("media1", "img.jpg", e.ChosenPhoto);
                request.AddField("key", "b76ecda29f7c47e0bfefd0b458e91fb5");
                request.AddField("oauth_token", (string)settings["twitter_token"]);
                request.AddField("oauth_secret", (string)settings["twitter_tokensecret"]);
                request.AddField("message", "");
                 client.BeginRequest(request, new RestCallback(Callback));
                    
             }
        }

        private void Callback(Hammock.RestRequest request, Hammock.RestResponse response, object userState)
        {
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Newtonsoft.Json.Linq.JObject o = Newtonsoft.Json.Linq.JObject.Parse(response.Content); // Parse the JSON from the response
                string url = (string)o["mediaurl"]; // Get the image's url
                twit_pic = url;
            }
        }

    }
}