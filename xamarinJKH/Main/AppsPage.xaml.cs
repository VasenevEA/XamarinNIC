﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xamarinJKH.Apps;
using xamarinJKH.Server;
using xamarinJKH.Server.RequestModel;
using xamarinJKH.Utils;

namespace xamarinJKH.Main
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AppsPage : ContentPage
    {
        public List<RequestInfo> RequestInfos { get; set; }
        private RequestList _requestList;
        private RestClientMP _server = new RestClientMP();
        private bool _isRefreshing = false;

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
            getApps();
            additionalList.ItemsSource = null;
            additionalList.ItemsSource = RequestInfos;
        }

        public AppsPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            SetText();
            getApps();
        }

        void SetText()
        {
            UkName.Text = Settings.MobileSettings.main_name;
            LabelPhone.Text = "Добрый день";
            Color hex = Color.FromHex(Settings.MobileSettings.color);
            SwitchApp.OnColor = hex;
            LabelSwitch.TextColor = hex;
            FrameBtnAdd.BackgroundColor = hex;
        }

        async void getApps()
        {
            _requestList = await _server.GetRequestsList();
            if (_requestList.Error == null)
            {
                RequestInfos = _requestList.Requests;
                Settings.UpdateKey = _requestList.UpdateKey;
                this.BindingContext = this;
            }
            else
            {
                await DisplayAlert("Ошибка", "Не удалось получить информацию о заявках", "OK");
            }
        }

        private async void OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            RequestInfo select = e.Item as RequestInfo;
            await Navigation.PushAsync(new AppPage(select));
        }
    }
}