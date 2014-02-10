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
using System.Net.NetworkInformation;

namespace HDStream
{
    public partial class Me2PostView : PhoneApplicationPage
    {
        private String id;
        private IsolatedStorageSettings settings;
        private string emptystr, photo;
        private int me2_cnt;
        private double origin_height;
        private Boolean reply_bool;
        public List<Me2Comment> lists;
        public Me2PostView()
        {
            reply_bool = true;
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
            String thumb_img, name, text, time, tag_txt, reply_visib, me2;
            NavigationContext.QueryString.TryGetValue("id", out id);
            NavigationContext.QueryString.TryGetValue("thumb_img", out thumb_img);
            NavigationContext.QueryString.TryGetValue("tag", out tag_txt);
            NavigationContext.QueryString.TryGetValue("name", out name);
            NavigationContext.QueryString.TryGetValue("text", out text);
            NavigationContext.QueryString.TryGetValue("time", out time);
            NavigationContext.QueryString.TryGetValue("cmt", out reply_visib);
            NavigationContext.QueryString.TryGetValue("me2", out me2);

            NavigationContext.QueryString.TryGetValue("photo", out photo);
            image.Visibility = Visibility.Collapsed;
            if (photo != null && photo != "")
            {
                image.Visibility = Visibility.Visible;
                image.Source = new ImageSourceConverter().ConvertFromString(photo) as ImageSource;
            }
            me2_cnt = System.Convert.ToInt16(me2);
            ApplicationTitle.Text = String.Format("{0}", name);
            hour.Text = time;
            tag.Text = tag_txt;
            hour.Text += "\n";
            if (me2_cnt > 0)
            {
                hour.Text += + me2_cnt + "명이 미투";
            }
            txt.Text = text;
            if (reply_visib == "true")
            {
                reply_bool = false;
                cmt_line.Visibility = Visibility.Collapsed;
            }
            img.Source = new BitmapImage(new Uri(thumb_img));
            string auth_key = String.Format("full_auth_token {0}", settings["me2day_token"]);
            string url = String.Format("http://me2day.net/api/get_comments/{0}.xml?akey=aed420d038f9b1a7fe3b5c0d94df22f5&post_id={1}", (string)settings["me2day_userid"], id);
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Method = "POST";
            webRequest.Credentials = new NetworkCredential((string)settings["me2day_userid"], auth_key);
            IAsyncResult token = webRequest.BeginGetResponse(new AsyncCallback(GetReqeustStreamCallback), webRequest);
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

                var items = from item in dom.Descendants("comment")
                            select new Me2Comment
                            {
                                id = (string)item.Element("commentId"),
                                name = (string)item.Element("author").Element("nickname"),
                                text = (string)item.Element("body").Value,
                                dtime = (DateTime)item.Element("pubDate"),
                                thumb_img = (string)item.Element("author").Element("face"),
                            };
                lists = new List<Me2Comment>();
                DateTime now = DateTime.Now;
                foreach (Me2Comment item in items)
                {
                    Me2Comment mv = new Me2Comment();
                    TimeSpan tsp = now - item.dtime;
                    mv.id = item.id;
                    mv.name = item.name;
                    mv.thumb_img = item.thumb_img;
                    mv.text = HtmlRemoval.StripTagsRegexCompiled(item.text);
                    mv.text = mv.text.Replace("&lt;", "<");
                    mv.text = mv.text.Replace("&gt;", ">");

                    if (tsp.Days > 0)
                        mv.time = tsp.Days + "일 전";
                    else if (tsp.Hours > 0)
                        mv.time = tsp.Hours + "시간 전";
                    else
                        mv.time = tsp.Minutes + "분 전";
                    lists.Add(mv);
                }
                Dispatcher.BeginInvoke(delegate()
                {
                    loadpg.Visibility = Visibility.Collapsed;
                    if (lists.Count > 0)
                    {
                        for (int i = 0; i < lists.Count; i++)
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
                            img.Source = new BitmapImage(new Uri(lists[i].thumb_img));
                            thumb_grid.Children.Add(img);
                            cmg.Children.Add(thumb_grid);

                            Grid post_grid = new Grid();
                            Grid.SetColumn(post_grid, 2);
                            
                            StackPanel stk = new StackPanel();
                            TextBlock tb0 = new TextBlock();
                            tb0.TextWrapping = TextWrapping.Wrap;
                            tb0.Style = (Style)App.Current.Resources["PhoneTextLargeStyle"];
                            tb0.Text = lists[i].name;
                            tb0.FontSize = 23;
                            stk.Children.Add(tb0);
                            TextBlock tb = new TextBlock();
                            tb.TextWrapping = TextWrapping.Wrap;
                            tb.Style = (Style)App.Current.Resources["PhoneTextLargeStyle"];
                            tb.Text = lists[i].text;
                            tb.FontSize = 21;
                            stk.Children.Add(tb);
                            TextBlock tb2 = new TextBlock();
                            tb2.TextWrapping = TextWrapping.Wrap;
                            tb2.Style = (Style)App.Current.Resources["PhoneTextLargeStyle"];
                            tb2.Text = lists[i].time;
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
                });
            }
            catch (WebException ex)
            {

            }
        }


        private void ApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            if (reply_bool == false)
            {
                MessageBox.Show("This article is locked.", "Sorry", MessageBoxButton.OK);
                return;
            }
            if (WatermarkTB.Text == emptystr)
            {
                MessageBox.Show("Please input your mind :)", "Sorry", MessageBoxButton.OK);
                return;
            }
            string auth_key = String.Format("full_auth_token {0}", settings["me2day_token"]);
            string url = String.Format("http://me2day.net/api/create_comment.xml?akey=aed420d038f9b1a7fe3b5c0d94df22f5&post_id={1}&body={2}", (string)settings["me2day_userid"], id, WatermarkTB.Text);
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Method = "POST";
            webRequest.Credentials = new NetworkCredential((string)settings["me2day_userid"], auth_key);
            IAsyncResult token = webRequest.BeginGetResponse(new AsyncCallback(GetReqeustStreamCallback2), webRequest);
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
            origin_height = ScrollViewer.Height;
            ScrollViewer.Height = 280;
            ScrollViewer.UpdateLayout();
            ScrollViewer.ScrollToVerticalOffset(double.MaxValue);
            KeyboardPanel.Visibility = Visibility.Visible;
        }

        private void TitlePanel_MouseEnter(object sender, MouseEventArgs e)
        {

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
                var error = dom.Element("error");
                XElement code = error.Element("code");
                if (code.Value == "0")
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
                    string auth_key = String.Format("full_auth_token {0}", settings["me2day_token"]);
                    string url = String.Format("http://me2day.net/api/get_comments/{0}.xml?akey=aed420d038f9b1a7fe3b5c0d94df22f5&post_id={1}", (string)settings["me2day_userid"], id);
                    HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
                    webRequest.Method = "POST";
                    webRequest.Credentials = new NetworkCredential((string)settings["me2day_userid"], auth_key);
                    IAsyncResult token = webRequest.BeginGetResponse(new AsyncCallback(GetReqeustStreamCallback), webRequest);
                    
                }
                else
                {
                    MessageBox.Show("Send comment failed. Please try again", "Sorry!", MessageBoxButton.OK);
                }
            }
            catch (WebException ex)
            {

            }
        }

        private void ApplicationBarIconButton_Click_1(object sender, EventArgs e)
        {
            string auth_key = String.Format("full_auth_token {0}", settings["me2day_token"]);
            string url = String.Format("http://me2day.net/api/metoo.xml?akey=aed420d038f9b1a7fe3b5c0d94df22f5&post_id={0}", id);
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Method = "POST";
            webRequest.Credentials = new NetworkCredential((string)settings["me2day_userid"], auth_key);
            IAsyncResult token = webRequest.BeginGetResponse(new AsyncCallback(GetReqeustStreamCallback3), webRequest);
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
                var error = dom.Element("error");
                XElement code = error.Element("code");
                if (code.Value == "0")
                {
                    Dispatcher.BeginInvoke(delegate()
                    {
                        if (me2_cnt > 0)
                            hour.Text += " 하고 나도 미투";
                        else
                            hour.Text += "내가 미투";
                    });
                }            
            }
            catch (WebException ex)
            {

            }
        }

        private void image_MouseEnter(object sender, MouseEventArgs e)
        {
            NavigationService.Navigate(new Uri("/PhotoViewer.xaml?photo=" + photo, UriKind.Relative));
        }
    }
}