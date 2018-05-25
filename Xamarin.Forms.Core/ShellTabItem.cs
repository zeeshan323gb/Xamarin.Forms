using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms
{
	[ContentProperty("Content")]
	public class ShellTabItem : BaseShellItem, IShellTabItemController
	{
		#region PropertyKeys

		private static readonly BindablePropertyKey MenuItemsPropertyKey =
			BindableProperty.CreateReadOnly(nameof(MenuItems), typeof(MenuItemCollection), typeof(ShellTabItem), null, defaultValueCreator: bo => new MenuItemCollection());

		#endregion PropertyKeys

		#region IShellTabItemController

		private Page _contentCache;

		event EventHandler<NavigationRequestedEventArgs> IShellTabItemController.NavigationRequested
		{
			add { _navigationRequested += value; }
			remove { _navigationRequested -= value; }
		}

		private event EventHandler<NavigationRequestedEventArgs> _navigationRequested;

		Page IShellTabItemController.CurrentPage
		{
			get
			{
				if (_navStack.Count > 1)
					return _navStack[_navStack.Count - 1];
				return ((IShellTabItemController)this).RootPage;
			}
		}

		Page IShellTabItemController.RootPage => _contentCache ?? (Content as Page);

		Page IShellTabItemController.GetOrCreateContent()
		{
			var template = ContentTemplate;
			var content = Content;

			Page result = null;
			if (template == null)
			{
				if (content is Page page)
					result = page;
			}
			else
			{
				result = _contentCache ?? (Page)template.CreateContent(content, this);
				_contentCache = result;
			}

			if (result != null && result.Parent != this)
				OnChildAdded(result);

			return result;
		}

		void IShellTabItemController.RecyclePage(Page page)
		{
			if (_contentCache == page)
			{
				OnChildRemoved(page);
				_contentCache = null;
			}
		}

		void IShellTabItemController.SendPopped()
		{
			if (_navStack.Count <= 1)
				throw new Exception("Nav Stack consistency error");

			var last = _navStack[_navStack.Count - 1];
			_navStack.Remove(last);

			RemovePage(last);

			SendUpdateCurrentState(ShellNavigationSource.Pop);
		}

		#endregion IShellTabItemController

		public static readonly BindableProperty ContentProperty =
			BindableProperty.Create(nameof(Content), typeof(object), typeof(ShellTabItem), null, BindingMode.OneTime, propertyChanged: OnContentChanged);

		public static readonly BindableProperty ContentTemplateProperty =
			BindableProperty.Create(nameof(ContentTemplate), typeof(DataTemplate), typeof(ShellTabItem), null, BindingMode.OneTime);

		public static readonly BindableProperty MenuItemsProperty = MenuItemsPropertyKey.BindableProperty;

		private IList<Element> _logicalChildren = new List<Element>();

		private ReadOnlyCollection<Element> _logicalChildrenReadOnly;

		private List<Page> _navStack = new List<Page> { null };

		public event EventHandler Reselected;

		public ShellTabItem()
		{
			((INotifyCollectionChanged)MenuItems).CollectionChanged += MenuItemsCollectionChanged;
			Navigation = new NavigationImpl(this);
		}

		public object Content
		{
			get { return GetValue(ContentProperty); }
			set { SetValue(ContentProperty, value); }
		}

		public DataTemplate ContentTemplate
		{
			get { return (DataTemplate)GetValue(ContentTemplateProperty); }
			set { SetValue(ContentTemplateProperty, value); }
		}

		public MenuItemCollection MenuItems => (MenuItemCollection)GetValue(MenuItemsProperty);

		public IReadOnlyList<Page> Stack => _navStack;

		internal override ReadOnlyCollection<Element> LogicalChildrenInternal => _logicalChildrenReadOnly ?? (_logicalChildrenReadOnly = new ReadOnlyCollection<Element>(_logicalChildren));

		private Shell Shell => Parent?.Parent as Shell;

		private ShellItem ShellItem => Parent as ShellItem;

		public static implicit operator ShellTabItem(TemplatedPage page)
		{
			var result = new ShellTabItem();

			result.Content = page;
			result.SetBinding(TitleProperty, new Binding("Title", BindingMode.OneWay));
			result.SetBinding(IconProperty, new Binding("Icon", BindingMode.OneWay));

			return result;
		}

		public virtual async Task GoToAsync(List<string> routes, IDictionary<string, string> queryData, bool animate)
		{
			if (routes == null || routes.Count == 0)
			{
				await Navigation.PopToRootAsync(animate);
				return;
			}

			for (int i = 0; i < routes.Count; i++)
			{
				bool isLast = i == routes.Count - 1;
				var route = routes[i];
				var navPage = _navStack.Count > i + 1 ? _navStack[i + 1] : null;

				if (navPage != null)
				{
					if (Routing.GetRoute(navPage) == route)
					{
						Shell.ApplyQueryAttributes(navPage, queryData, isLast);
						continue;
					}

					while (_navStack.Count > i + 1)
					{
						await OnPopAsync(false);
					}
				}

				var content = Routing.GetOrCreateContent(route) as Page;
				if (content == null)
					break;

				Shell.ApplyQueryAttributes(content, queryData, isLast);
				await OnPushAsync(content, i == routes.Count - 1 && animate);
			}

			SendAppearanceChanged();
		}

		protected virtual IReadOnlyList<Page> GetNavigationStack() => _navStack;

		protected virtual void OnInsertPageBefore(Page page, Page before)
		{
			var index = _navStack.IndexOf(before);
			if (index == -1)
				throw new ArgumentException("Page not found in nav stack");

			var args = new NavigationRequestedEventArgs(page, before, false)
			{
				RequestType = NavigationRequestType.Insert
			};
			_navStack.Insert(index, page);
			AddPage(page);
			SendAppearanceChanged();
			_navigationRequested?.Invoke(this, args);

			SendUpdateCurrentState(ShellNavigationSource.Insert);
		}

		protected async virtual Task<Page> OnPopAsync(bool animated)
		{
			if (_navStack.Count <= 1)
				throw new InvalidOperationException("Can't pop last page off stack");

			List<Page> stack = _navStack.ToList();
			stack.Remove(stack.Last());
			var allow = ((IShellController)Shell).ProposeNavigation(
				ShellNavigationSource.Pop,
				Parent as ShellItem,
				this,
				stack,
				true
			);

			if (!allow)
				return null;

			var page = _navStack[_navStack.Count - 1];
			var args = new NavigationRequestedEventArgs(page, animated)
			{
				RequestType = NavigationRequestType.Pop
			};

			_navStack.Remove(page);
			SendAppearanceChanged();
			_navigationRequested?.Invoke(this, args);
			if (args.Task != null)
				await args.Task;
			RemovePage(page);

			SendUpdateCurrentState(ShellNavigationSource.Pop);

			return page;
		}

		protected virtual async Task OnPopToRootAsync(bool animated)
		{
			if (_navStack.Count <= 1)
				return;

			var allow = ((IShellController)Shell).ProposeNavigation(
				ShellNavigationSource.PopToRoot,
				Parent as ShellItem,
				this,
				null,
				true
			);

			if (!allow)
				return;

			var page = _navStack[_navStack.Count - 1];
			var args = new NavigationRequestedEventArgs(page, animated)
			{
				RequestType = NavigationRequestType.PopToRoot
			};

			_navigationRequested?.Invoke(this, args);
			var oldStack = _navStack;
			_navStack = new List<Page> { null };
			SendAppearanceChanged();

			if (args.Task != null)
				await args.Task;

			for (int i = 1; i < oldStack.Count; i++)
			{
				RemovePage(oldStack[i]);
			}

			SendUpdateCurrentState(ShellNavigationSource.PopToRoot);
		}

		protected virtual Task OnPushAsync(Page page, bool animated)
		{
			List<Page> stack = _navStack.ToList();
			stack.Add(page);
			var allow = ((IShellController)Shell).ProposeNavigation(
				ShellNavigationSource.Push,
				ShellItem,
				this,
				stack,
				true
			);

			if (!allow)
				return Task.FromResult(true);

			var args = new NavigationRequestedEventArgs(page, animated)
			{
				RequestType = NavigationRequestType.Push
			};

			_navStack.Add(page);
			AddPage(page);
			SendAppearanceChanged();
			_navigationRequested?.Invoke(this, args);

			SendUpdateCurrentState(ShellNavigationSource.Push);

			if (args.Task == null)
				return Task.FromResult(true);
			return args.Task;
		}

		protected virtual void OnRemovePage(Page page)
		{
			if (!_navStack.Remove(page))
				return;

			SendAppearanceChanged();
			RemovePage(page);
			var args = new NavigationRequestedEventArgs(page, false)
			{
				RequestType = NavigationRequestType.Remove
			};
			_navigationRequested?.Invoke(this, args);

			SendUpdateCurrentState(ShellNavigationSource.Remove);
		}

		private static void OnContentChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var shellTabItem = (ShellTabItem)bindable;
			// This check is wrong but will work for testing
			if (shellTabItem.ContentTemplate == null)
			{
				// deparent old item
				if (oldValue is Page oldElement)
					shellTabItem.OnChildRemoved(oldElement);

				// make sure LogicalChildren collection stays consisten
				shellTabItem._logicalChildren.Clear();
				if (newValue is Page newElement)
				{
					shellTabItem._logicalChildren.Add((Element)newValue);
					// parent new item
					shellTabItem.OnChildAdded(newElement);
				}
			}

			if (shellTabItem.Parent is ShellItem shellItem)
			{
				shellItem?.SendStructureChanged();
			}
		}

		private void AddPage(Page page)
		{
			_logicalChildren.Add(page);
			OnChildAdded(page);
		}

		private void MenuItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.NewItems != null)
			{
				foreach (Element el in e.NewItems)
					OnChildAdded(el);
			}

			if (e.OldItems != null)
			{
				foreach (Element el in e.OldItems)
					OnChildRemoved(el);
			}
		}

		private void RemovePage(Page page)
		{
			if (_logicalChildren.Remove(page))
				OnChildRemoved(page);
		}

		private void SendAppearanceChanged() => ((IShellController)Parent?.Parent)?.AppearanceChanged(this, false);

		private void SendUpdateCurrentState(ShellNavigationSource source)
		{
			if (Parent?.Parent is IShellController shell)
			{
				shell?.UpdateCurrentState(source);
			}
		}

		public class NavigationImpl : NavigationProxy
		{
			private readonly ShellTabItem _owner;

			public NavigationImpl(ShellTabItem owner) => _owner = owner;

			protected override IReadOnlyList<Page> GetNavigationStack() => _owner.GetNavigationStack();

			protected override void OnInsertPageBefore(Page page, Page before) => _owner.OnInsertPageBefore(page, before);

			protected override Task<Page> OnPopAsync(bool animated) => _owner.OnPopAsync(animated);

			protected override Task OnPopToRootAsync(bool animated) => _owner.OnPopToRootAsync(animated);

			protected override Task OnPushAsync(Page page, bool animated) => _owner.OnPushAsync(page, animated);

			protected override void OnRemovePage(Page page) => _owner.OnRemovePage(page);
		}
	}
}