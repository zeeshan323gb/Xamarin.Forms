using CoreGraphics;
using Foundation;
using System;
using System.Collections.Generic;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public class ShellSectionRootRenderer : UIViewController, IShellSectionRootRenderer
	{
		#region IShellSectionRootRenderer

		bool IShellSectionRootRenderer.ShowNavBar => Shell.GetNavBarVisible(((IShellContentController)ShellSection.CurrentItem).GetOrCreateContent());

		UIViewController IShellSectionRootRenderer.ViewController => this;

		#endregion

		private ShellSection ShellSection { get; set; }
		private UIScrollView _scrollView;
		private Dictionary<ShellContent, IVisualElementRenderer> _renderers = new Dictionary<ShellContent, IVisualElementRenderer>();

		public ShellSectionRootRenderer(ShellSection shellSection)
		{
			ShellSection = shellSection ?? throw new ArgumentNullException(nameof(shellSection));
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			_scrollView = new UIScrollView();
			_scrollView.ContentInsetAdjustmentBehavior = UIScrollViewContentInsetAdjustmentBehavior.Never;
			_scrollView.InsetsLayoutMarginsFromSafeArea = false;
			_scrollView.AlwaysBounceHorizontal = false;
			_scrollView.AlwaysBounceVertical = false;
			_scrollView.Bounces = false;
			_scrollView.PagingEnabled = true;

			View.AddSubview(_scrollView);

			LoadRenderers();
		}

		protected virtual void LoadRenderers()
		{
			foreach (var item in ShellSection.Items)
			{
				var page = ((IShellContentController)item).GetOrCreateContent();
				var renderer = Platform.CreateRenderer(page);
				Platform.SetRenderer(page, renderer);

				AddChildViewController(renderer.ViewController);
				_scrollView.AddSubview(renderer.NativeView);

				_renderers[item] = renderer;
			}
		}

		protected virtual void LayoutRenderers()
		{
			var items = ShellSection.Items;
			int i;
			for (i = 0; i < items.Count; i++)
			{
				var shellContent = items[i];
				if (_renderers.TryGetValue(shellContent, out var renderer))
				{
					var view = renderer.NativeView;
					view.Frame = new CGRect(i * View.Bounds.Width, 0, View.Bounds.Width, View.Bounds.Height);
				}
			}

			_scrollView.ContentSize = new CGSize(View.Bounds.Width * i, View.Bounds.Height);
		}

		public override void ViewDidLayoutSubviews()
		{
			base.ViewDidLayoutSubviews();

			_scrollView.Frame = View.Bounds;

			LayoutRenderers();
		}
	}
}