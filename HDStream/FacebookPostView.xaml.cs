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
using System.Xml;
using System.Xml.Linq;
using System.IO;
using HDStream.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Hammock;
using Hammock.Serialization;
using Hammock.Web;
using Hammock.Authentication;
using Hammock.Authentication.Basic;
using Hammock.Authentication.OAuth;
using System.Net.NetworkInformation;
namespace HDStream
{
    public partial class FacebookPostView : PhoneApplicationPage
    {
        private String id, photo;
        private IsolatedStorageSettings settings;
        private string emptystr;
        private int like_cnt;        

        public FacebookPostView()
        {
            InitializeComponent();
            emptystr = "What's on your mind?";

            settings = IsolatedStorageSettings.ApplicationSettings;
            this.Loaded += new RoutedEventHandler(ListPage_Loaded);
        }

        private void ListPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                MessageBox.Show("This application must require for internet connection. Please check your internet connection status", "Sorry", MessageBoxButton.OK);
                this.NavigationService.GoBack();
            }
            String thumb_img, name, text, time, me2;
            NavigationContext.QueryString.TryGetValue("id", out id);
            NavigationContext.QueryString.TryGetValue("thumb_img", out thumb_img);
            NavigationContext.QueryString.TryGetValue("name", out name);
            NavigationContext.QueryString.TryGetValue("text", out text);
            NavigationContext.QueryString.TryGetValue("time", out time);            
            NavigationContext.QueryString.TryGetValue("photo", out photo);
            NavigationContext.QueryString.TryGetValue("like", out me2);
            like_cnt = System.Convert.ToInt16(me2);
            ApplicationTitle.Text = String.Format("{0}", name);
            hour.Text = time;            
            hour.Text += "\n";
            if (like_cnt > 0)
            {
                hour.Text += +like_cnt + "명이 좋아요";
            }
            txt.Text = text;

            if (photo != "")
            {
                img1.Source = new ImageSourceConverter().ConvertFromString(photo) as ImageSource;
            }
            
            img.Source = new BitmapImage(new Uri(thumb_img));
            string url = String.Format("https://graph.facebook.com/{0}?access_token={1}", id, (string)settings["facebook_token"]);
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
                loadpg.Visibility = Visibility.Collapsed;
                if (obj["comments"] != null)
                {
                    JArray comments = (JArray)obj["comments"]["data"];
                    DateTime now = DateTime.Now;
                    for (int i = 0; i < comments.Count(); i++)
                    {
                        JObject item = (JObject)comments[i];
                        Grid cmg = new Grid();
                        Grid.SetColumn(cmg, 2);
                        ColumnDefinition cdf = new ColumnDefinition();
                        cdf.Width = new GridLength(60);
                        cmg.ColumnDefinitions.Add(cdf);
                        cdf = new ColumnDefinition();
                        cdf.Width = new GridLength(400);
                        cmg.ColumnDefinitions.Add(cdf);

                        Grid thumb_grid = new Grid();
                        thumb_grid.HorizontalAlignment = HorizontalAlignment.Center;
                        thumb_grid.VerticalAlignment = VerticalAlignment.Top;
                        Image img = new Image();
                        img.Width = 48;
                        img.Height = 48;
                        String thumb_img = String.Format("http://graph.facebook.com/{0}/picture", (string)item["from"]["id"]);
                        img.Source = new BitmapImage(new Uri(thumb_img));
                        thumb_grid.Children.Add(img);
                        cmg.Children.Add(thumb_grid);

                        Grid post_grid = new Grid();
                        Grid.SetColumn(post_grid, 2);

                        StackPanel stk = new StackPanel();
                        TextBlock tb0 = new TextBlock();
                        tb0.TextWrapping = TextWrapping.Wrap;
                        tb0.Style = (Style)App.Current.Resources["PhoneTextLargeStyle"];
                        tb0.Text = (string)item["from"]["name"];
                        tb0.FontSize = 23;
                        stk.Children.Add(tb0);
                        TextBlock tb = new TextBlock();
                        tb.TextWrapping = TextWrapping.Wrap;
                        tb.Style = (Style)App.Current.Resources["PhoneTextLargeStyle"];
                        tb.Text = (string)item["message"];
                        tb.FontSize = 21;
                        stk.Children.Add(tb);
                        TextBlock tb2 = new TextBlock();
                        tb2.TextWrapping = TextWrapping.Wrap;
                        DateTime pt = DateTime.Parse((string)item["created_time"]);
                        TimeSpan tsp = now - pt;
                        string tme;
                        if (tsp.Days > 0)
                            tme = tsp.Days + "일 전";
                        else if (tsp.Hours > 0)
                            tme = tsp.Hours + "시간 전";
                        else
                            tme = tsp.Minutes + "분 전";
                        tb2.Style = (Style)App.Current.Resources["PhoneTextLargeStyle"];
                        tb2.Text = tme;
                        tb2.FontSize = 21;
                        stk.Children.Add(tb2);
                        Grid gd = new Grid();
                        gd.Height = 20;
                        stk.Children.Add(gd);
                        post_grid.Children.Add(stk);
                        cmg.Children.Add(post_grid);
                        comment_list.Children.Add(cmg);
                    }
                }

                
            }
        }

        private void ApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            if (WatermarkTB.Text == emptystr)
            {
                MessageBox.Show("Please input your mind :)", "Sorry", MessageBoxButton.OK);
                return;
            }
            RestClient client2 = new RestClient
            {
                Authority = "https://graph.facebook.com/",
            };

            RestRequest request2 = new RestRequest
            {
                Path = id+"/comments?message=" + WatermarkTB.Text
            };
            
            request2.AddField("access_token", (string)settings["facebook_token"]);
            var callback = new RestCallback(
                (restRequest, restResponse, userState) =>
                {
                    Dispatcher.BeginInvoke(delegate()
                    {
                        ScrollViewer.Height = 680;
                        ScrollViewer.UpdateLayout();
                        ScrollViewer.ScrollToVerticalOffset(double.MaxValue);
                        WatermarkTB.Text = emptystr;
                        keyboard.txt = "";
                        keyboard.Init();
                        KeyboardPanel.Visibility = Visibility.Collapsed;
                        comment_list.Children.Clear();
                        loadpg.Visibility = Visibility.Visible;
                        SolidColorBrush Brush1 = new SolidColorBrush();
                        Brush1.Color = Colors.Gray;
                        WatermarkTB.Foreground = Brush1;
                    });

                    string url = String.Format("https://graph.facebook.com/{0}?access_token={1}", id, (string)settings["facebook_token"]);
                    WebClient wc = new WebClient();
                    wc.DownloadStringCompleted += new DownloadStringCompletedEventHandler(wc_openHandler);
                    wc.DownloadStringAsync(new Uri(url), UriKind.Absolute);                   
                }
                );
            client2.BeginRequest(request2, callback);
            
        }

        private void ApplicationBarIconButton_Click_1(object sender, EventArgs e)
        {
            RestClient client2 = new RestClient
            {
                Authority = "https://graph.facebook.com/",
            };

            RestRequest request2 = new RestRequest
            {
                Path = id + "/likes",
            };

            request2.AddField("access_token", (string)settings["facebook_token"]);
            var callback = new RestCallback(
                (restRequest, restResponse, userState) =>
                {
                    Dispatcher.BeginInvoke(delegate()
                    {
                        if (like_cnt > 0)
                            hour.Text += " 하고 나도 좋아요";
                        else
                            hour.Text += "내가 좋아한 글";
                    });
                }
                );
            client2.BeginRequest(request2, callback);
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

        private void img1_MouseEnter(object sender, MouseEventArgs e)
        {
            NavigationService.Navigate(new Uri("/PhotoViewer.xaml?photo=" + photo, UriKind.Relative));
        }
    }
}