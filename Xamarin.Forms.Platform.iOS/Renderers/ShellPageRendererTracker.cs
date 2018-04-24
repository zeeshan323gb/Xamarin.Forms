using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public class ShellPageRendererTracker : IShellPageRendererTracker
	{
		private readonly IShellContext _context;
		private bool _disposed = false;
		private WeakReference<IVisualElementRenderer> _rendererRef;
		private BackButtonBehavior _backButtonBehavior;

		public ShellPageRendererTracker(IShellContext context)
		{
			_context = context;
		}

		public bool IsRootPage { get; set; }

		public IVisualElementRenderer Renderer
		{
			get
			{
				_rendererRef.TryGetTarget(out var target);
				return target;
			}
			set
			{
				_rendererRef = new WeakReference<IVisualElementRenderer>(value);
				Page = value.Element as Page;
				OnRendererSet();
			}
		}

		private BackButtonBehavior BackButtonBehavior => _backButtonBehavior;

		private async void SetBackButtonBehavior(BackButtonBehavior value)
		{
			if (_backButtonBehavior == value)
				return;

			if (_backButtonBehavior != null)
			{
				_backButtonBehavior.PropertyChanged -= OnBackButtonBehaviorPropertyChanged;
			}

			_backButtonBehavior = value;

			if (_backButtonBehavior != null)
			{
				_backButtonBehavior.PropertyChanged += OnBackButtonBehaviorPropertyChanged;
				await UpdateToolbarItems();
			}
		}

		protected virtual async void OnBackButtonBehaviorPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == BackButtonBehavior.CommandParameterProperty.PropertyName)
				return;
			await UpdateToolbarItems();
		}

		protected virtual async void OnRendererSet()
		{
			Page.PropertyChanged += OnPagePropertyChanged;
			((INotifyCollectionChanged)Page.ToolbarItems).CollectionChanged += OnToolbarItemsChanged;
			SetBackButtonBehavior(Shell.GetBackButtonBehavior(Page));
			await UpdateToolbarItems();
		}

		protected virtual void OnPagePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Shell.BackButtonBehaviorProperty.PropertyName)
			{
				SetBackButtonBehavior(Shell.GetBackButtonBehavior(Page));
			}
		}

		private UINavigationItem NavigationItem => Renderer.ViewController.NavigationItem;
		private Page Page { get; set; }

		private UIBarButtonItem[] ToolbarItems
		{
			get => Renderer.ViewController.ToolbarItems;
			set => Renderer.ViewController.ToolbarItems = value;
		}

		private async void OnToolbarItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			await UpdateToolbarItems();
		}

		protected virtual async Task UpdateToolbarItems()
		{
			if (NavigationItem.RightBarButtonItems != null)
			{
				for (var i = 0; i < NavigationItem.RightBarButtonItems.Length; i++)
					NavigationItem.RightBarButtonItems[i].Dispose();
			}

			List<UIBarButtonItem> items = new List<UIBarButtonItem>();
			foreach (var item in Page.ToolbarItems)
			{
				items.Add(item.ToUIBarButtonItem());
			}

			items.Reverse();
			NavigationItem.SetRightBarButtonItems(items.ToArray(), false);

			if (BackButtonBehavior != null)
			{
				var behavior = BackButtonBehavior;
				var command = behavior.Command;
				var commandParameter = behavior.CommandParameter;
				var image = behavior.IconOverride;
				var enabled = behavior.IsEnabled;
				if (image == null)
				{
					var text = BackButtonBehavior.TextOverride;
					NavigationItem.LeftBarButtonItem = 
						new UIBarButtonItem(text, UIBarButtonItemStyle.Plain, (s, e) => command?.Execute(commandParameter)) { Enabled = enabled };
				}
				else
				{
					var source = Internals.Registrar.Registered.GetHandlerForObject<IImageSourceHandler>(image);
					var icon = await source.LoadImageAsync(image);
					NavigationItem.LeftBarButtonItem = 
						new UIBarButtonItem(icon, UIBarButtonItemStyle.Plain, (s, e) => command?.Execute(commandParameter)) { Enabled = enabled };
				}
				
			}
			else if (IsRootPage)
			{
				ImageSource image = "3bar.png";
				var source = Internals.Registrar.Registered.GetHandlerForObject<IImageSourceHandler>(image);
				var icon = await source.LoadImageAsync(image);
				NavigationItem.LeftBarButtonItem = new UIBarButtonItem(icon, UIBarButtonItemStyle.Plain, OnMenuButtonPressed);
			}
		}

		private void OnMenuButtonPressed(object sender, EventArgs e)
		{
			_context.Shell.SetValueFromRenderer(Shell.FlyoutIsPresentedProperty, true);
		}

		#region IDisposable Support

		~ShellPageRendererTracker()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					Page.PropertyChanged -= OnPagePropertyChanged;
					((INotifyCollectionChanged)Page.ToolbarItems).CollectionChanged -= OnToolbarItemsChanged;
				}

				Page = null;
				SetBackButtonBehavior (null);
				_rendererRef = null;
				_disposed = true;
			}
		}

		#endregion IDisposable Support
	}
}