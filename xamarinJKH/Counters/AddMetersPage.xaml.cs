using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xamarinJKH.Counters;
using xamarinJKH.Main;
using xamarinJKH.InterfacesIntegration;
using xamarinJKH.Server;
using xamarinJKH.Server.RequestModel;
using xamarinJKH.Utils;
using Xamarin.Forms.Internals;
using System.Security.Cryptography;
using Plugin.Messaging;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms.Markup;
using Xamarin.Forms.PancakeView;
using xamarinJKH.Tech;
using Xamarin.Essentials;
using xamarinJKH.DialogViews;

namespace xamarinJKH.Counters
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddMetersPage : ContentPage
    {
        private RestClientMP _server = new RestClientMP();
        private MeterInfo meter = new MeterInfo();
        private List<MeterInfo> meters = new List<MeterInfo>();
        private CountersPage _countersPage;

        public Color CellColor { get; set; } 
        public decimal PrevCounter { get; set; }
        decimal PrevValue;
        bool SetPrev;
        int DecimalPoint { get; set; }
        public AddMetersPage(MeterInfo meter, List<MeterInfo> meters, CountersPage countersPage, decimal counterThisMonth = 0, decimal counterPrevMonth = 0)
        {            
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            _countersPage = countersPage;

            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    int statusBarHeight = DependencyService.Get<IStatusBar>().GetHeight();
                    Pancake.Padding = new Thickness(0, statusBarHeight, 0, 0);
                    break;
                default:
                    break;
            }

            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    //для iphone5,5s,se,5c
                    if (Xamarin.Essentials.DeviceDisplay.MainDisplayInfo.Width < 700)
                    {
                        CounterLayout.Margin = new Thickness(5, 0);
                        meterRootStack.Margin = new Thickness(5);

                        NameLbl.FontSize = 15;
                        UniqNumLbl.FontSize = 13;
                        CheckupLbl.FontSize = 13;
                        RecheckLbl.FontSize = 13;
                    }

                    UniqNumLbl.Margin = new Thickness(0, 5, 0, -5);
                    CheckupLbl.Margin = new Thickness(0, -5, 0, -5);
                    RecheckLbl.Margin = new Thickness(0, -5, 0, -5);

                    //BackgroundColor = Color.White;
                    // ImageFon.Margin = new Thickness(0, 0, 0, 0);
                    // StackLayout.Margin = new Thickness(0, 33, 0, 0);
                    // IconViewNameUk.Margin = new Thickness(0, 33, 0, 0);
                    // if (Application.Current.MainPage.Height > 800)
                    // {
                    //     ScrollViewContainer.Margin = new Thickness(0, 0, 0, -180);
                    //     BackStackLayout.Margin = new Thickness(-5, 35, 0, 0);
                    // }
                    break;
                case Device.Android:
                    // ScrollViewContainer.Margin = new Thickness(0, 0, 0, -162);
                    // double or = Math.Round(((double)App.ScreenWidth / (double)App.ScreenHeight), 2);
                    // if (Math.Abs(or - 0.5) < 0.02)
                    // {
                    //     ScrollViewContainer.Margin = new Thickness(0, 0, 0, -110);
                    //     BackStackLayout.Margin = new Thickness(-5, 15, 0, 0);
                    // }
                    break;
                default:
                    break;
            }
            this.meter = meter;
            this.meters = meters;
            var backClick = new TapGestureRecognizer();
            backClick.Tapped += async (s, e) => {
                try
                {
                    _ = await Navigation.PopAsync();
                }
                catch { }
            };
            BackStackLayout.GestureRecognizers.Add(backClick);
            var techSend = new TapGestureRecognizer();
            techSend.Tapped += async (s, e) => {     await PopupNavigation.Instance.PushAsync(new TechDialog()); };
            LabelTech.GestureRecognizers.Add(techSend);
            var call = new TapGestureRecognizer();
            call.Tapped += async (s, e) =>
            {
                if (Settings.Person.Phone != null)
                {
                    IPhoneCallTask phoneDialer;
                    phoneDialer = CrossMessaging.Current.PhoneDialer;
                    if (phoneDialer.CanMakePhoneCall && !string.IsNullOrWhiteSpace(Settings.Person.companyPhone)) 
                        phoneDialer.MakePhoneCall(System.Text.RegularExpressions.Regex.Replace(Settings.Person.companyPhone, "[^+0-9]", ""));
                }

            
            };
            LabelPhone.GestureRecognizers.Add(call);
            var saveClick = new TapGestureRecognizer();
            saveClick.Tapped += async (s, e) => { ButtonClick(FrameBtnLogin, null); };
            FrameBtnLogin.GestureRecognizers.Add(saveClick);
            FrameBtnLogin.BackgroundColor = Color.FromHex(xamarinJKH.Utils.Settings.MobileSettings.color);

            if (counterPrevMonth > 0)
            {
                SetPrevious(counterPrevMonth);
                SetPrev = true;
            }

            if(counterThisMonth>0)
            {
                meterReadingName.Text = AppResources.ChangePenance;
                SetCurrent(counterThisMonth);
            }
            else
            {
                meterReadingName.Text = AppResources.NewData;
            }

            Device.BeginInvokeOnMainThread(() =>
            {
                CellColor = Color.FromHex(Settings.MobileSettings.color);
            });            

            BindingContext = this;

            SetTextAndColor();

            d2.OnBackspace += D2_OnBackspace;
            d3.OnBackspace += D3_OnBackspace;
            d4.OnBackspace += D4_OnBackspace;
            d5.OnBackspace += D5_OnBackspace;
            d6.OnBackspace += D6_OnBackspace;
            d7.OnBackspace += D7_OnBackspace;
            d8.OnBackspace += D8_OnBackspace;



            d1.Focused += Entry_Focused;
            d2.Focused += Entry_Focused;
            d3.Focused += Entry_Focused;
            d4.Focused += Entry_Focused;
            d5.Focused += Entry_Focused;
            d6.Focused += Entry_Focused;
            d7.Focused += Entry_Focused;
            d8.Focused += Entry_Focused;


            //d1.Unfocused += Entry_Unfocused;
            //d2.Unfocused += Entry_Unfocused;
            //d3.Unfocused += Entry_Unfocused;
            //d4.Unfocused += Entry_Unfocused;
            //d5.Unfocused += Entry_Unfocused;
            //d6.Unfocused += Entry_Unfocused;
            //d7.Unfocused += Entry_Unfocused;
            //d8.Unfocused += Entry_Unfocused;

            StackLayout cds6 = (StackLayout)CounterDigitsContainer.FindByName("cds6"); ;
            StackLayout cds7 = (StackLayout)CounterDigitsContainer.FindByName("cds7"); ;
            StackLayout cds8 = (StackLayout)CounterDigitsContainer.FindByName("cds8"); ;

            DecimalPoint = meter.NumberOfDecimalPlaces;
            switch (DecimalPoint)
            {
                case 0: Divider.IsVisible = false;
                    (d6.Parent.Parent as VisualElement).IsVisible = (d7.Parent.Parent as VisualElement).IsVisible = (d8.Parent.Parent as VisualElement).IsVisible = false;
                    d6.IsVisible = d7.IsVisible = d8.IsVisible = false;
                   
                      
                    CounterDigitsContainer.Children.Remove(cds6);
                      
                    CounterDigitsContainer.Children.Remove(cds7);
                      
                    CounterDigitsContainer.Children.Remove(cds8);
                    pc5.CornerRadius = new CornerRadius(5, 5, 0, 5);
                    break;
                case 1: Divider.IsVisible = true;
                    (d7.Parent.Parent as VisualElement).IsVisible = (d8.Parent.Parent as VisualElement).IsVisible = false;
                    (d6.Parent.Parent as VisualElement).IsVisible = d6.IsVisible = true;
                    d7.IsVisible = d8.IsVisible = false;

                    
                    CounterDigitsContainer.Children.Remove(cds7);
                    CounterDigitsContainer.Children.Remove(cds8);

                    pc6.CornerRadius = new CornerRadius(5, 5, 0, 5);
                    break;
                case 2: Divider.IsVisible = true;
                    (d6.Parent.Parent as VisualElement).IsVisible = d6.IsVisible = (d7.Parent.Parent as VisualElement).IsVisible = d7.IsVisible = true;
                    (d8.Parent.Parent as VisualElement).IsVisible = false;
                    d8.IsVisible = false;

                    CounterDigitsContainer.Children.Remove(cds8);

                    pc7.CornerRadius = new CornerRadius(5, 5, 0, 5);
                    break;
                case 3: Divider.IsVisible = true;
                    (d6.Parent.Parent as VisualElement).IsVisible = 
                        d6.IsVisible = (d7.Parent.Parent as VisualElement).IsVisible = 
                        d7.IsVisible = (d8.Parent.Parent as VisualElement).IsVisible = d8.IsVisible = true;
                    break;
                    
            }
        }

        //private void Entry_Unfocused(object sender, FocusEventArgs e)
        //{
        //    if (Xamarin.Essentials.DeviceDisplay.MainDisplayInfo.Width < 800)
        //    {
        //        if (Device.RuntimePlatform == Device.iOS)
        //        {
        //            Device.BeginInvokeOnMainThread(() =>
        //            {
        //                FrameMeterReading.Margin = frameCounterMargin;
        //            });
        //        }
        //    }
        //}

        //Thickness frameCounterMargin = new Thickness();


        private void Entry_Focused(object sender, FocusEventArgs e)
        {
            //if (Xamarin.Essentials.DeviceDisplay.MainDisplayInfo.Width < 800)
            //{
            //    if (Device.RuntimePlatform == Device.iOS)
            //    {
            //        Device.BeginInvokeOnMainThread(() =>
            //        {
            //            frameCounterMargin = FrameMeterReading.Margin;
            //            FrameMeterReading.Margin = new Thickness(20, -150, 20, 15);
            //        });
            //    }
            //}

            Device.BeginInvokeOnMainThread(async () =>
            {
                if (Device.RuntimePlatform == Device.Android)
                    await Task.Delay(100);
                else
                    await Task.Delay(900);
                var entry = (CounterEntryNew)sender;
                if (!string.IsNullOrWhiteSpace(entry.Text))
                {
                    entry.CursorPosition = 0;
                    entry.SelectionLength = entry.Text.Length;
                }
            });
        }

        private void SetCurrent(decimal counterThisMonth)
        {
            var d = GetNumbers(counterThisMonth);

            Device.BeginInvokeOnMainThread(() =>
            {
                d8.Text = d[0];
                d7.Text = d[1];
                d6.Text = d[2];
                d5.Text = d[3];
                d4.Text = d[4];
                d3.Text = d[5];
                d2.Text = d[6];
                d1.Text = d[7];                
            });
        }

        private void D2_OnBackspace(object sender, EventArgs e)
        {
            if(string.IsNullOrWhiteSpace(d2.Text))
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    d1.Text = "";
                    d1.Focus();                    
                });
            }
        }
        private void D3_OnBackspace(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(d3.Text))
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    d2.Text = "";
                    d2.Focus();
                });
            }
        }
        private void D4_OnBackspace(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(d4.Text))
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    d3.Text = "";
                    d3.Focus();
                });
            }
        }
        private void D5_OnBackspace(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(d5.Text))
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    d4.Text = "";
                    d4.Focus();
                });
            }
        }
        private void D6_OnBackspace(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(d6.Text))
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    d5.Text = "";
                    d5.Focus();
                });
            }
        }
        private void D7_OnBackspace(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(d7.Text))
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    d6.Text = "";
                    d6.Focus();
                });
            }
        }
        private void D8_OnBackspace(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(d8.Text))
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    d7.Text = "";
                    d7.Focus();
                });
            }
        }

        List<string> GetNumbers(decimal counter)
        {
            var retList = new List<string>();
            var counter8 =Convert.ToInt32( counter * 1000);
            for (int i=0; i<8; i++)
            {
                var d = counter8 % 10;
                retList.Add(d.ToString());
                counter8 = (counter8 - d) / 10;
            }
            
            return retList;
        }

        void SetPrevious(decimal counterPrevMonth)
        {
            var d = GetNumbers(counterPrevMonth);

            Device.BeginInvokeOnMainThread(() =>
            {
                d08.Text = d[0];
                d07.Text = d[1];
                d06.Text = d[2];
                d05.Text = d[3];
                d04.Text = d[4];
                d03.Text = d[5];
                d02.Text = d[6];
                d01.Text = d[7];
            });            
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            Device.BeginInvokeOnMainThread(() =>
            {
                d1.Unfocus();
                d1.Focus();
            });
        }

        private async void ButtonClick(object sender, EventArgs e)
        {
            try {
                string count ="";
                var p1 = -1;
                var p2 = -1;
                var p3 = -1;
                var p4 = -1;
                var p5 = -1;
                var p6 = -1;
                var p7 = -1;
                var p8 = -1;

                if (int.TryParse(d1.Text,out p1) && int.TryParse(d2.Text, out p2) && int.TryParse(d3.Text, out p3) 
                    && int.TryParse(d4.Text, out p4) && int.TryParse(d5.Text, out p5) && int.TryParse(d6.Text == null ? "0" : d6.Text, out p6)
                    && int.TryParse(d7.Text == null ? "0" : d7.Text, out p7) && int.TryParse(d8.Text == null ? "0" : d8.Text, out p8))
                {
                    count += d1.Text;// != "0" ? d1.Text : "";
                    count += d2.Text;// != "0" ? d2.Text : "";
                    count += d3.Text;// != "0" ? d3.Text : "";
                    count += d4.Text;// != "0" ? d4.Text : "";
                    count += d5.Text+ ",";
                    count += d6.Text;// != "0" ? d6.Text : "";
                    count += d7.Text;// != "0" ? d7.Text : "";
                    count += d8.Text;// != "0" ? d8.Text : "";
                    
                    SaveInfoAccount(count);
                }
                else
                {
                    await DisplayAlert(AppResources.ErrorTitle, AppResources.AddMetersNotNumber, "OK");
                }
            }
            catch(Exception ex)
            {
                await DisplayAlert(AppResources.ErrorTitle, AppResources.AddMetersError, "OK");
            }
            
        }

        void SetTextAndColor()
        {
            OSAppTheme currentTheme = Application.Current.RequestedTheme;
            //if (Xamarin.Essentials.DeviceInfo.Platform == Xamarin.Essentials.DevicePlatform.iOS)
            //    currentTheme = OSAppTheme.Dark;
            if (meter.Resource.ToLower().Contains("холодное") || meter.Resource.ToLower().Contains("хвс"))
            {
                img.Source = ImageSource.FromFile("ic_cold_water");
                //if (Xamarin.Essentials.DeviceInfo.Platform == DevicePlatform.iOS)
                    meter.Resource += ", м3";
            }
            else if (meter.Resource.ToLower().Contains("горячее") || meter.Resource.ToLower().Contains("гвс")|| meter.Resource.ToLower().Contains("подог")|| meter.Resource.ToLower().Contains("отопл"))
            {
                img.Source = ImageSource.FromFile("ic_heat_water");
            }
            else if (meter.Resource.ToLower().Contains("эле"))
            {
                img.Source = ImageSource.FromFile("ic_electr");

                //если это э/э и не указаны ед. измерения, в RU локали добавляем их
                if (!meter.Resource.ToLower().Contains("кВт"))
                    meter.Resource += ", кВт";
            }
            else
            {
                img.Source = ImageSource.FromFile("ic_cold_water");
            }

            //если это вода и не указаны ед. измерения, в RU локали добавляем их
            if ((meter.Resource.ToLower().Contains("горячее") || meter.Resource.ToLower().Contains("гвс")
                || meter.Resource.ToLower().Contains("холодное") || meter.Resource.ToLower().Contains("хвс"))
                && !meter.Resource.ToLower().Contains("м3"))
                meter.Resource += ", м3";


            UkName.Text = Settings.MobileSettings.main_name;
            LabelPhone.Text =  "+" + Settings.Person.companyPhone.Replace("+","");
            NameLbl.Text = meter.CustomName != null && !meter.CustomName.Equals("") ? meter.CustomName : meter.Resource;
            LabelseparatorFio.BackgroundColor = Color.FromHex(Settings.MobileSettings.color);
            progress.Color = Color.FromHex(Settings.MobileSettings.color);
            FrameBtnLogin.BackgroundColor = Color.FromHex(Settings.MobileSettings.color);
            FormattedString formattedUniq = new FormattedString();
            formattedUniq.Spans.Add(new Span
            {
                Text = AppResources.FacNum,
                TextColor = currentTheme.Equals(OSAppTheme.Light) ? Color.Black : Color.LightGray,
                FontAttributes = FontAttributes.None,
                FontSize = 15
            });
            formattedUniq.Spans.Add(new Span
            {
                Text = meter.FactoryNumber,
                TextColor =  currentTheme.Equals(OSAppTheme.Light) ? Color.Black : Color.White,
                FontAttributes = currentTheme.Equals(OSAppTheme.Light) ? FontAttributes.Bold : FontAttributes.None,
                FontSize = 15
            });
            UniqNumLbl.FormattedText = formattedUniq;

            FormattedString formattedCheckup = new FormattedString();
            formattedCheckup.Spans.Add(new Span
            {
                Text = AppResources.LastCheck,
                TextColor = currentTheme.Equals(OSAppTheme.Light) ? Color.Black : Color.LightGray,
                FontAttributes = FontAttributes.None,
                FontSize = 15
            });
            formattedCheckup.Spans.Add(new Span
            {
                Text = meter.LastCheckupDate,
                TextColor =  currentTheme.Equals(OSAppTheme.Light) ? Color.Black : Color.White,
                FontAttributes = currentTheme.Equals(OSAppTheme.Light) ? FontAttributes.Bold : FontAttributes.None,
                FontSize = 15
            });
            CheckupLbl.FormattedText = formattedCheckup;

            FormattedString formattedRecheckup = new FormattedString();
            formattedRecheckup.Spans.Add(new Span
            {
                Text = AppResources.CheckInterval + " ",
                TextColor = currentTheme.Equals(OSAppTheme.Light) ? Color.Black : Color.LightGray,
                FontAttributes = FontAttributes.None,
                FontSize = 15
            });
            formattedRecheckup.Spans.Add(new Span
            {
                Text = meter.RecheckInterval.ToString() + " лет",
                TextColor =  currentTheme.Equals(OSAppTheme.Light) ? Color.Black : Color.White,
                FontAttributes = currentTheme.Equals(OSAppTheme.Light) ? FontAttributes.Bold : FontAttributes.None,
                FontSize = 15
            });
            RecheckLbl.FormattedText = formattedRecheckup;
            if (meter.Values.Count != 0)
            {
                BindingContext = new AddMetersPageViewModel(SetPrev ? PrevValue : meter.Values[0].Value);
                //PredCount.Text = meter.Values[0].Value.ToString(CultureInfo.InvariantCulture);
            }
            
            Color hexColor = (Color) Application.Current.Resources["MainColor"];
            IconViewLogin.SetAppThemeColor(IconView.ForegroundProperty, hexColor, Color.White);
            IconViewTech.SetAppThemeColor(IconView.ForegroundProperty, hexColor, Color.White);
            Pancake.SetAppThemeColor(PancakeView.BorderColorProperty, hexColor, Color.Transparent);
            PancakeViewIcon.SetAppThemeColor(PancakeView.BorderColorProperty, hexColor, Color.Transparent);if (Device.RuntimePlatform == Device.iOS){ if (AppInfo.PackageName == "rom.best.saburovo" || AppInfo.PackageName == "sys_rom.ru.tsg_saburovo"){PancakeViewIcon.Padding = new Thickness(0);}}
            LabelTech.SetAppThemeColor(Label.TextColorProperty, hexColor, Color.White);
            FrameTop.SetAppThemeColor(Frame.BorderColorProperty, hexColor, Color.FromHex("#494949"));
            FrameMeterReading.SetAppThemeColor(Frame.BorderColorProperty, hexColor, Color.White);
        }

        public async void SaveInfoAccount(string count)
        {
            bool rate = Preferences.Get("rate", true);

            if (Xamarin.Essentials.Connectivity.NetworkAccess != Xamarin.Essentials.NetworkAccess.Internet)
            {
                Device.BeginInvokeOnMainThread(async () => await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorNoInternet, "OK"));
                return;
            }
            if (!string.IsNullOrEmpty(count))
            {
                double d = Double.Parse(count.Replace('.',','));
                progress.IsVisible = true;
                FrameBtnLogin.IsVisible = false;
                progress.IsVisible = true;
                CommonResult result = await _server.SaveMeterValue(meter.ID.ToString(), d.ToString(CultureInfo.InvariantCulture), "", "");
                if (result.Error == null)
                {
                    Console.WriteLine(result.ToString());
                    Console.WriteLine("Отправлено");
                    await DisplayAlert("", AppResources.AddMetersSuccess, "OK");
                    FrameBtnLogin.IsVisible = true;
                    progress.IsVisible = false;
                    if (rate)
                    {
                        await PopupNavigation.Instance.PushAsync(new RatingAppMarketDialog());
                    }
                    try
                    {
                        _ = await Navigation.PopAsync();
                    }
                    catch { }
                    _countersPage.RefreshCountersData();
                }
                else
                {
                    Console.WriteLine("---ОШИБКА---");
                    Console.WriteLine(result.ToString());
                    FrameBtnLogin.IsVisible = true;
                    progress.IsVisible = false;
                    if (Device.RuntimePlatform == Device.iOS)
                    {
                        await DisplayAlert(AppResources.ErrorTitle, result.Error, "OK");
                    }
                    else
                    {
                        DependencyService.Get<IMessage>().ShortAlert(result.Error);
                    }
                }

                progress.IsVisible = false;
                FrameBtnLogin.IsVisible = true;
            }
            else
            {
                await DisplayAlert(AppResources.AddMetersNoData, "", "OK");
            }
        }

        void marginSetOnIos(CounterEntryNew d)
        {
            if(Device.RuntimePlatform==Device.iOS)
            {
                if (string.IsNullOrWhiteSpace(d.Text))
                {
                    d.Margin = new Thickness(0, 0, -15, -10 );
                }
                //else
                //{
                //    d.Margin = new Thickness(0, 0, 0, -10);
                //}
            }
            
        }

        private void d1_Completed(object sender, EventArgs e)
        {
            var entryNew = sender as CounterEntryNew;
            Device.BeginInvokeOnMainThread(() =>
            {
                if(string.IsNullOrWhiteSpace(entryNew.Text))
                {
                    return;
                }
                
                if(!int.TryParse(((TextChangedEventArgs)e).NewTextValue, out _))
                {
                    entryNew.Text = "";
                    entryNew.Focus();
                    return;
                }
                
                d2.Unfocus();
                d2.Focus();                
            });
        }
        private void d2_Completed(object sender, EventArgs e)
        {
            var entryNew = sender as CounterEntryNew;
            Device.BeginInvokeOnMainThread(() =>
            {
                if (string.IsNullOrWhiteSpace(entryNew.Text))
                {
                    return;
                }

                if (!int.TryParse(((TextChangedEventArgs)e).NewTextValue, out _))
                {
                    entryNew.Text = "";
                    entryNew.Focus();
                    return;
                }

                d3.Unfocus();
                d3.Focus();
            });
        }
        private void d3_Completed(object sender, EventArgs e)
        {
            var entryNew = sender as CounterEntryNew;
            Device.BeginInvokeOnMainThread(() =>
            {
                if (string.IsNullOrWhiteSpace(entryNew.Text))
                {
                    return;
                }

                if (!int.TryParse(((TextChangedEventArgs)e).NewTextValue, out _))
                {
                    entryNew.Text = "";
                    entryNew.Focus();
                    return;
                }

                d4.Unfocus();
                d4.Focus();
            });
        }
        private void d4_Completed(object sender, EventArgs e)
        {
            var entryNew = sender as CounterEntryNew;
            Device.BeginInvokeOnMainThread(() =>
            {
                if (string.IsNullOrWhiteSpace(entryNew.Text))
                {
                    return;
                }

                if (!int.TryParse(((TextChangedEventArgs)e).NewTextValue, out _))
                {
                    entryNew.Text = "";
                    entryNew.Focus();
                    return;
                }

                d5.Unfocus();
                d5.Focus();
            });
        }
        private void d5_Completed(object sender, EventArgs e)
        {
            var entryNew = sender as CounterEntryNew;
            Device.BeginInvokeOnMainThread(() =>
            {
                if (string.IsNullOrWhiteSpace(entryNew.Text))
                {
                    return;
                }

                if (!int.TryParse(((TextChangedEventArgs)e).NewTextValue, out _))
                {
                    entryNew.Text = "";
                    entryNew.Focus();
                    return;
                }

                d6.Unfocus();
                if (DecimalPoint >= 1)
                {
                    d6.Focus();
                }
            });
        }
        private void d6_Completed(object sender, EventArgs e)
        {
            var entryNew = sender as CounterEntryNew;
            Device.BeginInvokeOnMainThread(() =>
            {
                if (string.IsNullOrWhiteSpace(entryNew.Text))
                {
                    return;
                }

                if (!int.TryParse(((TextChangedEventArgs)e).NewTextValue, out _))
                {
                    entryNew.Text = "";
                    entryNew.Focus();
                    return;
                }

                d7.Unfocus();
                if (DecimalPoint >= 2)
                    d7.Focus();
            });
        }
        private void d7_Completed(object sender, EventArgs e)
        {
            var entryNew = sender as CounterEntryNew;
            Device.BeginInvokeOnMainThread(() =>
            {
                if (string.IsNullOrWhiteSpace(entryNew.Text))
                {
                    return;
                }

                if (!int.TryParse(((TextChangedEventArgs)e).NewTextValue, out _))
                {
                    entryNew.Text = "";
                    entryNew.Focus();
                    return;
                }

                d8.Unfocus();
                if (DecimalPoint >= 3)
                    d8.Focus();
            });
        }

        private void d8_Completed(object sender, EventArgs e)
        {
            var entryNew = sender as CounterEntryNew;
            Device.BeginInvokeOnMainThread(() =>
            {
                if (string.IsNullOrWhiteSpace(entryNew.Text))
                {
                    return;
                }

                if (!int.TryParse(((TextChangedEventArgs)e).NewTextValue, out _))
                {
                    entryNew.Text = "";
                    entryNew.Focus();
                    return;
                }
            });
        }
    }
}