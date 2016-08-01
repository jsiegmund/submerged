using Repsaj.Submerged.GatewayApp.Models;
using System;
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
    public partial class ModuleRowControl : UserControl
    {
        public ModuleRowControl()
        {
            this.InitializeComponent();
            this.DataContextChanged += (elem, args) => { this.Bindings.Update(); };
        }

        #region ModuleName Property
        public string ModuleName
        {
            get { return (string)GetValue(TileModuleNameProperty); }
            set { SetValue(TileModuleNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TileText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TileModuleNameProperty =
            DependencyProperty.Register(nameof(ModuleName), typeof(string), typeof(ModuleRowControl), null);
        #endregion

        #region ModuleStatus Property
        public string ModuleStatus
        {
            get { return (string)GetValue(TileModuleStatusProperty); }
            set { SetValue(TileModuleStatusProperty, value); }
        }

        public static readonly DependencyProperty TileModuleStatusProperty =
            DependencyProperty.Register(nameof(ModuleStatus), typeof(string), typeof(ModuleRowControl), null);
        #endregion

    }
}
