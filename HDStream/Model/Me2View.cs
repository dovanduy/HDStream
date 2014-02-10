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
    public class Me2View
    {
        public string id { get; set; }
        public string thumb_img { get; set; }
        public string text { get; set; }
        public string tag { get; set; }
        public string name { get; set; }
        public DateTime dtime { get; set; }
        public string time { get; set; }
        public string me2tag { get; set; }
        public string reply { get; set; }
        public Object media_obj { get; set; }
        public Visibility img_vis { get; set; }
        public string photo { get; set; }
        public string reply_visib { get; set; }
        public string me2 { get; set; }
    }
}
