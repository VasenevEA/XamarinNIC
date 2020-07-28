﻿using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xamarinJKH.ViewModels;
using xamarinJKH.Server.RequestModel;
using xamarinJKH.Utils;
using xamarinJKH.Server;
using xamarinJKH.InterfacesIntegration;
using Xamarin.Essentials;
using xamarinJKH.CustomRenderers;
using System.Net.Http;
using AiForms.Dialogs;

namespace xamarinJKH.Pays
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PayPdf : ContentPage
    {
        public PayPdfViewModel viewModel { get; set; }
        public PayPdf(PayPdfViewModel vm)
        {
            InitializeComponent();
            BindingContext = viewModel = vm;
            viewModel.LoadPdf.Execute(null);
            View pdfview;
            if (Device.RuntimePlatform == "Android")
            {
                pdfview = new CustomWebView()
                {
                    VerticalOptions = LayoutOptions.FillAndExpand,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    Uri = vm.Bill.FileLink
                };
                Content.Children.Add(pdfview);
            }
            else
            {
                pdfview = new WebView()
                {
                    VerticalOptions = LayoutOptions.FillAndExpand,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    Source = vm.Bill.FileLink
                };
                Content.Children.Add(pdfview);
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        async void GoBack(object sender, EventArgs args)
        {
            await Navigation.PopAsync();
        }

        async void ShareBill(object sender, EventArgs args)
        {
            await Xamarin.Essentials.Share.RequestAsync(new ShareTextRequest()
            {
                Uri = viewModel.Bill.FileLink,
                Text = "Поделиться квитанцией"
            });
        }

        async void Print(object sender, EventArgs args)
        {
            HttpClient client = new HttpClient();
            Loading.Instance.Show("Подождите, идет загрузка");
            try
            {
                var file = await client.GetByteArrayAsync(viewModel.Bill.FileLink);
                if (file != null)
                    DependencyService.Get<xamarinJKH.InterfacesIntegration.IPrintManager>().SendFileToPrint(file);
                else
                    await DisplayAlert(null, "Произошла ошибка при скачивании файла", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert(null, "Произошла ошибка, попробуйте перезапустить приложение", "ОК");
            }
            finally
            {
                Loading.Instance.Hide();
            }
        }
    }

    

    public class PayPdfViewModel : BaseViewModel
    {
        BillInfo _bill;
        public string Theme
        {
            get => "#" + Settings.MobileSettings.color;
        }
        public BillInfo Bill
        {
            get => _bill;
            set
            {
                _bill = value;
                OnPropertyChanged("Bill");
            }
        }
        public string Phone
        {
            get =>  "+" + Settings.Person.companyPhone.Replace("+","");
        }

        public Command LoadPdf { get; set; }
        string _path;
        public string Path
        {
            get => _path;
            set
            {
                _path = value;
                OnPropertyChanged("Path");
            }
        }
        public string Name
        {
            get => Settings.MobileSettings.main_name;
        }
        public PayPdfViewModel(BillInfo info)
        {
            Bill = info;
            RestClientMP server = new RestClientMP();
            
            LoadPdf = new Command(async () =>
            {
               //TODO: Получение ссылки на настоящий файл квитанции с бека
                Path = info.FileLink;//"file:///" + DependencyService.Get<IFileWorker>().GetFilePath(filename);
            });
        }
    }
}