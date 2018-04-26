using CoreGraphics;
using MediaPlayer;
using System;
using System.ComponentModel;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public class ShellFlyoutRenderer : UIViewController, IShellFlyoutRenderer
	{
		private bool _disposed;
		private bool _gestureActive;
		private bool _isOpen;

		public UIViewAnimationCurve AnimationCurve { get; set; } = UIViewAnimationCurve.EaseOut;

		public int AnimationDuration { get; set; } = 250;

		public IShellFlyoutTransition FlyoutTransition { get; set; }

		UIView IShellFlyoutRenderer.View => View;

		UIViewController IShellFlyoutRenderer.ViewController => this;

		private IShellContext Context { get; set; }

		private UIViewController Detail { get; set; }

		private IShellFlyoutContentRenderer Flyout { get; set; }

		private nfloat FlyoutWidth => (nfloat)(Math.Min(View.Frame.Width, View.Frame.Height) * 0.8);

		private bool IsOpen
		{
			get { return _isOpen; }
			set
			{
				if (_isOpen == value)
					return;

				_isOpen = value;
				Shell.SetValueFromRenderer(Shell.FlyoutIsPresentedProperty, value);
			}
		}

		private UIPanGestureRecognizer PanGestureRecognizer { get; set; }

		private Shell Shell { get; set; }

		private UIView TapoffView { get; set; }

		public void AttachFlyout(IShellContext context, UIViewController detail)
		{
			Context = context;
			Shell = Context.Shell;
			Detail = detail;

			Shell.PropertyChanged += OnShellPropertyChanged;

			PanGestureRecognizer = new UIPanGestureRecognizer(HandlePanGesture);
			PanGestureRecognizer.ShouldReceiveTouch += (sender, touch) =>
			{
				if (!context.AllowFlyoutGesture)
					return false;
				var view = View;
				CGPoint loc = touch.LocationInView(View);
				if (touch.View is UISlider ||
					touch.View is MPVolumeView ||
					(loc.X > view.Frame.Width * 0.1 && !IsOpen))
					return false;
				return true;
			};
		}

		public void CloseFlyout()
		{
			IsOpen = false;
			LayoutSidebar(true);
		}

		public override void ViewDidLayoutSubviews()
		{
			base.ViewDidLayoutSubviews();

			Detail.View.Frame = View.Bounds;
			LayoutSidebar(false);
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			AddChildViewController(Detail);
			View.AddSubview(Detail.View);

			Flyout = Context.CreateShellFlyoutContentRenderer();
			AddChildViewController(Flyout.ViewController);
			View.AddSubview(Flyout.ViewController.View);
			View.AddGestureRecognizer(PanGestureRecognizer);
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing)
			{
				if (!_disposed)
				{
					_disposed = true;

					Context = null;
					Shell = null;
					Detail = null;
				}
			}
		}

		protected virtual void OnShellPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Shell.FlyoutIsPresentedProperty.PropertyName)
			{
				var isPresented = Shell.FlyoutIsPresented;
				if (IsOpen != isPresented)
				{
					IsOpen = isPresented;
					LayoutSidebar(true);
				}
			}
		}

		private void AddTapoffView()
		{
			if (TapoffView != null)
				return;

			TapoffView = new UIView(View.Bounds);
			View.InsertSubviewBelow(TapoffView, Flyout.ViewController.View);
			TapoffView.AddGestureRecognizer(new UITapGestureRecognizer(t =>
			{
				IsOpen = false;
				LayoutSidebar(true);
			}));
		}

		private void HandlePanGesture(UIPanGestureRecognizer pan)
		{
			var translation = pan.TranslationInView(View).X;
			double openProgress = 0;
			double openLimit = Flyout.ViewController.View.Frame.Width;

			if (IsOpen)
			{
				openProgress = 1 - (-translation / openLimit);
			}
			else
			{
				openProgress = translation / openLimit;
			}

			openProgress = Math.Min(Math.Max(openProgress, 0.0), 1.0);
			var openPixels = openLimit * openProgress;

			switch (pan.State)
			{
				case UIGestureRecognizerState.Changed:
					_gestureActive = true;
					FlyoutTransition.LayoutViews((nfloat)openProgress, Flyout.ViewController.View, Detail.View, FlyoutWidth);
					break;

				case UIGestureRecognizerState.Ended:
					_gestureActive = false;
					if (IsOpen)
					{
						if (openProgress < .8)
							IsOpen = false;
					}
					else
					{
						if (openProgress > 0.2)
						{
							IsOpen = true;
						}
					}
					LayoutSidebar(true);
					break;
			}
		}

		private void LayoutSidebar(bool animate)
		{
			if (_gestureActive)
				return;

			if (animate)
			{
				UIView.BeginAnimations("Flyout");
			}
			FlyoutTransition.LayoutViews(IsOpen ? 1 : 0, Flyout.ViewController.View, Detail.View, FlyoutWidth);
			if (animate)
			{
				UIView.SetAnimationCurve(AnimationCurve);
				UIView.SetAnimationDuration(AnimationDuration);
				UIView.CommitAnimations();
				View.LayoutIfNeeded();
			}

			if (IsOpen)
				AddTapoffView();
			else
				RemoveTapoffView();
		}

		private void RemoveTapoffView()
		{
			if (TapoffView == null)
				return;

			TapoffView.RemoveFromSuperview();
			TapoffView = null;
		}
	}
}