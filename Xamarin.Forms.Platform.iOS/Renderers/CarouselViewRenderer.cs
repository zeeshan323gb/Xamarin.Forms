using System;
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
		bool _disposed;
		double _ElementHeight;
		double _ElementWidth;
		// To avoid triggering Position changed
		bool _isSwiping;
		UIPageControl _pageControl;
		UIPageViewController _pageController;
		int _prevPosition;

		int Count
		{
			get
			{
				return Element.ItemsSource?.Count ?? 0;
			}
		}

		public void InsertController(object item, int position)
		{
			if (Element != null && _pageController != null && Element.ItemsSource != null)
			{
				if (position > Element.ItemsSource.Count + 1)
					throw new CarouselViewException("Page cannot be inserted at a position bigger than ItemsSource.Count");

				if (position == -1)
					Element.ItemsSource.Add(item);
				else
					Element.ItemsSource.Insert(position, item);

				UIViewController firstViewController;
				if (_pageController.ViewControllers.Count() > 0)
					firstViewController = _pageController.ViewControllers[0];
				else
					firstViewController = CreateViewController(0);

				_pageController.SetViewControllers(new[] { firstViewController }, UIPageViewControllerNavigationDirection.Forward, false, async s =>
				{
					ConfigurePageControl();

					await Task.Delay(100);
				});
			}
		}

		public async void RemoveController(int position)
		{
			if (Element != null && _pageController != null && Element.ItemsSource != null && Element.ItemsSource?.Count > 0)
			{
				if (position > Element.ItemsSource.Count - 1)
					throw new CarouselViewException("Page cannot be removed at a position bigger than ItemsSource.Count - 1");

				if (Element.ItemsSource?.Count == 1)
				{
					Element.ItemsSource.RemoveAt(position);
					ConfigurePageController();
					AttachEvents();
					ConfigurePageControl();
				}
				else
				{
					Element.ItemsSource.RemoveAt(position);

					if (position == Element.Position)
					{
						var newPos = position - 1;
						if (newPos == -1)
							newPos = 0;

						//TODO: maybe move this to SetViewControllers to match InsertController
						await Task.Delay(100);
						var direction = position == 0 ? UIPageViewControllerNavigationDirection.Forward : UIPageViewControllerNavigationDirection.Reverse;
						var firstViewController = CreateViewController(newPos);
						_pageController.SetViewControllers(new[] { firstViewController }, direction, Element.AnimateTransition, s =>
						{
							_isSwiping = true;
							Element.Position = newPos;
							_isSwiping = false;

							ConfigurePageControl();

							Element.PositionSelected?.Invoke(Element, EventArgs.Empty);
						});
					}
					else
					{
						var firstViewController = _pageController.ViewControllers[0];
						_pageController.SetViewControllers(new[] { firstViewController }, UIPageViewControllerNavigationDirection.Forward, false, s =>
						{
							ConfigurePageControl();

							Element.PositionSelected?.Invoke(Element, EventArgs.Empty);
						});
					}
				}
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && !_disposed)
			{
				if (_pageController != null)
				{
					_pageController.GetPresentationCount = null;
					_pageController.GetPresentationIndex = null;
					_pageController.DidFinishAnimating -= PageController_DidFinishAnimating;
					_pageController.GetPreviousViewController = null;
					_pageController.GetNextViewController = null;

					foreach (var child in _pageController.ViewControllers)
						child.Dispose();

					_pageController.Dispose();
					_pageController = null;
				}

				if (_pageControl != null)
				{
					_pageControl.Dispose();
					_pageControl = null;
				}

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

				ConfigPosition();
				ConfigurePageController();
			}

			if (e.OldElement != null)
			{
				// Unsubscribe from event handlers and cleanup any resources

				if (_pageController != null)
				{
					_pageController.GetPresentationCount = null;
					_pageController.GetPresentationIndex = null;

					_pageController.DidFinishAnimating -= PageController_DidFinishAnimating;

					_pageController.GetPreviousViewController = null;
					_pageController.GetNextViewController = null;
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

				AttachEvents();

				Element.RemoveAction = new Action<int>(RemoveController);
				Element.InsertAction = new Action<object, int>(InsertController);
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			var rect = Element.Bounds;

			if (e.PropertyName == CarouselView.WidthProperty.PropertyName)
			{
				_ElementWidth = rect.Width;
			}
			else if (e.PropertyName == CarouselView.HeightProperty.PropertyName)
			{
				_ElementHeight = rect.Height;
				if (Element != null && _pageController != null && Element.ItemsSource != null && Element.ItemsSource?.Count > 0)
				{
					var firstViewController = CreateViewController(Element.Position);
					_pageController.SetViewControllers(new[] { firstViewController }, UIPageViewControllerNavigationDirection.Forward, false, s => { });
					ConfigurePageControl();
				}
			}
			else if (e.PropertyName == CarouselView.ShowIndicatorsProperty.PropertyName)
			{
				_pageControl.Hidden = !Element.ShowIndicators;
			}
			else if (e.PropertyName == CarouselView.ItemsSourceProperty.PropertyName)
			{
				// TODO: don't execute the first time
				if (Element != null && _pageController != null)
				{
					ConfigPosition();
					ConfigurePageController();

					AttachEvents();

					if (Element.ItemsSource != null && Element.ItemsSource?.Count > 0)
					{
						var secondViewController = CreateViewController(Element.Position);
						_pageController.SetViewControllers(new[] { secondViewController }, UIPageViewControllerNavigationDirection.Forward, false, s => { });
					}

					ConfigurePageControl();

					Element.PositionSelected?.Invoke(Element, EventArgs.Empty);
				}
			}
			else if (e.PropertyName == CarouselView.PositionProperty.PropertyName)
			{
				if (Element.Position != -1 && !_isSwiping)
					SetCurrentController(Element.Position);
			}
		}

		static UIView AddView(View view, CGRect size)
		{
			if (Platform.GetRenderer(view) == null)
				Platform.SetRenderer(view, Platform.CreateRenderer(view));
			var vRenderer = Platform.GetRenderer(view);

			vRenderer.NativeView.Frame = size;

			vRenderer.NativeView.AutoresizingMask = UIViewAutoresizing.All;
			vRenderer.NativeView.ContentMode = UIViewContentMode.ScaleToFill;

			vRenderer.Element.Layout(size.ToRectangle());

			var nativeView = vRenderer.NativeView;

			nativeView.SetNeedsLayout();

			return nativeView;
		}

		void AttachEvents()
		{
			_pageController.DidFinishAnimating += PageController_DidFinishAnimating;

			_pageController.GetPreviousViewController = (pageViewController, referenceViewController) =>
			{
				var controller = (ViewContainer)referenceViewController;

				if (controller != null)
				{
					var position = controller.Tag;

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
				var controller = (ViewContainer)referenceViewController;

				if (controller != null)
				{
					var position = controller.Tag;

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
		}

		void ConfigPosition()
		{
			_isSwiping = true;
			if (Element.ItemsSource != null)
			{
				if (Element.Position > Element.ItemsSource.Count - 1)
					Element.Position = Element.ItemsSource.Count - 1;

				if (Element.Position == -1)
					Element.Position = 0;
			}
			else
			{
				Element.Position = 0;
			}
			_isSwiping = false;
		}

		void ConfigurePageControl()
		{
			if (Element != null && _pageControl != null)
			{
				_pageControl.Pages = Count;
				_pageControl.CurrentPage = Element.Position;

				if (Element.IndicatorsShape == CarouselViewIndicatorsShape.Square)
				{
					foreach (var view in _pageControl.Subviews)
					{
						view.Layer.CornerRadius = 0;
						if (view.Frame.Width == 7)
						{
							var frame = new CGRect(view.Frame.X, view.Frame.Y, view.Frame.Width - 1, view.Frame.Height - 1);
							view.Frame = frame;
						}
					}
				}
			}
		}

		void ConfigurePageController()
		{
			var interPageSpacing = (float)Element.InterPageSpacing;
			var orientation = (UIPageViewControllerNavigationOrientation)Element.Orientation;
			_pageController = new UIPageViewController(UIPageViewControllerTransitionStyle.Scroll,
													  orientation, UIPageViewControllerSpineLocation.None, interPageSpacing);
			_pageController.View.BackgroundColor = Element.InterPageSpacingColor.ToUIColor();

			_pageControl = new UIPageControl();
			_pageControl.PageIndicatorTintColor = Element.PageIndicatorTintColor.ToUIColor();
			_pageControl.CurrentPageIndicatorTintColor = Element.CurrentPageIndicatorTintColor.ToUIColor();
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
			_pageControl.Hidden = !Element.ShowIndicators;

			foreach (var view in _pageController.View.Subviews)
			{
				var scroller = view as UIScrollView;
				if (scroller != null)
				{
					scroller.Bounces = Element.Bounces;
				}
			}

			SetNativeControl(_pageController.View);
		}

		UIViewController CreateViewController(int index)
		{
			_prevPosition = index;

			View formsView = null;

			object bindingContext = null;

			if (Element.ItemsSource != null && Element.ItemsSource?.Count > 0)
				bindingContext = Element.ItemsSource.Cast<object>().ElementAt(index);

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

			var rect = new CGRect(Element.X, Element.Y, _ElementWidth, _ElementHeight);
			var nativeConverted = AddView(formsView, rect);

			var viewController = new ViewContainer();
			viewController.Tag = index;
			viewController.View = nativeConverted;

			return viewController;
		}

		void PageController_DidFinishAnimating(object sender, UIPageViewFinishedAnimationEventArgs e)
		{
			if (e.Finished)
			{
				var controller = (ViewContainer)_pageController.ViewControllers[0];
				var position = controller.Tag;

				_isSwiping = true;
				Element.Position = position;
				_isSwiping = false;

				ConfigurePageControl();

				Element.PositionSelected?.Invoke(Element, EventArgs.Empty);
			}
		}

		void SetCurrentController(int position)
		{
			if (Element != null && _pageController != null && Element.ItemsSource != null && Element.ItemsSource?.Count > 0)
			{
				if (position > Element.ItemsSource.Count - 1)
					throw new CarouselViewException("Current page index cannot be bigger than ItemsSource.Count - 1");

				var direction = position > _prevPosition ? UIPageViewControllerNavigationDirection.Forward : UIPageViewControllerNavigationDirection.Reverse;

				var firstViewController = CreateViewController(position);
				_pageController.SetViewControllers(new[] { firstViewController }, direction, Element.AnimateTransition, s =>
				{
					ConfigurePageControl();

					Element.PositionSelected?.Invoke(Element, EventArgs.Empty);
				});
			}
		}
	}

	public class ViewContainer : UIViewController
	{
		public int Tag { get; set; }
	}
}