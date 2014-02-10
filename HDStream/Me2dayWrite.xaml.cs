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
    public partial class Me2dayWrite : PhoneApplicationPage
    {
        private Stream imgstream;
        private IsolatedStorageSettings settings;
        private string emptystr;
        private string me2_string;
        private string tag_string;
        private Boolean me2_bool;


        public Me2dayWrite()
        {
            me2_bool = true;
            me2_string = "";
            tag_string = "";
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
            if (!settings.Contains("me2day_userid"))
            {
                MessageBox.Show("Me2day account doesn't set-up. Please check your Me2day acount", "Sorry", MessageBoxButton.OK);
                this.NavigationService.GoBack();
                return;
            }
            title.Text = "ME2DAY - " + settings["me2day_userid"];
        }

        private void Border_MouseEnter(object sender, MouseEventArgs e)
        {
            me2_bool = true;
            KeyboardPanel.Visibility = Visibility.Visible;
            keyboard.Changehan(0, me2_string);
        }

        private void Border2_MouseEnter(object sender, MouseEventArgs e)
        {
            me2_bool = false;
            ScrollViewer.Height = 200;
            ScrollViewer.UpdateLayout();
            ScrollViewer.ScrollToVerticalOffset(double.MaxValue);    
            KeyboardPanel.Visibility = Visibility.Visible;
            keyboard.Changehan(1, tag_string);
        }

        private void keyboard_ContentChanged(object sender, KoreanKeyboard.ContentChangedEventArgs e)
        {
            if (me2_bool == true)
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
                me2_string = WatermarkTB.Text;
            }
            else
            {
                if (keyboard.txt != "")
                {
                    SolidColorBrush Brush1 = new SolidColorBrush();
                    Brush1.Color = Colors.Black;
                    WatermarkTB2.Foreground = Brush1;
                }
                else
                {
                    SolidColorBrush Brush1 = new SolidColorBrush();
                    Brush1.Color = Colors.Gray;
                    WatermarkTB2.Foreground = Brush1;
                    WatermarkTB2.Text = emptystr;
                }

                WatermarkTB2.Text = keyboard.txt;
                tag_string = WatermarkTB2.Text;
            }
        }

        private void ApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            string auth_key = String.Format("full_auth_token {0}", settings["me2day_token"]);
            IWebCredentials credentials = new BasicAuthCredentials
            {
                Username = (string)settings["me2day_userid"],
                Password = auth_key
            };

            RestClient client = new RestClient
            {
                Authority = "http://me2day.net/api/"
            };

            RestRequest request = new RestRequest
            {
                Credentials = credentials,
                Path = String.Format("create_post/{0}.xml?akey=aed420d038f9b1a7fe3b5c0d94df22f5", settings["me2day_userid"])
            };
            request.AddParameter("post[body]", WatermarkTB.Text);
            request.AddParameter("post[tags]", WatermarkTB2.Text);

            if (imgstream != null)
            {
                request.AddFile("attachment", "image.jpg", imgstream, "image/jpeg");
            }

            var callback = new RestCallback(
                (restRequest, restResponse, userState) =>
                {
                    // Callback when signalled
                }
            );

            client.BeginRequest(request, callback);


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
        
        private void TitlePanel_MouseEnter(object sender, MouseEventArgs e)
        {
            KeyboardPanel.Visibility = Visibility.Collapsed;
            ScrollViewer.Height = 600;
            ScrollViewer.UpdateLayout();
        }
    }
}