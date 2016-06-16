﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
    public sealed partial class RelayRowControl : UserControl
    {
        public RelayRowControl()
        {
            this.InitializeComponent();
            borMain.DataContext = this;
        }

        #region RelayName Property
        public string RelayName
        {
            get { return (string)GetValue(TileRelayNameProperty); }
            set { SetValue(TileRelayNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TileText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TileRelayNameProperty =
            DependencyProperty.Register(nameof(RelayName), typeof(string), typeof(RelayRowControl), null);
        #endregion

        #region RelayState Property
        public bool RelayState
        {
            get { return (bool)GetValue(TileRelayStateProperty); }
            set { SetValue(TileRelayStateProperty, value); }
        }

        public static readonly DependencyProperty TileRelayStateProperty =
            DependencyProperty.Register(nameof(RelayState), typeof(bool), typeof(RelayRowControl), null);
        #endregion

        #region RelayStateAsText Property
        public string RelayStateAsText
        {
            get { return RelayState ? "ON" : "OFF"; }
        }
        #endregion
    }
}