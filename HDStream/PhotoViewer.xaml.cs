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
using System.Net.NetworkInformation;

namespace HDStream
{
    public partial class PhotoViewer : PhoneApplicationPage
    {
        private double initialScale;
        public PhotoViewer()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(ListPage_Loaded);
        }

        private void ListPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                MessageBox.Show("This application must require for internet connection. Please check your internet connection status", "Sorry", MessageBoxButton.OK);
                this.NavigationService.GoBack();
            }
            String photo;
            NavigationContext.QueryString.TryGetValue("photo", out photo);
            image.Source = new ImageSourceConverter().ConvertFromString(photo) as ImageSource;
        }

        private void OnPinchStarted(object sender, PinchStartedGestureEventArgs e)
        {
            
            initialScale = transform.ScaleX;
        }


        private void OnPinchDelta(object sender, PinchGestureEventArgs e)
        {


            if (initialScale * e.DistanceRatio < 1.7 && initialScale * e.DistanceRatio > 0.5)
            {
                transform.ScaleX = initialScale * e.DistanceRatio;
                transform.ScaleY = initialScale * e.DistanceRatio;
            }
        }

        private void OnDragStarted(object sender, DragStartedGestureEventArgs e)
        {
            
        }


        private void OnDragDelta(object sender, DragDeltaGestureEventArgs e)
        {
            
            transform.TranslateX += e.HorizontalChange;
            transform.TranslateY += e.VerticalChange;
        }

        private void OnDragCompleted(object sender, DragCompletedGestureEventArgs e)
        {
            
        }

        private void OnTap(object sender, GestureEventArgs e)
        {
            
        }
    }
}