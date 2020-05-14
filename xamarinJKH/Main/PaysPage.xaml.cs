﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xamarinJKH.Pays;
using xamarinJKH.Server;
using xamarinJKH.Server.RequestModel;
using xamarinJKH.Utils;

namespace xamarinJKH.Main
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PaysPage : ContentPage
    {
        public List<AccountAccountingInfo> _accountingInfo { get; set; }
        private RestClientMP _server = new RestClientMP();
        public PaysPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            SetTextAndColor();
            getInfo();
            var openSaldos = new TapGestureRecognizer();
            openSaldos.Tapped += async (s, e) => { await Navigation.PushAsync(new SaldosPage(_accountingInfo)); };
            FrameBtnSaldos.GestureRecognizers.Add(openSaldos);
        }
        
        void SetTextAndColor()
        {
            UkName.Text = Settings.MobileSettings.main_name;
            LabelPhone.Text = "+" + Settings.Person.Phone;
            FrameBtnHistory.BorderColor = Color.FromHex(Settings.MobileSettings.color);
            FrameBtnSaldos.BorderColor = Color.FromHex(Settings.MobileSettings.color);
            IconViewHistory.Foreground = Color.FromHex(Settings.MobileSettings.color);
            IconViewSaldos.Foreground = Color.FromHex(Settings.MobileSettings.color);
            LabelSaldos.TextColor = Color.FromHex(Settings.MobileSettings.color);
            LabelHistory.TextColor = Color.FromHex(Settings.MobileSettings.color);
        }

        async void getInfo()
        {
            ItemsList<AccountAccountingInfo> info = await _server.GetAccountingInfo();
            if (info.Error == null)
            {
                _accountingInfo = info.Data;
                this.BindingContext = this;
            }
            else
            {
                await DisplayAlert("Ошибка", "Не удалось получить информацию о начислениях", "OK");
            }
        }

        private async void OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            AccountAccountingInfo select = e.Item as AccountAccountingInfo;
            await Navigation.PushAsync(new CostPage(select, _accountingInfo));
        }
    }
}