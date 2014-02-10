﻿using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace HDStream.HelperClasses
{
    public static class FBUris {
		//insert your appID appSecret here
		#region Hidden Strings (ApplicationID, Application Secret)
		#region AppID
        private static string m_strAppID = "100714686681329";
		#endregion
		#region AppSecret
        private static string m_strAppSecret = "795b2bf65bf626317a39735543565c5e";
		#endregion
		#endregion
		//permissions publish_stream, create_event, offline_access
		//user permissions read_stream, user_hometown
		//friends permissions friends_hometown
		//the correct url - but not working due to the WebBrowser fragment bug
		//private static string m_strLoginURL = "https://graph.facebook.com/oauth/authorize?client_id={0}&redirect_uri=http://www.facebook.com/connect/login_success.html&type=user_agent&display=touch&scope=publish_stream,user_hometown";
		//workaround - type=user_agent was removed
        private static string m_strLoginURL = "https://graph.facebook.com/oauth/authorize?client_id={0}&redirect_uri=http://www.facebook.com/connect/login_success.html&display=touch&scope=user_about_me,user_activities,user_birthday,user_education_history,user_events,user_groups,user_hometown,user_interests,user_likes,user_location,user_notes,user_online_presence,user_photo_video_tags,user_photos,user_relationships,user_relationship_details,user_religion_politics,user_status,user_videos,user_website,user_work_history,email,read_friendlists,read_insights,read_mailbox,read_requests,read_stream,friends_about_me,friends_activities,friends_birthday,friends_education_history,friends_events,friends_groups,friends_hometown,friends_interests,friends_likes,friends_location,friends_notes,friends_online_presence,friends_photo_video_tags,friends_photos,friends_relationships,friends_relationship_details,friends_religion_politics,friends_status,friends_videos,friends_website,friends_work_history,manage_friendlists,publish_stream";
		//neede for workaround step two to get the access toke - unfortunately requires app_secret
		private static string m_strGetAccessTokenURL = "https://graph.facebook.com/oauth/access_token?client_id={0}&redirect_uri=http://www.facebook.com/connect/login_success.html&client_secret={1}&code={2}";

		
		//logs out the app (must be followed by a user logout) - or you should kill the facebook cookies BEFORE you make this call - but we have no access to cookies in WP7
		private static string m_strAppLogoutURL = "http://facebook.com/logout.php?confirm=1&app_key={0}&session_key={1}&next=http://facebook.com/home.php";
		private static string m_strLoadUserDataURL = "https://graph.facebook.com/me?fields=id,name,gender,link,hometown,picture&locale=en_US&access_token={0}";
		private static string m_strLoadFriendsURL = "https://graph.facebook.com/me/friends?access_token={0}";
		private static string m_strPostMessageURL = "https://graph.facebook.com/{0}/feed";

		private static string m_strQueryURL = "https://api.facebook.com/method/fql.query?access_token={0}&format=JSON&query={1}";
		//http://developers.facebook.com/docs/reference/fql/stream
		//full query for all post (from your and your friends)
		//private static string m_strQuery = "SELECT post_id,viewer_id ,app_id,source_id,updated_time,created_time,attribution,actor_id,target_id,message,app_data,attachment,comments,likes,privacy,permalink FROM stream WHERE (source_id IN (SELECT uid2 FROM friend WHERE uid1 = me()) OR source_id=me())";
		private static string m_strQuery = "SELECT post_id,actor_id,source_id,attribution,message,attachment FROM stream WHERE app_id={0} AND (source_id IN (SELECT uid2 FROM friend WHERE uid1 = me()) OR source_id=me())";
		private static string m_strWall = "https://graph.facebook.com/{0}/feed?access_token={1}&fields=id,from,to,caption,description,attribution,message";
		private static string m_strNews = "https://graph.facebook.com/me/home?access_token={0}&fields=id,from,to,caption,description,attribution,message";

		public static Uri GetLoginUri() {
			return (new Uri(string.Format(m_strLoginURL, m_strAppID), UriKind.Absolute));
		}
		public static Uri GetAppLogoutUri(string strAccessToken) {
			return (new Uri(string.Format(m_strAppLogoutURL, m_strAppID,SplitToken(strAccessToken)), UriKind.Absolute));
		}

		public static Uri GetTokenLoadUri(string strCode) {
			return (new Uri(string.Format(m_strGetAccessTokenURL, m_strAppID, m_strAppSecret, strCode), UriKind.Absolute));
		}
		public static Uri GetLoadUserDataUri(string strAccessToken) {
			return (new Uri(string.Format(m_strLoadUserDataURL, strAccessToken), UriKind.Absolute));
		}
		public static Uri GetPostMessageUri(string strUserID="me"){
			return (new Uri(string.Format(m_strPostMessageURL,strUserID), UriKind.Absolute));
		}
		public static Uri GetLoadFriendsUri(string strAccessToken) {
			return (new Uri(string.Format(m_strLoadFriendsURL, strAccessToken), UriKind.Absolute));
		}
		public static Uri GetQueryUri(string strAccessToken) {
			return (new Uri(string.Format(m_strQueryURL, strAccessToken, HttpUtility.UrlEncode(string.Format(m_strQuery,m_strAppID))), UriKind.Absolute));
		}
		public static Uri GetWallUri(string strAccessToken, string strUserID="me") {
			return (new Uri(string.Format(m_strWall, strUserID, strAccessToken), UriKind.Absolute));
		}
		public static Uri GetNewsUri(string strAccessToken) {
			return (new Uri(string.Format(m_strNews, strAccessToken), UriKind.Absolute));
		}
		//retrieves the session key from the accesstoken
		private static string SplitToken(string strToken) {
			if(!string.IsNullOrEmpty(strToken)) {
				string[] aParts = strToken.Split('|');
				if(aParts.Length >= 3) {	//token format OK
					return (aParts[1]);
				}
			}
			return ("");
		}
	}
}
