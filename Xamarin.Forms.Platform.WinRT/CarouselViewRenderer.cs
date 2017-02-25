using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using Xamarin.Forms.PlatformConfiguration.WindowsSpecific;
using WButton = Windows.UI.Xaml.Controls.Button;
using WThickness = Windows.UI.Xaml.Thickness;
using WRectangle = Windows.UI.Xaml.Shapes.Rectangle;

#if WINDOWS_UWP

namespace Xamarin.Forms.Platform.UWP
#else

namespace Xamarin.Forms.Platform.WinRT
#endif
{
	internal delegate void TimerCallback(object state);

	public class CarouselViewRenderer : ViewRenderer<CarouselView, UserControl>
	{
		const string PreviousButtonHorizontal = "PreviousButtonHorizontal";
		const string NextButtonHorizontal = "NextButtonHorizontal";
		const string PreviousButtonVertical = "PreviousButtonVertical";
		const string NextButtonVertical = "NextButtonVertical";
		const string DotsPanelName = "dotsPanel";
		const string IndicatorsName = "indicators";
		const string FlipViewName = "flipView";

		bool _disposed;
		ObservableCollection<Shape> _Dots;
		double _ElementHeight;
		double _ElementWidth;
		SolidColorBrush _fillColor;
		FlipView _flipView;
		StackPanel _indicators;
		bool _IsLoading;
		bool _IsRemoving;
		// To avoid triggering Position changed
		bool _isSwiping;
		UserControl _nativeView;
		SolidColorBrush _selectedColor;
		ObservableCollection<FrameworkElement> _Source;
		Timer _timer;

		public void InsertItem(object item, int position)
		{
			if (Element != null && _flipView != null && Element.ItemsSource != null)
			{
				if (position > Element.ItemsSource.Count + 1)
					throw new CarouselViewException("Page cannot be inserted at a position bigger than ItemsSource.Count");

				if (position == -1)
				{
					Element.ItemsSource.Add(item);
					_Source.Add(CreateView(item));
					_Dots.Add(CreateDot(-1, position));
				}
				else
				{
					Element.ItemsSource.Insert(position, item);
					_Source.Insert(position, CreateView(item));
					_Dots.Insert(position, CreateDot(position, position));
				}
			}
		}

		public void ItemsSourceChanged()
		{
			if (Element != null && _flipView != null)
			{
				if (Element.ItemsSource != null && Element.ItemsSource?.Count > 0)
				{
					_IsLoading = true;

					_isSwiping = true;
					if (Element.ItemsSource != null)
					{
						if (Element.Position > Element.ItemsSource.Count - 1)
							Element.Position = Element.ItemsSource.Count - 1;

						if (Element.Position == -1)
							Element.Position = 0;

						SetItemFromPosition(Element.Position);
					}
					else
					{
						Element.Position = 0;
						Element.Item = null;
					}
					_isSwiping = false;

					var source = new List<FrameworkElement>();

					for (int j = 0; j <= Element.Position; j++)
					{
						source.Add(CreateView(Element.ItemsSource[j]));
					}

					_Source = new ObservableCollection<FrameworkElement>(source);

					_flipView.ItemsSource = _Source;

					var dots = new List<Shape>();

					int i = 0;
					foreach (var item in Element.ItemsSource)
					{
						dots.Add(CreateDot(i, Element.Position));
						i++;
					}

					_Dots = new ObservableCollection<Shape>(dots);

					var dotsPanel = _nativeView.FindName(DotsPanelName) as ItemsControl;
					dotsPanel.ItemsSource = _Dots;

					_flipView.SelectedIndex = Element.Position;

					for (var j = Element.Position + 1; j <= Element.ItemsSource.Count - 1; j++)
					{
						_Source.Add(CreateView(Element.ItemsSource[j]));
					}

					_IsLoading = false;
				}
				else
				{
					_isSwiping = true;
					Element.Position = 0;
					Element.Item = null;
					_isSwiping = false;

					var source = new List<FrameworkElement>();
					_Source = new ObservableCollection<FrameworkElement>(source);

					_flipView.ItemsSource = _Source;

					var dots = new List<Shape>();
					_Dots = new ObservableCollection<Shape>(dots);

					var dotsPanel = _nativeView.FindName(DotsPanelName) as ItemsControl;
					dotsPanel.ItemsSource = _Dots;
				}

				_isSwiping = false;
			}
		}

		public void RemoveItem(int position)
		{
			if (Element != null && _flipView != null && Element.ItemsSource != null && Element.ItemsSource?.Count > 0)
			{
				if (position > Element.ItemsSource.Count - 1)
					throw new CarouselViewException("Page cannot be removed at a position bigger than ItemsSource.Count - 1");

				if (Element.ItemsSource?.Count == 1)
				{
					Element.ItemsSource.RemoveAt(position);
					ItemsSourceChanged();
				}
				else
				{
					_IsRemoving = true;

					if (position == Element.Position)
					{
						if (position > 0)
						{
							var newPos = position - 1;
							if (newPos == -1)
								newPos = 0;

							_flipView.SelectedIndex = newPos;
						}
					}

					Element.ItemsSource.RemoveAt(position);
					_Source.RemoveAt(position);

					_IsRemoving = false;

					var newIdx = _flipView.SelectedIndex;

					_isSwiping = true;
					Element.Position = newIdx;
					SetItemFromPosition(newIdx);
					_isSwiping = false;

					_Dots.RemoveAt(position);
					UpdateIndicators();

					SendChangeEvents(newIdx);
				}
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && !_disposed)
			{
				if (_flipView != null)
				{
					_flipView.SelectionChanged -= FlipView_SelectionChanged;
					_flipView = null;
				}

				_indicators = null;

				_nativeView = null;

				_disposed = true;
			}

			try
			{
				base.Dispose(disposing);
			}
			catch (Exception)
			{
				return;
			}
		}

		protected override void OnElementChanged(ElementChangedEventArgs<CarouselView> e)
		{
			base.OnElementChanged(e);

			if (Control == null)
			{
				// Instantiate the native control and assign it to the Control property with
				// the SetNativeControl method

				if (Element.Orientation == CarouselViewOrientation.Horizontal)
					_nativeView = new FlipViewControl();
				else
					_nativeView = new VerticalFlipViewControl();

				_flipView = _nativeView.FindName(FlipViewName) as FlipView;

				_indicators = _nativeView.FindName(IndicatorsName) as StackPanel;
				_indicators.Visibility = Element.ShowIndicators ? Visibility.Visible : Visibility.Collapsed;

				var converter = new ColorConverter();
				_selectedColor = (SolidColorBrush)converter.Convert(Element.CurrentPageIndicatorTintColor, null, null, null);
				_fillColor = (SolidColorBrush)converter.Convert(Element.PageIndicatorTintColor, null, null, null);

				SetNativeControl(_nativeView);
			}

			if (e.OldElement != null)
			{
				// Unsubscribe from event handlers and cleanup any resources
				if (_flipView != null)
				{
					_flipView.Loaded -= FlipView_Loaded;
					_flipView.SelectionChanged -= FlipView_SelectionChanged;
				}

				if (Element != null)
				{
					Element.RemoveAction = null;
					Element.InsertAction = null;
				}
			}

			if (e.NewElement != null)
			{
				// Configure the control and subscribe to event handlers

				_flipView.Loaded += FlipView_Loaded;

				_flipView.SelectionChanged += FlipView_SelectionChanged;

				_flipView.SizeChanged += FlipView_SizeChanged;

				Element.RemoveAction = new Action<int>(RemoveItem);
				Element.InsertAction = new Action<object, int>(InsertItem);
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == CarouselView.WidthProperty.PropertyName)
			{
				var rect = Element.Bounds;
				if (_ElementWidth == 0)
					_ElementWidth = rect.Width;
			}
			else if (e.PropertyName == CarouselView.HeightProperty.PropertyName)
			{
				var rect = Element.Bounds;
				if (_ElementHeight == 0)
					_ElementHeight = rect.Height;
			}
			else if (e.PropertyName == CarouselView.ShowIndicatorsProperty.PropertyName)
			{
				_indicators.Visibility = Element.ShowIndicators ? Visibility.Visible : Visibility.Collapsed;
			}
			else if (e.PropertyName == CarouselView.ItemsSourceProperty.PropertyName)
			{
				ItemsSourceChanged();
			}
			else if (e.PropertyName == CarouselView.PositionProperty.PropertyName)
			{
				if (Element.Position != -1 && !_isSwiping)
					SetCurrentItem(Element.Position);
			}
			else if (e.PropertyName == CarouselView.ItemProperty.PropertyName)
			{
				if (Element?.ItemsSource?.Count > 0)
				{
					var item = Element.Item;
					int index = Element.ItemsSource.IndexOf(item);

					if (index != -1 && !_isSwiping && index != Element.Position)
						Element.Position = index;
				}
			}

			if (_Source == null && _ElementWidth > 0 && _ElementHeight > 0)
				ItemsSourceChanged();
		}

		void ButtonHide(FlipView f, string name)
		{
			var b = FindVisualChild<WButton>(f, name);
			if (b != null)
			{
				b.Opacity = 0.0;
				b.IsHitTestVisible = false;
			}
		}

		Shape CreateDot(int i, int position)
		{
			if (Element.IndicatorsShape == CarouselViewIndicatorsShape.Circle)
			{
				return new Ellipse()
				{
					Fill = i == position ? _selectedColor : _fillColor,
					Height = 7,
					Width = 7,
					Margin = new WThickness(4, 12, 4, 12)
				};
			}
			else
			{
				return new WRectangle()
				{
					Fill = i == position ? _selectedColor : _fillColor,
					Height = 6,
					Width = 6,
					Margin = new WThickness(4, 12, 4, 12)
				};
			}
		}

		FrameworkElement CreateView(object item)
		{
			View formsView = null;
			var bindingContext = item;

			var dt = bindingContext as DataTemplate;

			if (dt != null)
			{
				formsView = (View)dt.CreateContent();
			}
			else
			{
				var selector = Element.ItemTemplate as DataTemplateSelector;
				if (selector != null)
					formsView = (View)selector.SelectTemplate(bindingContext, Element).CreateContent();
				else
					formsView = (View)Element.ItemTemplate.CreateContent();

				formsView.BindingContext = bindingContext;
			}

			formsView.Parent = this.Element;

			var element = AddView(formsView, new Rectangle(0, 0, _ElementWidth, _ElementHeight));

			return element;
		}

		childItemType FindVisualChild<childItemType>(DependencyObject obj, string name) where childItemType : FrameworkElement
		{
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
			{
				DependencyObject child = VisualTreeHelper.GetChild(obj, i);
				if (child is childItemType && ((FrameworkElement)child).Name == name)
					return (childItemType)child;
				else
				{
					childItemType childOfChild = FindVisualChild<childItemType>(child, name);
					if (childOfChild != null)
						return childOfChild;
				}
			}
			return null;
		}

		void FlipView_Loaded(object sender, RoutedEventArgs e)
		{
			bool arrows = Element.OnThisPlatform().GetArrows();
			if (!arrows)
			{
				ButtonHide(_flipView, PreviousButtonHorizontal);
				ButtonHide(_flipView, NextButtonHorizontal);
				ButtonHide(_flipView, PreviousButtonVertical);
				ButtonHide(_flipView, NextButtonVertical);
			}
		}

		void FlipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!_IsLoading && !_IsRemoving)
			{
				var newPos = _flipView.SelectedIndex;

				_isSwiping = true;
				Element.Position = newPos;
				SetItemFromPosition(newPos);
				_isSwiping = false;

				UpdateIndicators();

				SendChangeEvents(newPos);
			}
		}

		void FlipView_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (e.NewSize.Width != _ElementWidth || e.NewSize.Height != _ElementHeight)
			{
				if (_timer != null)
					_timer.Dispose();
				_timer = null;

				_timer = new Timer(OnTick, e.NewSize, 0, 0);
			}
		}

		void OnTick(object args)
		{
			_timer?.Dispose();
			_timer = null;

			var size = (Windows.Foundation.Size)args;
			_ElementWidth = size.Width;
			_ElementHeight = size.Height;

			Device.BeginInvokeOnMainThread(() =>
			{
				ItemsSourceChanged();
			});
		}

		void SendChangeEvents(int position)
		{
			Element.PositionSelected?.Invoke(Element, new SelectedPositionChangedEventArgs(position));

			var itemsCount = Element.ItemsSource?.Count;
			if (itemsCount > 0 && itemsCount > position && position > -1)
				Element.ItemSelected?.Invoke(Element, new SelectedItemChangedEventArgs(Element.ItemsSource[position]));
			else
				Element.ItemSelected?.Invoke(Element, new SelectedItemChangedEventArgs(null));
		}

		void SetCurrentItem(int position)
		{
			if (Element != null && _flipView != null && Element.ItemsSource != null && Element.ItemsSource?.Count > 0)
			{
				if (position > Element.ItemsSource.Count - 1)
					throw new CarouselViewException("Current page index cannot be bigger than ItemsSource.Count - 1");

				SetItemFromPosition(position);

				_flipView.SelectedIndex = position;
			}
		}

		void SetItemFromPosition(int position)
		{
			if (Element == null)
				return;

			if (Element.ItemsSource?.Count > 0 && position > -1)
			{
				var thisItem = Element.ItemsSource[position];
				if (Element.Item != thisItem)
					Element.Item = thisItem;
			}
			else if (Element.Item != null)
				Element.Item = null;
		}

		void UpdateIndicators()
		{
			var dotsPanel = _nativeView.FindName(DotsPanelName) as ItemsControl;
			int i = 0;
			foreach (var item in dotsPanel.Items)
			{
				((Shape)item).Fill = i == Element.Position ? _selectedColor : _fillColor;
				i++;
			}
		}

		static FrameworkElement AddView(View view, Rectangle size)
		{
			if (Platform.GetRenderer(view) == null)
				Platform.SetRenderer(view, Platform.CreateRenderer(view));

			var vRenderer = Platform.GetRenderer(view);

			view.Layout(new Rectangle(0, 0, size.Width, size.Height));

			return vRenderer.ContainerElement;
		}
	}

	internal sealed class Timer : CancellationTokenSource, IDisposable
	{
		public Timer(TimerCallback callback, object state, int dueTime, int period)
		{
			Task.Delay(dueTime, Token).ContinueWith(async (t, s) =>
			{
				var tuple = (Tuple<TimerCallback, object>)s;

				while (true)
				{
					if (IsCancellationRequested)
						break;
					await Task.Run(() => tuple.Item1(tuple.Item2));
					await Task.Delay(period);
				}
			}, Tuple.Create(callback, state), CancellationToken.None,
				TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnRanToCompletion,
				TaskScheduler.Default);
		}

		public new void Dispose()
		{
			base.Cancel();
		}
	}
}