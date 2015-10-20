using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using ChangeWallpaperApp.ViewModels;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ChangeWallpaperApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            var vm = DataContext as MainPageViewModel;
            if (vm == null) return;
            vm.ChangeSuccessful += OnChangeSuccessful;
            vm.ChangeFailed += OnChangeFailed;
        }

        private static async void OnChangeSuccessful(object sender, EventArgs e)
        {
            var msg = new MessageDialog("Your Wallpaper should be updated!", "Sweet!");
            await msg.ShowAsync();
        }

        private static async void OnChangeFailed(object sender, EventArgs e)
        {
            var msg = new MessageDialog("Um... something happened, something bad. There was an error :(.", "Oh Fuck");
            await msg.ShowAsync();
        }
    }
}
