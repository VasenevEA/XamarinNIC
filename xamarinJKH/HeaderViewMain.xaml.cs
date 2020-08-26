﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xamarinJKH.Utils;
using xamarinJKH.InterfacesIntegration;
using Plugin.Messaging;

namespace xamarinJKH
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HeaderViewMain : ContentView
    {
        public static BindableProperty PictureSource = BindableProperty.Create("Picture", typeof(string), typeof(string));
        public static BindableProperty TitleSource = BindableProperty.Create("Title", typeof(string), typeof(string));
        public string Phone
        {
            get => Settings.Person.companyPhone.Replace("+", "");
        }

        public string Name
        {
            get => Settings.MobileSettings.main_name;
        }

        public string Picture
        {
            get => (string)GetValue(PictureSource);
            set
            {
                SetValue(PictureSource, value);
                OnPropertyChanged("Picture");
            }
        }

        
        public string Title
        {
            get => (string)GetValue(TitleSource);
            set
            {
                SetValue(TitleSource, value);
                OnPropertyChanged("Title");
            }
        }
        public HeaderViewMain()
        {
            InitializeComponent();
            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    int statusBarHeight = DependencyService.Get<IStatusBar>().GetHeight();
                    Pancake.Padding = new Thickness(0, statusBarHeight, 0, 0);
                    //BackgroundColor = Color.White;                    
                    break;
                default:
                    break;
            }
            BindingContext = this;
            this.SizeChanged += HeaderViewMain_SizeChanged;
            this.MeasureInvalidated += HeaderViewMain_SizeChanged;
        }

        private void HeaderViewMain_SizeChanged(object sender, EventArgs e)
        {
            if (Application.Current.UserAppTheme == OSAppTheme.Dark || Application.Current.UserAppTheme == OSAppTheme.Unspecified)
            {
                Picture = Picture.Replace("_light", "");
            }
            else
            {
                Picture += "_light";
            }
        }

        private void Call(object sender, EventArgs args)
        {
            if (Settings.Person.Phone != null)
            {
                IPhoneCallTask phoneDialer;
                phoneDialer = CrossMessaging.Current.PhoneDialer;
                if (phoneDialer.CanMakePhoneCall)
                    phoneDialer.MakePhoneCall(Settings.Person.companyPhone);
            }
        }

        private void Tech(object sender, EventArgs args)
        {
            MessagingCenter.Send<HeaderViewMain>(this, "SendTech");
        }
    }
}