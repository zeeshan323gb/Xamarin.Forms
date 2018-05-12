using Android.Support.V4.App;
using System;
using System.Collections.Specialized;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.Android
{
	public class ShellItemRendererBase : Fragment, IShellItemRenderer
	{
		#region IShellItemRenderer

		Fragment IShellItemRenderer.Fragment => this;

		ShellItem IShellItemRenderer.ShellItem
		{
			get { return ShellItem; }
			set { ShellItem = value; }
		}

#endregion IShellItemRenderer
		private ShellTabItem _currentTabItem;

		public ShellItemRendererBase(IShellContext shellContext)
		{
			ShellContext = shellContext;
		}

		protected ShellTabItem CurrentTabItem
		{
			get { return _currentTabItem; }
			set
			{
				_currentTabItem = value;
				if (value != null)
				{
					OnCurrentTabItemChanged();
				}
			}
		}

		protected IShellContext ShellContext { get; }

		protected ShellItem ShellItem { get; private set; }

		protected virtual void HookEvents(ShellItem shellItem)
		{
			shellItem.PropertyChanged += OnShellItemPropertyChanged;
			((INotifyCollectionChanged)shellItem.Items).CollectionChanged += OnItemsChanged;
			CurrentTabItem = shellItem.CurrentItem;

			foreach (var shellTabItem in shellItem.Items)
			{
				HookTabEvents(shellTabItem);
			}
		}

		private void OnItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			foreach (ShellTabItem tab in e.OldItems)
				UnhookTabEvents(tab);

			foreach (ShellTabItem tab in e.NewItems)
				HookTabEvents(tab);
		}

		protected virtual void HookTabEvents(ShellTabItem shellTabItem)
		{
			((IShellTabItemController)shellTabItem).NavigationRequested += OnNavigationRequested;
		}

		protected virtual void OnCurrentTabItemChanged()
		{
		}

		protected virtual void OnNavigationRequested(object sender, NavigationRequestedEventArgs e)
		{
		}

		protected virtual void OnShellItemPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == ShellItem.CurrentItemProperty.PropertyName)
				CurrentTabItem = ShellItem.CurrentItem;
		}

		protected virtual void UnhookEvents(ShellItem shellItem)
		{
			foreach (var shellTabItem in shellItem.Items)
			{
				UnhookTabEvents(shellTabItem);
			}

			ShellItem.PropertyChanged -= OnShellItemPropertyChanged;
			CurrentTabItem = null;
		}

		protected virtual void UnhookTabEvents(ShellTabItem shellTabItem)
		{
			((IShellTabItemController)shellTabItem).NavigationRequested -= OnNavigationRequested;
		}
	}
}