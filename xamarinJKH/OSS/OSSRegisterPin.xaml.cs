﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xamarinJKH.Server;
using xamarinJKH.Utils;

namespace xamarinJKH
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class OSSRegisterPin : ContentPage
    {
        Color hex = Color.FromHex(Settings.MobileSettings.color);

        public OSSRegisterPin()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);

            //Запрос в смс/пуше кода подтверждения 
            GetAuthCode();

            NavigationPage.SetHasNavigationBar(this, false);

            UkName.Text = Settings.MobileSettings.main_name;
            IconViewCode.Foreground = hex;

            IconViewLogin.Foreground = hex;
            
            IconViewPass0.Foreground = hex;
            IconViewPass.Foreground = hex;

            ImageClosePass0.Foreground = hex;
            ImageClosePass.Foreground = hex;

            FrameBtnLogin.BackgroundColor = hex;
            LabelseparatorPass0.BackgroundColor = hex;
            LabelseparatorPass.BackgroundColor = hex;
            
            progress.Color = hex;

            var backClick = new TapGestureRecognizer();
            backClick.Tapped += async (s, e) => { _ = await Navigation.PopAsync(); };
            BackStackLayout.GestureRecognizers.Add(backClick);

            var forgetPasswordVisible0 = new TapGestureRecognizer();
            forgetPasswordVisible0.Tapped += async (s, e) =>
            {
                EntryPin0.IsPassword = !EntryPin0.IsPassword;
                if (EntryPin0.IsPassword)
                {
                    ImageClosePass0.Foreground = Color.FromHex(Settings.MobileSettings.color);
                }
                else
                {
                    ImageClosePass0.Foreground = Color.DarkSlateGray;
                }

            };
            ImageClosePass0.GestureRecognizers.Add(forgetPasswordVisible0);

            var forgetPasswordVisible = new TapGestureRecognizer();
            forgetPasswordVisible.Tapped += async (s, e) =>
            {
                EntryPin.IsPassword = !EntryPin.IsPassword;
                if (EntryPin.IsPassword)
                {
                    ImageClosePass.Foreground = Color.FromHex(Settings.MobileSettings.color);
                }
                else
                {
                    ImageClosePass.Foreground = Color.DarkSlateGray;
                }

            };
            ImageClosePass.GestureRecognizers.Add(forgetPasswordVisible);

            var startLogin = new TapGestureRecognizer();
            startLogin.Tapped += RegisterPin;
            FrameBtnLogin.GestureRecognizers.Add(startLogin);
        }

        readonly RestClientMP rc = new RestClientMP();
        async void GetAuthCode()
        {
            if(Settings.Person.Accounts!=null && Settings.Person.Accounts.Count>0)
            {
                if(string.IsNullOrWhiteSpace(Settings.Person.Phone))
                {
                    await DisplayAlert("Ошибка", "Добавьте номер Вашего телефона", "OK");
                    return;
                }
                //вызвали метод сервера для получения регстрационного кода по телефону/номеру счета 
                var r = await rc.OSSCheckCode(Settings.Person.Phone, Settings.Person.Accounts[0].Ident);
                if(!string.IsNullOrWhiteSpace( r.Error))
                {
                    await DisplayAlert("Ошибка", "Ошибка при отправке проверочного кода: "+r.Error, "OK");
                    return;
                }
                await DisplayAlert("Внимание", "Вам отправлен проверочный код, введите его в соответсвующее поле", "OK");
            }
            else
            {
                await DisplayAlert("Ошибка", "Добавьте Ваш лицевой счет", "OK");
                return;
            }
        }

        async void RegisterPin(object sender, EventArgs e)
        {
            progress.IsVisible = true;
            FrameBtnLogin.IsVisible = false;

            //код
            var code = EntryCode.Text;
            if(string.IsNullOrWhiteSpace(code))
            {
                await DisplayAlert("Ошибка", "Введите проверочный код", "OK");
                progress.IsVisible = false;
                FrameBtnLogin.IsVisible = true;
                return;
            }

            if (string.IsNullOrWhiteSpace( EntryPin0.Text) || string.IsNullOrWhiteSpace( EntryPin.Text))
            {
                //пин не введен
                await DisplayAlert("Ошибка", "Введите пин-код в оба поля", "OK");

                progress.IsVisible = false;
                FrameBtnLogin.IsVisible = true;

                return;
            }

            if (EntryPin0.Text!=EntryPin.Text)
            {
                //введенный пин не совпадает
                await DisplayAlert("Ошибка", "Введенные пин-коды не совпадают", "OK");
                
                progress.IsVisible = false;
                FrameBtnLogin.IsVisible = true;
                
                return;
            }

            //сохраняем пин на сервер, вместе с проверочным кодом
            var rez = await rc.OSSSavePin(Settings.Person.Phone, code, EntryPin0.Text);

            if(!string.IsNullOrWhiteSpace(rez.Error))
            {
                await DisplayAlert("Ошибка", rez.Error, "OK");
                progress.IsVisible = false;
                FrameBtnLogin.IsVisible = true;
                return;

            }

            progress.IsVisible = false;
            FrameBtnLogin.IsVisible = true;

            //далее переход на форму голосования, или на форму ввода пина
            await Navigation.PushAsync(new OSSAuth());            
        }
    }
}