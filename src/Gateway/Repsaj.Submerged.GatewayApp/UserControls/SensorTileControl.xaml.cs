using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Repsaj.Submerged.GatewayApp.UserControls
{
    public partial class SensorTileControl : UserControl
    {
        public SensorTileControl()
        {
            this.InitializeComponent();
            borMain.DataContext = this;
        }


        #region SensorName Property
        public string SensorName
        {
            get { return (string)GetValue(TileSensorNameProperty); }
            set { SetValue(TileSensorNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TileText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TileSensorNameProperty =
            DependencyProperty.Register("SensorName", typeof(string), typeof(SensorTileControl), null);
        #endregion

        #region Reading Property
        public string Reading
        {
            get { return (string)GetValue(TileReadingProperty); }
            set { SetValue(TileReadingProperty, value); }
        }

        public static readonly DependencyProperty TileReadingProperty =
            DependencyProperty.Register("Reading", typeof(string), typeof(SensorTileControl), null);
        #endregion

        #region ImageUri Property
        public Uri ImageUri
        {
            get { return (Uri)GetValue(TileImageUriProperty); }
            set { SetValue(TileImageUriProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TileImageUri.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TileImageUriProperty =
            DependencyProperty.Register("ImageUri", typeof(Uri), typeof(SensorTileControl), null);
        #endregion


    }
}
