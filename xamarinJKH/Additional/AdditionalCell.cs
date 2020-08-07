﻿using System;
using System.IO;
using Xamarin.Forms;
using xamarinJKH.CustomRenderers;
using xamarinJKH.Server;

namespace xamarinJKH.Additional
{
    public class AdditionalCell : ViewCell
    {
        Image image;
        RestClientMP _server = new RestClientMP();

        public AdditionalCell()
        {
            image = new Image();            
            MaterialFrame frame = new MaterialFrame();
            frame.Elevation = 20;
            frame.HorizontalOptions = LayoutOptions.FillAndExpand;
            frame.VerticalOptions = LayoutOptions.Start;
            frame.BackgroundColor =  Color.White;
            frame.IsClippedToBounds = true;
            frame.Margin = new Thickness(10, 0, 10, 10);
            frame.Padding = new Thickness(0);
            frame.CornerRadius = 40;
            //StackLayout cell = new StackLayout();
            //cell.Children.Add(image);
            frame.Content = image;// cell;
            View = frame;
        }

        public static readonly BindableProperty ImagePathProperty =
            BindableProperty.Create("ImagePath", typeof(ImageSource), typeof(AdditionalCell), null);

        public static readonly BindableProperty ImageWidthProperty =
            BindableProperty.Create("ImageWidth", typeof(int), typeof(AdditionalCell), 100);

        public static readonly BindableProperty ImageHeightProperty =
            BindableProperty.Create("ImageHeight", typeof(int), typeof(AdditionalCell), 100);

        public static readonly BindableProperty DetailProperty =
            BindableProperty.Create("Detail", typeof(string), typeof(AdditionalCell), "");

        public int ImageWidth
        {
            get { return (int) GetValue(ImageWidthProperty); }
            set { SetValue(ImageWidthProperty, value); }
        }

        public int ImageHeight
        {
            get { return (int) GetValue(ImageHeightProperty); }
            set { SetValue(ImageHeightProperty, value); }
        }

        public String ImagePath
        {
            get { return (String) GetValue(ImagePathProperty); }
            set { SetValue(ImagePathProperty, value); }
        }

        public string Detail
        {
            get { return (string) GetValue(DetailProperty); }
            set { SetValue(DetailProperty, value); }
        }

        protected override async void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            if (BindingContext != null)
            {
                byte[] imageByte = await _server.GetPhotoAdditional(Detail);
                if (imageByte != null)
                {
                    Stream stream = new MemoryStream(imageByte);
                    image.Source = ImageSource.FromStream(() => { return stream; });
                    image.VerticalOptions = LayoutOptions.FillAndExpand;
                    image.Aspect = Aspect.AspectFill;
                    image.HorizontalOptions = LayoutOptions.FillAndExpand;
                    image.HeightRequest = ImageHeight;
                }
                
            }
        }
    }
}