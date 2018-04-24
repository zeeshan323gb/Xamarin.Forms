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
		private UIView _blurView;
		private UIView _colorView;
		private IShellContext _context;
		private UIImage _defaultBackgroundImage;
		private UIImage _defaultShadowImage;
		private UIColor _defaultTint;
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
				CreateTabControllers();
				UpdateShellAppearance();
			}
		}

		public UIViewController ViewController => this;

		private IShellTabItemRenderer CurrentRenderer { get; set; }

		public override void ViewDidLayoutSubviews()
		{
			base.ViewDidLayoutSubviews();

			_blurView.Frame = TabBar.Bounds;
			_colorView.Frame = _blurView.Frame;
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

		protected virtual void OnItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			throw new NotImplementedException();
		}

		protected virtual void OnShellItemSet(ShellItem item)
		{
			item.PropertyChanged += OnElementPropertyChanged;
			((IShellItemController)item).CurrentShellAppearanceChanged += OnShellAppearanceChanged;
			((INotifyCollectionChanged)item.Items).CollectionChanged += OnItemsCollectionChanged;
		}

		protected virtual void ResetTintColors()
		{
			if (_blurView == null)
				return;

			TabBar.ShadowImage = _defaultShadowImage;
			TabBar.BackgroundImage = _defaultBackgroundImage;
			TabBar.TintColor = _defaultTint;

			_blurView.RemoveFromSuperview();
			_colorView.RemoveFromSuperview();
		}

		protected virtual void SetTintColors(UIColor foreground, UIColor background)
		{
			if (_blurView == null)
			{
				_defaultBackgroundImage = TabBar.BackgroundImage;
				_defaultShadowImage = TabBar.ShadowImage;
				_defaultTint = TabBar.TintColor;

				var effect = UIBlurEffect.FromStyle(UIBlurEffectStyle.Regular);
				_blurView = new UIVisualEffectView(effect);
				_blurView.Frame = TabBar.Bounds;

				_colorView = new UIView(_blurView.Frame);
			}

			TabBar.BackgroundImage = new UIImage();
			TabBar.ShadowImage = new UIImage();

			TabBar.InsertSubview(_colorView, 0);
			TabBar.InsertSubview(_blurView, 0);

			_colorView.BackgroundColor = background;
			TabBar.TintColor = foreground;
		}

		protected virtual void UpdateShellAppearance()
		{
			if (ShellItem == null)
				return;

			var appearance = ((IShellItemController)ShellItem).CurrentShellAppearance;
			IShellTabItemRenderer currentRenderer = CurrentRenderer;

			if (appearance == null)
			{
				ResetTintColors();
				currentRenderer?.ResetTintColors();
				return;
			}

			var background = appearance.BackgroundColor.ToUIColor();
			var foreground = appearance.ForegroundColor.ToUIColor();

			currentRenderer?.SetTintColors(foreground, background);

			SetTintColors(foreground, background);
		}

		private void CreateTabControllers()
		{
			UIViewController[] viewControllers = new UIViewController[ShellItem.Items.Count];
			int i = 0;
			foreach (var shellTabItem in ShellItem.Items)
			{
				var tabController = _context.CreateShellTabItemRenderer(shellTabItem);
				_tabRenderers.Add(tabController);
				viewControllers[i++] = tabController.ViewController;
			}
			ViewControllers = viewControllers;

			GoTo((ShellTabItem)ShellItem.CurrentItem);
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