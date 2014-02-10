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
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.IO.IsolatedStorage;
using Microsoft.Phone.Tasks;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using TweetSharp;
using Hammock;
using Hammock.Serialization;
using Hammock.Web;
using Hammock.Authentication;
using Hammock.Authentication.Basic;
using Hammock.Authentication.OAuth;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using HDStream.Model;
using Microsoft.Xna.Framework.GamerServices;
using System.Net.NetworkInformation;


namespace HDStream
{
    public partial class MainPage : PhoneApplicationPage
    {
        private Boolean share_status;
        private Boolean write_bool;
        private Boolean keyboard_bool;

        private Boolean t_chk;
        private Boolean f_chk;
        private Boolean m_chk;

        private Boolean tr_chk;
        private Boolean fr_chk;
        private Boolean mr_chk;

        private Boolean tw_chk;
        private Boolean fw_chk;
        private Boolean mw_chk;

        private String emptystr = "";
        private IsolatedStorageSettings settings;
        private Boolean img_bool;
        private string twit_pic;
        private List<NotifiView> lists;
        // Constructor
        public MainPage()
        {
            img_bool = false;
            write_bool = false;
            keyboard_bool = false;
            emptystr = "What's on your mind?";
            InitializeComponent();
            Loaded += new RoutedEventHandler(MainPage_Loaded);
        }

        // Load data for the ViewModel Items
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            loadtext.Visibility = Visibility.Collapsed;
            pgbar.Visibility = Visibility.Collapsed;
            wrapPanel.Children.Clear();
            settings = IsolatedStorageSettings.ApplicationSettings;
            
                string twitter_screenname = (string)settings["twitter_screenname"];
                string twitter_token = (string)settings["twitter_token"];
                string twitter_tokensecret = (string)settings["twitter_tokensecret"];
            
            
                string me2day_userid = (string)settings["me2day_userid"];
                string me2day_token = (string)settings["me2day_token"];

                if (!settings.Contains("locchk"))
                {
                    toggle.IsChecked = false;
                    toggle.Content = "Off";
                }
                else
                {
                    toggle.IsChecked = true;
                    toggle.Content = "On";
                }
            
                            
            Border b = new Border()
            {
                Width = 120,
                Height = 120,
                BorderThickness = new Thickness(0),
                Margin = new Thickness(10)
            };
            TiltEffect.SetIsTiltEnabled(b, true);
            ImageBrush berriesBrush = new ImageBrush();

            berriesBrush.ImageSource =
                new BitmapImage(
                    new Uri(@"twitter.png", UriKind.Relative)
                );

            b.Background = berriesBrush;
            b.DataContext = "0";
            GestureListener listener = GestureService.GetGestureListener(b);
            listener.Tap += new EventHandler<GestureEventArgs>(WrapPanelSample_Tap);
            TextBlock tb = new TextBlock();
            if (settings.Contains("twitter_screenname"))
                tb.Text = " " + settings["twitter_screenname"];
            else
                tb.Text = " Set account";
            tb.FontSize = 20;
            tb.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            tb.VerticalAlignment = VerticalAlignment.Bottom;

            b.Child = tb;
            wrapPanel.Children.Add(b);
            b = new Border()
            {
                Width = 120,
                Height = 120,
                BorderThickness = new Thickness(0),
                Margin = new Thickness(10)
            };
            berriesBrush = new ImageBrush();
            TiltEffect.SetIsTiltEnabled(b, true);
            berriesBrush.ImageSource =
                new BitmapImage(
                    new Uri(@"facebook.png", UriKind.Relative)
                );

            b.Background = berriesBrush;
            b.DataContext = "1";
            listener = GestureService.GetGestureListener(b);
            listener.Tap += new EventHandler<GestureEventArgs>(WrapPanelSample_Tap);
            tb = new TextBlock();
            if (settings.Contains("facebook_name"))
                tb.Text = " " + settings["facebook_name"];
            else
                tb.Text = " Set account";
            tb.FontSize = 20;
            tb.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            tb.VerticalAlignment = VerticalAlignment.Bottom;

            b.Child = tb;
            wrapPanel.Children.Add(b);

            b = new Border()
            {
                Width = 120,
                Height = 120,
                BorderThickness = new Thickness(0),
                Margin = new Thickness(10)
            };
            berriesBrush = new ImageBrush();
            TiltEffect.SetIsTiltEnabled(b, true);
            berriesBrush.ImageSource =
                new BitmapImage(
                    new Uri(@"me2day.png", UriKind.Relative)
                );

            b.Background = berriesBrush;
            b.DataContext = "2";
            listener = GestureService.GetGestureListener(b);
            listener.Tap += new EventHandler<GestureEventArgs>(WrapPanelSample_Tap);
            tb = new TextBlock();
            if (settings.Contains("me2day_userid"))
                tb.Text = " " + settings["me2day_userid"];
            else
                tb.Text = " Set account";
            tb.FontSize = 20;
            tb.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            tb.VerticalAlignment = VerticalAlignment.Bottom;

            b.Child = tb;
            wrapPanel.Children.Add(b);

            if (settings.Contains("facebook_chk"))
            {
                f_chk = true;
                facebook_chk.IsChecked = true;
                write_bool = true;
            }

            if (settings.Contains("twitter_chk"))
            {
                t_chk = true;
                twitter_chk.IsChecked = true;
                write_bool = true;
            }

            if (settings.Contains("me2day_chk"))
            {
                m_chk = true;
                me2day_chk.IsChecked = true;
                write_bool = true;
            }

            if (write_bool == false)
                share.IsEnabled = false;
            else
                share.IsEnabled = true;
            
        }

        private void WrapPanelSample_Tap(object sender, GestureEventArgs e)
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                MessageBox.Show("This application must require for internet connection. Please check your internet connection status", "Sorry", MessageBoxButton.OK);
                return;
            }
            if (share_status == true)
            {
                MessageBox.Show("Please wait for share completed", "Sorry", MessageBoxButton.OK);
                return;
            }
            Border b = (Border)sender;
            if ((string)b.DataContext == "0")
            {
                if (settings.Contains("twitter_token"))
                    NavigationService.Navigate(new Uri("/TwitterView.xaml", UriKind.Relative));
                else
                    NavigationService.Navigate(new Uri("/TwitterAuth.xaml", UriKind.Relative));
            }
            else if ((string)b.DataContext == "1")
                if (settings.Contains("facebook_token"))
                    NavigationService.Navigate(new Uri("/FacebookView.xaml", UriKind.Relative));
                else
                    NavigationService.Navigate(new Uri("/FacebookAuth.xaml", UriKind.Relative));
            else if ((string)b.DataContext == "2")
                if (settings.Contains("me2day_token"))
                    NavigationService.Navigate(new Uri("/Me2dayView.xaml", UriKind.Relative)); 
                else
                    NavigationService.Navigate(new Uri("/Me2dayAuth.xaml", UriKind.Relative)); 
        }

        private void toggle_Checked(object sender, RoutedEventArgs e)
        {
            IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
            toggle.Content = "On";
            settings["locchk"] = 1;
            settings.Save();

        }

        private void toggle_Unchecked(object sender, RoutedEventArgs e)
        {
            IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
            toggle.Content = "Off";
            settings.Remove("locchk");
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //history_label.Visibility = Visibility.Visible;
            IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
            settings.Remove("twitter_chk");
            settings.Remove("me2day_chk");
            settings.Remove("facebook_chk");
            settings.Remove("locchk");
            settings["pushchk"] = "false";
            settings["history"] = "";
            toggle.Content = "Off";
            toggle.IsChecked = false;
            settings.Save();
            MessageBox.Show("Initialize all data.", "Confirm", MessageBoxButton.OK);
        }

        private void TextBlock_MouseEnter(object sender, MouseEventArgs e)
        {
            WebBrowserTask task = new WebBrowserTask();
            task.URL = HttpUtility.UrlEncode("http://lhd1413.sshel.com/privarcy.html");
            task.Show();
        }

        private void Border_MouseEnter(object sender, MouseEventArgs e)
        {
            keyboard_bool = true;
            keyboard.Visibility = Visibility.Visible;
            
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

        
        private void PanoramaRoot_MouseEnter(object sender, MouseEventArgs e)
        {
            if (keyboard_bool == false)
                keyboard.Visibility = Visibility.Collapsed;
        }

        private void Border_MouseLeave(object sender, MouseEventArgs e)
        {
            keyboard_bool = false;
        }

        private void twitter_chk_Click(object sender, RoutedEventArgs e)
        {
            if (twitter_chk.IsChecked == false)
            {
                settings.Remove("twitter_chk");
            }
            else
            {
                if (settings.Contains("twitter_token"))
                {
                    settings["twitter_chk"] = 1;
                    settings.Save();
                }
                else
                {
                    MessageBox.Show("Twitter account hasn't available. Please setting your account", "Sorry", MessageBoxButton.OK);
                    twitter_chk.IsChecked = false;
                }
            }
        }

        private void facebook_chk_Click(object sender, RoutedEventArgs e)
        {
            if (facebook_chk.IsChecked == false)
            {
                settings.Remove("facebook_chk");
            }
            else
            {
                if (settings.Contains("facebook_token"))
                {
                    settings["facebook_chk"] = 1;
                    settings.Save();
                }
                else
                {
                    MessageBox.Show("Facebook account hasn't available. Please setting your account", "Sorry", MessageBoxButton.OK);
                    facebook_chk.IsChecked = false;
                }
            }
        }

        private void me2day_chk_Click(object sender, RoutedEventArgs e)
        {
            if (me2day_chk.IsChecked == false)
            {
                settings.Remove("me2day_chk");
            }
            else
            {
                if (settings.Contains("me2day_token"))
                {
                    settings["me2day_chk"] = 1;
                    settings.Save();
                }
                else
                {
                    MessageBox.Show("Me2day account hasn't available. Please setting your account", "Sorry", MessageBoxButton.OK);
                    me2day_chk.IsChecked = false;
                }
            }
        }

        private void share_Click(object sender, RoutedEventArgs e)
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                MessageBox.Show("This application must require for internet connection. Please check your internet connection status", "Sorry", MessageBoxButton.OK);
                return;
            }
            if (twitter_chk.IsChecked == false && facebook_chk.IsChecked == false && me2day_chk.IsChecked == false)
            {
                MessageBox.Show("Any account hasn't availiable. Please setting your account.", "Sorry", MessageBoxButton.OK);
                return;
            }
            else
            {
                IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication();
                if (WatermarkTB.Text.Length == 0 || WatermarkTB.Text == emptystr)
                {
                    MessageBox.Show("Please input your minds :)", "Sorry", MessageBoxButton.OK);
                    return;
                }
                share_status = true;
                wait_text.Text = "Share in progress.. Pleas wait";
                share_pg.Visibility = Visibility.Visible;
                tw_chk = false;
                mw_chk = false;
                fw_chk = false;
                share.Visibility = Visibility.Collapsed;

                if (me2day_chk.IsChecked == true)
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

                    if (img_bool)
                    {
                        IsolatedStorageFileStream fileStream2 = myIsolatedStorage.OpenFile("img.jpg", FileMode.Open, FileAccess.Read);
                        request.AddFile("attachment", "image.jpg", fileStream2, "image/jpeg");                        
                    }
                    
                    var callback = new RestCallback(
                        (restRequest, restResponse, userState) =>
                        {
                            Dispatcher.BeginInvoke(delegate()
                            {
                                mw_chk = true;
                                chk_share();
                            });
                        }
                    );

                    client.BeginRequest(request, callback);
                    
                }

                if (twitter_chk.IsChecked == true)
                {
                    TwitterService service = new TwitterService("g8F2KdKH40gGp9BXemw13Q", "OyFRFsI05agcJtURtLv8lpYbYRwZAIL5gr5xQNPW0Q");
                    service.AuthenticateWith((string)settings["twitter_token"], (string)settings["twitter_tokensecret"]);
                    string tweet = WatermarkTB.Text;
                    if (img_bool == true)
                        tweet += " " + twit_pic;

                    service.SendTweet(tweet,
                        (tweets, response) =>
                        {
                            Dispatcher.BeginInvoke(delegate()
                            {
                                tw_chk = true;
                                chk_share();
                            });
                        });
                    
                }

                if (facebook_chk.IsChecked == true)
                {
                    RestClient client2 = new RestClient
                    {
                        Authority = "https://graph.facebook.com/",
                    };

                    RestRequest request2 = new RestRequest
                    {
                        Path = "/me/feed?message=" + WatermarkTB.Text
                    };
                    if (img_bool)
                    {
                        IsolatedStorageFileStream fileStream2 = myIsolatedStorage.OpenFile("img.jpg", FileMode.Open, FileAccess.Read);
                        
                        request2 = new RestRequest
                        {
                            Path = "/photos?message=" + WatermarkTB.Text
                        };
                        request2.AddFile("photo", "image.jpg", fileStream2);
                    }                    
                    request2.AddField("access_token", (string)settings["facebook_token"]);
                    var callback = new RestCallback(
                        (restRequest, restResponse, userState) =>
                        {
                            Dispatcher.BeginInvoke(delegate()
                            {
                                fw_chk = true;
                                chk_share();
                            });
                        }
                        );
                    client2.BeginRequest(request2, callback);
                }
                                
            }
        }

        private void chk_share()
        {
            if (f_chk == true && fw_chk == false)
                return;
            if (m_chk == true && mw_chk == false)
                return;
            if (t_chk == true && tw_chk == false)
                return;
            share_status = false;
            share_pg.Visibility = Visibility.Collapsed;
            wait_text.Text = "Upload in progress.. Please wait";
            keyboard.Init();
            keyboard.txt = "";
            MessageBox.Show("Share successfully.", "Thanks", MessageBoxButton.OK);
            SolidColorBrush Brush1 = new SolidColorBrush();
            Brush1.Color = Colors.Gray;
            WatermarkTB.Foreground = Brush1;
            WatermarkTB.Text = emptystr;
            share.Visibility = Visibility.Visible;
        }

        private void photo_Click(object sender, RoutedEventArgs e)
        {
            if ((string)photo.Content == "Remove a picture")
            {
                img_bool = false;
                share.Content = "Share";
                share.IsEnabled = true;
                MessageBox.Show("Image unselected", "Notice", MessageBoxButton.OK);
                photo.Content = "Add a picture";
            }
            else
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

        }

        private void photoChooserTask_Completed(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                img_bool = true;


                using (IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (myIsolatedStorage.FileExists("img.jpg"))
                    {
                        myIsolatedStorage.DeleteFile("img.jpg");
                    }
                    IsolatedStorageFileStream fileStream = myIsolatedStorage.CreateFile("img.jpg");
                    Uri uri = new Uri("img.jpg", UriKind.Relative);
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.SetSource(e.ChosenPhoto);
                    WriteableBitmap wb = new WriteableBitmap(bitmap);
                    System.Windows.Media.Imaging.Extensions.SaveJpeg(wb, fileStream, wb.PixelWidth, wb.PixelHeight, 0, 85);
                    wb.SaveJpeg(fileStream, wb.PixelWidth, wb.PixelHeight, 0, 85);
                    fileStream.Close();

                    photo.Content = "Remove a picture";
                    if (twitter_chk.IsChecked == true)
                    {
                        share_pg.Visibility = Visibility.Visible;
                        share.Visibility = Visibility.Collapsed;
                        var client = new RestClient
                        {
                            Authority = "http://api.twipl.net/",
                            VersionPath = "1",
                        };

                        var request = new RestRequest
                        {
                            Path = "upload.json" // can be upload.xml or whatever, but you have to parse it accordingly
                        };

                        IsolatedStorageFileStream fileStream2 = myIsolatedStorage.OpenFile("img.jpg", FileMode.Open, FileAccess.Read);
                        request.AddFile("media1", "img.jpg", fileStream2);
                        request.AddField("key", "b76ecda29f7c47e0bfefd0b458e91fb5");
                        request.AddField("oauth_token", (string)settings["twitter_token"]);
                        request.AddField("oauth_secret", (string)settings["twitter_tokensecret"]);
                        request.AddField("message", "");
                        client.BeginRequest(request, new RestCallback(Callback));
                    }
                }
            }
        }

        private void Callback(Hammock.RestRequest request, Hammock.RestResponse response, object userState)
        {   
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Dispatcher.BeginInvoke(delegate()
                {
                    share_pg.Visibility = Visibility.Collapsed;
                    share.Visibility = Visibility.Visible;
                });
                Newtonsoft.Json.Linq.JObject o = Newtonsoft.Json.Linq.JObject.Parse(response.Content); // Parse the JSON from the response
                string url = (string)o["mediaurl"]; // Get the image's url
                twit_pic = url;        
            }
        }

        private void Panorama_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var item = (PanoramaItem)e.AddedItems[0];
                string header = (string)item.Header;
                if (header == "notifications")
                {
                    if (!NetworkInterface.GetIsNetworkAvailable())
                    {
                        loadtext.Text = "Please check internet status";
                        return;
                    }
                    listdata.Visibility = Visibility.Collapsed;
                    loadtext.Visibility = Visibility.Visible;
                    if (m_chk || t_chk || f_chk)
                    {                        
                        pgbar.Visibility = Visibility.Visible;
                        tr_chk = false;
                        fr_chk = false;
                        mr_chk = false;
                    }
                    else
                    {
                        loadtext.Text = "No items";
                    }
                    start_update();  
                }
            }
        }

        private void start_update()
        {
            tr_chk = false;
            fr_chk = false;
            mr_chk = false;
            lists = new List<NotifiView>();
            if (t_chk == true)
            {
                TwitterService service = new TwitterService("g8F2KdKH40gGp9BXemw13Q", "OyFRFsI05agcJtURtLv8lpYbYRwZAIL5gr5xQNPW0Q");
                service.AuthenticateWith((string)settings["twitter_token"], (string)settings["twitter_tokensecret"]);
                service.ListTweetsMentioningMe(0, 10,
                        (tweets, response) =>
                        {
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                tr_chk = true;
                                foreach (var tweet in tweets)
                                {
                                    NotifiView list = new NotifiView();
                                    list.name = tweet.Author.ScreenName;
                                    list.id = tweet.Id.ToString();
                                    list.type = "twitter.png";
                                    list.thumb_img = tweet.User.ProfileImageUrl;
                                    list.text = tweet.Text;
                                    list.dtime = tweet.CreatedDate.ToLocalTime();
                                    Dispatcher.BeginInvoke(delegate()
                                    {                                        
                                        lists.Add(list);
                                    });
                                }
                                Dispatcher.BeginInvoke(delegate()
                                {
                                    check_end();
                                });
                            }
                            
                        });
                
                
            }

            if (f_chk == true)
            {
                string url = String.Format("https://api.facebook.com/method/notifications.getList?access_token={0}&format=json", (string)settings["facebook_token"]);
                WebClient wc = new WebClient();
                wc.DownloadStringCompleted += new DownloadStringCompletedEventHandler(wc_openHandler_facebook);
                wc.DownloadStringAsync(new Uri(url), UriKind.Absolute);
            }

            if (m_chk == true)
            {
                string url = String.Format("http://me2day.net/api/track_comments/{0}.xml?akey=aed420d038f9b1a7fe3b5c0d94df22f5&scope=to_me&count=10", (string)settings["me2day_userid"]);
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
                webRequest.Method = "POST";

                string auth_key = String.Format("full_auth_token {0}", settings["me2day_token"]);
                webRequest.Credentials = new NetworkCredential((string)settings["me2day_userid"], auth_key);
                IAsyncResult token = webRequest.BeginGetResponse(new AsyncCallback(wc_openHandler_me2day), webRequest);
            }

        }

        private void wc_openHandler_me2day(IAsyncResult asynchronousResult)
        {
            try
            {
                WebResponse response = ((HttpWebRequest)asynchronousResult.AsyncState).EndGetResponse(asynchronousResult);
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string responseString = reader.ReadToEnd();

                reader.Close();
                var dom = XDocument.Parse(responseString);

                var items = from item in dom.Descendants("commentToMe")
                            select new Me2View
                            {
                                id = (string)item.Element("post").Element("post_id").Value,
                                name = (string)item.Element("comment").Element("author").Element("nickname").Value,
                                text = (string)item.Element("comment").Element("body").Value,
                                tag = (string)item.Element("post").Element("body"),
                                dtime = (DateTime)item.Element("comment").Element("pubDate"),
                                thumb_img = (string)item.Element("comment").Element("author").Element("face"),
                            };

                int i = 0;
                foreach (Me2View item in items)
                {
                    i++;
                    NotifiView list = new NotifiView();
                    list.id = item.id;
                    list.type = "me2day.png";
                    list.text = HtmlRemoval.StripTagsRegexCompiled(item.text);
                    list.text = list.text.Replace("&lt;", "<");
                    list.text = list.text.Replace("&gt;", ">");
                    list.thumb_img = item.thumb_img;
                    list.dtime = item.dtime;
                    lists.Add(list);
                    if (i > 9)
                        break;
                }
                mr_chk = true;
                check_end();
            }
            catch (WebException ex)
            {

            }
        }

        private void wc_openHandler_facebook(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                string jsonstr = e.Result.ToString();
                JObject obj = JObject.Parse(jsonstr);
                JArray items = (JArray)obj["notifications"];
                
                if (items != null)
                {
                    for (int i = 0; i < items.Count; i++)
                    {
                        JObject item = (JObject)items[i];
                        NotifiView list = new NotifiView();
                        list.id = (string)item["object_id"];
                        list.type = "facebook.png";
                        list.text = (string)item["title_text"];
                        list.dtime = ConvertTimestamp((long)item["created_time"]);
                        list.thumb_img = string.Format("http://graph.facebook.com/{0}/picture", item["sender_id"].ToString());
                        lists.Add(list);
                        if (i > 9)
                            break;
                    }
                }
                fr_chk = true;
                check_end();
            }
        }

        private void check_end()
        {
            if (f_chk == true && fr_chk == false)
                return;
            if (m_chk == true && mr_chk == false)
                return;
            if (t_chk == true && tr_chk == false)
                return;

            pgbar.Visibility = Visibility.Collapsed;
            if (lists.Count == 0)
            {
                loadtext.Text = "No items";
            }
            
            lists.Sort(delegate(NotifiView p1, NotifiView p2) { return p2.dtime.CompareTo(p1.dtime); });


            List<NotifiView> datas = new List<NotifiView>();
            DateTime now = DateTime.Now;
            foreach (NotifiView item in lists)
            {
                TimeSpan tsp = now - item.dtime;
                if (tsp.Days > 0)
                    item.dstring = tsp.Days + "일 전";
                else if (tsp.Hours > 0)
                    item.dstring = tsp.Hours + "시간 전";
                else
                    item.dstring = tsp.Minutes + "분 전";
                datas.Add(item);
            }

            if (lists.Count > 0)
            {
                loadtext.Visibility = Visibility.Collapsed;
                listdata.Visibility = Visibility.Visible;
                listdata.ItemsSource = datas;
            }
        }

        private DateTime ConvertTimestamp(double timestamp)
        {

            //create a new DateTime value based on the Unix Epoch
            DateTime converted = new DateTime(1970, 1, 1, 0, 0, 0, 0);

            //add the timestamp to the value
            DateTime newDateTime = converted.AddSeconds(timestamp);

            //return the value in string format
            return newDateTime.ToLocalTime();
        }

        private void listdata_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var item = (NotifiView)e.AddedItems[0];
                if (item != null)
                {
                    if (share_status == true)
                    {
                        MessageBox.Show("Please wait for share completed", "Sorry", MessageBoxButton.OK);
                        return;
                    }
                    if (item.type == "twitter.png")
                    {
                        NavigationService.Navigate(new Uri("/TweetView.xaml?id=" + item.id + "&thumb_img=" + item.thumb_img + "&name=" + item.name + "&text=" + item.text + "&time=" + item.dtime, UriKind.Relative));
                    }
                    else if (item.type == "me2day.png")
                    {
                        string url = String.Format("http://me2day.net/api/get_posts.xml?post_id={0}&akey=aed420d038f9b1a7fe3b5c0d94df22f5", item.id);
                        HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
                        webRequest.Method = "POST";

                        string auth_key = String.Format("full_auth_token {0}", settings["me2day_token"]);
                        webRequest.Credentials = new NetworkCredential((string)settings["me2day_userid"], auth_key);
                        IAsyncResult token = webRequest.BeginGetResponse(new AsyncCallback(wc_openHandler_me2day2), webRequest);
                    }
                }
            }
        }

        private void wc_openHandler_me2day2(IAsyncResult asynchronousResult)
        {
            try
            {
                WebResponse response = ((HttpWebRequest)asynchronousResult.AsyncState).EndGetResponse(asynchronousResult);
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string responseString = reader.ReadToEnd();

                reader.Close();
                var dom = XDocument.Parse(responseString);
                XElement elem = dom.Element("posts");
                XElement post = elem.Element("post");                
                DateTime now = DateTime.Now;
                string dstring;
                TimeSpan tsp = now - DateTime.Parse(post.Element("pubDate").Value);
                if (tsp.Days > 0)
                    dstring = tsp.Days + "일 전";
                else if (tsp.Hours > 0)
                    dstring = tsp.Hours + "시간 전";
                else
                    dstring = tsp.Minutes + "분 전";

                string me2_id, me2_thumb,  me2_name, me2_txt, me2_tag, me2_me2, me2_cmt;
                me2_id = post.Element("post_id").Value;
                me2_thumb = post.Element("author").Element("face").Value;
                me2_name = post.Element("author").Element("nickname").Value;
                me2_txt = post.Element("textBody").Value;
                me2_tag = post.Element("tagText").Value;
                me2_me2 = post.Element("metooCount").Value;
                me2_cmt = post.Element("commentClosed").Value;
                Dispatcher.BeginInvoke(delegate()
                {
                    NavigationService.Navigate(new Uri("/Me2PostView.xaml?id=" + me2_id + "&thumb_img=" + me2_thumb + "&name=" + me2_name + "&text=" + me2_txt + "&time=" + dstring + "&tag=" + me2_tag + "&me2=" + me2_me2 + "&cmt=" + me2_cmt, UriKind.Relative));
                });
            }
            catch (WebException ex)
            {

            }
        }

    }
}