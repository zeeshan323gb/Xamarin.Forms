using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public class CarouselViewRenderer : ViewRenderer<CarouselView, UIView>
	{
		UIPageViewController _pageController;
		UIPageControl _pageControl;

		UIButton _prevBtn;
		UIButton _nextBtn;

		// To avoid triggering Position changed more than once
		bool _isChangingPosition;
		bool _orientationChanged;
		bool _disposed;

		double _elementWidth;
		double _elementHeight;

		int _prevPosition;
		double _percentCompleted;
		nfloat _prevPoint;

		// A local copy of ItemsSource so we can use CollectionChanged events
		List<object> Source;

		// Used only when ItemsSource is a List<View>
		List<FormsUIViewContainer> ChildViewControllers;

		int Count
		{
			get
			{
				return Source?.Count ?? 0;
			}
		}

		// Fix #129 CarouselViewControl not rendered when loading a page from memory bug
		// Fix #157 CarouselView Binding breaks when returning to Page bug duplicate
		public override void MovedToSuperview()
		{
			if (Control == null)
				Element_SizeChanged(Element, null);

			base.MovedToSuperview();
		}

		public override void MovedToWindow()
		{
			if (Control == null)
				Element_SizeChanged(Element, null);

			base.MovedToWindow();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && !_disposed)
			{
				// CarouselViewRenderer.Dispose Null reference Unhandled Exception: #210
				// Exception thrown on Dispose #233
				try
				{
					_pageController.DidFinishAnimating -= PageController_DidFinishAnimating;
					_pageController.GetPreviousViewController = null;
					_pageController.GetNextViewController = null;

					CleanUpPageController();

					_pageController.View.RemoveFromSuperview();
					_pageController.View.Dispose();

					_pageController.Dispose();
					_pageController = null;

				}
				catch (Exception ex)
				{
					Console.Write(ex.Message);
				}

				if (Element != null)
				{
					Element.SizeChanged -= Element_SizeChanged;
					if (Element.ItemsSource != null && Element.ItemsSource is INotifyCollectionChanged)
						((INotifyCollectionChanged)Element.ItemsSource).CollectionChanged -= ItemsSource_CollectionChanged;
				}

				Source = null;

				_disposed = true;
			}

			try
			{
				base.Dispose(disposing);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				return;
			}
		}

		protected override void OnElementChanged(ElementChangedEventArgs<CarouselView> e)
		{
			base.OnElementChanged(e);

			if (Control == null)
			{
				// Instantiate the native control and assign it to the Control property with
				// the SetNativeControl method (called when Height BP changes)
				_orientationChanged = true;
			}

			if (e.OldElement != null)
			{
				// Unsubscribe from event handlers and cleanup any resources

				if (Element == null) return;

				Element.SizeChanged -= Element_SizeChanged;
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

			if (Element == null || _pageController == null) return;

			if (e.PropertyName == "Renderer")
			{
				// Fix for issues after recreating the control #86
				_prevPosition = Element.Position;
			}
			else if (e.PropertyName == CarouselView.OrientationProperty.PropertyName)
			{
				UpdateOrientation();
			}
			else if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName)
			{
				_pageController.View.BackgroundColor = Element.BackgroundColor.ToUIColor();
			}
			else if (e.PropertyName == CarouselView.IsSwipeEnabledProperty.PropertyName)
			{
				SetIsSwipeEnabled();
			}
			else if (e.PropertyName == CarouselView.IsSwipeEnabledProperty.PropertyName)
			{
				SetIsSwipeEnabled();
			}
			else if (e.PropertyName == CarouselView.IndicatorsTintColorProperty.PropertyName)
			{
				SetIndicatorsTintColor();
			}
			else if (e.PropertyName == CarouselView.CurrentPageIndicatorTintColorProperty.PropertyName)
			{
				SetCurrentPageIndicatorTintColor();
			}
			else if (e.PropertyName == CarouselView.IndicatorsShapeProperty.PropertyName)
			{
				SetIndicatorsShape();
			}
			else if (e.PropertyName == CarouselView.ShowIndicatorsProperty.PropertyName)
			{
				SetIndicators();
			}
			else if (e.PropertyName == CarouselView.ItemsSourceProperty.PropertyName)
			{
				UpdateItemsSource();
			}
			else if (e.PropertyName == CarouselView.ItemTemplateProperty.PropertyName)
			{
				UpdateItemTemplate();
			}
			else if (e.PropertyName == CarouselView.PositionProperty.PropertyName)
			{
				UpdatePosition();
			}
			else if (e.PropertyName == CarouselView.ShowArrowsProperty.PropertyName)
			{
				SetArrows();
			}
			else if (e.PropertyName == CarouselView.ArrowsBackgroundColorProperty.PropertyName)
			{
				UpdateArrowsBackgroundColor();
			}
			else if (e.PropertyName == CarouselView.ArrowsTintColorProperty.PropertyName)
			{
				ArrowsTintColor();
			}
			else if (e.PropertyName == CarouselView.ArrowsTransparencyProperty.PropertyName)
			{
				ArrowsTransparency();
			}
		}

		void UpdateOrientation()
		{
			_orientationChanged = true;
			SetNativeView();
			Element.SendPositionSelected();
			Element.PositionSelectedCommand?.Execute(null);
		}

		void ArrowsTransparency()
		{
			if (_prevBtn == null || _nextBtn == null) return;
			_prevBtn.Alpha = Element.ArrowsTransparency;
			_nextBtn.Alpha = Element.ArrowsTransparency;
		}

		void ArrowsTintColor()
		{
			var prevArrow = (UIImageView)_prevBtn.Subviews[0];
			prevArrow.TintColor = Element.ArrowsTintColor.ToUIColor();
			var nextArrow = (UIImageView)_nextBtn.Subviews[0];
			nextArrow.TintColor = Element.ArrowsTintColor.ToUIColor();
		}

		void UpdateArrowsBackgroundColor()
		{
			if (_prevBtn == null || _nextBtn == null) return;
			_prevBtn.BackgroundColor = Element.ArrowsBackgroundColor.ToUIColor();
			_nextBtn.BackgroundColor = Element.ArrowsBackgroundColor.ToUIColor();
		}

		void UpdatePosition()
		{
			if (!_isChangingPosition)
			{
				SetCurrentPage(Element.Position);
			}
		}

		void UpdateItemTemplate()
		{
			SetNativeView();
			Element.SendPositionSelected();
			Element.PositionSelectedCommand?.Execute(null);
		}

		void UpdateItemsSource()
		{
			SetPosition();
			SetNativeView();
			Element.SendPositionSelected();
			Element.PositionSelectedCommand?.Execute(null);
			if (Element.ItemsSource != null && Element.ItemsSource is INotifyCollectionChanged)
				((INotifyCollectionChanged)Element.ItemsSource).CollectionChanged += ItemsSource_CollectionChanged;
		}

		async void ItemsSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			// NewItems contains the item that was added.
			// If NewStartingIndex is not -1, then it contains the index where the new item was added.
			if (e.Action == NotifyCollectionChangedAction.Add)
			{
				InsertPage(Element?.GetItem(e.NewStartingIndex), e.NewStartingIndex);
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
				if (Element == null && _pageController == null && Source == null) return;

				Source.RemoveAt(e.OldStartingIndex);
				Source.Insert(e.NewStartingIndex, e.OldItems[e.OldStartingIndex]);

				var firstViewController = CreateViewController(e.NewStartingIndex);

				_pageController.SetViewControllers(new[] { firstViewController }, UIPageViewControllerNavigationDirection.Forward, false, s =>
				{
					_isChangingPosition = true;
					Element.Position = e.NewStartingIndex;
					_isChangingPosition = false;

					SetArrowsVisibility();
					SetIndicatorsCurrentPage();

					Element.SendPositionSelected();
					Element.PositionSelectedCommand?.Execute(null);
				});
			}

			// NewItems contains the replacement item.
			// NewStartingIndex and OldStartingIndex are equal, and if they are not -1,
			// then they contain the index where the item was replaced.
			if (e.Action == NotifyCollectionChangedAction.Replace)
			{
				if (Element == null && _pageController == null && Source == null) return;

				// Remove controller from ChildViewControllers
				if (ChildViewControllers != null)
					ChildViewControllers.RemoveAll(c => c.Tag == Source[e.OldStartingIndex]);

				Source[e.OldStartingIndex] = e.NewItems[e.NewStartingIndex];

				var firstViewController = CreateViewController(Element.Position);

				_pageController.SetViewControllers(new[] { firstViewController }, UIPageViewControllerNavigationDirection.Forward, false, s =>
				{
				});
			}

			// No other properties are valid.
			if (e.Action == NotifyCollectionChangedAction.Reset)
			{
				if (Element == null) return;

				SetPosition();
				SetNativeView();
				Element.SendPositionSelected();
				Element.PositionSelectedCommand?.Execute(null);
			}
		}

		void Element_SizeChanged(object sender, EventArgs e)
		{
			if (Element == null) return;

			var rect = this.Element.Bounds;
			// To avoid extra DataTemplate instantiations #158
			if (rect.Height > 0)
			{
				_elementWidth = rect.Width;
				_elementHeight = rect.Height;
				SetNativeView();
				Element.SendPositionSelected();
				Element.PositionSelectedCommand?.Execute(null);
			}
		}

		void Scroller_Scrolled(object sender, EventArgs e)
		{
			var scrollView = (UIScrollView)sender;
			var point = scrollView.ContentOffset;

			double currentPercentCompleted;
			ScrollDirection direction;

			if (Element.Orientation == CarouselViewOrientation.Horizontal)
			{
				currentPercentCompleted = Math.Floor((Math.Abs(point.X - _pageController.View.Frame.Size.Width) / _pageController.View.Frame.Size.Width) * 100);
				direction = _prevPoint > point.X ? ScrollDirection.Left : ScrollDirection.Right;
				_prevPoint = point.X;
			}
			else
			{
				currentPercentCompleted = Math.Floor((Math.Abs(point.Y - _pageController.View.Frame.Size.Height) / _pageController.View.Frame.Size.Height) * 100);
				direction = _prevPoint > point.Y ? ScrollDirection.Up : ScrollDirection.Down;
				_prevPoint = point.Y;
			}


			if (currentPercentCompleted <= 100 && currentPercentCompleted > _percentCompleted)
			{
				Element.SendScrolled(currentPercentCompleted, direction);
				_percentCompleted = currentPercentCompleted;
			}
			else
			{
				_percentCompleted = 0;
			}
		}

		void PageController_DidFinishAnimating(object sender, UIPageViewFinishedAnimationEventArgs e)
		{
			if (e.Completed)
			{
				var controller = (FormsUIViewContainer)_pageController.ViewControllers[0];
				var position = Source.IndexOf(controller.Tag);
				_isChangingPosition = true;
				Element.Position = position;
				_prevPosition = position;
				_isChangingPosition = false;
				SetArrowsVisibility();
				SetIndicatorsCurrentPage();
				Element.SendPositionSelected();
				Element.PositionSelectedCommand?.Execute(null);
			}
		}

		void SetNativeView()
		{
			// Rotation bug(iOS) #115 Fix
			CleanUpPageController();

			if (_orientationChanged)
			{
				var interPageSpacing = (float)Element.InterPageSpacing;

				// Orientation BP
				var orientation = (UIPageViewControllerNavigationOrientation)Element.Orientation;

				// InterPageSpacing BP
				_pageController = new UIPageViewController(UIPageViewControllerTransitionStyle.Scroll,
														  orientation, UIPageViewControllerSpineLocation.None, interPageSpacing);

				_pageController.View.ClipsToBounds = true;
			}

			Source = Element.ItemsSource != null ? new List<object>(Element.GetList()) : null;

			// BackgroundColor BP
			_pageController.View.BackgroundColor = Element.BackgroundColor.ToUIColor();

			_pageController.GetPreviousViewController = (pageViewController, referenceViewController) =>
			{
				var controller = (FormsUIViewContainer)referenceViewController;

				if (controller != null)
				{
					var position = Source.IndexOf(controller.Tag);

					// Determine if we are on the first page
					if (position == 0)
					{
						// We are on the first page, so there is no need for a controller before that
						return null;
					}
					else
					{
						int previousPageIndex = position - 1;
						return CreateViewController(previousPageIndex);
					}
				}
				else
				{
					return null;
				}
			};

			_pageController.GetNextViewController = (pageViewController, referenceViewController) =>
			{
				var controller = (FormsUIViewContainer)referenceViewController;

				if (controller != null)
				{
					var position = Source.IndexOf(controller.Tag);

					// Determine if we are on the last page
					if (position == Count - 1)
					{
						// We are on the last page, so there is no need for a controller after that
						return null;
					}
					else
					{
						int nextPageIndex = position + 1;
						return CreateViewController(nextPageIndex);
					}
				}
				else
				{
					return null;
				}
			};

			_pageController.DidFinishAnimating += PageController_DidFinishAnimating;

			// IsSwipeEnabled BP
			SetIsSwipeEnabled();

			if (Source != null && Source?.Count > 0)
			{
				var firstViewController = CreateViewController(Element.Position);

				_pageController.SetViewControllers(new[] { firstViewController }, UIPageViewControllerNavigationDirection.Forward, false, s =>
				{
				});
			}

			SetNativeControl(_pageController.View);

			// ARROWS
			SetArrows();

			// INDICATORS
			SetIndicators();
		}

		void SetIsSwipeEnabled()
		{
			foreach (var view in _pageController?.View.Subviews)
			{
				var scroller = view as UIScrollView;
				if (scroller != null)
				{
					scroller.ScrollEnabled = Element.IsSwipeEnabled;
					scroller.Scrolled += Scroller_Scrolled;
				}
			}
		}

		void SetPosition()
		{
			_isChangingPosition = true;
			if (Element.ItemsSource != null)
			{
				var elementCount = Element.GetCount();
				if (Element.Position > elementCount - 1)
					Element.Position = elementCount - 1;
				if (Element.Position == -1)
					Element.Position = 0;
			}
			else
			{
				Element.Position = 0;
			}
			_prevPosition = Element.Position;
			_isChangingPosition = false;
		}

		void SetArrows()
		{
			CleanUpArrows();

			if (Element.ShowArrows)
			{
				var o = Element.Orientation == CarouselViewOrientation.Horizontal ? "H" : "V";
				var formatOptions = Element.Orientation == CarouselViewOrientation.Horizontal ? NSLayoutFormatOptions.AlignAllCenterY : NSLayoutFormatOptions.AlignAllCenterX;

				var elementCount = Element.GetCount();

				_prevBtn = new UIButton();
				_prevBtn.Hidden = Element.Position == 0 || elementCount == 0;
				_prevBtn.BackgroundColor = Element.ArrowsBackgroundColor.ToUIColor();
				_prevBtn.Alpha = Element.ArrowsTransparency;
				_prevBtn.TranslatesAutoresizingMaskIntoConstraints = false;

				var prevArrow = new UIImageView();
				var prevArrowImage = new UIImage(Element.Orientation == CarouselViewOrientation.Horizontal ? "Prev.png" : "Up.png");
				prevArrow.Image = prevArrowImage.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
				prevArrow.TranslatesAutoresizingMaskIntoConstraints = false;
				prevArrow.TintColor = Element.ArrowsTintColor.ToUIColor();
				_prevBtn.AddSubview(prevArrow);

				_prevBtn.TouchUpInside += PrevBtn_TouchUpInside;

				var prevViewsDictionary = NSDictionary.FromObjectsAndKeys(new NSObject[] { _prevBtn, prevArrow }, new NSObject[] { new NSString("superview"), new NSString("prevArrow") });
				_prevBtn.AddConstraints(NSLayoutConstraint.FromVisualFormat("[prevArrow(==17)]", 0, new NSDictionary(), prevViewsDictionary));
				_prevBtn.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:[prevArrow(==17)]", 0, new NSDictionary(), prevViewsDictionary));
				_prevBtn.AddConstraints(NSLayoutConstraint.FromVisualFormat(o + ":[prevArrow]-(2)-|", 0, new NSDictionary(), prevViewsDictionary));
				_prevBtn.AddConstraints(NSLayoutConstraint.FromVisualFormat(o + ":[superview]-(<=1)-[prevArrow]", formatOptions, new NSDictionary(), prevViewsDictionary));

				_pageController.View.AddSubview(_prevBtn);

				_nextBtn = new UIButton();
				_nextBtn.Hidden = Element.Position == elementCount - 1 || elementCount == 0;
				_nextBtn.BackgroundColor = Element.ArrowsBackgroundColor.ToUIColor();
				_nextBtn.Alpha = Element.ArrowsTransparency;
				_nextBtn.TranslatesAutoresizingMaskIntoConstraints = false;

				var nextArrow = new UIImageView();
				var nextArrowImage = new UIImage(Element.Orientation == CarouselViewOrientation.Horizontal ? "Next.png" : "Down.png");
				nextArrow.Image = nextArrowImage.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
				nextArrow.TranslatesAutoresizingMaskIntoConstraints = false;
				nextArrow.TintColor = Element.ArrowsTintColor.ToUIColor();
				_nextBtn.AddSubview(nextArrow);

				_nextBtn.TouchUpInside += NextBtn_TouchUpInside;

				var nextViewsDictionary = NSDictionary.FromObjectsAndKeys(new NSObject[] { _nextBtn, nextArrow }, new NSObject[] { new NSString("superview"), new NSString("nextArrow") });
				_nextBtn.AddConstraints(NSLayoutConstraint.FromVisualFormat("[nextArrow(==17)]", 0, new NSDictionary(), nextViewsDictionary));
				_nextBtn.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:[nextArrow(==17)]", 0, new NSDictionary(), nextViewsDictionary));
				_nextBtn.AddConstraints(NSLayoutConstraint.FromVisualFormat(o + ":|-(2)-[nextArrow]", 0, new NSDictionary(), nextViewsDictionary));
				_nextBtn.AddConstraints(NSLayoutConstraint.FromVisualFormat(o + ":[superview]-(<=1)-[nextArrow]", formatOptions, new NSDictionary(), nextViewsDictionary));

				_pageController.View.AddSubview(_nextBtn);

				var btnsDictionary = NSDictionary.FromObjectsAndKeys(new NSObject[] { _pageController.View, _prevBtn, _nextBtn }, new NSObject[] { new NSString("superview"), new NSString("prevBtn"), new NSString("nextBtn") });

				var w = Element.Orientation == CarouselViewOrientation.Horizontal ? 20 : 36;
				var h = Element.Orientation == CarouselViewOrientation.Horizontal ? 36 : 20;

				_pageController.View.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:[prevBtn(==" + w + ")]", 0, new NSDictionary(), btnsDictionary));
				_pageController.View.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:[prevBtn(==" + h + ")]", 0, new NSDictionary(), btnsDictionary));
				_pageController.View.AddConstraints(NSLayoutConstraint.FromVisualFormat(o + ":|[prevBtn]", 0, new NSDictionary(), btnsDictionary));
				_pageController.View.AddConstraints(NSLayoutConstraint.FromVisualFormat(o + ":[superview]-(<=1)-[prevBtn]", formatOptions, new NSDictionary(), btnsDictionary));

				_pageController.View.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:[nextBtn(==" + w + ")]", 0, new NSDictionary(), btnsDictionary));
				_pageController.View.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:[nextBtn(==" + h + ")]", 0, new NSDictionary(), btnsDictionary));
				_pageController.View.AddConstraints(NSLayoutConstraint.FromVisualFormat(o + ":[nextBtn]|", 0, new NSDictionary(), btnsDictionary));
				_pageController.View.AddConstraints(NSLayoutConstraint.FromVisualFormat(o + ":[superview]-(<=1)-[nextBtn]", formatOptions, new NSDictionary(), btnsDictionary));
			}
		}

		void PrevBtn_TouchUpInside(object sender, EventArgs e)
		{
			if (Element.Position > 0)
				Element.Position = Element.Position - 1;
		}

		void NextBtn_TouchUpInside(object sender, EventArgs e)
		{
			if (Element.Position < Element.GetCount() - 1)
				Element.Position = Element.Position + 1;
		}

		void SetArrowsVisibility()
		{
			if (_prevBtn == null || _nextBtn == null) return;
			var elementCount = Element?.GetCount();
			_prevBtn.Hidden = Element.Position == 0 || elementCount == 0;
			_nextBtn.Hidden = Element.Position == elementCount - 1 || elementCount == 0;
		}

		void SetIndicators()
		{
			if (Element.ShowIndicators)
			{
				_pageControl = new UIPageControl();
				_pageControl.TranslatesAutoresizingMaskIntoConstraints = false;
				_pageControl.Enabled = false;
				_pageController.View.AddSubview(_pageControl);
				var viewsDictionary = NSDictionary.FromObjectsAndKeys(new NSObject[] { _pageControl }, new NSObject[] { new NSString("pageControl") });
				if (Element.Orientation == CarouselViewOrientation.Horizontal)
				{
					_pageController.View.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[pageControl]|", NSLayoutFormatOptions.AlignAllCenterX, new NSDictionary(), viewsDictionary));
					_pageController.View.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:[pageControl]|", 0, new NSDictionary(), viewsDictionary));
				}
				else
				{
					_pageControl.Transform = CGAffineTransform.MakeRotation(3.14159265f / 2);

					_pageController.View.AddConstraints(NSLayoutConstraint.FromVisualFormat("[pageControl(==36)]", 0, new NSDictionary(), viewsDictionary));
					_pageController.View.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:[pageControl]|", 0, new NSDictionary(), viewsDictionary));
					_pageController.View.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|[pageControl]|", NSLayoutFormatOptions.AlignAllTop, new NSDictionary(), viewsDictionary));
				}

				_pageControl.Pages = Count;
				_pageControl.PageIndicatorTintColor = Element.IndicatorsTintColor.ToUIColor();
				_pageControl.CurrentPageIndicatorTintColor = Element.CurrentPageIndicatorTintColor.ToUIColor();
				_pageControl.CurrentPage = Element.Position;
				SetIndicatorsShape();
			}
			else
			{
				CleanUpPageControl();
			}
		}

		void SetIndicatorsTintColor()
		{
			if (_pageControl == null) return;

			_pageControl.PageIndicatorTintColor = Element.IndicatorsTintColor.ToUIColor();
			SetIndicatorsShape();
		}

		void SetCurrentPageIndicatorTintColor()
		{
			if (_pageControl == null) return;

			_pageControl.CurrentPageIndicatorTintColor = Element.CurrentPageIndicatorTintColor.ToUIColor();
			SetIndicatorsShape();
		}

		void SetIndicatorsCurrentPage()
		{
			if (_pageControl == null) return;

			_pageControl.Pages = Count;
			_pageControl.CurrentPage = Element.Position;
			SetIndicatorsShape();

		}

		void SetIndicatorsShape()
		{
			if (_pageControl == null) return;

			if (Element.IndicatorsShape == IndicatorsShape.Square)
			{
				foreach (var view in _pageControl.Subviews)
				{
					if (view.Frame.Width == 7)
					{
						view.Layer.CornerRadius = 0;
						var frame = new CGRect(view.Frame.X, view.Frame.Y, view.Frame.Width - 1, view.Frame.Height - 1);
						view.Frame = frame;
					}
				}
			}
			else
			{
				foreach (var view in _pageControl.Subviews)
				{
					if (view.Frame.Width == 6)
					{
						view.Layer.CornerRadius = 3.5f;
						var frame = new CGRect(view.Frame.X, view.Frame.Y, view.Frame.Width + 1, view.Frame.Height + 1);
						view.Frame = frame;
					}
				}
			}
		}

		void InsertPage(object item, int position)
		{
			if (Element == null || _pageController == null || Source == null) return;

			Source.Insert(position, item);

			// Because we maybe inserting into an empty PageController
			UIViewController firstViewController;
			if (_pageController.ViewControllers.Count() > 0)
				firstViewController = _pageController.ViewControllers[0];
			else
				firstViewController = CreateViewController(Element.Position);

			_pageController.SetViewControllers(new[] { firstViewController }, UIPageViewControllerNavigationDirection.Forward, false, s =>
			{
				//var prevPos = Element.Position;

				// To keep the same view visible when inserting in a position <= current (like Android ViewPager)
				_isChangingPosition = true;
				if (position <= Element.Position && Source.Count > 1)
				{
					Element.Position++;
					_prevPosition = Element.Position;
				}
				_isChangingPosition = false;

				SetArrowsVisibility();
				SetIndicatorsCurrentPage();

				//if (position != prevPos)
				//{
				Element.SendPositionSelected();
				Element.PositionSelectedCommand?.Execute(null);
				//}
			});
		}

		async Task RemovePageAsync(int position)
		{
			if (Element == null || _pageController == null || Source == null) return;

			if (Source?.Count > 0)
			{
				// To remove latest page, rebuild pageController or the page wont disappear
				if (Source.Count == 1)
				{
					Source.RemoveAt(position);
					SetNativeView();
				}
				else
				{
					// Remove controller from ChildViewControllers
					if (ChildViewControllers != null)
						ChildViewControllers.RemoveAll(c => c.Tag == Source[position]);

					Source.RemoveAt(position);

					// To remove current page
					if (position == Element.Position)
					{
						var newPos = position - 1;
						if (newPos == -1)
							newPos = 0;

						// With a swipe transition
						if (Element.AnimateTransition)
							await Task.Delay(100);

						var direction = position == 0 ? UIPageViewControllerNavigationDirection.Forward : UIPageViewControllerNavigationDirection.Reverse;
						var firstViewController = CreateViewController(newPos);

						_pageController.SetViewControllers(new[] { firstViewController }, direction, Element.AnimateTransition, s =>
						{
							_isChangingPosition = true;
							Element.Position = newPos;
							_isChangingPosition = false;

							SetArrowsVisibility();
							SetIndicatorsCurrentPage();

							// Invoke PositionSelected as DidFinishAnimating is only called when touch to swipe
							Element.SendPositionSelected();
							Element.PositionSelectedCommand?.Execute(null);
						});
					}
					else
					{
						var firstViewController = _pageController.ViewControllers[0];

						_pageController.SetViewControllers(new[] { firstViewController }, UIPageViewControllerNavigationDirection.Forward, false, s =>
						{
							SetArrowsVisibility();
							SetIndicatorsCurrentPage();

							// Invoke PositionSelected as DidFinishAnimating is only called when touch to swipe
							Element.SendPositionSelected();
							Element.PositionSelectedCommand?.Execute(null);
						});
					}
				}

				_prevPosition = Element.Position;
			}
		}

		void SetCurrentPage(int position)
		{
			var elementCount = Element?.GetCount();
			if (position < 0 || position > elementCount - 1)
				return;

			if (Element == null || _pageController == null || Element.ItemsSource == null) return;

			if (elementCount > 0)
			{
				// Transition direction based on prevPosition
				var direction = position >= _prevPosition ? UIPageViewControllerNavigationDirection.Forward : UIPageViewControllerNavigationDirection.Reverse;
				_prevPosition = position;

				var firstViewController = CreateViewController(position);

				_pageController.SetViewControllers(new[] { firstViewController }, direction, Element.AnimateTransition, s =>
				{
					SetArrowsVisibility();
					SetIndicatorsCurrentPage();

					// Invoke PositionSelected as DidFinishAnimating is only called when touch to swipe
					Element.SendPositionSelected();
					Element.PositionSelectedCommand?.Execute(null);
				});
			}
		}

		UIViewController CreateViewController(int index)
		{
			// Significant Memory Leak for iOS when using custom layout for page content #125
			var newTag = Source[index];
			foreach (FormsUIViewContainer child in _pageController.ChildViewControllers)
			{
				if (child.Tag == newTag)
					return child;
			}

			View formsView = null;

			object bindingContext = null;

			if (Source != null && Source?.Count > 0)
				bindingContext = Source.Cast<object>().ElementAt(index);

			var dt = bindingContext as DataTemplate;
			// Support for List<View> as ItemsSource
			var view = bindingContext as View;

			// Support for List<DataTemplate> as ItemsSource
			if (dt != null)
			{
				formsView = (View)dt.CreateContent();
			}
			else
			{
				if (view != null)
				{
					if (ChildViewControllers == null)
						ChildViewControllers = new List<FormsUIViewContainer>();

					// Return from the local copy of controllers
					foreach (FormsUIViewContainer controller in ChildViewControllers)
					{
						if (controller.Tag == view)
						{
							return controller;
						}
					}

					formsView = view;
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
			}

			// HeightRequest fix
			formsView.Parent = this.Element;

			// UIScreen.MainScreen.Bounds.Width, UIScreen.MainScreen.Bounds.Height
			var rect = new CGRect(Element.X, Element.Y, _elementWidth, _elementHeight);
			var nativeConverted = formsView.ToiOS(rect);

			//if (dt == null && view == null)
			//formsView.Parent = null;

			var viewController = new FormsUIViewContainer();
			viewController.Tag = bindingContext;
			viewController.View = nativeConverted;

			// Only happens when ItemsSource is List<View>
			if (ChildViewControllers != null)
			{
				ChildViewControllers.Add(viewController);
			}

			return viewController;
		}

		void CleanUpArrows()
		{
			if (_prevBtn != null)
			{
				_prevBtn.TouchUpInside -= PrevBtn_TouchUpInside;
				_prevBtn.RemoveFromSuperview();
				_prevBtn.Dispose();
				_prevBtn = null;
			}

			if (_nextBtn != null)
			{
				_nextBtn.TouchUpInside -= NextBtn_TouchUpInside;
				_nextBtn.RemoveFromSuperview();
				_nextBtn.Dispose();
				_nextBtn = null;
			}
		}

		void CleanUpPageControl()
		{
			if (_pageControl == null) return;

			_pageControl.RemoveFromSuperview();
			_pageControl.Dispose();
			_pageControl = null;
		}

		void CleanUpPageController()
		{
			CleanUpPageControl();

			if (_pageController == null) return;

			foreach (var child in _pageController.ChildViewControllers)
				child.Dispose();

			foreach (var child in _pageController.ViewControllers)
				child.Dispose();

			// Cleanup ChildViewControllers
			if (ChildViewControllers != null)
			{
				foreach (var child in ChildViewControllers)
				{
					child.Dispose();
				}

				ChildViewControllers = null;
			}
		}

		class FormsUIViewContainer : UIViewController
		{
			// To save current position
			public object Tag { get; set; }

			protected override void Dispose(bool disposing)
			{
				// because this runs in the finalizer thread and disposing is equal false
				InvokeOnMainThread(() => {

					WillMoveToParentViewController(null);

					// Significant Memory Leak for iOS when using custom layout for page content #125
					foreach (var view in View.Subviews)
					{
						view.RemoveFromSuperview();
						view.Dispose();
					}

					View.RemoveFromSuperview();
					View.Dispose();
					View = null;

					RemoveFromParentViewController();
				});

				base.Dispose(disposing);
			}
		}
	}
}