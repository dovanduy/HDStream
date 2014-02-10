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

namespace HDStream
{
    public partial class SpeechLeft : UserControl
    {
        public string text { get; set; }
        public SpeechLeft()
        {   
            text = "";
            InitializeComponent();            
        }
        public void change_ui()
        {
            txtbox.Text = text;
            
        }
    }
}
