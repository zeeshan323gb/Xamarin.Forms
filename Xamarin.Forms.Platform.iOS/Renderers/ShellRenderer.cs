using CoreAnimation;
using System;
using System.ComponentModel;
using System.Diagnostics;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{

	public class ShellRenderer : UIViewController, IShellContext, IVisualElementRenderer, IEffectControlProvider
	{
		bool _disposed;

		private IShellFlyoutRenderer FlyoutRenderer { get; set; }

		public IShellItemRenderer CurrentShellItemRenderer { get; private set; }

		protected async void SetCurrentShellItemController(IShellItemRenderer value)
		{
			var oldRenderer = CurrentShellItemRenderer;
			var newRenderer = value;

			CurrentShellItemRenderer = value;

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

		public VisualElement Element { get; private set; }

		public UIView NativeView => View;

		public UIViewController ViewController => this;

		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;

		public Shell Shell => (Shell)Element;

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

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			SetupCurrentShellItem();

			FlyoutRenderer = CreateFlyoutRenderer();
			FlyoutRenderer.AttachFlyout(this);

			UpdateBackgroundColor();
		}

		public override void ViewDidLayoutSubviews()
		{
			base.ViewDidLayoutSubviews();

			FlyoutRenderer.PerformLayout();

			CurrentShellItemRenderer.ViewController.View.Frame = View.Bounds;
		}

		protected virtual IShellFlyoutRenderer CreateFlyoutRenderer()
		{
			return new ShellFlyoutRenderer()
			{
				FlyoutTransition = new SlideFlyoutTransition()
			};
		}

		protected virtual IShellItemRenderer CreateShellItemRenderer(ShellItem item)
		{
			return new ShellItemRenderer (this)
			{
				ShellItem = item
			};
		}

		protected virtual IShellTabItemRenderer CreateShellTabItemRenderer(ShellTabItem tabItem)
		{
			return new ShellTabItemRenderer
			{
				ShellTabItem = tabItem
			};
		}

		protected virtual IShellItemTransition CreateShellItemTransition()
		{
			return new ShellItemTransition();
		}

		protected virtual IShellFlyoutContentRenderer CreateShellFlyoutContentRenderer()
		{
			return new ShellFlyoutContentRenderer(this);
		}

		protected virtual void UpdateBackgroundColor()
		{
			var color = Shell.BackgroundColor;
			if (color.IsDefault)
				color = Color.Black;

			View.BackgroundColor = color.ToUIColor();
		}

		protected virtual void OnElementSet(Shell element)
		{
			if (element == null)
				return;

			element.PropertyChanged += OnElementPropertyChanged;
		}

		protected virtual void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Shell.CurrentItemProperty.PropertyName)
			{
				OnCurrentItemChanged();
			}
		}

		protected virtual void OnCurrentItemChanged ()
		{
			var currentItem = Shell.CurrentItem;
			if (CurrentShellItemRenderer?.ShellItem != currentItem)
			{
				var newController = CreateShellItemRenderer(currentItem);
				SetCurrentShellItemController (newController);
			}
		}

		private void SetupCurrentShellItem()
		{
			if (Shell.CurrentItem == null)
			{
				throw new InvalidOperationException("Shell CurrentItem should not be null");
			}
			else if (CurrentShellItemRenderer == null)
			{
				OnCurrentItemChanged();
			}
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing && !_disposed)
			{
				_disposed = true;

				FlyoutRenderer?.Dispose();
				FlyoutRenderer = null;
			}
		}

		IShellTabItemRenderer IShellContext.CreateShellTabItemRenderer(ShellTabItem tabItem)
		{
			return CreateShellTabItemRenderer(tabItem);
		}

		IShellFlyoutContentRenderer IShellContext.CreateShellFlyoutContentRenderer()
		{
			var content = CreateShellFlyoutContentRenderer();

			content.ElementSelected += OnFlyoutItemSelected;

			return content;
		}

		private void GoTo(ShellItem item, ShellTabItem tab)
		{
			if (item != Shell.CurrentItem)
			{
				Shell.SetValueFromRenderer(Shell.CurrentItemProperty, item);
			}

			if (tab != null)
			{
				item.SetValueFromRenderer(ShellItem.CurrentItemProperty, tab);
			}
		}

		private void OnFlyoutItemSelected(object sender, ElementSelectedEventArgs e)
		{
			var element = e.Element;
			ShellItem shellItem = null; ;
			ShellTabItem shellTabItem = null;
			if (element is ShellItem item)
			{
				shellItem = item;
			}
			else if (element is ShellTabItem tab)
			{
				shellItem = tab.Parent as ShellItem;
				shellTabItem = tab;
			}

			FlyoutRenderer.CloseFlyout();
			GoTo(shellItem, shellTabItem);
		}
	}
}