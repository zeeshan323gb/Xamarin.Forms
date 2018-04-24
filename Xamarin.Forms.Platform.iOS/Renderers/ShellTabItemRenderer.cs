using Foundation;
using ObjCRuntime;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using UIKit;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.iOS
{
	public class ShellTabItemRenderer : UINavigationController, IShellTabItemRenderer, IUIGestureRecognizerDelegate
	{
		#region IShellTabItemRenderer

		void IShellTabItemRenderer.ResetTintColors()
		{
			ResetTintColors();
		}

		void IShellTabItemRenderer.SetTintColors(UIColor foreground, UIColor background)
		{
			SetTintColors(foreground, background);
		}

		#endregion IShellTabItemRenderer

		private readonly IShellContext _context;
		private UIView _blurView;
		private UIView _colorView;

		private Dictionary<UIViewController, TaskCompletionSource<bool>> _completionTasks =
			new Dictionary<UIViewController, TaskCompletionSource<bool>>();

		private UIImage _defaultBackgroundImage;

		private UIColor _defaultTint;

		private bool _disposed;

		private bool _ignorePop;

		private Page _page;

		private TaskCompletionSource<bool> _popCompletionTask;

		private IVisualElementRenderer _renderer;

		private ShellTabItem _shellTabItem;

		private Dictionary<Page, IShellPageRendererTracker> _trackers =
			new Dictionary<Page, IShellPageRendererTracker>();

		public ShellTabItemRenderer(IShellContext context)
		{
			Delegate = new NavDelegate(this);
			_context = context;
		}

		public Page Page
		{
			get => _page;
			private set
			{
				((IShellTabItemController)_shellTabItem).RootPageProjection = value;
				_page = value;
			}
		}

		public ShellTabItem ShellTabItem
		{
			get { return _shellTabItem; }
			set
			{
				if (_shellTabItem == value)
					return;
				_shellTabItem = value;
				LoadPages();
				OnShellTabItemSet();
				_shellTabItem.PropertyChanged += HandlePropertyChanged;
				((IShellTabItemController)_shellTabItem).NavigationRequested += OnNavigationRequested;
			}
		}

		public UIViewController ViewController => this;

		public override void TraitCollectionDidChange(UITraitCollection previousTraitCollection)
		{
			base.TraitCollectionDidChange(previousTraitCollection);

			UpdatePageInsetPadding(PageForViewController(TopViewController));
		}

		public override UIViewController PopViewController(bool animated)
		{
			if (!_ignorePop)
			{
				_popCompletionTask = new TaskCompletionSource<bool>();
				SendPoppedOnCompletion(_popCompletionTask.Task);
			}

			return base.PopViewController(animated);
		}

		[Export("navigationBar:shouldPopItem:")]
		public bool ShouldPopItem(UINavigationBar navigationBar, UINavigationItem item)
		{
			// this means the pop is already done, nothing we can do
			if (ViewControllers.Length < NavigationBar.Items.Length)
				return true;

			bool allowPop = ShouldPop();

			if (allowPop)
			{
				CoreFoundation.DispatchQueue.MainQueue.DispatchAsync(() => PopViewController(true));
			}
			else
			{
				for (int i = 0; i < NavigationBar.Subviews.Length; i++)
				{
					var child = NavigationBar.Subviews[i];
					if (child.Alpha != 1)
						UIView.Animate(.2f, () => child.Alpha = 1);
				}
			}

			return false;
		}

		public override void ViewDidLayoutSubviews()
		{
			base.ViewDidLayoutSubviews();

			_renderer.Element.Layout(View.Bounds.ToRectangle());

			if (_blurView?.Superview == NavigationBar)
			{
				NavigationBar.SendSubviewToBack(_colorView);
				NavigationBar.SendSubviewToBack(_blurView);

				var frame = NavigationBar.Frame;
				frame.Height += frame.Y;
				frame.Y = -frame.Y;

				_blurView.Frame = frame;
				_colorView.Frame = frame;
			}
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			InteractivePopGestureRecognizer.Delegate = new GestureDelegate(this, ShouldPop);
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing && !_disposed)
			{
				_disposed = true;
				_shellTabItem.PropertyChanged -= HandlePropertyChanged;
				((IShellTabItemController)_shellTabItem).NavigationRequested -= OnNavigationRequested;
				DisposePage(Page);
			}

			// must be set null prior to _shellTabItem to ensure weak ref page gets cleared
			Page = null;
			_shellTabItem = null;
			_renderer = null;
		}

		protected virtual void HandlePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == ShellTabItem.TitleProperty.PropertyName)
				UpdateTabBarItem();
			else if (e.PropertyName == ShellTabItem.IconProperty.PropertyName)
				UpdateTabBarItem();
		}

		protected virtual void LoadPages()
		{
			var content = ((IShellTabItemController)ShellTabItem).GetOrCreateContent();
			Page = content;

			_renderer = Platform.CreateRenderer(content);
			Platform.SetRenderer(content, _renderer);

			var tracker = _context.CreatePageRendererTracker();
			tracker.IsRootPage = true; // default tracker requires this be set first
			tracker.Renderer = _renderer;

			_trackers[Page] = tracker;

			_renderer.SetElementSize(View.Bounds.ToRectangle().Size);

			PushViewController(_renderer.ViewController, false);

			var stack = ShellTabItem.Stack;
			for (int i = 1; i < stack.Count; i++)
			{
				PushPage(stack[i], false);
			}
		}

		protected virtual void OnNavigationRequested(object sender, NavigationRequestedEventArgs e)
		{
			switch (e.RequestType)
			{
				case NavigationRequestType.Push:
					OnPushRequested(e);
					break;

				case NavigationRequestType.Pop:
					OnPopRequested(e);
					break;

				case NavigationRequestType.PopToRoot:
					OnPopToRootRequested(e);
					break;

				case NavigationRequestType.Insert:
					break;

				case NavigationRequestType.Remove:
					break;
			}
		}

		protected virtual async void OnPopRequested(NavigationRequestedEventArgs e)
		{
			var page = e.Page;
			var animated = e.Animated;

			_popCompletionTask = new TaskCompletionSource<bool>();
			e.Task = _popCompletionTask.Task;

			_ignorePop = true;
			PopViewController(animated);
			_ignorePop = false;

			await _popCompletionTask.Task;

			DisposePage(page);
		}

		protected virtual async void OnPopToRootRequested(NavigationRequestedEventArgs e)
		{
			var animated = e.Animated;

			var task = new TaskCompletionSource<bool>();
			var pages = _shellTabItem.Stack.ToList();
			_completionTasks[_renderer.ViewController] = task;
			e.Task = task.Task;

			PopToRootViewController(animated);

			await e.Task;

			for (int i = pages.Count - 1; i >= 1; i--)
			{
				var page = pages[i];
				DisposePage(page);
			}
		}

		protected virtual void OnPushRequested(NavigationRequestedEventArgs e)
		{
			var page = e.Page;
			var animated = e.Animated;

			var taskSource = new TaskCompletionSource<bool>();
			PushPage(page, animated, taskSource);

			e.Task = taskSource.Task;
		}

		protected virtual void OnShellTabItemSet()
		{
			UpdateTabBarItem();
		}

		protected virtual void ResetTintColors()
		{
			if (_blurView == null)
				return;

			NavigationBar.SetBackgroundImage(_defaultBackgroundImage, UIBarMetrics.Default);
			NavigationBar.TintColor = _defaultTint;

			_blurView.RemoveFromSuperview();
			_colorView.RemoveFromSuperview();
		}

		protected virtual void SetTintColors(UIColor foreground, UIColor background)
		{
			if (_blurView == null)
			{
				_defaultBackgroundImage = NavigationBar.GetBackgroundImage(UIBarMetrics.Default);
				_defaultTint = NavigationBar.TintColor;

				var frame = NavigationBar.Frame;
				frame.Height += frame.Y;
				frame.Y = -frame.Y;

				var effect = UIBlurEffect.FromStyle(UIBlurEffectStyle.Regular);
				_blurView = new UIVisualEffectView(effect);
				_blurView.UserInteractionEnabled = false;
				_blurView.Frame = frame;

				_colorView = new UIView(frame);
				_colorView.UserInteractionEnabled = false;

				if (Forms.IsiOS11OrNewer)
				{
					_blurView.Layer.ShadowColor = UIColor.Black.CGColor;
					_blurView.Layer.ShadowOpacity = 1f;
					_blurView.Layer.ShadowRadius = 3;
				}
			}

			if (Forms.IsiOS11OrNewer)
			{
				NavigationBar.SetBackgroundImage(new UIImage(), UIBarMetrics.Default);

				NavigationBar.InsertSubview(_colorView, 0);
				NavigationBar.InsertSubview(_blurView, 0);

				_colorView.BackgroundColor = background;
			}
			else
			{
				NavigationBar.SetBackgroundImage(new UIImage(), UIBarMetrics.Default);

				NavigationBar.InsertSubview(_colorView, 0);
				NavigationBar.InsertSubview(_blurView, 0);

				_colorView.BackgroundColor = background;
			}

			NavigationBar.TintColor = foreground;
		}

		protected virtual void UpdatePageInsetPadding(Page page)
		{
			var setInsets = Shell.GetSetPaddingInsets(page);
			if (setInsets)
			{
				var navBarFrame = NavigationBar.Frame;
				var topPadding = navBarFrame.Y + navBarFrame.Height;

				var tabBarFrame = TabBarController.TabBar.Frame;
				var bottomPadding = View.Frame.Height - tabBarFrame.Top;

				page.Padding = new Thickness(0, topPadding, 0, bottomPadding);
			}
		}

		protected virtual async void UpdateTabBarItem()
		{
			Title = ShellTabItem.Title;
			var imageSource = ShellTabItem.Icon;
			UIImage icon = null;
			if (imageSource != null)
			{
				var source = Internals.Registrar.Registered.GetHandlerForObject<IImageSourceHandler>(imageSource);
				icon = await source.LoadImageAsync(imageSource);
			}
			TabBarItem = new UITabBarItem(ShellTabItem.Title, icon, null);
		}

		private void DisposePage(Page page)
		{
			var renderer = Platform.GetRenderer(page);
			if (renderer != null)
			{
				renderer.Dispose();
				page.ClearValue(Platform.RendererProperty);
			}

			if (_trackers.TryGetValue(page, out var tracker))
			{
				tracker.Dispose();
				_trackers.Remove(page);
			}
		}

		private Page PageForViewController(UIViewController viewController)
		{
			if (_renderer.ViewController == viewController)
				return Page;

			foreach (var child in ShellTabItem.Stack)
			{
				if (child == null)
					continue;
				var renderer = Platform.GetRenderer(child);
				if (viewController == renderer.ViewController)
					return child;
			}

			return null;
		}

		private void PushPage(Page page, bool animated, TaskCompletionSource<bool> completionSource = null)
		{
			var renderer = Platform.CreateRenderer(page);
			Platform.SetRenderer(page, renderer);

			var tracker = _context.CreatePageRendererTracker();
			tracker.Renderer = renderer;

			_trackers[page] = tracker;

			renderer.SetElementSize(View.Bounds.ToRectangle().Size);
			if (completionSource != null)
				_completionTasks[renderer.ViewController] = completionSource;

			PushViewController(renderer.ViewController, animated);
		}

		private async void SendPoppedOnCompletion(Task popTask)
		{
			if (popTask == null)
			{
				throw new ArgumentNullException(nameof(popTask));
			}

			await popTask;

			var poppedPage = _shellTabItem.Stack[_shellTabItem.Stack.Count - 1];
			((IShellTabItemController)_shellTabItem).SendPopped();
			DisposePage(poppedPage);
		}

		private bool ShouldPop()
		{
			var shellItem = _context.Shell.CurrentItem;
			var tab = shellItem?.CurrentItem;
			var stack = tab?.Stack.ToList();

			stack.RemoveAt(stack.Count - 1);

			return ((IShellController)_context.Shell).ProposeNavigation(ShellNavigationSource.PopEvent, shellItem, tab, stack, true);
		}

		private class GestureDelegate : UIGestureRecognizerDelegate
		{
			private readonly UINavigationController _parent;
			private readonly Func<bool> _shouldPop;

			public GestureDelegate(UINavigationController parent, Func<bool> shouldPop)
			{
				_parent = parent;
				_shouldPop = shouldPop;
			}

			public override bool ShouldBegin(UIGestureRecognizer recognizer)
			{
				if (_parent.ViewControllers.Length == 1)
					return false;
				return _shouldPop();
			}
		}

		private class NavDelegate : UINavigationControllerDelegate
		{
			private readonly ShellTabItemRenderer _self;

			public NavDelegate(ShellTabItemRenderer renderer)
			{
				_self = renderer;
			}

			public override void DidShowViewController(UINavigationController navigationController, [Transient] UIViewController viewController, bool animated)
			{
				var tasks = _self._completionTasks;
				var popTask = _self._popCompletionTask;

				if (tasks.TryGetValue(viewController, out var source))
				{
					source.TrySetResult(true);
					tasks.Remove(viewController);
				}
				else if (popTask != null)
				{
					popTask.TrySetResult(true);
				}
			}

			public override void WillShowViewController(UINavigationController navigationController, [Transient] UIViewController viewController, bool animated)
			{
				var page = _self.PageForViewController(viewController);

				bool navBarVisible = ShellAppearance.GetNavBarVisible(page);
				bool tabBarVisible = ShellAppearance.GetTabBarVisible(page);

				navigationController.SetNavigationBarHidden(!navBarVisible, true);
				viewController.TabBarController.TabBar.Hidden = !tabBarVisible;

				_self.UpdatePageInsetPadding(page);
			}
		}
	}
}