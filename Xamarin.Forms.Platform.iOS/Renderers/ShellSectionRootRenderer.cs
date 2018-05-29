using System;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public class ShellSectionRootRenderer : UIViewController, IShellSectionRootRenderer
	{
		#region IShellSectionRootRenderer

		bool IShellSectionRootRenderer.ShowNavBar => Shell.GetNavBarVisible(((IShellContentController)_shellSection.CurrentItem).GetOrCreateContent());

		UIViewController IShellSectionRootRenderer.ViewController => this;

		#endregion

		private readonly ShellSection _shellSection;

		public ShellSectionRootRenderer(ShellSection shellSection)
		{
			_shellSection = shellSection ?? throw new ArgumentNullException(nameof(shellSection));
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			View.BackgroundColor = UIColor.Purple;
		}

		public override void ViewDidLayoutSubviews()
		{
			base.ViewDidLayoutSubviews();
		}
	}
}