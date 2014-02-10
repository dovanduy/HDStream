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
using TweetSharp;
using HDStream.Model;
using Microsoft.Phone.Shell;
using System.Net.NetworkInformation;

namespace HDStream
{
    public partial class TwitterView : PhoneApplicationPage
    {
        private TwitterService service;
        private IsolatedStorageSettings settings;
        public List<Tweet> lists;
        private int recent_page;
        private int mention_page;
        private int message_page;
        private int my_page;
        private Boolean mesage_bool;
        public TwitterView()
        {
            recent_page = 1;
            mention_page = 1;
            message_page = 1;
            my_page = 1;
            InitializeComponent();
            Loaded += new RoutedEventHandler(MainPage_Loaded);
            settings = IsolatedStorageSettings.ApplicationSettings;
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
            title.Title = "TWITTER - @" + settings["twitter_screenname"];
                    
            
        }

        private void title_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                listdata.Visibility = Visibility.Collapsed;
                listdata2.Visibility = Visibility.Collapsed;
                listdata3.Visibility = Visibility.Collapsed;
                listdata4.Visibility = Visibility.Collapsed;
                loadtext.Visibility = Visibility.Visible;
                pgbar.Visibility = Visibility.Visible;
                var item = (PivotItem)e.AddedItems[0];
                string header = (string)item.Header;
                service = new TwitterService("g8F2KdKH40gGp9BXemw13Q", "OyFRFsI05agcJtURtLv8lpYbYRwZAIL5gr5xQNPW0Q");
                service.AuthenticateWith((string)settings["twitter_token"], (string)settings["twitter_tokensecret"]);    
                if (header == "recent")
                {
                    
                    service.ListTweetsOnFriendsTimeline(0, 40,
                    (tweets, response) =>
                    {
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            DateTime ti = DateTime.Now;
                            DateTime now = ti.ToUniversalTime();

                            lists = new List<Tweet>();
                            foreach (var tweet in tweets)
                            {
                                Tweet tv = new Tweet();
                                TimeSpan tsp = now - tweet.CreatedDate;
                                
                                tv.id = tweet.Id;
                                tv.name = tweet.User.ScreenName;
                                tv.thumb_img = tweet.User.ProfileImageUrl;
                                tv.text = tweet.Text + "\n";

                                if (tsp.Days > 0)
                                    tv.time = tsp.Days + "일 전";
                                else if (tsp.Hours > 0)
                                    tv.time = tsp.Hours + "시간 전";
                                else
                                    tv.time = tsp.Minutes + "분 전";

                                lists.Add(tv);
                            }
                            Dispatcher.BeginInvoke(delegate()
                            {
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

                            });
                        }
                    });
                }
                else if (header == "replies")
                {
                    service.ListTweetsMentioningMe(0, 40,
                    (tweets, response) =>
                    {
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            DateTime ti = DateTime.Now;
                            DateTime now = ti.ToUniversalTime();

                            lists = new List<Tweet>();
                            foreach (var tweet in tweets)
                            {
                                Tweet tv = new Tweet();
                                TimeSpan tsp = now - tweet.CreatedDate;
                                tv.id = tweet.Id;
                                tv.name = tweet.User.ScreenName;
                                tv.thumb_img = tweet.User.ProfileImageUrl;
                                tv.text = tweet.Text + "\n";

                                if (tsp.Days > 0)
                                    tv.time = tsp.Days + "일 전";
                                else if (tsp.Hours > 0)
                                    tv.time = tsp.Hours + "시간 전";
                                else
                                    tv.time = tsp.Minutes + "분 전";
                                lists.Add(tv);
                            }
                            Dispatcher.BeginInvoke(delegate()
                            {
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

                            });
                        }
                    });
                }
                else if (header == "messages")
                {
                    
                    service.ListDirectMessagesReceived(40,
                    (tweets, response) =>
                    {
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            DateTime ti = DateTime.Now;
                            DateTime now = ti.ToUniversalTime();

                            lists = new List<Tweet>();
                            foreach (var tweet in tweets)
                            {
                                Tweet tv = new Tweet();
                                TimeSpan tsp = now - tweet.CreatedDate;
                                tv.id = tweet.Id;
                                tv.name = tweet.Sender.ScreenName;
                                tv.thumb_img = tweet.Sender.ProfileImageUrl;
                                tv.text = tweet.Text + "\n";
                                if (tsp.Days > 0)
                                    tv.time = tsp.Days + "일 전";
                                else if (tsp.Hours > 0)
                                    tv.time = tsp.Hours + "시간 전";
                                else
                                    tv.time = tsp.Minutes + "분 전";
                                lists.Add(tv);
                            }
                            Dispatcher.BeginInvoke(delegate()
                            {
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

                            });
                        }
                    });
                }
                else if (header == "my")
                {
                    service.ListTweetsOnUserTimeline(0, 40,
                    (tweets, response) =>
                    {
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            DateTime ti = DateTime.Now;
                            DateTime now = ti.ToUniversalTime();

                            lists = new List<Tweet>();
                            foreach (var tweet in tweets)
                            {
                                Tweet tv = new Tweet();
                                TimeSpan tsp = now - tweet.CreatedDate;
                                tv.id = tweet.Id;
                                tv.name = tweet.User.ScreenName;
                                tv.thumb_img = tweet.User.ProfileImageUrl;
                                tv.text = tweet.Text + "\n";

                                if (tsp.Days > 0)
                                    tv.time = tsp.Days + "일 전";
                                else if (tsp.Hours > 0)
                                    tv.time = tsp.Hours + "시간 전";
                                else
                                    tv.time = tsp.Minutes + "분 전";
                                lists.Add(tv);
                            }
                            Dispatcher.BeginInvoke(delegate()
                            {
                                if (lists.Count > 0)
                                {
                                    loadtext.Visibility = Visibility.Collapsed;
                                    pgbar.Visibility = Visibility.Collapsed;
                                    listdata4.ItemsSource = lists;
                                    listdata4.Visibility = Visibility.Visible;
                                }
                                else
                                {
                                    pgbar.Visibility = Visibility.Collapsed;
                                    loadtext.Text = "No items";
                                }

                            });
                        }
                    });
                }

            }
        }

        private void appbar_button1_Click(object sender, EventArgs e)
        {
            listdata.Visibility = Visibility.Collapsed;
            listdata2.Visibility = Visibility.Collapsed;
            listdata3.Visibility = Visibility.Collapsed;
            listdata4.Visibility = Visibility.Collapsed;
            loadtext.Visibility = Visibility.Visible;
            pgbar.Visibility = Visibility.Visible;
            var item = (PivotItem)title.SelectedItem;
            string header = (string)item.Header;
            service = new TwitterService("g8F2KdKH40gGp9BXemw13Q", "OyFRFsI05agcJtURtLv8lpYbYRwZAIL5gr5xQNPW0Q");
            service.AuthenticateWith((string)settings["twitter_token"], (string)settings["twitter_tokensecret"]);
            if (header == "recent")
            {
                service.ListTweetsOnFriendsTimeline(0, 40,
                (tweets, response) =>
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        DateTime ti = DateTime.Now;
                        DateTime now = ti.ToUniversalTime();

                        lists = new List<Tweet>();
                        foreach (var tweet in tweets)
                        {
                            Tweet tv = new Tweet();
                            TimeSpan tsp = now - tweet.CreatedDate;
                            tv.id = tweet.Id;
                            tv.name = tweet.User.ScreenName;
                            tv.thumb_img = tweet.User.ProfileImageUrl;
                            tv.text = tweet.Text + "\n";

                            if (tsp.Days > 0)
                                tv.time = tsp.Days + "일 전";
                            else if (tsp.Hours > 0)
                                tv.time = tsp.Hours + "시간 전";
                            else
                                tv.time = tsp.Minutes + "분 전";

                            lists.Add(tv);
                        }
                        Dispatcher.BeginInvoke(delegate()
                        {
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

                        });
                    }
                });
            }
            else if (header == "replies")
            {
                service.ListTweetsMentioningMe(0, 40,
                (tweets, response) =>
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        DateTime ti = DateTime.Now;
                        DateTime now = ti.ToUniversalTime();

                        lists = new List<Tweet>();
                        foreach (var tweet in tweets)
                        {
                            Tweet tv = new Tweet();
                            TimeSpan tsp = now - tweet.CreatedDate;
                            tv.id = tweet.Id;
                            tv.name = tweet.User.ScreenName;
                            tv.thumb_img = tweet.User.ProfileImageUrl;
                            tv.text = tweet.Text + "\n";

                            if (tsp.Days > 0)
                                tv.time = tsp.Days + "일 전";
                            else if (tsp.Hours > 0)
                                tv.time = tsp.Hours + "시간 전";
                            else
                                tv.time = tsp.Minutes + "분 전";
                            lists.Add(tv);
                        }
                        Dispatcher.BeginInvoke(delegate()
                        {
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

                        });
                    }
                });
            }
            else if (header == "messages")
            {
                service.ListDirectMessagesReceived(40,
                (tweets, response) =>
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        DateTime ti = DateTime.Now;
                        DateTime now = ti.ToUniversalTime();

                        lists = new List<Tweet>();
                        foreach (var tweet in tweets)
                        {
                            Tweet tv = new Tweet();
                            TimeSpan tsp = now - tweet.CreatedDate;
                            tv.id = tweet.Id;
                            tv.name = tweet.Sender.ScreenName;
                            tv.thumb_img = tweet.Sender.ProfileImageUrl;
                            tv.text = tweet.Text + "\n";
                            if (tsp.Days > 0)
                                tv.time = tsp.Days + "일 전";
                            else if (tsp.Hours > 0)
                                tv.time = tsp.Hours + "시간 전";
                            else
                                tv.time = tsp.Minutes + "분 전";
                            lists.Add(tv);
                        }
                        Dispatcher.BeginInvoke(delegate()
                        {
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

                        });
                    }
                });
            }
            else if (header == "my")
            {
                service.ListTweetsOnUserTimeline(0, 40,
                (tweets, response) =>
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        DateTime ti = DateTime.Now;
                        DateTime now = ti.ToUniversalTime();

                        lists = new List<Tweet>();
                        foreach (var tweet in tweets)
                        {
                            Tweet tv = new Tweet();
                            TimeSpan tsp = now - tweet.CreatedDate;
                            tv.id = tweet.Id;
                            tv.name = tweet.User.ScreenName;
                            tv.thumb_img = tweet.User.ProfileImageUrl;
                            tv.text = tweet.Text + "\n";

                            if (tsp.Days > 0)
                                tv.time = tsp.Days + "일 전";
                            else if (tsp.Hours > 0)
                                tv.time = tsp.Hours + "시간 전";
                            else
                                tv.time = tsp.Minutes + "분 전";
                            lists.Add(tv);
                        }
                        Dispatcher.BeginInvoke(delegate()
                        {
                            if (lists.Count > 0)
                            {
                                loadtext.Visibility = Visibility.Collapsed;
                                pgbar.Visibility = Visibility.Collapsed;
                                listdata4.ItemsSource = lists;
                                listdata4.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                pgbar.Visibility = Visibility.Collapsed;
                                loadtext.Text = "No items";
                            }

                        });
                    }
                });
            }
        }

        private void appbar_button2_Click(object sender, EventArgs e)
        {
            var item = (PivotItem)title.SelectedItem;
            string header = (string)item.Header;
            if (header == "messages")
            {
                NavigationService.Navigate(new Uri("/TwitterMessageWrite.xaml", UriKind.Relative));
            }
            else
            {
                NavigationService.Navigate(new Uri("/TwitterWrite.xaml", UriKind.Relative));
            }
        }

        private void listdata_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var item = (Tweet)e.AddedItems[0];
                if (item != null)
                {
                    NavigationService.Navigate(new Uri("/TweetView.xaml?id="+item.id+"&thumb_img="+item.thumb_img+"&name="+item.name+"&text="+item.text+"&time="+item.time, UriKind.Relative));
                }
                listdata.SelectedIndex = -1;
            }
        }

        private void listdata3_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var item = (Tweet)e.AddedItems[0];
                if (item != null)
                {
                    NavigationService.Navigate(new Uri("/TwitterMessageView.xaml?sender_id="+item.name, UriKind.Relative));
                }
                listdata3.SelectedIndex = -1;
            }
        }
    }
}