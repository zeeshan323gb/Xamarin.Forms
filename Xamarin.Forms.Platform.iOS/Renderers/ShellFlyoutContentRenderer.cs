using System;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{

	public class ShellFlyoutContentRenderer : UIViewController, IShellFlyoutContentRenderer
	{
		public UIViewController ViewController => this;

		private ShellTableViewController _tableViewController;

		private UIView _headerView;

		public event EventHandler<ElementSelectedEventArgs> ElementSelected;

		public ShellFlyoutContentRenderer(IShellContext context)
		{
			_headerView = new UIContainerView(((IShellController)context.Shell).FlyoutHeader);
			_tableViewController = new ShellTableViewController(context, _headerView, OnElementSelected);

			AddChildViewController(_tableViewController);
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			
			View.AddSubview(_tableViewController.View);
			View.AddSubview(_headerView);
		}

		public override void ViewDidLayoutSubviews()
		{
			base.ViewDidLayoutSubviews();

			_tableViewController.LayoutParallax();
		}

		private void OnElementSelected (Element element)
		{
			ElementSelected?.Invoke(this, new ElementSelectedEventArgs { Element = element });
		}
	}
}