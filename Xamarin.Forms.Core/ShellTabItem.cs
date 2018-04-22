using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms
{
	[ContentProperty("Content")]
	public class ShellTabItem : FrameworkElement, IShellTabItemController
	{
		static readonly BindablePropertyKey MenuItemsPropertyKey =
			BindableProperty.CreateReadOnly(nameof(MenuItems), typeof(MenuItemCollection), typeof(ShellTabItem), null, defaultValueCreator: bo => new MenuItemCollection());

		public static readonly BindableProperty ContentProperty =
			BindableProperty.Create(nameof(Content), typeof(object), typeof(ShellTabItem), null, BindingMode.OneTime, propertyChanged: OnContentChanged);

		public static readonly BindableProperty ContentTemplateProperty =
			BindableProperty.Create(nameof(ContentTemplate), typeof(DataTemplate), typeof(ShellTabItem), null, BindingMode.OneTime);

		public static readonly BindableProperty IconProperty =
			BindableProperty.Create(nameof(Icon), typeof(ImageSource), typeof(ShellTabItem), null, BindingMode.OneTime);

		public static readonly BindableProperty IsEnabledProperty =
			BindableProperty.Create(nameof(IsEnabled), typeof(bool), typeof(ShellTabItem), true, BindingMode.OneWay);

		public static readonly BindableProperty MenuItemsProperty = MenuItemsPropertyKey.BindableProperty;

		public static readonly BindableProperty TitleProperty =
			BindableProperty.Create(nameof(Title), typeof(string), typeof(ShellTabItem), null, BindingMode.OneTime);

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
		}
		
		List<Page> _navStack = new List<Page> { null };
		IList<Element> _logicalChildren = new List<Element> ();
		ReadOnlyCollection<Element> _logicalChildrenReadOnly;

		public ShellTabItem()
		{
			((INotifyCollectionChanged)MenuItems).CollectionChanged += MenuItemsCollectionChanged;
			Navigation = new NavigationImpl(this);
		}

		event EventHandler<NavigationRequestedEventArgs> _navigationRequested;
		event EventHandler<NavigationRequestedEventArgs> IShellTabItemController.NavigationRequested
		{
			add { _navigationRequested += value; }
			remove { _navigationRequested -= value; }
		}

		internal override ReadOnlyCollection<Element> LogicalChildrenInternal => _logicalChildrenReadOnly ?? (_logicalChildrenReadOnly = new ReadOnlyCollection<Element>(_logicalChildren));

		public string Route
		{
			get { return Router.GetRoute(this); }
			set { Router.SetRoute(this, value); }
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

		public ImageSource Icon
		{
			get { return (ImageSource)GetValue(IconProperty); }
			set { SetValue(IconProperty, value); }
		}

		public bool IsEnabled
		{
			get { return (bool)GetValue(IsEnabledProperty); }
			set { SetValue(IsEnabledProperty, value); }
		}

		public MenuItemCollection MenuItems => (MenuItemCollection)GetValue(MenuItemsProperty);

		public IList<Page> Stack => _navStack;

		public string Title
		{
			get { return (string)GetValue(TitleProperty); }
			set { SetValue(TitleProperty, value); }
		}

		private void MenuItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			foreach (Element el in e.NewItems)
				OnChildAdded(el);
			foreach (Element el in e.OldItems)
				OnChildRemoved(el);
		}

		void IShellTabItemController.SendPopped()
		{
			if (_navStack.Count <= 1)
				throw new Exception("Nav Stack consistency error");

			var last = _navStack[_navStack.Count - 1];
			_navStack.Remove(last);

			RemovePage(last);
		}

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
				result = (Page)template.CreateContent(content, this);
			}

			if (result != null)
				OnChildAdded(result);

			return result;
		}

		protected virtual IReadOnlyList<Page> GetNavigationStack()
		{
			return _navStack;
		}

		protected virtual Task OnPushAsync(Page page, bool animated)
		{
			var args = new NavigationRequestedEventArgs(page, animated) {
				RequestType = NavigationRequestType.Push
			};

			_navStack.Add(page);
			AddPage(page);
			_navigationRequested?.Invoke(this, args);
			return args.Task;
		}

		protected async virtual Task<Page> OnPopAsync(bool animated)
		{
			if (_navStack.Count <= 1)
				throw new InvalidOperationException("Can't pop last page off stack");

			var page = _navStack[_navStack.Count - 1];
			var args = new NavigationRequestedEventArgs(page, animated)
			{
				RequestType = NavigationRequestType.Pop
			};

			_navStack.Remove(page);
			_navigationRequested?.Invoke(this, args);
			await args.Task;
			RemovePage(page);
			return page;
		}

		protected virtual async Task OnPopToRootAsync(bool animated)
		{
			if (_navStack.Count <= 1)
				return;

			var page = _navStack[_navStack.Count - 1];
			var args = new NavigationRequestedEventArgs(page, animated)
			{
				RequestType = NavigationRequestType.PopToRoot
			};
			
			_navigationRequested?.Invoke(this, args);
			var oldStack = _navStack;
			_navStack = new List<Page> { null };

			await args.Task;

			for (int i = 1; i < oldStack.Count; i++)
			{
				RemovePage(oldStack[i]);
			}
		}

		protected virtual void OnInsertPageBefore(Page page, Page before)
		{
			var index = _navStack.IndexOf(page);
			if (index == -1)
				throw new ArgumentException("Page not found in nav stack");

			var args = new NavigationRequestedEventArgs(page, before, false)
			{
				RequestType = NavigationRequestType.PopToRoot
			};
			_navStack.Insert(index, page);
			AddPage(page);
			_navigationRequested?.Invoke(this, args);
		}

		protected virtual void OnRemovePage(Page page)
		{
			if (!_navStack.Remove(page))
				return;

			RemovePage(page);
			var args = new NavigationRequestedEventArgs(page, false)
			{
				RequestType = NavigationRequestType.Remove
			};
			_navigationRequested?.Invoke(this, args);
		}

		void AddPage(Page page)
		{
			_logicalChildren.Add(page);
			OnChildAdded(page);
		}

		void RemovePage(Page page)
		{
			if (_logicalChildren.Remove(page))
				OnChildRemoved(page);
		}

		public class NavigationImpl : NavigationProxy
		{
			private readonly ShellTabItem _owner;

			public NavigationImpl(ShellTabItem owner)
			{
				_owner = owner;
			}

			protected override IReadOnlyList<Page> GetNavigationStack()
			{
				return _owner.GetNavigationStack();
			}

			protected override Task OnPushAsync(Page page, bool animated)
			{
				return _owner.OnPushAsync(page, animated);
			}

			protected override Task<Page> OnPopAsync(bool animated)
			{
				return _owner.OnPopAsync(animated);
			}

			protected override Task OnPopToRootAsync(bool animated)
			{
				return _owner.OnPopToRootAsync(animated);
			}

			protected override void OnInsertPageBefore(Page page, Page before)
			{
				_owner.OnInsertPageBefore(page, before);
			}

			protected override void OnRemovePage(Page page)
			{
				_owner.OnRemovePage(page);
			}
		}
	}
}