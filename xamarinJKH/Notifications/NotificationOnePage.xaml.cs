﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AppCenter.Analytics;
using Plugin.Messaging;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xamarinJKH.Additional;
using xamarinJKH.InterfacesIntegration;
using xamarinJKH.Main;
using xamarinJKH.Questions;
using xamarinJKH.Server;
using xamarinJKH.Server.RequestModel;
using xamarinJKH.Shop;
using xamarinJKH.Tech;
using xamarinJKH.Utils;
using FileInfo = xamarinJKH.Server.RequestModel.FileInfo;

namespace xamarinJKH.Notifications
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NotificationOnePage : ContentPage
    {
        private AnnouncementInfo _announcementInfo;
        private AdditionalService _additional;
        private PollInfo _polls;
        private RestClientMP _server = new RestClientMP();
        
        public List<FileInfo> Files { get; set; }
        public string Title { get; set; }

        public NotificationOnePage(AnnouncementInfo announcementInfo)
        {
            
            _announcementInfo = announcementInfo;
            InitializeComponent();
            Analytics.TrackEvent("Уведомление " + announcementInfo.ID);
            CollectionViewFiles.ItemsLayout = new GridItemsLayout(2, ItemsLayoutOrientation.Vertical);
            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    int statusBarHeight = DependencyService.Get<IStatusBar>().GetHeight();
                    Pancake2.HeightRequest = statusBarHeight;
                    break;
                default:
                    break;
            }

            NavigationPage.SetHasNavigationBar(this, false);

            var profile = new TapGestureRecognizer();
            profile.Tapped += async (s, e) =>
            {
                if (Navigation.NavigationStack.FirstOrDefault(x => x is ProfilePage) == null)
                    await Navigation.PushAsync(new ProfilePage());
            };
            IconViewProfile.GestureRecognizers.Add(profile);

            var techSend = new TapGestureRecognizer();
            techSend.Tapped += async (s, e) =>
            {
                if (Navigation.NavigationStack.FirstOrDefault(x => x is AppPage) == null)
                    await Navigation.PushAsync(new AppPage());
            };
            LabelTech.GestureRecognizers.Add(techSend);
            var call = new TapGestureRecognizer();
            call.Tapped += async (s, e) =>
            {
                if (Settings.Person.Phone != null)
                {
                    IPhoneCallTask phoneDialer;
                    phoneDialer = CrossMessaging.Current.PhoneDialer;
                    if (phoneDialer.CanMakePhoneCall && !string.IsNullOrWhiteSpace(Settings.Person.companyPhone)) 
                        phoneDialer.MakePhoneCall(Regex.Replace(Settings.Person.companyPhone, "[^+0-9]", ""));
                }

            
            };
            var backClick = new TapGestureRecognizer();
            backClick.Tapped += async (s, e) => { close(); };
            BackStackLayout.GestureRecognizers.Add(backClick);
            SetText();
            Files = announcementInfo.Files;
            BindingContext = this;
            if (!announcementInfo.IsReaded)
                Task.Run(async () =>
                {
                    var result = await _server.SetNotificationReadFlag(announcementInfo.ID);
                    MessagingCenter.Send<Object, int>(this, "SetEventsAmount", -1);
                    MessagingCenter.Send<Object>(this, "ReduceAnnounsements");
                });
        }

        async void SetText()
        {
         
            Title = _announcementInfo.Header;
            LabelDate.Text = _announcementInfo.Created;
            LabelText.Text = _announcementInfo.Text.Trim();
            FrameBtnQuest.BackgroundColor = (Color)Application.Current.Resources["MainColor"];
            int announcementInfoAdditionalServiceId = _announcementInfo.AdditionalServiceId;
            if (announcementInfoAdditionalServiceId != -1)
            {
                _additional = Settings.GetAdditionalService(announcementInfoAdditionalServiceId);
                byte[] imageByte = await _server.GetPhotoAdditional(announcementInfoAdditionalServiceId.ToString());
                if (imageByte != null)
                {
                    Stream stream = new MemoryStream(imageByte);
                    ImageAdd.Source = ImageSource.FromStream(() => { return stream; });
                    ImageAdd.IsVisible = true;

                    var openAdditional = new TapGestureRecognizer();
                    openAdditional.Tapped += async (s, e) =>
                    {
                        if (_additional.ShopID == null)
                        {
                            await Navigation.PushAsync(new AdditionalOnePage(_additional));
                        }
                        else
                        {
                            if (Navigation.NavigationStack.FirstOrDefault(x => x is ShopPageNew) == null)
                                await Navigation.PushAsync(new ShopPageNew(_additional));
                        }
                    };
                    ImageAdd.GestureRecognizers.Add(openAdditional);
                }
            }

            if (_announcementInfo.QuestionGroupID != -1)
            {
                _polls = Settings.GetPollInfo(_announcementInfo.QuestionGroupID);
                FrameBtnQuest.IsVisible = true;
            }
            
            Color hexColor = (Color) Application.Current.Resources["MainColor"];
          
        }

        async void open(Page page)
        {
            try
            {
                await Navigation.PushAsync(page);
            }
            catch (Exception e)
            {
                await Navigation.PushModalAsync(page);
            }
        }
        async void close()
        {
            try
            {
                await Navigation.PopAsync();
            }
            catch (Exception e)
            {
                await Navigation.PopModalAsync();
            }
        }

        private async void ButtonClick(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new PollsPage(_polls, false));
        }

        private async void OnCollectionViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FileInfo select = (e.CurrentSelection.FirstOrDefault() as FileInfo);
            try
            {
                if (@select != null) await Launcher.OpenAsync(@select.Link);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorAdditionalLink, "OK");
            }
        }
    }
}