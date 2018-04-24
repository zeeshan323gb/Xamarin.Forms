using CoreGraphics;
using MediaPlayer;
using System;
using System.ComponentModel;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public class ShellFlyoutRenderer : IShellFlyoutRenderer
	{
		private bool _disposed;
		private bool _gestureActive;
		private bool _isOpen;
		public UIViewAnimationCurve AnimationCurve { get; set; } = UIViewAnimationCurve.EaseOut;
		public int AnimationDuration { get; set; } = 250;
		public IShellFlyoutTransition FlyoutTransition { get; set; }

		private IShellContext Context { get; set; }

		private UIViewController Detail => Context.CurrentShellItemRenderer.ViewController;

		private IShellFlyoutContentRenderer Flyout { get; set; }

		private nfloat FlyoutWidth => (nfloat)(Math.Min(Context.ViewController.View.Frame.Width, Context.ViewController.View.Frame.Height) * 0.8);

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

		public void AttachFlyout(IShellContext context)
		{
			Context = context;
			Shell = Context.Shell;

			Shell.PropertyChanged += OnShellPropertyChanged;

			Flyout = context.CreateShellFlyoutContentRenderer();
			context.ViewController.AddChildViewController(Flyout.ViewController);
			context.ViewController.View.AddSubview(Flyout.ViewController.View);

			PanGestureRecognizer = new UIPanGestureRecognizer(HandlePanGesture);
			PanGestureRecognizer.ShouldReceiveTouch += (sender, touch) =>
			{
				if (!context.AllowFlyoutGesture)
					return false;
				var view = context.ViewController.View;
				CGPoint loc = touch.LocationInView(context.ViewController.View);
				if (touch.View is UISlider || 
					touch.View is MPVolumeView || 
					(loc.X > view.Frame.Width * 0.1 && !IsOpen))
					return false;
				return true;
			};

			Context.ViewController.View.AddGestureRecognizer(PanGestureRecognizer);
		}

		public void CloseFlyout()
		{
			IsOpen = false;
			LayoutSidebar(true);
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

		public void PerformLayout()
		{
			LayoutSidebar(false);
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

			TapoffView = new UIView(Context.ViewController.View.Bounds);
			Context.ViewController.View.InsertSubviewBelow(TapoffView, Flyout.ViewController.View);
			TapoffView.AddGestureRecognizer(new UITapGestureRecognizer(t =>
			{
				IsOpen = false;
				LayoutSidebar(true);
			}));
		}

		private void HandlePanGesture(UIPanGestureRecognizer pan)
		{
			var translation = pan.TranslationInView(Context.ViewController.View).X;
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
				Context.ViewController.View.LayoutIfNeeded();
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