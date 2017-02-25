using System.Linq;
using System.Windows.Input;
using Xamarin.Forms.Internals;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace Xamarin.Forms.Controls.GalleryPages
{
	public class CarouselViewGallery : ContentPage
	{
		Label _addPageButton;
		ICommand _addPageCommand;
		CarouselView _carouselView;
		Label _clearItemsSourceButton;
		ICommand _clearItemsSourceCommand;
		int _currentPosition;
		Label _nextButton;
		ICommand _nextCommand;
		Label _nullItemsSourceButton;
		ICommand _nullItemsSourceCommand;
		Label _prevButton;
		ICommand _prevCommand;
		Label _removePageButton;
		ICommand _removePageCommand;
		Label _selectedItemLabel;
		Label _selectedPositionLabel;

		public CarouselViewGallery()
		{
			InitCommands();

			InitControls();

			SetButtonVisibility();

			Content = GetMainGrid();
		}

		void AddPage()
		{
			if (_carouselView.ItemsSource == null)
				_carouselView.ItemsSource = new ObservableCollection<int>();

			_carouselView.InsertPage(_carouselView.ItemsSource.Count);

			if (_carouselView.ItemsSource.Count > 1)
				_carouselView.Position = _carouselView.ItemsSource.Count - 1;

			SetButtonVisibility();
		}

		void Clear()
		{
			_carouselView.ItemsSource = new List<int>();
		}

		Label GetButton(string text, ICommand command)
		{
			var label = new Label
			{
				HorizontalTextAlignment = TextAlignment.Center,
				Text = text,
				VerticalTextAlignment = TextAlignment.Center
			};
			label.GestureRecognizers.Add(new TapGestureRecognizer { Command = command });
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
				Orientation = CarouselViewOrientation.Horizontal,
				ShowIndicators = true,
				ItemTemplate = new MyTemplateSelector(),
				ItemsSource = new ObservableCollection<int>(Enumerable.Range(0, 10)),
				Position = 0
			};
			carouselView.PositionSelected += PositionSelected;
			carouselView.ItemSelected += ItemSelected;

			carouselView.On<iOS>().SetInterPageSpacing(10).SetInterPageSpacingColor(Color.Purple);
			carouselView.On<Android>().SetInterPageSpacing(10).SetInterPageSpacingColor(Color.Purple);

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
			controlsGrid.AddChild(new StackLayout
			{
				Orientation = StackOrientation.Horizontal,
				Children = {
					_addPageButton, _clearItemsSourceButton, _nullItemsSourceButton, _removePageButton
				}
			}, 1, 0);
			controlsGrid.AddChild(_nextButton, 2, 0);

			return controlsGrid;
		}

		Grid GetMainGrid()
		{
			var mainGrid = new Grid { RowSpacing = 0 };

			mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
			mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50, GridUnitType.Absolute) });
			mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50, GridUnitType.Absolute) });

			mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

			mainGrid.AddChild(_carouselView, 0, 0);
			mainGrid.AddChild(GetControlsGrid(), 0, 1);
			mainGrid.AddChild(GetStatusGrid(), 0, 2);
			return mainGrid;
		}

		Grid GetStatusGrid()
		{
			var statusGrid = new Grid
			{
				BackgroundColor = Color.White,
				Padding = new Thickness(12, 0, 12, 0)
			};

			statusGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

			statusGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
			statusGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

			statusGrid.AddChild(_selectedItemLabel, 0, 0);
			statusGrid.AddChild(_selectedPositionLabel, 1, 0);

			return statusGrid;
		}

		void InitCommands()
		{
			_addPageCommand = new Command(AddPage);
			_nextCommand = new Command(Next);
			_prevCommand = new Command(Prev);
			_nullItemsSourceCommand = new Command(Null);
			_clearItemsSourceCommand = new Command(Clear);
			_removePageCommand = new Command(RemovePage);
		}

		void InitControls()
		{
			_carouselView = GetCarouselView();
			_addPageButton = GetButton("Add", _addPageCommand);
			_prevButton = GetButton("Prev", _prevCommand);
			_nextButton = GetButton("Next", _nextCommand);
			_nullItemsSourceButton = GetButton("Null", _nullItemsSourceCommand);
			_clearItemsSourceButton = GetButton("Clear", _clearItemsSourceCommand);
			_removePageButton = GetButton("Remove", _removePageCommand);
			_selectedItemLabel = new Label();
			_selectedPositionLabel = new Label();
		}

		void ItemSelected(object sender, SelectedItemChangedEventArgs e)
		{
			SetButtonVisibility();

			if (((int)(_carouselView.Item ?? 0)) != ((int)(e.SelectedItem ?? 0)))
			{
				_selectedItemLabel.TextColor = Color.Red;
				_selectedItemLabel.Text = $"CarouselView.Item ({_carouselView.Item}) != e.SelectedItem ({e.SelectedItem})";
			}
			else
			{
				_selectedItemLabel.TextColor = Color.Black;
				_selectedItemLabel.Text = $"Item \"{e.SelectedItem}\" selected";
			}
		}

		void Next()
		{
			if (_carouselView.Position < _carouselView.ItemsSource?.Count - 1)
				_carouselView.Position++;
		}

		void Null()
		{
			_carouselView.ItemsSource = null;
		}

		void PositionSelected(object sender, SelectedPositionChangedEventArgs e)
		{
			SetButtonVisibility();

			int position = _currentPosition = (int)e.SelectedPosition;

			if (_carouselView.Position != position)
			{
				_selectedPositionLabel.TextColor = Color.Red;
				_selectedPositionLabel.Text = $"CarouselView.Position ({_carouselView.Position}) != e.SelectedPosition ({position})";
			}
			else
			{
				_selectedPositionLabel.TextColor = Color.Black;
				_selectedPositionLabel.Text = $"Position \"{position}\" selected";
			}
		}

		void Prev()
		{
			if (_carouselView.Position > 0)
				_carouselView.Position--;
		}

		void RemovePage()
		{
			_carouselView.RemovePage(_currentPosition);

			SetButtonVisibility();
		}

		void SetButtonVisibility()
		{
			if ((_carouselView.ItemsSource?.Count ?? 0) == 0)
			{
				_prevButton.IsVisible = _nextButton.IsVisible = _removePageButton.IsVisible = false;
			}
			else
			{
				_removePageButton.IsVisible = true;
				_prevButton.IsVisible = _carouselView.Position > 0;
				_nextButton.IsVisible = _carouselView.Position < _carouselView.ItemsSource.Count - 1;
			}
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

				Content = stack;
				BackgroundColor = Color.White;
			}
		}

		[Preserve(AllMembers = true)]
		class MySecondView : ContentView
		{
			public MySecondView()
			{
				var newLabel = new Label();
				newLabel.SetBinding(Label.TextProperty, ".");
				var stack = new StackLayout
				{
					Padding = new Thickness(12),
					Spacing = 12,
					Children =
					{
						new Image{ Source = "crimson.jpg" },
						newLabel
					}
				};

				Content = stack;
				BackgroundColor = Color.Beige;
			}
		}

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
				if ((int)item % 2 == 0)
					return templateTwo;
				return templateOne;
			}
		}
	}
}
