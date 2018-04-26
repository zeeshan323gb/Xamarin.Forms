using CoreGraphics;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public class ShellItemRenderer : UITabBarController, IShellItemRenderer
	{
		private IShellTabBarAppearanceTracker _appearanceTracker;
		private IShellContext _context;
		private bool _disposed;
		private ShellItem _shellItem;
		private Dictionary<UIViewController, IShellTabItemRenderer> _tabRenderers = new Dictionary<UIViewController, IShellTabItemRenderer>();

		public ShellItemRenderer(IShellContext context)
		{
			_context = context;
		}

		public override UIViewController SelectedViewController
		{
			get { return base.SelectedViewController; }
			set
			{
				base.SelectedViewController = value;

				var renderer = RendererForViewController(value);
				if (renderer != null)
				{
					ShellItem.SetValueFromRenderer(ShellItem.CurrentItemProperty, renderer.ShellTabItem);
					CurrentRenderer = renderer;
				}
			}
		}

		public ShellItem ShellItem
		{
			get => _shellItem;
			set
			{
				if (_shellItem == value)
					return;
				_shellItem = value;
				OnShellItemSet(_shellItem);
				CreateTabRenderers();
				UpdateShellAppearance();
			}
		}

		public UIViewController ViewController => this;

		private IShellTabItemRenderer CurrentRenderer { get; set; }

		public override void ViewDidLayoutSubviews()
		{
			base.ViewDidLayoutSubviews();

			_appearanceTracker?.UpdateLayout(this);
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			ShouldSelectViewController = (tabController, viewController) =>
			{
				bool accept = true;
				var r = RendererForViewController(viewController);
				if (r != null)
				{
					var controller = (IShellController)_context.Shell;
					accept = controller.ProposeNavigation(ShellNavigationSource.ShellTabItemChanged,
						ShellItem,
						r.ShellTabItem,
						r.ShellTabItem.Stack.ToList(),
						true
					);
				}

				return accept;
			};

			UpdateShellAppearance();
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing && !_disposed)
			{
				_disposed = true;
				foreach (var kvp in _tabRenderers.ToList())
				{
					var renderer = kvp.Value;
					RemoveRenderer(renderer);
					renderer.Dispose();
				}
				_tabRenderers.Clear();
				CurrentRenderer = null;
				ShellItem.PropertyChanged -= OnElementPropertyChanged;
				((IShellItemController)ShellItem).CurrentShellAppearanceChanged -= OnShellAppearanceChanged;
				((INotifyCollectionChanged)ShellItem.Items).CollectionChanged -= OnItemsCollectionChanged;
				_shellItem = null;
			}
		}

		protected virtual void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == ShellItem.CurrentItemProperty.PropertyName)
			{
				GoTo(ShellItem.CurrentItem);
			}
		}

		protected virtual void OnItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.OldItems != null)
			{
				foreach (ShellTabItem shellTabItem in e.OldItems)
				{
					var renderer = RendererForShellTabItem(shellTabItem);
					if (renderer != null)
					{
						ViewControllers = ViewControllers.Remove(renderer.ViewController);
						RemoveRenderer(renderer);
					}
				}
			}

			if (e.NewItems != null && e.NewItems.Count > 0)
			{
				UIViewController[] viewControllers = new UIViewController[ShellItem.Items.Count];
				int i = 0;
				bool goTo = false; // its possible we are in a transitionary state and should not nav
				var current = ShellItem.CurrentItem;
				foreach (var shellTabItem in ShellItem.Items)
				{
					var renderer = RendererForShellTabItem(shellTabItem) ?? _context.CreateShellTabItemRenderer(shellTabItem);
					AddRenderer(renderer);
					viewControllers[i++] = renderer.ViewController;
					if (shellTabItem == current)
						goTo = true;
				}

				ViewControllers = viewControllers;

				if (goTo)
					GoTo(ShellItem.CurrentItem);
			}
		}

		protected virtual void OnShellItemSet(ShellItem item)
		{
			_appearanceTracker = _context.CreateTabBarAppearanceTracker();
			item.PropertyChanged += OnElementPropertyChanged;
			((IShellItemController)item).CurrentShellAppearanceChanged += OnShellAppearanceChanged;
			((INotifyCollectionChanged)item.Items).CollectionChanged += OnItemsCollectionChanged;
		}

		protected virtual void OnShellTabItemPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == ShellTabItem.IsEnabledProperty.PropertyName)
			{
				var tabItem = (ShellTabItem)sender;
				var renderer = RendererForShellTabItem(tabItem);
				var index = ViewControllers.ToList().IndexOf(renderer.ViewController);
				TabBar.Items[index].Enabled = tabItem.IsEnabled;
			}
		}

		protected virtual void UpdateShellAppearance()
		{
			if (ShellItem == null)
				return;

			var appearance = ((IShellItemController)ShellItem).CurrentShellAppearance;
			IShellTabItemRenderer currentRenderer = CurrentRenderer;

			if (appearance == null)
			{
				_appearanceTracker.ResetAppearance(this);
				currentRenderer?.ResetAppearance();
				return;
			}

			currentRenderer?.SetAppearance(appearance);
			_appearanceTracker.SetAppearance(this, appearance);
		}

		private void AddRenderer(IShellTabItemRenderer renderer)
		{
			if (_tabRenderers.ContainsKey(renderer.ViewController))
				return;
			_tabRenderers[renderer.ViewController] = renderer;
			renderer.ShellTabItem.PropertyChanged += OnShellTabItemPropertyChanged;
		}

		private void CreateTabRenderers()
		{
			UIViewController[] viewControllers = new UIViewController[ShellItem.Items.Count];
			int i = 0;
			foreach (var shellTabItem in ShellItem.Items)
			{
				var renderer = _context.CreateShellTabItemRenderer(shellTabItem);
				AddRenderer(renderer);
				viewControllers[i++] = renderer.ViewController;
			}
			ViewControllers = viewControllers;
			GoTo(ShellItem.CurrentItem);

			// now that they are applied we can set the enabled state of the TabBar items
			for (i = 0; i < ViewControllers.Length; i++)
			{
				var renderer = RendererForViewController(ViewControllers[i]);
				if (!renderer.ShellTabItem.IsEnabled)
				{
					TabBar.Items[i].Enabled = false;
				}
			}
		}

		private void GoTo(ShellTabItem tabItem)
		{
			if (tabItem == null)
				return;
			var renderer = RendererForShellTabItem(tabItem);
			if (renderer?.ViewController != SelectedViewController)
				SelectedViewController = renderer.ViewController;
			CurrentRenderer = renderer;
		}

		private UIImage ImageFromColor(UIColor color)
		{
			CGRect rect = new CGRect(0, 0, 1, 1);
			UIGraphics.BeginImageContext(rect.Size);
			CGContext context = UIGraphics.GetCurrentContext();
			context.SetFillColor(color.CGColor);
			context.FillRect(rect);
			UIImage image = UIGraphics.GetImageFromCurrentImageContext();
			UIGraphics.EndImageContext();

			return image;
		}

		private void OnShellAppearanceChanged(object sender, EventArgs e)
		{
			UpdateShellAppearance();
		}

		private void RemoveRenderer(IShellTabItemRenderer renderer)
		{
			if (_tabRenderers.Remove(renderer.ViewController))
			{
				renderer.ShellTabItem.PropertyChanged -= OnShellTabItemPropertyChanged;
			}
		}

		private IShellTabItemRenderer RendererForShellTabItem(ShellTabItem tab)
		{
			foreach (var item in _tabRenderers)
			{
				if (item.Value.ShellTabItem == tab)
					return item.Value;
			}
			return null;
		}

		private IShellTabItemRenderer RendererForViewController(UIViewController viewController)
		{
			if (_tabRenderers.TryGetValue(viewController, out var value))
				return value;
			return null;
		}
	}
}