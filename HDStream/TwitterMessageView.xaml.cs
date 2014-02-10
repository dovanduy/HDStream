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
using TweetSharp;
using System.Net.NetworkInformation;


namespace HDStream
{
    public partial class TwitterMessageView : PhoneApplicationPage
    {
        private List<Tweet> lists;
        private TwitterService service;        
        private IsolatedStorageSettings settings;
        private string emptystr;
        private string sender_id;
        private long sender_int_id;
        public TwitterMessageView()
        {
            settings = IsolatedStorageSettings.ApplicationSettings;
            service = new TwitterService("g8F2KdKH40gGp9BXemw13Q", "OyFRFsI05agcJtURtLv8lpYbYRwZAIL5gr5xQNPW0Q");
            service.AuthenticateWith((string)settings["twitter_token"], (string)settings["twitter_tokensecret"]);
            emptystr = "What's on your mind?";
            InitializeComponent();
            
            this.Loaded += new RoutedEventHandler(ListPage_Loaded);
            
        }
        private void ListPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                MessageBox.Show("This application must require for internet connection. Please check your internet connection status", "Sorry", MessageBoxButton.OK);
                this.NavigationService.GoBack();
            }
            NavigationContext.QueryString.TryGetValue("sender_id", out sender_id);
            lists = new List<Tweet>();
            ApplicationTitle.Text = "@" + sender_id;
            init();
        }

        public void init()
        {
            service.ListDirectMessagesReceived(30,
                (tweets, response) =>
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        foreach (var tweet in tweets)
                        {
                            if (tweet.Author.ScreenName == sender_id)
                            {
                                sender_int_id = tweet.SenderId;
                                Tweet tv = new Tweet();
                                tv.id = tweet.Id;
                                tv.name = tweet.Author.ScreenName;
                                tv.thumb_img = tweet.Author.ProfileImageUrl;
                                tv.text = tweet.Text;
                                tv.dtime = tweet.CreatedDate;
                                lists.Add(tv);
                            }
                        }
                    }
                    Dispatcher.BeginInvoke(delegate()
                    {
                        send_load();
                    });
                });
        }

        private void send_load()
        {
            service.ListDirectMessagesSent(30,
                (tweets, response) =>
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        foreach (var tweet in tweets)
                        {
                            if (tweet.RecipientScreenName == sender_id)
                            {
                                Tweet tv = new Tweet();
                                tv.id = tweet.Id;
                                tv.name = tweet.Author.ScreenName;
                                tv.thumb_img = tweet.Author.ProfileImageUrl;
                                tv.text = tweet.Text;
                                tv.dtime = tweet.CreatedDate;
                                lists.Add(tv);
                            }
                        }                        
                    }
                    Dispatcher.BeginInvoke(delegate()
                    {
                        organaize();
                    });
                });
        }

        public void organaize()
        {
            cmt_line.Visibility = Visibility.Visible;
            loadpanel.Visibility = Visibility.Collapsed;
            lists.Sort(delegate(Tweet p1, Tweet p2) { return p2.dtime.CompareTo(p1.dtime); });
            DateTime ti = DateTime.Now;
            DateTime now = ti.ToUniversalTime();
            foreach (Tweet tweet in lists)
            {
                TimeSpan tsp = now - tweet.dtime;
                if (tsp.Days > 0)
                    tweet.time = tsp.Days + "일 전";
                else if (tsp.Hours > 0)
                    tweet.time = tsp.Hours + "시간 전";
                else
                    tweet.time = tsp.Minutes + "분 전";

                if (tweet.name == sender_id)
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
                    img.Source = new BitmapImage(new Uri(tweet.thumb_img));
                    thumb_grid.Children.Add(img);
                    cmg.Children.Add(thumb_grid);

                    Grid post_grid = new Grid();
                    Grid.SetColumn(post_grid, 2);

                    StackPanel stk = new StackPanel();
                    SpeechLeft spb = new SpeechLeft();
                    spb.text = tweet.text + "\n" + tweet.time;                    
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
                    spb.text = tweet.text + "\n" + tweet.time;
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
                    img.Source = new BitmapImage(new Uri(tweet.thumb_img));
                    thumb_grid.Children.Add(img);
                    cmg.Children.Add(thumb_grid);
                    
                    StackPanel.Children.Add(cmg);
                }
            }


        }

        private void Border_MouseEnter(object sender, MouseEventArgs e)
        {
            ScrollViewer.Height = 280;
            ScrollViewer.UpdateLayout();
            ScrollViewer.ScrollToVerticalOffset(double.MaxValue);
            KeyboardPanel.Visibility = Visibility.Visible;
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

        private void ApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            if (WatermarkTB.Text == emptystr)
            {
                MessageBox.Show("Please input your mind :)", "Sorry", MessageBoxButton.OK);
                return;
            }
            service.SendDirectMessage(sender_id, WatermarkTB.Text,
                (tweets, response) =>
                {
                    Dispatcher.BeginInvoke(delegate()
                    {
                        SolidColorBrush Brush1 = new SolidColorBrush();
                        Brush1.Color = Colors.Gray;
                        WatermarkTB.Foreground = Brush1;
                        WatermarkTB.Text = emptystr;
                        keyboard.Init();
                        ScrollViewer.Height = 650;
                        lists = new List<Tweet>();
                        KeyboardPanel.Visibility = Visibility.Collapsed;
                        StackPanel.Children.Clear();
                        cmt_line.Visibility = Visibility.Collapsed;
                        loadpanel.Visibility = Visibility.Visible;
                        init();
                    });
                });


        }

    }
}