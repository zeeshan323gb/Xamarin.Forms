using System;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public class SlideFlyoutTransition : IShellFlyoutTransition
	{
		public void LayoutViews(nfloat openPercent, UIView flyout, UIView shell, nfloat flyoutWidth)
		{
			nfloat openLimit = flyoutWidth;
			nfloat openPixels = openLimit * openPercent;

			flyout.Frame = new CoreGraphics.CGRect(-openLimit + openPixels, 0, flyoutWidth, flyout.Frame.Height);
			var shellOpacity = (nfloat)(0.5 + (0.5 * (1 - openPercent)));
			shell.Layer.Opacity = (float)shellOpacity;
		}
	}
}