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
using System.Xml;
using System.Xml.Linq;
using System.IO;
using HDStream.Model;
using System.IO.IsolatedStorage;
using System.Net.NetworkInformation;

namespace HDStream
{
    public partial class Me2dayView : PhoneApplicationPage
    {
        private IsolatedStorageSettings settings;
        private List<Me2View> lists;
        public Me2dayView()
        {
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
            if (!settings.Contains("me2day_userid"))
            {
                MessageBox.Show("Me2day account doesn't set-up. Please check your Me2day acount", "Sorry", MessageBoxButton.OK);
                this.NavigationService.GoBack();
                return;
            }
            title.Title = "ME2DAY - " + settings["me2day_userid"];
            
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
                string auth_key = String.Format("full_auth_token {0}", settings["me2day_token"]);
                if (header == "recent")
                {
                    string url = String.Format("http://me2day.net/api/get_posts/{0}.xml?akey=aed420d038f9b1a7fe3b5c0d94df22f5&scope=friend[all]&count=30", (string)settings["me2day_userid"]);
                    HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
                    webRequest.Method = "POST";

                    
                    webRequest.Credentials = new NetworkCredential((string)settings["me2day_userid"], auth_key);
                    IAsyncResult token = webRequest.BeginGetResponse(new AsyncCallback(GetReqeustStreamCallback), webRequest);
                }
                if (header == "my")
                {
                    string url = String.Format("http://me2day.net/api/get_posts/{0}.xml?akey=aed420d038f9b1a7fe3b5c0d94df22f5&count=30", (string)settings["me2day_userid"]);
                    HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
                    webRequest.Method = "POST";

                    webRequest.Credentials = new NetworkCredential((string)settings["me2day_userid"], auth_key);
                    IAsyncResult token = webRequest.BeginGetResponse(new AsyncCallback(GetReqeustStreamCallback2), webRequest);
                }
                else if (header == "comments")
                {
                    string url = String.Format("http://me2day.net/api/track_comments/{0}.xml?akey=aed420d038f9b1a7fe3b5c0d94df22f5&scope=to_me&count=30", (string)settings["me2day_userid"]);
                    HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
                    webRequest.Method = "POST";
                    
                    webRequest.Credentials = new NetworkCredential((string)settings["me2day_userid"], auth_key);
                    IAsyncResult token = webRequest.BeginGetResponse(new AsyncCallback(GetReqeustStreamCallback3), webRequest);
                }
            }
        }

        private void GetReqeustStreamCallback(IAsyncResult asynchronousResult)
        {
            try
            {
                WebResponse response = ((HttpWebRequest)asynchronousResult.AsyncState).EndGetResponse(asynchronousResult);
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string responseString = reader.ReadToEnd();

                reader.Close();
                var dom = XDocument.Parse(responseString);

                var items = from item in dom.Descendants("post")
                            select new Me2View
                            {
                                id = (string)item.Element("post_id"),
                                name = (string)item.Element("author").Element("nickname"),
                                text = (string)item.Element("body").Value,
                                tag = (string)item.Element("tagText"),
                                dtime = (DateTime)item.Element("pubDate"),                                
                                thumb_img = (string)item.Element("author").Element("face"),
                                media_obj = (Object)item.Element("media"),
                                me2 = (string)item.Element("metooCount").Value,
                                reply = (string)item.Element("commentsCount").Value,
                                reply_visib = (string)item.Element("commentClosed").Value,
                            };

                lists = new List<Me2View>();
                DateTime now = DateTime.Now;
                foreach (Me2View item in items)
                {
                    XElement media = XElement.Parse(item.media_obj.ToString());
                    
                    Me2View mv = new Me2View();
                    mv.img_vis = Visibility.Collapsed;
                    if (media.Element("me2photo") != null)
                    {
                        mv.img_vis = Visibility.Visible;
                        mv.photo = (string)media.Element("me2photo").Element("photoUrl");
                    }
                    TimeSpan tsp = now - item.dtime;
                    mv.id = item.id;
                    mv.name = item.name;
                    mv.me2 = item.me2;
                    mv.thumb_img = item.thumb_img;
                    mv.text = HtmlRemoval.StripTagsRegexCompiled(item.text);
                    mv.tag = item.tag;
                    mv.text = mv.text.Replace("&lt;", "<");
                    mv.text = mv.text.Replace("&gt;", ">");

                    if (tsp.Days > 0)
                        mv.time = tsp.Days + "일 전";
                    else if (tsp.Hours > 0)
                        mv.time = tsp.Hours + "시간 전";
                    else
                        mv.time = tsp.Minutes + "분 전";
                    mv.me2tag = "미투 " + item.me2 + "개  댓글";
                    if (item.reply_visib == "true")
                        mv.me2tag += "닷힘";
                    else
                    {
                        mv.me2tag += item.reply + "개";
                    }
                    mv.me2tag += "\n";
                    lists.Add(mv);

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
            catch (WebException ex)
            {
                Dispatcher.BeginInvoke(delegate()
                {
                    MessageBox.Show("Progress isn't completed. Please retry login", "Sorry", MessageBoxButton.OK);
                    this.NavigationService.GoBack();
                });
            }
        }

        private void GetReqeustStreamCallback2(IAsyncResult asynchronousResult)
        {
            try
            {
                WebResponse response = ((HttpWebRequest)asynchronousResult.AsyncState).EndGetResponse(asynchronousResult);
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string responseString = reader.ReadToEnd();

                reader.Close();
                var dom = XDocument.Parse(responseString);

                var items = from item in dom.Descendants("post")
                            select new Me2View
                            {
                                id = (string)item.Element("post_id"),
                                name = (string)item.Element("author").Element("nickname"),
                                text = (string)item.Element("body").Value,
                                tag = (string)item.Element("tagText"),
                                dtime = (DateTime)item.Element("pubDate"),
                                thumb_img = (string)item.Element("author").Element("face"),
                                media_obj = (Object)item.Element("media"),
                                me2 = (string)item.Element("metooCount").Value,
                                reply = (string)item.Element("commentsCount").Value,
                                reply_visib = (string)item.Element("commentClosed").Value,
                            };

                lists = new List<Me2View>();
                DateTime now = DateTime.Now;
                foreach (Me2View item in items)
                {
                    Me2View mv = new Me2View();
                    XElement media = XElement.Parse(item.media_obj.ToString());
                    mv.img_vis = Visibility.Collapsed;
                    if (media.Element("me2photo") != null)
                    {
                        mv.img_vis = Visibility.Visible;
                        mv.photo = (string)media.Element("me2photo").Element("photoUrl");
                    }
                    TimeSpan tsp = now - item.dtime;
                    mv.id = item.id;
                    mv.name = item.name;
                    mv.me2 = item.me2;
                    mv.thumb_img = item.thumb_img;
                    mv.text = HtmlRemoval.StripTagsRegexCompiled(item.text);
                    mv.tag = item.tag;
                    mv.text = mv.text.Replace("&lt;", "<");
                    mv.text = mv.text.Replace("&gt;", ">");

                    if (tsp.Days > 0)
                        mv.time = tsp.Days + "일 전";
                    else if (tsp.Hours > 0)
                        mv.time = tsp.Hours + "시간 전";
                    else
                        mv.time = tsp.Minutes + "분 전";
                    mv.me2tag = "미투 " + item.me2 + "개  댓글";
                    if (item.reply_visib == "true")
                        mv.me2tag += "닷힘";
                    else
                    {
                        mv.me2tag += item.reply + "개";
                    }
                    mv.me2tag += "\n";
                    lists.Add(mv);

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
            catch (WebException ex)
            {
                Dispatcher.BeginInvoke(delegate()
                {
                    MessageBox.Show("Progress isn't completed. Please retry login", "Sorry", MessageBoxButton.OK);
                    this.NavigationService.GoBack();
                });
            }
        }

        private void GetReqeustStreamCallback3(IAsyncResult asynchronousResult)
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

                lists = new List<Me2View>();
                DateTime now = DateTime.Now;
                foreach (Me2View item in items)
                {
                    Me2View mv = new Me2View();
                    TimeSpan tsp = now - item.dtime;
                    mv.id = item.id;
                    mv.name = item.name;
                    mv.thumb_img = item.thumb_img;
                    mv.text = HtmlRemoval.StripTagsRegexCompiled(item.text);
                    mv.tag = HtmlRemoval.StripTagsRegexCompiled(item.tag)+"\n";
                    mv.text = mv.text.Replace("&lt;", "<");
                    mv.text = mv.text.Replace("&gt;", ">");
                    lists.Add(mv);

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
            catch (WebException ex)
            {
                Dispatcher.BeginInvoke(delegate()
                {
                    MessageBox.Show("Progress isn't completed. Please retry login", "Sorry", MessageBoxButton.OK);
                    this.NavigationService.GoBack();
                });
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
            string auth_key = String.Format("full_auth_token {0}", settings["me2day_token"]);
            if (header == "recent")
            {
                string url = String.Format("http://me2day.net/api/get_posts/{0}.xml?akey=aed420d038f9b1a7fe3b5c0d94df22f5&scope=friend[all]&count=30", (string)settings["me2day_userid"]);
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
                webRequest.Method = "POST";


                webRequest.Credentials = new NetworkCredential((string)settings["me2day_userid"], auth_key);
                IAsyncResult token = webRequest.BeginGetResponse(new AsyncCallback(GetReqeustStreamCallback), webRequest);
            }
            if (header == "my")
            {
                string url = String.Format("http://me2day.net/api/get_posts/{0}.xml?akey=aed420d038f9b1a7fe3b5c0d94df22f5&count=30", (string)settings["me2day_userid"]);
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
                webRequest.Method = "POST";

                webRequest.Credentials = new NetworkCredential((string)settings["me2day_userid"], auth_key);
                IAsyncResult token = webRequest.BeginGetResponse(new AsyncCallback(GetReqeustStreamCallback2), webRequest);
            }
            else if (header == "comments")
            {
                string url = String.Format("http://me2day.net/api/track_comments/{0}.xml?akey=aed420d038f9b1a7fe3b5c0d94df22f5&scope=to_me&count=30", (string)settings["me2day_userid"]);
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
                webRequest.Method = "POST";

                webRequest.Credentials = new NetworkCredential((string)settings["me2day_userid"], auth_key);
                IAsyncResult token = webRequest.BeginGetResponse(new AsyncCallback(GetReqeustStreamCallback3), webRequest);
            }
        }

        private void appbar_button2_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Me2dayWrite.xaml", UriKind.Relative));
        }

        private void listdata_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var item = (Me2View)e.AddedItems[0];
                if (item != null)
                {
                    NavigationService.Navigate(new Uri("/Me2PostView.xaml?id=" + item.id + "&thumb_img=" + item.thumb_img + "&name=" + item.name + "&text=" + item.text + "&time=" + item.time +"&tag="+item.tag+"&me2="+item.me2+"&cmt="+item.reply_visib+"&photo="+item.photo, UriKind.Relative));
                }
                listdata.SelectedIndex = -1;
            }
        }

        private void listdata2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var item = (Me2View)e.AddedItems[0];
                if (item != null)
                {
                    string url = String.Format("http://me2day.net/api/get_posts.xml?post_id={0}&akey=aed420d038f9b1a7fe3b5c0d94df22f5", item.id);
                    HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
                    webRequest.Method = "POST";

                    string auth_key = String.Format("full_auth_token {0}", settings["me2day_token"]);
                    webRequest.Credentials = new NetworkCredential((string)settings["me2day_userid"], auth_key);
                    IAsyncResult token = webRequest.BeginGetResponse(new AsyncCallback(wc_openHandler_me2day), webRequest);
                }
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

                string me2_id, me2_thumb, me2_name, me2_txt, me2_tag, me2_me2, me2_cmt;
                me2_id = post.Element("post_id").Value;
                me2_thumb = post.Element("author").Element("face").Value;
                me2_name = post.Element("author").Element("nickname").Value;
                me2_txt = post.Element("textBody").Value;
                me2_tag = post.Element("tagText").Value;
                me2_me2 = post.Element("metooCount").Value;
                me2_cmt = post.Element("commentClosed").Value;
                if(post.Element("media").Element("") != null)
                {

                }
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