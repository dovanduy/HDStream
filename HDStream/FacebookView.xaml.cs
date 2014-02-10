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
using System.Xml;
using System.Xml.Linq;
using System.IO;
using HDStream.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.NetworkInformation;

namespace HDStream
{
    public partial class FacebookView : PhoneApplicationPage
    {
        private IsolatedStorageSettings settings;
        public FacebookView()
        {
            InitializeComponent();
            settings = IsolatedStorageSettings.ApplicationSettings;
            Loaded += new RoutedEventHandler(MainPage_Loaded);
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
            title.Title = "FACEBOOK - " + settings["facebook_name"];
            string url = String.Format("https://graph.facebook.com/me/home?access_token={0}", (string)settings["facebook_token"]);
            WebClient wc = new WebClient();
            wc.DownloadStringCompleted += new DownloadStringCompletedEventHandler(wc_openHandler);
            wc.DownloadStringAsync(new Uri(url), UriKind.Absolute);
        }

        private void wc_openHandler(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                string jsonstr = e.Result.ToString();
                JObject obj = JObject.Parse(jsonstr);
                JArray items = (JArray)obj["data"];

                List<FBView> lists = new List<FBView>();
                DateTime now = DateTime.Now;
                for (int i = 0; i < items.Count; i++)
                {

                    JObject item = (JObject)items[i];

                    FBView list = new FBView();
                    list.img1 = "";
                    list.id = (string)item["id"];
                    list.name = (string)item["from"]["name"];
                    if ((string)item["type"] == "status")
                    {
                        list.text = (string)item["message"];
                        list.cap_vis = Visibility.Collapsed;
                    }
                    else
                    {
                        if (item["message"] != null)
                            list.text = (string)item["message"];
                        else
                            list.text = (string)item["name"];
                        list.cap_vis = Visibility.Collapsed;
                        if (item["caption"] != null)
                        {
                            list.caption = (string)item["caption"];
                            list.cap_vis = Visibility.Visible;
                        }
                    }

                    list.img_vis = Visibility.Collapsed;
                    if (item["picture"] != null)
                    {
                        list.img1 = (string)item["picture"];
                        list.img_vis = Visibility.Visible;
                    }
                    DateTime pt = DateTime.Parse((string)item["created_time"]);
                    TimeSpan tsp = now - pt;
                    list.thumb_img = string.Format("http://graph.facebook.com/{0}/picture", (string)item["from"]["id"]);
                    if (tsp.Days > 0)
                        list.time = tsp.Days + "일 전";
                    else if (tsp.Hours > 0)
                        list.time = tsp.Hours + "시간 전";
                    else
                        list.time = tsp.Minutes + "분 전";
                    if (item["comments"] != null)
                    {
                        list.comment = string.Format("댓글 {0}개", item["comments"]["count"]);
                    }
                    else
                    {
                        list.comment = "댓글 0개";
                    }
                    list.like = "0";
                    if (item["likes"] != null)
                    {
                        if (item["likes"]["count"] != null)
                        {
                            list.like = item["likes"]["count"].ToString();
                            list.comment += string.Format("  좋아요 {0}명", item["likes"]["count"]);
                        }
                    }
                    lists.Add(list);
                }
                if (lists.Count > 0)
                {
                    loadtext.Visibility = Visibility.Collapsed;
                    pgbar.Visibility = Visibility.Collapsed;
                    listdata.ItemsSource = lists;
                    listdata.Visibility = Visibility.Visible;
                }
                else
                {
                    pgbar.Visibility = Visibility.Collapsed;
                    loadtext.Text = "No items";
                }

            }
            else
            {
                pgbar.Visibility = Visibility.Collapsed;
                loadtext.Text = "No items";
                MessageBox.Show("Your facebook open api is expired. Please re-login facebook account", "Sorry", MessageBoxButton.OK);
                settings.Remove("facebook_token");
                settings.Remove("facebook_name");
                settings.Remove("facebook_chk");
                settings.Remove("facebook_id");
            }
        }

        private void wc_openHandler2(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                String jsonstr = e.Result.ToString();
                jsonstr = "{\"data\": " + jsonstr + "}";
                JObject obj = JObject.Parse(jsonstr);
                JArray items = (JArray)obj["data"];

                List<FBView> lists = new List<FBView>();
                DateTime now = DateTime.Now;
                for (int i = 0; i < items.Count; i++)
                {

                    JObject item = (JObject)items[i];

                    FBView list = new FBView();
                    list.id = (string)item["thread_id"];
                    list.name = (string)item["subject"];
                    list.text = (string)item["snippet"];
                    if (item["recipients"].Count() == 1)
                        list.thumb_img = string.Format("http://graph.facebook.com/{0}/picture",item["recipients"][0].ToString());
                    else
                    {
                        if((string)settings["facebook_id"] == item["recipients"][0].ToString())
                            list.thumb_img = string.Format("http://graph.facebook.com/{0}/picture",item["recipients"][1].ToString());
                        else
                            list.thumb_img = string.Format("http://graph.facebook.com/{0}/picture",item["recipients"][0].ToString());
                    }

                    DateTime pt = ConvertTimestamp((long)item["updated_time"]);
                    TimeSpan tsp = now - pt;
                    
                    if (tsp.Days > 0)
                        list.time = tsp.Days + "일 전";
                    else if (tsp.Hours > 0)
                        list.time = tsp.Hours + "시간 전";
                    else
                        list.time = tsp.Minutes + "분 전";

                    lists.Add(list);
                }
                if (lists.Count > 0)
                {
                    loadtext.Visibility = Visibility.Collapsed;
                    pgbar.Visibility = Visibility.Collapsed;
                    listdata2.ItemsSource = lists;
                    listdata2.Visibility = Visibility.Visible;
                }
                else
                {
                    pgbar.Visibility = Visibility.Collapsed;
                    loadtext.Text = "No items";
                }
            }
            else
            {
                pgbar.Visibility = Visibility.Collapsed;
                loadtext.Text = "No items";
                MessageBox.Show("Your facebook open api is expired. Please re-login facebook account", "Sorry", MessageBoxButton.OK);
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

        private void wc_openHandler3(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                string jsonstr = e.Result.ToString();
                JObject obj = JObject.Parse(jsonstr);
                JArray items = (JArray)obj["data"];

                List<FBView> lists = new List<FBView>();
                DateTime now = DateTime.Now;
                for (int i = 0; i < items.Count; i++)
                {

                    JObject item = (JObject)items[i];

                    FBView list = new FBView();
                    list.id = (string)item["id"];
                    list.name = (string)item["from"]["name"];
                    if ((string)item["type"] == "status")
                    {
                        list.text = (string)item["message"];
                        list.cap_vis = Visibility.Collapsed;
                    }
                    else
                    {
                        if (item["message"] != null)
                            list.text = (string)item["message"];
                        else
                            list.text = (string)item["name"];
                        list.cap_vis = Visibility.Collapsed;
                        if (item["caption"] != null)
                        {
                            list.caption = (string)item["caption"];
                            list.cap_vis = Visibility.Visible;
                        }
                    }

                    list.img_vis = Visibility.Collapsed;
                    if (item["picture"] != null)
                    {
                        list.img1 = (string)item["picture"];
                        list.img_vis = Visibility.Visible;
                    }
                    DateTime pt = DateTime.Parse((string)item["created_time"]);
                    TimeSpan tsp = now - pt;
                    list.thumb_img = string.Format("http://graph.facebook.com/{0}/picture", (string)item["from"]["id"]);
                    if (tsp.Days > 0)
                        list.time = tsp.Days + "일 전";
                    else if (tsp.Hours > 0)
                        list.time = tsp.Hours + "시간 전";
                    else
                        list.time = tsp.Minutes + "분 전";


                    if (item["comments"] != null)
                    {
                        list.comment = string.Format("댓글 {0}개", item["comments"]["count"]);
                    }
                    else
                    {
                        list.comment = "댓글 0개";
                    }
                    list.like = "0";
                    if (item["likes"] != null)
                    {
                        if (item["likes"]["count"] != null)
                        {
                            list.like = item["likes"]["count"].ToString();
                            list.comment += string.Format("  좋아요 {0}명", item["likes"]["count"]);
                        }
                    }

                    lists.Add(list);
                }
                if (lists.Count > 0)
                {
                    loadtext.Visibility = Visibility.Collapsed;
                    pgbar.Visibility = Visibility.Collapsed;
                    listdata3.ItemsSource = lists;
                    listdata3.Visibility = Visibility.Visible;
                }
                else
                {
                    pgbar.Visibility = Visibility.Collapsed;
                    loadtext.Text = "No items";
                }

            }
            else
            {
                pgbar.Visibility = Visibility.Collapsed;
                loadtext.Text = "No items";
                MessageBox.Show("Your facebook open api is expired. Please re-login facebook account", "Sorry", MessageBoxButton.OK);
            }
        }

        private void title_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                listdata.Visibility = Visibility.Collapsed;
                listdata2.Visibility = Visibility.Collapsed;
                listdata3.Visibility = Visibility.Collapsed;
                loadtext.Visibility = Visibility.Visible;
                pgbar.Visibility = Visibility.Visible;
                var item = (PivotItem)e.AddedItems[0];
                
                string header = (string)item.Header;
                if (header == "recent")
                {
                    this.ApplicationBar.IsVisible = true;
                    string url = String.Format("https://graph.facebook.com/me/home?access_token={0}", (string)settings["facebook_token"]);
                    WebClient wc = new WebClient();
                    wc.DownloadStringCompleted += new DownloadStringCompletedEventHandler(wc_openHandler);
                    wc.DownloadStringAsync(new Uri(url), UriKind.Absolute);
                }
                else if (header == "inbox")
                {
                    this.ApplicationBar.IsVisible = false;
                    string url = String.Format("https://api.facebook.com/method/message.getThreadsInFolder?access_token={0}&format=json", (string)settings["facebook_token"]);
                    WebClient wc = new WebClient();
                    wc.DownloadStringCompleted += new DownloadStringCompletedEventHandler(wc_openHandler2);
                    wc.DownloadStringAsync(new Uri(url), UriKind.Absolute);
                }
                else if (header == "my")
                {
                    this.ApplicationBar.IsVisible = true;
                    string url = String.Format("https://graph.facebook.com/me/feed?access_token={0}", (string)settings["facebook_token"]);
                    WebClient wc = new WebClient();
                    wc.DownloadStringCompleted += new DownloadStringCompletedEventHandler(wc_openHandler3);
                    wc.DownloadStringAsync(new Uri(url), UriKind.Absolute);
                }
            }
        }

        private void appbar_button1_Click(object sender, EventArgs e)
        {
            listdata.Visibility = Visibility.Collapsed;
            listdata2.Visibility = Visibility.Collapsed;
            listdata3.Visibility = Visibility.Collapsed;
            loadtext.Visibility = Visibility.Visible;
            pgbar.Visibility = Visibility.Visible;
            var item = (PivotItem)title.SelectedItem;
            string header = (string)item.Header;
            if (header == "recent")
            {
                string url = String.Format("https://graph.facebook.com/me/home?access_token={0}", (string)settings["facebook_token"]);
                WebClient wc = new WebClient();
                wc.DownloadStringCompleted += new DownloadStringCompletedEventHandler(wc_openHandler);
                wc.DownloadStringAsync(new Uri(url), UriKind.Absolute);
            }
            else if (header == "inbox")
            {
                string url = String.Format("https://api.facebook.com/method/message.getThreadsInFolder?access_token={0}&format=json", (string)settings["facebook_token"]);
                WebClient wc = new WebClient();
                wc.DownloadStringCompleted += new DownloadStringCompletedEventHandler(wc_openHandler2);
                wc.DownloadStringAsync(new Uri(url), UriKind.Absolute);
            }
            else if (header == "my")
            {
                string url = String.Format("https://graph.facebook.com/me/feed?access_token={0}", (string)settings["facebook_token"]);
                WebClient wc = new WebClient();
                wc.DownloadStringCompleted += new DownloadStringCompletedEventHandler(wc_openHandler3);
                wc.DownloadStringAsync(new Uri(url), UriKind.Absolute);
            }
        }

        private void appbar_button2_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/FacebookWrite.xaml", UriKind.Relative));
        }

        private void listdata_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var item = (FBView)e.AddedItems[0];
                if (item != null)
                {
                    NavigationService.Navigate(new Uri("/FacebookPostView.xaml?id=" + item.id + "&thumb_img=" + item.thumb_img + "&name=" + item.name + "&text=" + item.text + "&time=" + item.time + "&like=" + item.like +"&photo="+item.img1, UriKind.Relative));
                }
                listdata.SelectedIndex = -1;
            }
        }

        private void listdata2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var item = (FBView)e.AddedItems[0];
                if (item != null)
                {
                    NavigationService.Navigate(new Uri("/FacebookMessageView.xaml?id=" + item.id + "&title=" + item.name, UriKind.Relative));
                }
                listdata.SelectedIndex = -1;
            }
        }
    }
}