using CoreGraphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public class ShellSectionRootRenderer : UIViewController, IShellSectionRootRenderer
	{
		#region IShellSectionRootRenderer

		bool IShellSectionRootRenderer.ShowNavBar => Shell.GetNavBarVisible(((IShellContentController)ShellSection.CurrentItem).GetOrCreateContent());

		UIViewController IShellSectionRootRenderer.ViewController => this;

		#endregion IShellSectionRootRenderer

		private readonly IShellContext _shellContext;
		private UIView _containerArea;
		private int _currentIndex;
		private ShellSectionRootHeader _header;
		private bool _isAnimating = false;
		private Dictionary<ShellContent, IVisualElementRenderer> _renderers = new Dictionary<ShellContent, IVisualElementRenderer>();
		private IShellPageRendererTracker _tracker;

		public ShellSectionRootRenderer(ShellSection shellSection, IShellContext shellContext)
		{
			ShellSection = shellSection ?? throw new ArgumentNullException(nameof(shellSection));
			_shellContext = shellContext;
		}

		private ShellSection ShellSection { get; set; }

		public override void ViewDidLayoutSubviews()
		{
			base.ViewDidLayoutSubviews();

			_containerArea.Frame = View.Bounds;

			LayoutRenderers();

			LayoutHeader();
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			_containerArea = new UIView();
			_containerArea.InsetsLayoutMarginsFromSafeArea = false;

			View.AddSubview(_containerArea);

			LoadRenderers();

			ShellSection.PropertyChanged += OnShellSectionPropertyChanged;

			_header = new ShellSectionRootHeader(ShellSection);

			AddChildViewController(_header);
			View.AddSubview(_header.View);

			UpdateHeaderVisibility();

			var tracker = _shellContext.CreatePageRendererTracker();
			tracker.IsRootPage = true;
			tracker.ViewController = this;
			tracker.Page = ((IShellContentController)ShellSection.CurrentItem).GetOrCreateContent();
			_tracker = tracker;
		}

		public override void ViewSafeAreaInsetsDidChange()
		{
			base.ViewSafeAreaInsetsDidChange();

			LayoutHeader();
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing)
			{
				_tracker?.Dispose();
				_tracker = null;
			}
		}

		protected virtual void LayoutRenderers()
		{
			if (_isAnimating)
				return;

			var items = ShellSection.Items;
			for (int i = 0; i < items.Count; i++)
			{
				var shellContent = items[i];
				if (_renderers.TryGetValue(shellContent, out var renderer))
				{
					var view = renderer.NativeView;
					view.Frame = new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
				}
			}
		}

		protected virtual void LoadRenderers()
		{
			var currentItem = ShellSection.CurrentItem;
			for (int i = 0; i < ShellSection.Items.Count; i++)
			{
				ShellContent item = ShellSection.Items[i];
				var page = ((IShellContentController)item).GetOrCreateContent();
				var renderer = Platform.CreateRenderer(page);
				Platform.SetRenderer(page, renderer);

				AddChildViewController(renderer.ViewController);

				if (item == currentItem)
				{
					_containerArea.AddSubview(renderer.NativeView);
					_currentIndex = i;
				}

				_renderers[item] = renderer;
			}
		}

		protected virtual void OnShellSectionPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == ShellSection.CurrentItemProperty.PropertyName)
			{
				var items = ShellSection.Items;
				var currentItem = ShellSection.CurrentItem;

				var oldIndex = _currentIndex;
				var oldItem = items[oldIndex];

				_currentIndex = items.IndexOf(currentItem);

				var oldRenderer = _renderers[oldItem];
				var currentRenderer = _renderers[currentItem];

				// -1 == slide left, 1 ==  slide right
				int motionDirection = _currentIndex > oldIndex ? -1 : 1;

				_containerArea.AddSubview(currentRenderer.NativeView);

				_isAnimating = true;

				currentRenderer.NativeView.Frame = new CGRect(-motionDirection * View.Bounds.Width, 0, View.Bounds.Width, View.Bounds.Height);
				oldRenderer.NativeView.Frame = _containerArea.Bounds;

				UIView.Animate(.25, 0, UIViewAnimationOptions.CurveEaseOut, () =>
				{
					currentRenderer.NativeView.Frame = _containerArea.Bounds;
					oldRenderer.NativeView.Frame = new CGRect(motionDirection * View.Bounds.Width, 0, View.Bounds.Width, View.Bounds.Height);
				},
				() =>
				{
					oldRenderer.NativeView.RemoveFromSuperview();
					_isAnimating = false;

					_tracker.Page = ((IShellContentController)currentItem).Page;
				});
			}
		}

		protected virtual void UpdateHeaderVisibility()
		{
			_header.View.Hidden = ShellSection.Items.Count <= 1;
		}

		private void LayoutHeader()
		{
			_header.View.Frame = new CGRect(View.Bounds.X, View.SafeAreaInsets.Top, View.Bounds.Width, 35);
		}
	}
}