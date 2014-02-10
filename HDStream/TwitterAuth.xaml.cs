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
using System.Windows.Threading;
using System.Threading;
using TweetSharp;
using System.IO.IsolatedStorage;
using System.Net.NetworkInformation;

namespace HDStream
{
    public partial class TwitterAuth : PhoneApplicationPage
    {
        private readonly string _consumerKey;
        private readonly string _consumerSecret;
        private IsolatedStorageSettings settings;
        private TwitterService _service;
        private OAuthRequestToken _requestToken;
        private OAuthAccessToken _accessToken;

        private bool _browserLoaded;

        public TwitterAuth()
        {
            
            _browserLoaded = false;
            _consumerKey = "g8F2KdKH40gGp9BXemw13Q";
            _consumerSecret = "OyFRFsI05agcJtURtLv8lpYbYRwZAIL5gr5xQNPW0Q";
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
            if (settings.Contains("twitter_screenname"))
            {
                if (MessageBox.Show("You've aleardy setting twitter account. Reset your account?", "Notice", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                {
                    this.NavigationService.GoBack();
                }
            }
            _service = new TwitterService(_consumerKey, _consumerSecret);
            _service.GetRequestToken(HandleAuthenticationRequestToken);
        }

        protected virtual void HandleAuthenticationRequestToken(OAuthRequestToken token, TwitterResponse response)
        {
            _requestToken = token; // Save the token

            while (!_browserLoaded)
            {
                Thread.Sleep(200);
            }

            Dispatcher.BeginInvoke(
                () => wbLogin.Navigate(_service.GetAuthorizationUri(_requestToken))
                );
        }

        private void wbLogin_Loaded(object sender, RoutedEventArgs e)
        {
            _browserLoaded = true;
        }

        
        private void HandleAuthenticationAccessToken(OAuthAccessToken token, TwitterResponse response)
        {
            _accessToken = token;
            settings["twitter_screenname"] = _accessToken.ScreenName;
            settings["twitter_token"] = _accessToken.Token;
            settings["twitter_tokensecret"] = _accessToken.TokenSecret;
            settings["twitter_chk"] = "1";
            settings.Save();
            Dispatcher.BeginInvoke(delegate()
            {
                success();
            });

        }

        private void success()
        {
            MessageBox.Show("Login successfully", "Thanks", MessageBoxButton.OK);
            this.NavigationService.GoBack();
        }

        private void wbLogin_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            string strTest = wbLogin.SaveToString();
            if (strTest.Contains("oauth_pin>"))
            {
                int nPos = strTest.IndexOf("oauth_pin>");
                string strPart = strTest.Substring(nPos + 10);
                nPos = strPart.IndexOf("</DIV>");
                strPart = strPart.Substring(0, nPos);
                _service.GetAccessToken(_requestToken, strPart, HandleAuthenticationAccessToken);
            }
        }
    }
}