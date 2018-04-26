using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public class TabletShellFlyoutRenderer : UISplitViewController, IShellFlyoutRenderer
	{
		#region IShellFlyoutRenderer

		UIViewController IShellFlyoutRenderer.ViewController => this;

		UIView IShellFlyoutRenderer.View => View;

		void IShellFlyoutRenderer.AttachFlyout(IShellContext context, UIViewController content)
		{
			_context = context;
			_content = content;
			
			FlyoutContent = _context.CreateShellFlyoutContentRenderer();

			ViewControllers = new UIViewController[]
			{
				FlyoutContent.ViewController,
				content
			};
		}

		void IShellFlyoutRenderer.CloseFlyout()
		{
		}

		#endregion

		private IShellContext _context;
		private UIViewController _content;

		private IShellFlyoutContentRenderer FlyoutContent { get; set; }

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
		}

		public override void ViewDidLayoutSubviews()
		{
			base.ViewDidLayoutSubviews();
		}
	}
}