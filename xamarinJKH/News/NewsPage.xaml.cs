﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Plugin.Messaging;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xamarinJKH.Server;
using xamarinJKH.Server.RequestModel;
using xamarinJKH.Tech;
using xamarinJKH.Utils;

namespace xamarinJKH.News
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NewsPage : ContentPage
    {
        public List<NewsInfo> NewsInfos { get; set; }
        private bool _isRefreshing = false;
        private RestClientMP server = new RestClientMP();

        public bool IsRefreshing
        {
            get { return _isRefreshing; }
            set
            {
                _isRefreshing = value;
                OnPropertyChanged(nameof(IsRefreshing));
            }
        }

        public ICommand RefreshCommand
        {
            get
            {
                return new Command(async () =>
                {
                    IsRefreshing = true;

                    await RefreshData();

                    IsRefreshing = false;
                });
            }
        }

        private async Task RefreshData()
        {
            Settings.EventBlockData = await server.GetEventBlockData();
            if (Settings.EventBlockData.Error == null)
            {
                NewsInfos = Settings.EventBlockData.News;
                NotificationList.ItemsSource = null;
                NotificationList.ItemsSource = NewsInfos;
            }else
            {
                await DisplayAlert("Ошибка", "Не удалось получить информацию о новостях", "OK");
            }
        }

        public NewsPage()
        {
            InitializeComponent();
            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    BackgroundColor = Color.White;
                    BackgroundColor = Color.White;
                    // ImageTop.Margin = new Thickness(0, 0, 0, 0);
                    // StackLayout.Margin = new Thickness(0, 33, 0, 0);
                    // IconViewNameUk.Margin = new Thickness(0, 33, 0, 0);
                    // RelativeLayoutTop.Margin = new Thickness(0,0,0,0);
                    // if (App.ScreenHeight <= 667)//iPhone6
                    // {
                    //     //NotificationList.Margin = new Thickness(0,-110,0,0);
                    //     RelativeLayoutTop.Margin = new Thickness(0, 0, 0, -110);
                    // }
                    // else if (App.ScreenHeight <= 736)//iPhone8Plus Height=736
                    // {
                    //     RelativeLayoutTop.Margin = new Thickness(0, 0, 0, -145);
                    // }
                    // else
                    // {
                    //     RelativeLayoutTop.Margin = new Thickness(0, 0, 0, -180);
                    // }
                    // if (Application.Current.MainPage.Height > 800)
                    // {
                    //     RelativeLayoutTop.Margin = new Thickness(0, 0, 0, -180);
                    // }
                    break;
                case Device.Android:
                    // RelativeLayoutTop.Margin = new Thickness(0,0,0,-135);
                    // double or = Math.Round(((double) App.ScreenWidth / (double) App.ScreenHeight), 2);
                    // if (Math.Abs(or - 0.5) < 0.02)
                    // {
                    //     RelativeLayoutTop.Margin = new Thickness(0,0,0,-90);
                    // }

                    break;
                default:
                    break;
            }
            var techSend = new TapGestureRecognizer();
            techSend.Tapped += async (s, e) => {     await Navigation.PushAsync(new TechSendPage()); };
            LabelTech.GestureRecognizers.Add(techSend);
            var call = new TapGestureRecognizer();
            call.Tapped += async (s, e) =>
            {
                if (Settings.Person.Phone != null)
                {
                    IPhoneCallTask phoneDialer;
                    phoneDialer = CrossMessaging.Current.PhoneDialer;
                    if (phoneDialer.CanMakePhoneCall) 
                        phoneDialer.MakePhoneCall(Settings.Person.Phone);
                }

            
            };
            LabelPhone.GestureRecognizers.Add(call);
            NavigationPage.SetHasNavigationBar(this, false);
            var backClick = new TapGestureRecognizer();
            backClick.Tapped += async (s, e) => { _ = await Navigation.PopAsync(); };
            BackStackLayout.GestureRecognizers.Add(backClick);
            SetText();
            NewsInfos = Settings.EventBlockData.News;
            this.BindingContext = this;
            NotificationList.BackgroundColor = Color.Transparent;
            NotificationList.Effects.Add(Effect.Resolve("MyEffects.ListViewHighlightEffect"));

        }

        void SetText()
        {
            UkName.Text = Settings.MobileSettings.main_name;
            LabelPhone.Text =  "+" + Settings.Person.companyPhone.Replace("+","");
        }

        private async void OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            NewsInfo select = e.Item as NewsInfo;
            await Navigation.PushAsync(new NewPage(select));
        }
    }
}