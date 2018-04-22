using System.ComponentModel;
using Xamarin.Forms.Internals;
using UIKit;
using System;
using Foundation;
using System.Threading.Tasks;
using System.Collections.Generic;
using ObjCRuntime;
using System.Linq;

namespace Xamarin.Forms.Platform.iOS
{
	public class ShellTabItemRenderer : UINavigationController, IShellTabItemRenderer
	{
		private bool _ignorePop;
		private ShellTabItem _shellTabItem;
		private IVisualElementRenderer _renderer;
		private TaskCompletionSource<bool> _popCompletionTask;
		private Dictionary<UIViewController, TaskCompletionSource<bool>> _completionTasks = 
			new Dictionary<UIViewController, TaskCompletionSource<bool>>();

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
		}

		public ShellTabItemRenderer()
		{
			Delegate = new NavDelegate(this);
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

		public override UIViewController PopViewController(bool animated)
		{
			if (!_ignorePop)
			{
				_popCompletionTask = new TaskCompletionSource<bool>();
				SendPoppedOnCompletion(_popCompletionTask.Task);
			}

			return base.PopViewController(animated);
		}

		private async void SendPoppedOnCompletion (Task popTask)
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

		private void DisposePage (Page page)
		{
			var renderer = Platform.GetRenderer(page);
			renderer.Dispose();
			page.ClearValue(Platform.RendererProperty);
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

		protected virtual void OnPushRequested(NavigationRequestedEventArgs e)
		{
			var page = e.Page;
			var animated = e.Animated;

			var renderer = Platform.CreateRenderer(page);
			Platform.SetRenderer(page, renderer);

			renderer.SetElementSize(View.Bounds.ToRectangle().Size);

			var taskSource = new TaskCompletionSource<bool>();
			_completionTasks[renderer.ViewController] = taskSource;

			PushViewController(renderer.ViewController, animated);

			e.Task = taskSource.Task;
		}

		protected virtual void HandlePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == ShellTabItem.TitleProperty.PropertyName)
				UpdateTabBarItem();
			else if (e.PropertyName == ShellTabItem.IconProperty.PropertyName)
				UpdateTabBarItem();
		}

		public UIViewController ViewController => this;

		public Page Page { get; private set; }

		protected virtual void OnShellTabItemSet ()
		{
			UpdateTabBarItem();
		}

		protected virtual void LoadPages ()
		{
			var content = ((IShellTabItemController)ShellTabItem).GetOrCreateContent();
			Page = content;

			_renderer = Platform.CreateRenderer(content);
			Platform.SetRenderer(content, _renderer);

			_renderer.SetElementSize(View.Bounds.ToRectangle().Size);

			PushViewController(_renderer.ViewController, false);
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

		public override void ViewDidLayoutSubviews()
		{
			base.ViewDidLayoutSubviews();

			_renderer.Element.Layout(View.Bounds.ToRectangle());
		}

		bool _disposed;
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing && !_disposed)
			{
				_shellTabItem.PropertyChanged -= HandlePropertyChanged;
				_shellTabItem = null;
				_renderer.Dispose();
				_renderer = null;
				_disposed = true;
				Page = null;
			}
		}

		void IShellTabItemRenderer.SetTintColors(UIColor foreground, UIColor background)
		{
			SetTintColors(foreground, background);
		}

		void IShellTabItemRenderer.ResetTintColors()
		{
			ResetTintColors();
		}

		UIColor _defaultTint;
		UIImage _defaultBackgroundImage;
		UIImage _defaultShadowImage;
		UIView _blurView;
		UIView _colorView;
		protected virtual void SetTintColors(UIColor foreground, UIColor background)
		{
			if (_blurView == null)
			{
				_defaultBackgroundImage = NavigationBar.GetBackgroundImage(UIBarMetrics.Default);
				_defaultShadowImage = NavigationBar.ShadowImage;
				_defaultTint = NavigationBar.TintColor;

				var frame = NavigationBar.Bounds;
				frame.Y -= 20;
				frame.Height += 20;

				var effect = UIBlurEffect.FromStyle(UIBlurEffectStyle.Regular);
				_blurView = new UIVisualEffectView(effect);
				_blurView.Frame = frame;

				_blurView.Layer.ShadowColor = UIColor.Black.CGColor;
				_blurView.Layer.ShadowOpacity = 1f;
				_blurView.Layer.ShadowRadius = 3;

				_colorView = new UIView(frame);
			}

			NavigationBar.SetBackgroundImage(new UIImage(), UIBarMetrics.Default);
			NavigationBar.ShadowImage = new UIImage();

			NavigationBar.InsertSubview(_colorView, 0);
			NavigationBar.InsertSubview(_blurView, 0);

			_colorView.BackgroundColor = background;
			NavigationBar.TintColor = foreground;
		}

		protected virtual void ResetTintColors()
		{
			if (_blurView == null)
				return;

			NavigationBar.ShadowImage = _defaultShadowImage;
			NavigationBar.SetBackgroundImage(_defaultBackgroundImage, UIBarMetrics.Default);
			NavigationBar.TintColor = _defaultTint;

			_blurView.RemoveFromSuperview();
			_colorView.RemoveFromSuperview();
		}
	}
}