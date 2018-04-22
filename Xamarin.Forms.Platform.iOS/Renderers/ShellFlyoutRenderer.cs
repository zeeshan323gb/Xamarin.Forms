using CoreGraphics;
using MediaPlayer;
using System;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{

	public class ShellFlyoutRenderer : IShellFlyoutRenderer
	{
		public IShellFlyoutTransition FlyoutTransition { get; set; }
		public UIViewAnimationCurve AnimationCurve { get; set; } = UIViewAnimationCurve.EaseOut;
		public int AnimationDuration { get; set; } = 250;

		Shell Shell { get; set; }
		IShellContext Context { get; set; }
		IShellFlyoutContentRenderer Flyout { get; set; }
		UIPanGestureRecognizer PanGestureRecognizer { get; set; }
		UIViewController Detail => Context.CurrentShellItemRenderer.ViewController;
		nfloat FlyoutWidth => (nfloat)(Math.Min(Context.ViewController.View.Frame.Width, Context.ViewController.View.Frame.Height) * 0.8);
		UIView TapoffView { get; set; }

		private bool _isOpen;
		private bool _gestureActive;
		private bool _disposed;

		public void AttachFlyout(IShellContext context)
		{
			Context = context;
			Shell = Context.Shell;

			Flyout = context.CreateShellFlyoutContentRenderer();
			context.ViewController.AddChildViewController(Flyout.ViewController);
			context.ViewController.View.AddSubview(Flyout.ViewController.View);

			Flyout.ViewController.View.BackgroundColor = UIColor.Blue;

			PanGestureRecognizer = new UIPanGestureRecognizer(HandlePanGesture);
			PanGestureRecognizer.ShouldReceiveTouch += (sender, touch) => {
				var view = context.ViewController.View;
				CGPoint loc = touch.LocationInView(context.ViewController.View);
				if (touch.View is UISlider || touch.View is MPVolumeView || loc.X > view.Frame.Width * 0.1)
					return false;
				return true;
			};

			Context.ViewController.View.AddGestureRecognizer(PanGestureRecognizer);
		}

		private void HandlePanGesture(UIPanGestureRecognizer pan)
		{
			var translation = pan.TranslationInView(Context.ViewController.View).X;
			double openProgress = 0;
			double openLimit = Flyout.ViewController.View.Frame.Width;

			if (_isOpen)
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
					if (_isOpen)
					{
						if (openProgress < .8)
							_isOpen = false;
					}
					else
					{
						if (openProgress > 0.2)
						{
							_isOpen = true;
						}
					}
					LayoutSidebar(true);
					break;
			}
		}

		public void PerformLayout ()
		{
			LayoutSidebar(false);
		}

		void AddTapoffView ()
		{
			if (TapoffView != null)
				return;

			TapoffView = new UIView(Context.ViewController.View.Bounds);
			Context.ViewController.View.InsertSubviewBelow(TapoffView, Flyout.ViewController.View);
			TapoffView.AddGestureRecognizer(new UITapGestureRecognizer(t => {
				_isOpen = false;
				LayoutSidebar(true);
			}));
		}

		void RemoveTapoffView ()
		{
			if (TapoffView == null)
				return;

			TapoffView.RemoveFromSuperview();
			TapoffView = null;
		}

		void LayoutSidebar (bool animate)
		{
			if (_gestureActive)
				return;

			if (animate)
			{
				UIView.BeginAnimations("Flyout");
			}
			FlyoutTransition.LayoutViews(_isOpen ? 1 : 0, Flyout.ViewController.View, Detail.View, FlyoutWidth);
			if (animate)
			{
				UIView.SetAnimationCurve(AnimationCurve);
				UIView.SetAnimationDuration(AnimationDuration);
				UIView.CommitAnimations();
				Context.ViewController.View.LayoutIfNeeded();
			}

			if (_isOpen)
				AddTapoffView();
			else
				RemoveTapoffView();
		}

		public void Dispose()
		{
			if (_disposed)
			{
				_disposed = true;

				Context = null;
				Shell = null;
			}
		}

		public void CloseFlyout()
		{
			_isOpen = false;
			LayoutSidebar(true);
		}
	}
}