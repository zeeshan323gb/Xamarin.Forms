using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.GalleryPages
{
	public class CarouselViewGallery : ContentPage
	{
		Label _addPageButton;
		ICommand _addPageCommand;
		CarouselView _carouselView;
		Label _nextButton;
		ICommand _nextCommand;
		Label _prevButton;
		ICommand _prevCommand;

		public CarouselViewGallery()
		{
			InitCommands();

			InitControls();

			SetButtonVisibility();

			Content = GetMainGrid();
		}

		void AddPage()
		{
			if (_carouselView.ItemsSource != null)
			{
				_carouselView.InsertPage(_carouselView.ItemsSource.Count);

				if (_carouselView.ItemsSource.Count > 1)
					_carouselView.Position = _carouselView.ItemsSource.Count - 1;
			}
		}

		Label GetButtonAddPage()
		{
			var label = new Label
			{
				HorizontalTextAlignment = TextAlignment.Center,
				Text = "Add Page",
				VerticalTextAlignment = TextAlignment.Center
			};
			label.GestureRecognizers.Add(new TapGestureRecognizer { Command = _addPageCommand });
			return label;
		}

		Label GetButtonNext()
		{
			var label = new Label
			{
				HorizontalTextAlignment = TextAlignment.End,
				Text = "Next",
				VerticalTextAlignment = TextAlignment.Center
			};
			label.GestureRecognizers.Add(new TapGestureRecognizer { Command = _nextCommand });
			return label;
		}

		Label GetButtonPrev()
		{
			var label = new Label
			{
				HorizontalTextAlignment = TextAlignment.Start,
				Text = "Prev",
				VerticalTextAlignment = TextAlignment.Center
			};
			label.GestureRecognizers.Add(new TapGestureRecognizer { Command = _prevCommand });
			return label;
		}

		CarouselView GetCarouselView()
		{
			var carouselView = new CarouselView
			{
				HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.FillAndExpand,
				AnimateTransition = true,
				IndicatorsShape = CarouselViewIndicatorsShape.Square,
				InterPageSpacing = 10,
				InterPageSpacingColor = Color.Purple,
				Orientation = CarouselViewOrientation.Horizontal,
				ShowIndicators = true,
				ItemTemplate = new MyTemplateSelector(),
				ItemsSource = Enumerable.Range(0, 3000).ToList(),
				Position = 0
			};
			carouselView.PositionSelected += PositionSelected;
			return carouselView;
		}

		Grid GetControlsGrid()
		{
			var controlsGrid = new Grid
			{
				BackgroundColor = Color.Silver,
				Padding = new Thickness(12, 0, 12, 0)
			};

			controlsGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

			controlsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80, GridUnitType.Absolute) });
			controlsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
			controlsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80, GridUnitType.Absolute) });

			controlsGrid.AddChild(_prevButton, 0, 0);
			controlsGrid.AddChild(_addPageButton, 1, 0);
			controlsGrid.AddChild(_nextButton, 2, 0);

			return controlsGrid;
		}

		Grid GetMainGrid()
		{
			var mainGrid = new Grid { RowSpacing = 0 };

			mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
			mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(56, GridUnitType.Absolute) });

			mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

			mainGrid.AddChild(_carouselView, 0, 0);
			mainGrid.AddChild(GetControlsGrid(), 0, 1);
			return mainGrid;
		}

		void InitCommands()
		{
			_addPageCommand = new Command(AddPage);
			_nextCommand = new Command(Next);
			_prevCommand = new Command(Prev);
		}

		void InitControls()
		{
			_carouselView = GetCarouselView();
			_addPageButton = GetButtonAddPage();
			_prevButton = GetButtonPrev();
			_nextButton = GetButtonNext();
		}

		void Next()
		{
			if (_carouselView.Position < _carouselView.ItemsSource?.Count - 1)
				_carouselView.Position++;
		}

		void PositionSelected(object sender, EventArgs e)
		{
			SetButtonVisibility();
			Debug.WriteLine($"Position {_carouselView.Position} selected");
		}

		void Prev()
		{
			if (_carouselView.Position > 0)
				_carouselView.Position--;
		}

		void SetButtonVisibility()
		{
			_prevButton.IsVisible = _carouselView.Position > 0;
			_addPageButton.IsVisible = _carouselView.ItemsSource != null;
			_nextButton.IsVisible = _carouselView.Position < _carouselView.ItemsSource.Count - 1;
		}

		[Preserve(AllMembers = true)]
		class MyFirstView : ContentView
		{
			public MyFirstView()
			{
				var newLabel = new Label();
				newLabel.SetBinding(Label.TextProperty, ".");
				var stack = new StackLayout
				{
					Padding = new Thickness(12),
					Spacing = 12,
					Children =
					{
						new Image{ Source = "photo.jpg" },
						newLabel
					}
				};

				var scroller = new ScrollView { Content = stack };

				Content = scroller;
				BackgroundColor = Color.White;
			}
		}

		[Preserve(AllMembers = true)]
		class MySecondView : ContentView { }

		[Preserve(AllMembers = true)]
		class MyTemplateSelector : DataTemplateSelector
		{
			readonly DataTemplate templateOne;
			readonly DataTemplate templateTwo;

			public MyTemplateSelector()
			{
				templateOne = new DataTemplate(typeof(MyFirstView));
				templateTwo = new DataTemplate(typeof(MySecondView));
			}

			protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
			{
				//if ((int)item % 2 == 0)
				//return templateTwo;
				return templateOne;
			}
		}
	}
}
