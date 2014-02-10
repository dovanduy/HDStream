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
using System.Xml;
using System.Xml.Linq;
using System.IO;
using Microsoft.Phone.Controls;
using System.IO.IsolatedStorage;
using System.Net.NetworkInformation;

namespace HDStream
{
    public partial class Me2dayAuth : PhoneApplicationPage
    {
        private String url;
        private String token;
        private IsolatedStorageSettings settings;

        public Me2dayAuth()
        {
            InitializeComponent();
            Loaded += MainPageLoaded;
        }

        private void MainPageLoaded(object sender, RoutedEventArgs e)
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                MessageBox.Show("This application must require for internet connection. Please check your internet connection status", "Sorry", MessageBoxButton.OK);
                this.NavigationService.GoBack();
            }
            settings = IsolatedStorageSettings.ApplicationSettings;
            if (settings.Contains("me2day_userid"))
            {
                if (MessageBox.Show("You've aleardy setting me2day account. Reset your account?", "Notice", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                {
                    this.NavigationService.GoBack();
                }
            }
            string url;
            url = "http://me2day.net/api/get_auth_url.xml?akey=aed420d038f9b1a7fe3b5c0d94df22f5";
            WebClient wc = new WebClient();
            wc.OpenReadCompleted += new OpenReadCompletedEventHandler(wc_openHandler);
            wc.OpenReadAsync(new Uri(url), UriKind.Absolute);
        }

        private void wc_openHandler(object sender, OpenReadCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                using (Stream s = e.Result)
                {
                    XElement xml = XElement.Load(s);
                    XElement url_el = xml.Element("url");
                    XElement token_el = xml.Element("token");
                    url = url_el.Value;
                    wbLogin.Navigate(new Uri(url_el.Value));
                    token = token_el.Value;
                }
            }
        }

        private void wc_openHandler2(object sender, OpenReadCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                using (Stream s = e.Result)
                {
                    XElement xml = XElement.Load(s);
                    XElement id_el = xml.Element("user_id");
                    XElement token_el = xml.Element("full_auth_token");
                    settings["me2day_userid"] = id_el.Value;
                    settings["me2day_token"] = token_el.Value;
                    settings["me2day_chk"] = "1";
                    settings.Save();

                    MessageBox.Show("Login successfully", "Thanks", MessageBoxButton.OK);
                    this.NavigationService.GoBack();
                   
                    /*
                    string url = "http://me2day.net/api/noop.xml?akey=aed420d038f9b1a7fe3b5c0d94df22f5";
                    HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
                    webRequest.Method = "POST";

                    string auth_key = String.Format("full_auth_token {0}", token_el.Value);
                    webRequest.Credentials = new NetworkCredential(id_el.Value, auth_key);
                    IAsyncResult token = webRequest.BeginGetResponse(new AsyncCallback(GetReqeustStreamCallback), webRequest);
                     */
                }
            }
            else
            {
                MessageBox.Show("Progress isn't completed. Please retry login", "Sorry", MessageBoxButton.OK);
                wbLogin.Navigate(new Uri(url));
            }
        }

        /*
        private void GetReqeustStreamCallback(IAsyncResult asynchronousResult)
        {
            try
            {
                WebResponse response = ((HttpWebRequest)asynchronousResult.AsyncState).EndGetResponse(asynchronousResult);
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string responseString = reader.ReadToEnd();
                reader.Close();
                token = "";
            }
            catch (WebException ex)
            {
                MessageBox.Show("Progress isn't completed. Please retry login", "Sorry", MessageBoxButton.OK);
                wbLogin.Navigate(new Uri(url));
            }
        }
         * */

        private void wbLogin_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            string strTest = wbLogin.SaveToString();
            if (strTest.Contains("X-ME2API-AUTH-RESULT"))
            {
                if (strTest.Contains("accepted"))
                {
                    string url;
                    url = String.Format("http://me2day.net/api/get_full_auth_token.xml?akey=aed420d038f9b1a7fe3b5c0d94df22f5&token={0}", token);
                    WebClient wc = new WebClient();
                    wc.OpenReadCompleted += new OpenReadCompletedEventHandler(wc_openHandler2);
                    wc.OpenReadAsync(new Uri(url), UriKind.Absolute);
                }else{
                    MessageBox.Show("Please accept me2day", "Sorry", MessageBoxButton.OK);
                    wbLogin.Navigate(new Uri(url));
                }
            }
        }
    }
}