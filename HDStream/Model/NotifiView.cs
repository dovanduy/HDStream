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
    public class NotifiView
    {
        public string name { get; set; }
        public string id { get; set; }
        public string type { get; set; }
        public string thumb_img { get; set; }
        public string text { get; set; }
        public DateTime dtime { get; set; }
        public string dstring { get; set; }
    }
}
