using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public class ShellPageRendererTracker : IShellPageRendererTracker
	{
		private readonly IShellContext _context;
		private bool _disposed = false;
		private WeakReference<IVisualElementRenderer> _rendererRef;

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

				((INotifyCollectionChanged)Page.ToolbarItems).CollectionChanged += OnToolbarItemsChanged;
				UpdateToolbarItems();
			}
		}

		private UINavigationItem NavigationItem => Renderer.ViewController.NavigationItem;
		private Page Page { get; set; }

		private UIBarButtonItem[] ToolbarItems
		{
			get => Renderer.ViewController.ToolbarItems;
			set => Renderer.ViewController.ToolbarItems = value;
		}

		private void OnToolbarItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			UpdateToolbarItems();
		}

		private async void UpdateToolbarItems()
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

			if (IsRootPage)
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
					((INotifyCollectionChanged)Page.ToolbarItems).CollectionChanged -= OnToolbarItemsChanged;
				}

				Page = null;
				_rendererRef = null;
				_disposed = true;
			}
		}

		#endregion IDisposable Support
	}
}