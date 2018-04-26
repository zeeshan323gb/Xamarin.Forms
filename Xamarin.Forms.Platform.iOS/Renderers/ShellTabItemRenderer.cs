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

		void IShellTabItemRenderer.ResetAppearance()
		{
			_appearanceTracker.ResetAppearance(this);
		}

		void IShellTabItemRenderer.SetAppearance(ShellAppearance appearance)
		{
			_appearanceTracker.SetAppearance(this, appearance);
		}

		#endregion IShellTabItemRenderer

		private readonly IShellContext _context;

		private Dictionary<UIViewController, TaskCompletionSource<bool>> _completionTasks =
			new Dictionary<UIViewController, TaskCompletionSource<bool>>();

		private bool _disposed;
		private bool _ignorePop;
		private Page _page;
		private TaskCompletionSource<bool> _popCompletionTask;
		private IVisualElementRenderer _renderer;
		private ShellTabItem _shellTabItem;
		private IShellNavBarAppearanceTracker _appearanceTracker;

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

			_appearanceTracker.UpdateLayout(this);
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
				_appearanceTracker.Dispose();
				_shellTabItem.PropertyChanged -= HandlePropertyChanged;
				((IShellTabItemController)_shellTabItem).NavigationRequested -= OnNavigationRequested;
				DisposePage(Page);
			}

			// must be set null prior to _shellTabItem to ensure weak ref page gets cleared
			Page = null;
			_shellTabItem = null;
			_appearanceTracker = null;
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
					OnInsertRequested(e);
					break;

				case NavigationRequestType.Remove:
					OnRemoveRequested(e);
					break;
			}
		}

		protected virtual void OnRemoveRequested(NavigationRequestedEventArgs e)
		{
			var page = e.Page;

			var renderer = Platform.GetRenderer(page);

			if (renderer != null)
			{
				if (renderer.ViewController == TopViewController)
				{
					e.Animated = false;
					OnPopRequested(e);
				}
				ViewControllers = ViewControllers.Remove(renderer.ViewController);
				DisposePage(page);
			}
		}

		protected virtual void OnInsertRequested(NavigationRequestedEventArgs e)
		{
			var page = e.Page;
			var before = e.BeforePage;

			var beforeRenderer = Platform.GetRenderer(before);

			var renderer = Platform.CreateRenderer(page);
			Platform.SetRenderer(page, renderer);

			var tracker = _context.CreatePageRendererTracker();
			tracker.Renderer = renderer;

			_trackers[page] = tracker;

			renderer.SetElementSize(View.Bounds.ToRectangle().Size);

			ViewControllers.Insert(ViewControllers.IndexOf(beforeRenderer.ViewController), renderer.ViewController);
			
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
			_appearanceTracker = _context.CreateNavBarAppearanceTracker();
			UpdateTabBarItem();
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
			if (_trackers.TryGetValue(page, out var tracker))
			{
				tracker.Dispose();
				_trackers.Remove(page);
			}

			var renderer = Platform.GetRenderer(page);
			if (renderer != null)
			{
				renderer.Dispose();
				page.ClearValue(Platform.RendererProperty);
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
			}
		}
	}
}