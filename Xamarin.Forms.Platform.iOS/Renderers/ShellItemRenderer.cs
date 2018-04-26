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
		private List<IShellTabItemRenderer> _tabRenderers = new List<IShellTabItemRenderer>();

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

				foreach (var controller in _tabRenderers)
				{
					if (controller.ViewController == value)
					{
						ShellItem.SetValueFromRenderer(ShellItem.CurrentItemProperty, controller.ShellTabItem);
						CurrentRenderer = controller;
						break;
					}
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
				foreach (var r in _tabRenderers)
				{
					if (r.ViewController == viewController)
					{
						var controller = (IShellController)_context.Shell;
						accept = controller.ProposeNavigation(ShellNavigationSource.ShellTabItemChanged,
							ShellItem,
							r.ShellTabItem,
							r.ShellTabItem.Stack.ToList(),
							true
						);
					}
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
				foreach (var renderer in _tabRenderers)
				{
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

		IShellTabItemRenderer RendererForShellTabItem (ShellTabItem item, List<IShellTabItemRenderer> renderers)
		{
			foreach (var renderer in renderers)
			{
				if (renderer.ShellTabItem == item)
					return renderer;
			}
			return null;
		}

		protected virtual void OnItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.OldItems != null)
			{
				foreach (var shellTabItem in e.OldItems)
				{
					foreach (var renderer in _tabRenderers)
					{

						if (renderer.ShellTabItem == shellTabItem)
						{
							ViewControllers = ViewControllers.Remove(renderer.ViewController);
							break;
						}
					}
				}
			}

			if (e.NewItems != null && e.NewItems.Count > 0)
			{
				var oldRenderersList = _tabRenderers;
				_tabRenderers = new List<IShellTabItemRenderer>();

				UIViewController[] viewControllers = new UIViewController[ShellItem.Items.Count];
				int i = 0;
				bool goTo = false; // its possible we are in a transitionary state and should not nav
				var current = ShellItem.CurrentItem;
				foreach (var shellTabItem in ShellItem.Items)
				{
					var renderer = RendererForShellTabItem(shellTabItem, oldRenderersList) ?? _context.CreateShellTabItemRenderer(shellTabItem);
					_tabRenderers.Add(renderer);
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

		private void CreateTabRenderers()
		{
			UIViewController[] viewControllers = new UIViewController[ShellItem.Items.Count];
			int i = 0;
			foreach (var shellTabItem in ShellItem.Items)
			{
				var tabRenderer = _context.CreateShellTabItemRenderer(shellTabItem);
				_tabRenderers.Add(tabRenderer);
				viewControllers[i++] = tabRenderer.ViewController;
			}
			ViewControllers = viewControllers;

			GoTo(ShellItem.CurrentItem);
		}

		private void GoTo(ShellTabItem tabItem)
		{
			if (tabItem == null)
				return;
			foreach (var renderer in _tabRenderers)
			{
				if (renderer.ShellTabItem == tabItem)
				{
					if (renderer.ViewController != SelectedViewController)
						SelectedViewController = renderer.ViewController;
					CurrentRenderer = renderer;
					break;
				}
			}
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
	}
}