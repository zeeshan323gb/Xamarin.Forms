using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace Xamarin.Forms.Controls
{
    public class CarouselViewGalleryPage : ContentPage
    {
        public CarouselViewGalleryPage()
        {
            Content = new CarouselView()
            {
                ItemsSource = new List<View>()
                {
                    new Label() {
                        Text = "Page 1",
                        HorizontalTextAlignment = TextAlignment.Center,
                        VerticalTextAlignment = TextAlignment.Center,
                        HorizontalOptions = LayoutOptions.CenterAndExpand,
                        VerticalOptions = LayoutOptions.CenterAndExpand,
                        BackgroundColor = Color.LightSalmon
                    },
                    new Label() {
                        Text = "Page 2",
                        HorizontalTextAlignment = TextAlignment.Center,
                        VerticalTextAlignment = TextAlignment.Center,
                        HorizontalOptions = LayoutOptions.CenterAndExpand,
                        VerticalOptions = LayoutOptions.CenterAndExpand,
                        BackgroundColor = Color.LightCoral
                    },
                    new Label() {
                        Text = "Page 3",
                        HorizontalTextAlignment = TextAlignment.Center,
                        VerticalTextAlignment = TextAlignment.Center,
                        HorizontalOptions = LayoutOptions.CenterAndExpand,
                        VerticalOptions = LayoutOptions.CenterAndExpand,
                        BackgroundColor = Color.LightSkyBlue
                    }
                },
                ShowArrows = true,
                ShowIndicators = true
            };
        }
    }
}

