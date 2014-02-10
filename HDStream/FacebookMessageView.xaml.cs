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
using HDStream.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
    public partial class FacebookMessageView : PhoneApplicationPage
    {
        private string thread_id;
        private List<FBView> lists;        
        private IsolatedStorageSettings settings;
        private string emptystr;
        private string sender_id;
        private long sender_int_id;
        public FacebookMessageView()
        {
            InitializeComponent();

            emptystr = "What's on your mind?";
            InitializeComponent();
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
            string title;
            NavigationContext.QueryString.TryGetValue("id", out thread_id);
            NavigationContext.QueryString.TryGetValue("title", out title);
            lists = new List<FBView>();
            ApplicationTitle.Text = title;
            init();
        }

        public void init()
        {
            string url = String.Format("https://graph.facebook.com/{0}?access_token={1}", thread_id, (string)settings["facebook_token"]);
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
                DateTime now = DateTime.Now;
                
                if (obj["comments"] != null)
                {
                    JArray arr = (JArray)obj["comments"]["data"];                    
                    for (int i = arr.Count - 1; i >= 0 ; i--)
                    {
                        string time;
                        JObject item = (JObject)arr[i];

                        TimeSpan tsp = now - DateTime.Parse((string)item["created_time"]);
                        if (tsp.Days > 0)
                            time = tsp.Days + "일 전";
                        else if (tsp.Hours > 0)
                            time = tsp.Hours + "시간 전";
                        else
                            time = tsp.Minutes + "분 전";

                        if ((string)item["from"]["name"] != (string)settings["facebook_name"])
                        {
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
                            img.Source = new BitmapImage(new Uri(string.Format("http://graph.facebook.com/{0}/picture", (string)item["from"]["id"])));
                            thumb_grid.Children.Add(img);
                            cmg.Children.Add(thumb_grid);

                            Grid post_grid = new Grid();
                            Grid.SetColumn(post_grid, 2);

                            StackPanel stk = new StackPanel();
                            SpeechLeft spb = new SpeechLeft();
                            spb.text = (string)item["message"] + "\n" + time;
                            spb.change_ui();
                            stk.Children.Add(spb);
                            Grid gd = new Grid();
                            gd.Height = 20;
                            stk.Children.Add(gd);
                            post_grid.Children.Add(stk);
                            cmg.Children.Add(post_grid);
                            StackPanel.Children.Add(cmg);
                        }
                        else
                        {
                            Grid cmg = new Grid();
                            Grid.SetColumn(cmg, 2);
                            ColumnDefinition cdf = new ColumnDefinition();
                            cdf.Width = new GridLength(400);
                            cmg.ColumnDefinitions.Add(cdf);
                            cdf = new ColumnDefinition();
                            cdf.Width = new GridLength(60);
                            cmg.ColumnDefinitions.Add(cdf);
                            Grid post_grid = new Grid();
                            StackPanel stk = new StackPanel();
                            SpeechBubble spb = new SpeechBubble();
                            spb.text = (string)item["message"] + "\n" + time;
                            spb.change_ui();
                            stk.Children.Add(spb);
                            Grid gd = new Grid();
                            gd.Height = 20;
                            stk.Children.Add(gd);
                            post_grid.Children.Add(stk);
                            cmg.Children.Add(post_grid);

                            Grid thumb_grid = new Grid();
                            Grid.SetColumn(thumb_grid, 2);
                            thumb_grid.HorizontalAlignment = HorizontalAlignment.Center;
                            thumb_grid.VerticalAlignment = VerticalAlignment.Top;
                            Image img = new Image();
                            img.Width = 48;
                            img.Height = 48;
                            img.Source = new BitmapImage(new Uri(string.Format("http://graph.facebook.com/{0}/picture", (string)item["from"]["id"])));
                            thumb_grid.Children.Add(img);
                            cmg.Children.Add(thumb_grid);

                            StackPanel.Children.Add(cmg);
                        }
                    }
                }

                string t_label;

                TimeSpan tp = now - DateTime.Parse((string)obj["updated_time"]);
                if (tp.Days > 0)
                    t_label = tp.Days + "일 전";
                else if (tp.Hours > 0)
                    t_label = tp.Hours + "시간 전";
                else
                    t_label = tp.Minutes + "분 전";
                if ((string)obj["from"]["name"] != (string)settings["facebook_name"])
                {
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
                    img.Source = new BitmapImage(new Uri(string.Format("http://graph.facebook.com/{0}/picture", (string)obj["from"]["id"])));
                    thumb_grid.Children.Add(img);
                    cmg.Children.Add(thumb_grid);

                    Grid post_grid = new Grid();
                    Grid.SetColumn(post_grid, 2);

                    StackPanel stk = new StackPanel();
                    SpeechLeft spb = new SpeechLeft();
                    spb.text = (string)obj["message"] + "\n" + t_label;
                    spb.change_ui();
                    stk.Children.Add(spb);
                    Grid gd = new Grid();
                    gd.Height = 20;
                    stk.Children.Add(gd);
                    post_grid.Children.Add(stk);
                    cmg.Children.Add(post_grid);
                    StackPanel.Children.Add(cmg);
                }
                else
                {
                    Grid cmg = new Grid();
                    Grid.SetColumn(cmg, 2);
                    ColumnDefinition cdf = new ColumnDefinition();
                    cdf.Width = new GridLength(400);
                    cmg.ColumnDefinitions.Add(cdf);
                    cdf = new ColumnDefinition();
                    cdf.Width = new GridLength(60);
                    cmg.ColumnDefinitions.Add(cdf);
                    Grid post_grid = new Grid();
                    StackPanel stk = new StackPanel();
                    SpeechBubble spb = new SpeechBubble();
                    spb.text = (string)obj["message"] + "\n" + t_label;
                    spb.change_ui();
                    stk.Children.Add(spb);
                    Grid gd = new Grid();
                    gd.Height = 20;
                    stk.Children.Add(gd);
                    post_grid.Children.Add(stk);
                    cmg.Children.Add(post_grid);

                    Grid thumb_grid = new Grid();
                    Grid.SetColumn(thumb_grid, 2);
                    thumb_grid.HorizontalAlignment = HorizontalAlignment.Center;
                    thumb_grid.VerticalAlignment = VerticalAlignment.Top;
                    Image img = new Image();
                    img.Width = 48;
                    img.Height = 48;
                    img.Source = new BitmapImage(new Uri(string.Format("http://graph.facebook.com/{0}/picture", (string)obj["from"]["id"])));
                    thumb_grid.Children.Add(img);
                    cmg.Children.Add(thumb_grid);

                    StackPanel.Children.Add(cmg);
                }
                loadpanel.Visibility = Visibility.Collapsed;
            }
        }
    }
}