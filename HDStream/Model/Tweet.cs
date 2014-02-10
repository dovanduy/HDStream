using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace HDStream.Model
{
    public class Tweet
    {
        public long id { get; set; }
        public string thumb_img { get; set; }
        public string text { get; set; }
        public string name { get; set; }
        public string time { get; set; }
        public DateTime dtime { get; set; }
    }
}
