using System;
using System.Threading.Tasks;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public class ShellItemTransition : IShellItemTransition
	{
		public Task Transition (IShellItemRenderer oldRenderer, IShellItemRenderer newRenderer)
		{
			TaskCompletionSource<bool> task = new TaskCompletionSource<bool>();
			var oldView = oldRenderer.ViewController.View;
			var newView = newRenderer.ViewController.View;
			newView.Alpha = (nfloat)0;

			newView.Superview.InsertSubviewAbove(newView, oldView);

			var animation = new UIViewPropertyAnimator(0.5, UIViewAnimationCurve.EaseInOut, () =>
			{
				newView.Alpha = 1;
			});

			animation.AddCompletion(pos =>
			{
				task.TrySetResult(true);
			});
			animation.StartAnimation();

			return task.Task;
		}
	}
}