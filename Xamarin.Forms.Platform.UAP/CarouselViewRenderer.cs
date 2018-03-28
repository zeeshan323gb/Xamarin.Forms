using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Specialized;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace Xamarin.Forms.Platform.UWP
{
	/// <summary>
	/// CarouselView Renderer
	/// </summary>
	public class CarouselViewRenderer : ViewRenderer<CarouselView, UserControl>
	{
		bool _orientationChanged;
		double _lastOffset;

		FlipViewControl _nativeView;
		FlipView _flipView;
		StackPanel _indicators;

		ColorConverter _converter;
		SolidColorBrush _selectedColor;
		SolidColorBrush _fillColor;

		double _elementWidth;
		double _elementHeight;

		// To hold all the rendered views
		ObservableCollection<FrameworkElement> Source;

		// To hold the indicators dots
		ObservableCollection<Shape> Dots;

		// To manage SizeChanged
		Timer _timer;

		bool _disposed;

		// To avoid triggering Position changed more than once
		bool _isChangingPosition;

		ScrollViewer _scrollingHost;
		Windows.UI.Xaml.Controls.Button _prevBtn;
		Windows.UI.Xaml.Controls.Button _nextBtn;

		protected override void OnElementChanged(ElementChangedEventArgs<CarouselView> e)
		{
			base.OnElementChanged(e);

			if (Control == null)
			{
				// Instantiate the native control and assign it to the Control property with
				// the SetNativeControl method
				_orientationChanged = true;
			}

			if (e.OldElement != null)
			{
				// Unsubscribe from event handlers and cleanup any resources
				if (_flipView != null)
				{
					_flipView.Loaded -= FlipView_Loaded;
					_flipView.SelectionChanged -= FlipView_SelectionChanged;
					_flipView.SizeChanged -= FlipView_SizeChanged;
				}

				if (Element == null) return;

				if (Element.ItemsSource != null && Element.ItemsSource is INotifyCollectionChanged)
					((INotifyCollectionChanged)Element.ItemsSource).CollectionChanged -= ItemsSource_CollectionChanged;
			}

			if (e.NewElement != null)
			{
				Element.SizeChanged += Element_SizeChanged;
				// Configure the control and subscribe to event handlers
				if (Element.ItemsSource != null && Element.ItemsSource is INotifyCollectionChanged)
					((INotifyCollectionChanged)Element.ItemsSource).CollectionChanged += ItemsSource_CollectionChanged;
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (Element == null || _flipView == null) return;

			var rect = this.Element?.Bounds;

			if (e.PropertyName == CarouselView.OrientationProperty.PropertyName)
			{
				UpdateOrientation();
			}
			else if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName)
			{
				UpdateBackgroundColor();
			}
			else if (e.PropertyName == CarouselView.IsSwipeEnabledProperty.PropertyName)
			{
				_nativeView.IsSwipeEnabled = Element.IsSwipeEnabled;
			}
			else if (e.PropertyName == CarouselView.IndicatorsTintColorProperty.PropertyName)
			{
				_fillColor = (SolidColorBrush)_converter.Convert(Element.IndicatorsTintColor, null, null, null);
				UpdateIndicatorsTint();
			}
			else if (e.PropertyName == CarouselView.CurrentPageIndicatorTintColorProperty.PropertyName)
			{
				_selectedColor = (SolidColorBrush)_converter.Convert(Element.CurrentPageIndicatorTintColor, null, null, null);
				UpdateIndicatorsTint();
			}
			else if ((e.PropertyName == CarouselView.IndicatorsShapeProperty.PropertyName) || (e.PropertyName == CarouselView.ShowIndicatorsProperty.PropertyName))
			{
				SetIndicators();
			}
			else if (e.PropertyName == CarouselView.ItemsSourceProperty.PropertyName)
			{
				UpdateItemsSource();
			}
			else if (e.PropertyName == CarouselView.ItemTemplateProperty.PropertyName)
			{
				SetNativeView();
				Element.SendPositionSelected();
			}
			else if (e.PropertyName == CarouselView.PositionProperty.PropertyName)
			{
				if (!_isChangingPosition)
				{
					SetCurrentPage(Element.Position);
				}
			}
			else if (e.PropertyName == CarouselView.ShowArrowsProperty.PropertyName)
			{
				UpdateArrows();
			}
		}

		protected override void UpdateBackgroundColor()
		{
			if (_flipView == null)
				return;
			_flipView.Background = (SolidColorBrush)_converter.Convert(Element.BackgroundColor, null, null, null);
		}

		void SetNativeView()
		{
			var position = Element.Position;

			if (_nativeView == null)
			{
				_nativeView = new FlipViewControl(Element.IsSwipeEnabled);
				_flipView = _nativeView.FindName("flipView") as FlipView;
			}

			if (_orientationChanged)
			{
				// Orientation BP
				if (Element.Orientation == CarouselViewOrientation.Horizontal)
					_flipView.ItemsPanel = _nativeView.Resources["HPanel"] as ItemsPanelTemplate;
				else
					_flipView.ItemsPanel = _nativeView.Resources["VPanel"] as ItemsPanelTemplate;

				_orientationChanged = false;
			}

			var source = new List<FrameworkElement>();
			if (Element.ItemsSource != null && Element.ItemsSource?.GetCount() > 0)
			{
				for (int j = 0; j <= Element.ItemsSource.GetCount() - 1; j++)
				{
					source.Add(CreateView(Element.ItemsSource.GetItem(j)));
				}
			}

			Source = new ObservableCollection<FrameworkElement>(source);
			_flipView.ItemsSource = Source;

			//flipView.ItemsSource = Element.ItemsSource;
			//flipView.ItemTemplateSelector = new MyTemplateSelector(Element); (the way it should be)

			_converter = new ColorConverter();

			// BackgroundColor BP
			_flipView.Background = (SolidColorBrush)_converter.Convert(Element.BackgroundColor, null, null, null);

			// IndicatorsTintColor BP
			_fillColor = (SolidColorBrush)_converter.Convert(Element.IndicatorsTintColor, null, null, null);

			// CurrentPageIndicatorTintColor BP
			_selectedColor = (SolidColorBrush)_converter.Convert(Element.CurrentPageIndicatorTintColor, null, null, null);

			_flipView.Loaded += FlipView_Loaded;
			_flipView.SelectionChanged += FlipView_SelectionChanged;
			_flipView.SizeChanged += FlipView_SizeChanged;

			if (Source.Count > 0)
			{
				_flipView.SelectedIndex = position;
			}

			SetNativeControl(_nativeView);

			//SetArrows();

			// INDICATORS
			_indicators = _nativeView.FindName("indicators") as StackPanel;
			SetIndicators();
		}

		void UpdateArrows()
		{
			FlipView_Loaded(_flipView, null);
		}

		void UpdateItemsSource()
		{
			SetPosition();
			SetNativeView();
			SetArrowsVisibility();
			Element.SendPositionSelected();
			if (Element.ItemsSource != null && Element.ItemsSource is INotifyCollectionChanged)
				((INotifyCollectionChanged)Element.ItemsSource).CollectionChanged += ItemsSource_CollectionChanged;
		}

		void UpdateOrientation()
		{
			_orientationChanged = true;
			SetNativeView();
			Element.SendPositionSelected();
		}

		void ScrollingHost_ViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
		{
			var scrollView = (ScrollViewer)sender;

			double percentCompleted;
			ScrollDirection direction;

			// Get Horizontal or Vertical Offset depending on carousel orientation
			var currentOffset = Element.Orientation == CarouselViewOrientation.Horizontal ? scrollView.HorizontalOffset : scrollView.VerticalOffset;

			// Scrolling to the right
			if (currentOffset > _lastOffset)
			{
				percentCompleted = Math.Floor((currentOffset - (int)currentOffset) * 100);
				direction = Element.Orientation == CarouselViewOrientation.Horizontal ? ScrollDirection.Right : ScrollDirection.Down;
			}
			else
			{
				percentCompleted = Math.Floor((_lastOffset - currentOffset) * 100);
				direction = Element.Orientation == CarouselViewOrientation.Horizontal ? ScrollDirection.Left : ScrollDirection.Up;
			}

			if (percentCompleted <= 100)
				Element.SendScrolled(percentCompleted, direction);
		}

		void ScrollingHost_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
		{
			if (!e.IsIntermediate)
			{
				var scrollView = (ScrollViewer)sender;
				_lastOffset = Element.Orientation == CarouselViewOrientation.Horizontal ? scrollView.HorizontalOffset : scrollView.VerticalOffset;
			}
		}

		async void ItemsSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			// NewItems contains the item that was added.
			// If NewStartingIndex is not -1, then it contains the index where the new item was added.
			if (e.Action == NotifyCollectionChangedAction.Add)
			{
				InsertPage(Element?.ItemsSource.GetItem(e.NewStartingIndex), e.NewStartingIndex);
			}

			// OldItems contains the item that was removed.
			// If OldStartingIndex is not -1, then it contains the index where the old item was removed.
			if (e.Action == NotifyCollectionChangedAction.Remove)
			{
				await RemovePageAsync(e.OldStartingIndex);
			}

			// OldItems contains the moved item.
			// OldStartingIndex contains the index where the item was moved from.
			// NewStartingIndex contains the index where the item was moved to.
			if (e.Action == NotifyCollectionChangedAction.Move)
			{
				if (Element == null || _flipView == null || Source == null) return;

				var obj = Source[e.OldStartingIndex];
				Source.RemoveAt(e.OldStartingIndex);
				Source.Insert(e.NewStartingIndex, obj);

				SetCurrentPage(Element.Position);
			}

			// NewItems contains the replacement item.
			// NewStartingIndex and OldStartingIndex are equal, and if they are not -1,
			// then they contain the index where the item was replaced.
			if (e.Action == NotifyCollectionChangedAction.Replace)
			{
				if (Element == null || _flipView == null || Source == null) return;

				Source[e.OldStartingIndex] = CreateView(e.NewItems[e.NewStartingIndex]);
			}

			// No other properties are valid.
			if (e.Action == NotifyCollectionChangedAction.Reset)
			{
				if (Element == null) return;

				SetPosition();
				SetNativeView();

				SetArrowsVisibility();

				Element.SendPositionSelected();
			}
		}

		void FlipView_Loaded(object sender, RoutedEventArgs e)
		{
			ButtonHide(_flipView, "PreviousButtonHorizontal");
			ButtonHide(_flipView, "NextButtonHorizontal");
			ButtonHide(_flipView, "PreviousButtonVertical");
			ButtonHide(_flipView, "NextButtonVertical");

			//var controls = AllChildren(flipView);

			if (_scrollingHost == null)
			{
				_scrollingHost = FindVisualChild<Windows.UI.Xaml.Controls.ScrollViewer>(_flipView, "ScrollingHost");

				_scrollingHost.ViewChanging += ScrollingHost_ViewChanging;
				_scrollingHost.ViewChanged += ScrollingHost_ViewChanged;
			}

			SetArrows();
		}

		void Element_SizeChanged(object sender, EventArgs e)
		{
			if (Element == null) return;

			var rect = Element.Bounds;

			if (_nativeView == null)
			{
				_elementWidth = ((Xamarin.Forms.Rectangle)rect).Width;
				_elementHeight = ((Xamarin.Forms.Rectangle)rect).Height;
				SetNativeView();
				Element.SendPositionSelected();
			}
		}

		void FlipView_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (e.NewSize.Width != _elementWidth || e.NewSize.Height != _elementHeight)
			{
				if (_timer != null)
					_timer.Dispose();
				_timer = null;

				_timer = new Timer(OnTick, e.NewSize, 100, 100);
			}
		}

		void FlipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (Element != null && !_isChangingPosition)
			{
				Element.Position = _flipView.SelectedIndex;
				UpdateIndicatorsTint();

				Element.SendPositionSelected();
			}
		}

		void OnTick(object args)
		{
			_timer.Dispose();
			_timer = null;

			// Save new dimensions when resize completes
			var size = (Windows.Foundation.Size)args;
			_elementWidth = size.Width;
			_elementHeight = size.Height;

			// Refresh UI
			Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
			{
				if (Element != null)
				{
					SetNativeView();
					Element.SendPositionSelected();
				}
			});
		}

		void SetPosition()
		{
			_isChangingPosition = true;
			if (Element.ItemsSource != null)
			{
				if (Element.Position > Element.ItemsSource.GetCount() - 1)
					Element.Position = Element.ItemsSource.GetCount() - 1;

				if (Element.Position == -1)
					Element.Position = 0;
			}
			else
			{
				Element.Position = 0;
			}
			_isChangingPosition = false;
		}

		void SetArrows()
		{
			if (Element.Orientation == CarouselViewOrientation.Horizontal)
			{
				_prevBtn = FindVisualChild<Windows.UI.Xaml.Controls.Button>(_flipView, "PreviousButtonHorizontal");
				_nextBtn = FindVisualChild<Windows.UI.Xaml.Controls.Button>(_flipView, "NextButtonHorizontal");
			}
			else
			{
				_prevBtn = FindVisualChild<Windows.UI.Xaml.Controls.Button>(_flipView, "PreviousButtonVertical");
				_nextBtn = FindVisualChild<Windows.UI.Xaml.Controls.Button>(_flipView, "NextButtonVertical");
			}

			// TODO: Set BackgroundColor, TintColor and Transparency
		}

		void SetArrowsVisibility()
		{
			if (_prevBtn == null || _nextBtn == null) return;
			_prevBtn.Visibility = Element.Position == 0 || Element.ItemsSource.GetCount() == 0 ? Visibility.Collapsed : Visibility.Visible;
			_nextBtn.Visibility = Element.Position == Element.ItemsSource.GetCount() - 1 || Element.ItemsSource.GetCount() == 0 ? Visibility.Collapsed : Visibility.Visible;
		}

		void SetIndicators()
		{
			var dotsPanel = _nativeView.FindName("dotsPanel") as ItemsControl;

			if (Element.ShowIndicators)
			{
				if (Element.Orientation == CarouselViewOrientation.Horizontal)
				{
					_indicators.HorizontalAlignment = HorizontalAlignment.Stretch;
					_indicators.VerticalAlignment = VerticalAlignment.Bottom;
					_indicators.Width = Double.NaN;
					_indicators.Height = 32;
					dotsPanel.HorizontalAlignment = HorizontalAlignment.Center;
					dotsPanel.VerticalAlignment = VerticalAlignment.Bottom;
					dotsPanel.ItemsPanel = _nativeView.Resources["dotsHPanel"] as ItemsPanelTemplate;
				}
				else
				{
					_indicators.HorizontalAlignment = HorizontalAlignment.Right;
					_indicators.VerticalAlignment = VerticalAlignment.Center;
					_indicators.Width = 32;
					_indicators.Height = Double.NaN;
					dotsPanel.HorizontalAlignment = HorizontalAlignment.Center;
					dotsPanel.VerticalAlignment = VerticalAlignment.Center;
					dotsPanel.ItemsPanel = _nativeView.Resources["dotsVPanel"] as ItemsPanelTemplate;
				}

				var dots = new List<Shape>();

				if (Element.ItemsSource != null && Element.ItemsSource?.GetCount() > 0)
				{
					int i = 0;
					foreach (var item in Element.ItemsSource)
					{
						dots.Add(CreateDot(i, Element.Position));
						i++;
					}
				}

				Dots = new ObservableCollection<Shape>(dots);
				dotsPanel.ItemsSource = Dots;
			}
			else
			{
				dotsPanel.ItemsSource = new List<Shape>();
			}

			// ShowIndicators BP
			_indicators.Visibility = Element.ShowIndicators ? Visibility.Visible : Visibility.Collapsed;
		}

		void UpdateIndicatorsTint()
		{
			var dotsPanel = _nativeView.FindName("dotsPanel") as ItemsControl;
			int i = 0;
			foreach (var item in dotsPanel.Items)
			{
				((Shape)item).Fill = i == Element.Position ? _selectedColor : _fillColor;
				i++;
			}
		}

		void InsertPage(object item, int position)
		{
			if (Element == null || _flipView == null || Source == null) return;

			if (position <= Element.Position)
			{
				_isChangingPosition = true;
				Element.Position++;
				_isChangingPosition = false;
			}

			Source.Insert(position, CreateView(item));
			Dots?.Insert(position, CreateDot(position, Element.Position));

			_flipView.SelectedIndex = Element.Position;

			//if (position <= Element.Position)
			Element.SendPositionSelected();
		}

		async Task RemovePageAsync(int position)
		{
			if (Element == null || _flipView == null || Source == null) return;

			if (Source?.Count > 0)
			{
				// To remove latest page, rebuild flipview or the page wont disappear
				if (Source.Count == 1)
				{
					SetNativeView();
				}
				else
				{

					_isChangingPosition = true;

					// To remove current page
					if (position == Element.Position)
					{
						// Swipe animation at position 0 doesn't work :(
						/*if (position == 0)
                        {
                            flipView.SelectedIndex = 1;
                        }
                        else
                        {*/
						if (position > 0)
						{
							var newPos = position - 1;
							if (newPos == -1)
								newPos = 0;

							_flipView.SelectedIndex = newPos;
						}

						// With a swipe transition
						if (Element.AnimateTransition)
							await Task.Delay(100);
					}

					Source.RemoveAt(position);

					Element.Position = _flipView.SelectedIndex;

					Dots?.RemoveAt(position);
					UpdateIndicatorsTint();

					_isChangingPosition = false;

					Element.SendPositionSelected();
				}
			}
		}

		void SetCurrentPage(int position)
		{
			if (position < 0 || position > Element.ItemsSource?.GetCount() - 1)
				return;

			if (Element == null || _flipView == null || Element?.ItemsSource == null) return;

			if (Element.ItemsSource?.GetCount() > 0)
			{
				_flipView.SelectedIndex = position;
			}
		}

		FrameworkElement CreateView(object item)
		{
			Xamarin.Forms.View formsView = null;
			var bindingContext = item;

			var dt = bindingContext as Xamarin.Forms.DataTemplate;
			var view = bindingContext as View;

			// Support for List<DataTemplate> as ItemsSource
			if (dt != null)
			{
				formsView = (Xamarin.Forms.View)dt.CreateContent();
			}
			else
			{

				if (view != null)
				{
					formsView = view;
				}
				else
				{
					var selector = Element.ItemTemplate as Xamarin.Forms.DataTemplateSelector;
					if (selector != null)
						formsView = (Xamarin.Forms.View)selector.SelectTemplate(bindingContext, Element).CreateContent();
					else
						formsView = (Xamarin.Forms.View)Element.ItemTemplate.CreateContent();

					formsView.BindingContext = bindingContext;
				}
			}

			formsView.Parent = this.Element;

			var element = formsView.ToWindows(new Xamarin.Forms.Rectangle(0, 0, _elementWidth, _elementHeight));

			//if (dt == null && view == null)
			//formsView.Parent = null;

			return element;
		}

		Shape CreateDot(int i, int position)
		{
			if (Element.IndicatorsShape == IndicatorsShape.Circle)
			{
				return new Ellipse()
				{
					Fill = i == position ? _selectedColor : _fillColor,
					Height = 7,
					Width = 7,
					Margin = new Windows.UI.Xaml.Thickness(4, 12, 4, 12)
				};
			}
			else
			{
				return new Windows.UI.Xaml.Shapes.Rectangle()
				{
					Fill = i == position ? _selectedColor : _fillColor,
					Height = 6,
					Width = 6,
					Margin = new Windows.UI.Xaml.Thickness(4, 12, 4, 12)
				};
			}
		}

		void ButtonHide(FlipView f, string name)
		{
			var b = FindVisualChild<Windows.UI.Xaml.Controls.Button>(f, name);
			if (b != null)
			{
				b.Opacity = Element.ShowArrows ? 1.0 : 0.0;
				b.IsHitTestVisible = Element.ShowArrows;
			}
		}

		ChildItemType FindVisualChild<ChildItemType>(DependencyObject obj, string name) where ChildItemType : FrameworkElement
		{
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
			{
				DependencyObject child = VisualTreeHelper.GetChild(obj, i);
				if (child is ChildItemType && ((FrameworkElement)child).Name == name)
					return (ChildItemType)child;
				else
				{
					ChildItemType childOfChild = FindVisualChild<ChildItemType>(child, name);
					if (childOfChild != null)
						return childOfChild;
				}
			}
			return null;
		}

		List<Control> AllChildren(DependencyObject parent)
		{
			var _list = new List<Control>();
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
			{
				var _child = VisualTreeHelper.GetChild(parent, i);
				if (_child is Control)
					_list.Add(_child as Control);
				_list.AddRange(AllChildren(_child));
			}
			return _list;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && !_disposed)
			{
				_prevBtn = null;
				_nextBtn = null;

				_indicators = null;

				if (_flipView != null)
				{
					_flipView.SelectionChanged -= FlipView_SelectionChanged;
					_flipView = null;
				}

				if (Element != null)
				{
					if (Element.ItemsSource != null && Element.ItemsSource is INotifyCollectionChanged)
						((INotifyCollectionChanged)Element.ItemsSource).CollectionChanged -= ItemsSource_CollectionChanged;
				}

				_nativeView = null;

				_disposed = true;
			}


			base.Dispose(disposing);
		}
	}

	// UWP DataTemplate doesn't support loadTemplate function as parameter
	// Having that, rendering all the views ahead of time is not needed
	/*public class MyTemplateSelector : DataTemplateSelector
    {
        CarouselView Element;

        public MyTemplateSelector(CarouselView element)
        {
            Element = element;
        }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            Xamarin.Forms.View formsView = null;
            var bindingContext = item;

            var dt = bindingContext as Xamarin.Forms.DataTemplate;

            // Support for List<DataTemplate> as ItemsSource
            if (dt != null)
            {
                formsView = (Xamarin.Forms.View)dt.CreateContent();
            }
            else {

                var selector = Element.ItemTemplate as Xamarin.Forms.DataTemplateSelector;
                if (selector != null)
                    formsView = (Xamarin.Forms.View)selector.SelectTemplate(bindingContext, Element).CreateContent();
                else
                    formsView = (Xamarin.Forms.View)Element.ItemTemplate.CreateContent();

                formsView.BindingContext = bindingContext;
            }

            formsView.Parent = this.Element;


            var element = FormsViewToNativeUWP.ConvertFormsToNative(formsView, new Xamarin.Forms.Rectangle(0, 0, ElementWidth, ElementHeight));

            var template = new DataTemplate(() => return element; ); // THIS IS NOT SUPPORTED :(

            return template;
        }
    }*/
}