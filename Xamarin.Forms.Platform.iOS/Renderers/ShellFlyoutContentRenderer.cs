using System;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public class ShellFlyoutContentRenderer : UIViewController, IShellFlyoutContentRenderer
	{
		private UIView _headerView;
		private ShellTableViewController _tableViewController;

		public ShellFlyoutContentRenderer(IShellContext context)
		{
			_headerView = new UIContainerView(((IShellController)context.Shell).FlyoutHeader);
			_tableViewController = new ShellTableViewController(context, _headerView, OnElementSelected);

			AddChildViewController(_tableViewController);
		}

		public event EventHandler<ElementSelectedEventArgs> ElementSelected;

		public UIViewController ViewController => this;

		public override void ViewDidLayoutSubviews()
		{
			base.ViewDidLayoutSubviews();

			_tableViewController.LayoutParallax();
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			View.AddSubview(_tableViewController.View);
			View.AddSubview(_headerView);
		}

		private void OnElementSelected(Element element)
		{
			ElementSelected?.Invoke(this, new ElementSelectedEventArgs { Element = element });
		}
	}
}