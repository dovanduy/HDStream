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
using System.IO;
using Microsoft.Phone.Controls;
using HDStream.HelperClasses;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Xml;
using System.Xml.Linq;
using System.IO.IsolatedStorage;
using System.Net.NetworkInformation;

namespace HDStream
{
    public partial class FacebookAuth : PhoneApplicationPage
    {
        private string token;
        private string name;
        private string id;
        private IsolatedStorageSettings settings;
        public FacebookAuth()
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
            if (settings.Contains("facebook_token"))
            {
                if (MessageBox.Show("You've aleardy setting facebook account. Reset your account?", "Notice", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                {
                    this.NavigationService.GoBack();
                }
            }
            wbLogin.Navigate(FBUris.GetLoginUri()); 
        }

        private void wbLogin_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            string strLoweredAddress = e.Uri.OriginalString.ToLower();
            if (strLoweredAddress.StartsWith("http://www.facebook.com/connect/login_success.html?code="))
            {   
                TryToGetToken(e.Uri.OriginalString.Substring(56));
                return;
            }
            string strTest = wbLogin.SaveToString();
            if (strTest.Contains("access_token"))
            {
                int nPos = strTest.IndexOf("access_token");
                string strPart = strTest.Substring(nPos + 13);
                nPos = strPart.IndexOf("</PRE>");
                strPart = strPart.Substring(0, nPos);
                nPos = strPart.IndexOf("&amp;expires");
                if (nPos != -1)
                {
                    strPart = strPart.Substring(0, nPos);
                }
                token = strPart;
                string url;
                url = string.Format("https://graph.facebook.com/me/?access_token={0}", token);
                WebClient wc = new WebClient(); 
                wc.DownloadStringCompleted += new DownloadStringCompletedEventHandler(wc_openHandler);
                wc.DownloadStringAsync(new Uri(url), UriKind.Absolute);
                //wndLoginConfirmed.IsOpen = true;
                return;
            }
        }

        private void wc_openHandler(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                string jsonstr = e.Result.ToString();
                JObject o = JObject.Parse(jsonstr);                
                name = (String)o["name"];
                id = (String)o["id"];
                settings["facebook_token"] = token;
                settings["facebook_name"] = name;
                settings["facebook_chk"] = "1";
                settings["facebook_id"] = id;
                settings.Save();
                MessageBox.Show("Login successfully", "Thnaks", MessageBoxButton.OK);
                this.NavigationService.GoBack();
            }
        }
        
        private void TryToGetToken(string strCode)
        {
            wbLogin.Navigate(FBUris.GetTokenLoadUri(strCode));
        }
    }
}