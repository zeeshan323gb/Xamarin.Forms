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
		private Dictionary<ShellContent, IVisualElementRenderer> _renderers = new Dictionary<ShellContent, IVisualElementRenderer>();
		private UIView _containerArea;
		private ShellSectionRootHeader _header;

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

		private void LayoutHeader()
		{
			_header.View.Frame = new CGRect(View.Bounds.X, View.SafeAreaInsets.Top, View.Bounds.Width, 35);
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
		}

		public override void ViewSafeAreaInsetsDidChange()
		{
			base.ViewSafeAreaInsetsDidChange();

			LayoutHeader();
		}

		protected virtual void UpdateHeaderVisibility()
		{
			_header.View.Hidden = ShellSection.Items.Count <= 1;
		}

		protected virtual void LayoutRenderers()
		{
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
			foreach (var item in ShellSection.Items)
			{
				var page = ((IShellContentController)item).GetOrCreateContent();
				var renderer = Platform.CreateRenderer(page);
				Platform.SetRenderer(page, renderer);

				AddChildViewController(renderer.ViewController);
				_containerArea.AddSubview(renderer.NativeView);

				if (item == ShellSection.CurrentItem)
					renderer.NativeView.Hidden = false;
				else
					renderer.NativeView.Hidden = true;

				_renderers[item] = renderer;
			}
		}

		protected virtual void OnShellSectionPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == ShellSection.CurrentItemProperty.PropertyName)
			{
				var selectedIndex = ShellSection.Items.IndexOf(ShellSection.CurrentItem);

				foreach (var item in ShellSection.Items)
				{
					var renderer = _renderers[item];
					if (item == ShellSection.CurrentItem)
						renderer.NativeView.Hidden = false;
					else
						renderer.NativeView.Hidden = true;
				}
			}
		}
	}
}