﻿using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.PancakeView;
using Xamarin.Forms.Xaml;
using xamarinJKH.DialogViews;
using xamarinJKH.InterfacesIntegration;
using xamarinJKH.Utils;
using xamarinJKH.ViewModels.Shop;

namespace xamarinJKH.Shop
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BasketPageNew : ContentPage
    {
        private Color hex;

        public BasketPageNew(ShopViewModel vm)
        {
            InitializeComponent();

            if (Device.RuntimePlatform == Device.iOS)
            {
                int statusBarHeight = DependencyService.Get<IStatusBar>().GetHeight();
                Pancake.Padding = new Thickness(0, statusBarHeight, 0, 0);
            }

            //BorderColor = "{AppThemeBinding Light={x:DynamicResource MainColor}, Dark=#e7e7e7}"
            hex = Color.FromHex(Settings.MobileSettings.color);
            Color hexColor = (Color)Application.Current.Resources["MainColor"];
            GoodsLayot.SetAppThemeColor(PancakeView.BorderColorProperty, hexColor, Color.White);

            var techSend = new TapGestureRecognizer();
            techSend.Tapped += async (s, e) => { await PopupNavigation.Instance.PushAsync(new TechDialog()); };
            LabelTech.GestureRecognizers.Add(techSend);


            BindingContext = vm;
        }
        async void Back(object sender, EventArgs args)
        {
            try
            {
                _ = await Navigation.PopAsync();
            }
            catch { }
        }

    }
}