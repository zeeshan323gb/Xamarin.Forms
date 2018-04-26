using CoreGraphics;
using System;
using System.ComponentModel;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public class ShellFlyoutContentRenderer : UIViewController, IShellFlyoutContentRenderer
	{
		private UIVisualEffectView _blurView;
		private readonly IShellContext _context;
		private UIView _headerView;
		private ShellTableViewController _tableViewController;

		public ShellFlyoutContentRenderer(IShellContext context)
		{
			_headerView = new UIContainerView(((IShellController)context.Shell).FlyoutHeader);
			_tableViewController = new ShellTableViewController(context, _headerView, OnElementSelected);

			AddChildViewController(_tableViewController);

			context.Shell.PropertyChanged += HandleShellPropertyChanged;

			_context = context;
		}

		protected virtual void HandleShellPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Shell.FlyoutBackgroundColorProperty.PropertyName)
				UpdateBackgroundColor();
		}

		protected virtual void UpdateBackgroundColor()
		{
			var color = _context.Shell.FlyoutBackgroundColor;
			View.BackgroundColor = color.ToUIColor(Color.White);

			if (View.BackgroundColor.CGColor.Alpha < 1)
			{
				View.InsertSubview(_blurView, 0);
			}
			else
			{
				if (_blurView.Superview != null)
					_blurView.RemoveFromSuperview();
			}
		}

		public event EventHandler<ElementSelectedEventArgs> ElementSelected;

		public UIViewController ViewController => this;

		public override void ViewDidLayoutSubviews()
		{
			base.ViewDidLayoutSubviews();

			_tableViewController.LayoutParallax();
			_blurView.Frame = View.Bounds;
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			View.AddSubview(_tableViewController.View);
			View.AddSubview(_headerView);

			_tableViewController.TableView.BackgroundView = null;
			_tableViewController.TableView.BackgroundColor = UIColor.Clear;

			var effect = UIBlurEffect.FromStyle(UIBlurEffectStyle.Regular);
			_blurView = new UIVisualEffectView(effect);
			_blurView.Frame = View.Bounds;

			UpdateBackgroundColor();
		}

		private void OnElementSelected(Element element)
		{
			ElementSelected?.Invoke(this, new ElementSelectedEventArgs { Element = element });
		}
	}
}