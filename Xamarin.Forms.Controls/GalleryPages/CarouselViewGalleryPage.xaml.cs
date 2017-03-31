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
				Command = new Command(async () =>
				{
					var source = ((CarouselViewModel)BindingContext).Source;
					int context = 1;
					if (source.Count > 0)
						context = source.Max() + 1;
					await myCarousel.InsertPage(context);
					myCarousel.Position = source.Count - 1;
				})
			});

			ToolbarItems.Add(new ToolbarItem
			{
				Text = "Remove",
				Order = ToolbarItemOrder.Primary,
				Command = new Command(async () =>
				{
					var source = ((CarouselViewModel)BindingContext).Source;
					if (source.Count > 0)
						await myCarousel.RemovePage(myCarousel.Position);
				})
			});

			prevBtn.Clicked += (sender, e) =>
			{
				if (myCarousel.Position > 0)
					myCarousel.Position--;
			};

			nextBtn.Clicked += (sender, e) =>
			{
				if (myCarousel.Position < myCarousel.ItemsSource?.Count - 1)
					myCarousel.Position++;
			};

			ButtonsVisibility();
		}

		void MyCarousel_PositionSelected(object sender, EventArgs e)
		{
			Debug.WriteLine("Position " + myCarousel.Position + " selected");
			ButtonsVisibility();
		}

		void ButtonsVisibility()
		{
			prevBtn.IsVisible = myCarousel.Position > 0;
			nextBtn.IsVisible = myCarousel.Position < myCarousel.ItemsSource?.Count - 1;
		}
	}
}
