using System;
using System.ComponentModel;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public class ShellRenderer : UIViewController, IShellContext, IVisualElementRenderer, IEffectControlProvider
	{
		#region IShellContext

		bool IShellContext.AllowFlyoutGesture
		{
			get
			{
				ShellTabItem currentItem = Shell?.CurrentItem?.CurrentItem;
				if (currentItem == null)
					return true;
				return currentItem.Stack.Count <= 1;
			}
		}

		IShellItemRenderer IShellContext.CurrentShellItemRenderer => _currentShellItemRenderer;

		IShellPageRendererTracker IShellContext.CreatePageRendererTracker()
		{
			return CreatePageRendererTracker();
		}

		IShellNavBarAppearanceTracker IShellContext.CreateNavBarAppearanceTracker()
		{
			return CreateNavBarAppearanceTracker();
		}

		IShellTabBarAppearanceTracker IShellContext.CreateTabBarAppearanceTracker()
		{
			return CreateTabBarAppearanceTracker();
		}

		IShellFlyoutContentRenderer IShellContext.CreateShellFlyoutContentRenderer()
		{
			var content = CreateShellFlyoutContentRenderer();

			content.ElementSelected += OnFlyoutItemSelected;

			return content;
		}

		IShellTabItemRenderer IShellContext.CreateShellTabItemRenderer(ShellTabItem tabItem)
		{
			return CreateShellTabItemRenderer(tabItem);
		}

		#endregion IShellContext

		private IShellItemRenderer _currentShellItemRenderer;
		private bool _disposed;

		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;

		public VisualElement Element { get; private set; }
		public UIView NativeView => View;
		public Shell Shell => (Shell)Element;
		public UIViewController ViewController => this;
		private IShellFlyoutRenderer FlyoutRenderer { get; set; }

		public SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint) => new SizeRequest(new Size(100, 100));

		public void RegisterEffect(Effect effect)
		{
			throw new NotImplementedException();
		}

		public void SetElement(VisualElement element)
		{
			if (Element != null)
				throw new NotSupportedException("Reuse of the Shell Renderer is not supported");
			Element = element;
			OnElementSet((Shell)Element);

			ElementChanged?.Invoke(this, new VisualElementChangedEventArgs(null, Element));
		}

		public virtual void SetElementSize(Size size)
		{
			Element.Layout(new Rectangle(Element.X, Element.Y, size.Width, size.Height));
		}

		public override void ViewDidLayoutSubviews()
		{
			base.ViewDidLayoutSubviews();

			FlyoutRenderer.PerformLayout();

			_currentShellItemRenderer.ViewController.View.Frame = View.Bounds;
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			SetupCurrentShellItem();

			FlyoutRenderer = CreateFlyoutRenderer();
			FlyoutRenderer.AttachFlyout(this);

			UpdateBackgroundColor();
		}

		protected virtual IShellFlyoutRenderer CreateFlyoutRenderer()
		{
			return new ShellFlyoutRenderer()
			{
				FlyoutTransition = new SlideFlyoutTransition()
			};
		}

		protected virtual IShellNavBarAppearanceTracker CreateNavBarAppearanceTracker()
		{
			return new SafeShellNavBarAppearanceTracker();
		}

		protected virtual IShellTabBarAppearanceTracker CreateTabBarAppearanceTracker()
		{
			return new SafeShellTabBarAppearanceTracker();
		}

		protected virtual IShellPageRendererTracker CreatePageRendererTracker()
		{
			return new ShellPageRendererTracker(this);
		}

		protected virtual IShellFlyoutContentRenderer CreateShellFlyoutContentRenderer()
		{
			return new ShellFlyoutContentRenderer(this);
		}

		protected virtual IShellItemRenderer CreateShellItemRenderer(ShellItem item)
		{
			return new ShellItemRenderer(this)
			{
				ShellItem = item
			};
		}

		protected virtual IShellItemTransition CreateShellItemTransition()
		{
			return new ShellItemTransition();
		}

		protected virtual IShellTabItemRenderer CreateShellTabItemRenderer(ShellTabItem tabItem)
		{
			return new ShellTabItemRenderer(this)
			{
				ShellTabItem = tabItem
			};
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing && !_disposed)
			{
				_disposed = true;
				FlyoutRenderer?.Dispose();
			}

			FlyoutRenderer = null;
		}

		protected virtual void OnCurrentItemChanged()
		{
			var currentItem = Shell.CurrentItem;
			if (_currentShellItemRenderer?.ShellItem != currentItem)
			{
				var newController = CreateShellItemRenderer(currentItem);
				SetCurrentShellItemController(newController);
			}
		}

		protected virtual void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Shell.CurrentItemProperty.PropertyName)
			{
				OnCurrentItemChanged();
			}
		}

		protected virtual void OnElementSet(Shell element)
		{
			if (element == null)
				return;

			element.PropertyChanged += OnElementPropertyChanged;
		}

		protected async void SetCurrentShellItemController(IShellItemRenderer value)
		{
			var oldRenderer = _currentShellItemRenderer;
			var newRenderer = value;

			_currentShellItemRenderer = value;

			AddChildViewController(newRenderer.ViewController);
			View.AddSubview(newRenderer.ViewController.View);
			View.SendSubviewToBack(newRenderer.ViewController.View);

			newRenderer.ViewController.View.Frame = View.Bounds;

			if (oldRenderer != null)
			{
				var transition = CreateShellItemTransition();
				await transition.Transition(oldRenderer, newRenderer);

				oldRenderer.ViewController.RemoveFromParentViewController();
				oldRenderer.ViewController.View.RemoveFromSuperview();
				oldRenderer.Dispose();
			}
		}

		protected virtual void UpdateBackgroundColor()
		{
			var color = Shell.BackgroundColor;
			if (color.IsDefault)
				color = Color.Black;

			View.BackgroundColor = color.ToUIColor();
		}

		private async void GoTo(ShellItem item, ShellTabItem tab)
		{
			if (tab == null)
				tab = item.CurrentItem;
			var state = ((IShellController)Shell).GetNavigationState(item, tab);
			await Shell.GoToAsync(state);
		}

		private void OnFlyoutItemSelected(object sender, ElementSelectedEventArgs e)
		{
			var element = e.Element;
			ShellItem shellItem = null;
			ShellTabItem shellTabItem = null;

			if (element is ShellItem.MenuShellItem menuShellItem)
			{
				menuShellItem.MenuItem.Activate();
			}
			else if (element is ShellItem item)
			{
				shellItem = item;
			}
			else if (element is ShellTabItem tab)
			{
				shellItem = tab.Parent as ShellItem;
				shellTabItem = tab;
			}
			else if (element is MenuItem menuItem)
			{
				menuItem.Activate();
			}

			FlyoutRenderer.CloseFlyout();
			if (shellItem != null && shellItem.IsEnabled)
				GoTo(shellItem, shellTabItem);
		}

		private void SetupCurrentShellItem()
		{
			if (Shell.CurrentItem == null)
			{
				throw new InvalidOperationException("Shell CurrentItem should not be null");
			}
			else if (_currentShellItemRenderer == null)
			{
				OnCurrentItemChanged();
			}
		}
	}
}