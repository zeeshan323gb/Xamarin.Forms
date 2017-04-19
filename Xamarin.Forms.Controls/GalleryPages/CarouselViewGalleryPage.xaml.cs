using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace Xamarin.Forms.Controls
{
	public partial class CarouselViewGalleryPage : ContentPage
	{
		public CarouselViewGalleryPage()
		{
			InitializeComponent();

			BindingContext = new CarouselViewModel();

			myCarousel.ItemTemplate = new DataTemplate(() => { return new MyView(); });

			myCarousel.PositionSelected += MyCarousel_PositionSelected;

			ToolbarItems.Add(new ToolbarItem
			{
				Text = "Reset",
				Order = ToolbarItemOrder.Primary,
				Command = new Command(() =>
				{
					((CarouselViewModel)BindingContext).Source = new ObservableCollection<int>();
				})
			});

			ToolbarItems.Add(new ToolbarItem
			{
				Text = "Add",
				Order = ToolbarItemOrder.Primary,
				Command = new Command(() =>
				{
					var source = ((CarouselViewModel)BindingContext).Source;
					int context = 1;
					if (source.Count > 0)
						context = source.Max() + 1;
					source.Add(context);
					myCarousel.Position = source.Count - 1;
				})
			});

			ToolbarItems.Add(new ToolbarItem
			{
				Text = "Remove",
				Order = ToolbarItemOrder.Primary,
				Command = new Command(() =>
				{
					var source = ((CarouselViewModel)BindingContext).Source;
					if (source.Count > 0)
						source.RemoveAt(myCarousel.Position);
				})
			});

			prevBtn.Clicked += (sender, e) =>
			{
				if (myCarousel.Position > 0)
					myCarousel.Position--;
			};

			nextBtn.Clicked += (sender, e) =>
			{
				if (myCarousel.Position < myCarousel.ItemsSource?.GetCount() - 1)
					myCarousel.Position++;
			};

			ButtonsVisibility();
		}

		void MyCarousel_PositionSelected(object sender, int position)
		{
			if (position == myCarousel.Position)
			{
				Debug.WriteLine("Position " + myCarousel.Position + " selected");
				ButtonsVisibility();
			}
		}

		void ButtonsVisibility()
		{
			var _vm = (CarouselViewModel)BindingContext;
			prevBtn.IsVisible = myCarousel.Position > 0;
			nextBtn.IsVisible = myCarousel.Position < _vm.Source.Count - 1;
		}
	}
}
