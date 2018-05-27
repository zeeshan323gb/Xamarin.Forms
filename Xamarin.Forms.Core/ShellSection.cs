using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms
{
	[ContentProperty ("Items")]
	public class ShellSection : BaseShellItem, IShellSectionController
	{
		#region PropertyKeys

		private static readonly BindablePropertyKey ItemsPropertyKey =
			BindableProperty.CreateReadOnly(nameof(Items), typeof(ShellContentCollection), typeof(ShellSection), null,
				defaultValueCreator: bo => new ShellContentCollection());
		private static readonly BindablePropertyKey MenuItemsPropertyKey =
					BindableProperty.CreateReadOnly(nameof(MenuItems), typeof(MenuItemCollection), typeof(ShellSection), null,
				defaultValueCreator: bo => new MenuItemCollection());

		#endregion PropertyKeys

		#region IShellSectionController

		event EventHandler<NavigationRequestedEventArgs> IShellSectionController.NavigationRequested
		{
			add { _navigationRequested += value; }
			remove { _navigationRequested -= value; }
		}

		private event EventHandler<NavigationRequestedEventArgs> _navigationRequested;

		Element IShellSectionController.PresentedElement
		{
			get
			{
				if (_navStack.Count > 1)
					return _navStack[_navStack.Count - 1];
				return CurrentItem;
			}
		}

		void IShellSectionController.SendPopped()
		{
			if (_navStack.Count <= 1)
				throw new Exception("Nav Stack consistency error");

			var last = _navStack[_navStack.Count - 1];
			_navStack.Remove(last);

			RemovePage(last);

			SendUpdateCurrentState(ShellNavigationSource.Pop);
		}

		void IShellSectionController.UpdateChecked()
		{
			var shell = Parent?.Parent as Shell;
			bool isChecked = shell?.CurrentItem?.CurrentItem == this;
			UpdateChildrenChecked(isChecked, Items, CurrentItem);
		}

		Task IShellSectionController.GoToPart(List<string> parts, Dictionary<string, string> queryData)
		{
			var shellContentRoute = parts[0];

			foreach (var shellContent in Items)
			{
				if (Routing.CompareRoutes(shellContent.Route, shellContentRoute, out var isImplicit))
				{
					Shell.ApplyQueryAttributes(shellContent, queryData, parts.Count == 1);

					if (CurrentItem != shellContent)
						SetValueFromRenderer(CurrentItemProperty, shellContent);

					if (!isImplicit)
						parts.RemoveAt(0);
					if (parts.Count > 0)
					{
						return GoToAsync(parts, queryData, false);
					}
					break;
				}
			}

			return Task.FromResult(true);
		}

		#endregion IShellSectionController

		public static readonly BindableProperty CurrentItemProperty =
			BindableProperty.Create(nameof(CurrentItem), typeof(ShellContent), typeof(ShellSection), null, BindingMode.TwoWay,
				propertyChanged: OnCurrentItemChanged);

		public static readonly BindableProperty ItemsProperty = ItemsPropertyKey.BindableProperty;

		public static readonly BindableProperty MenuItemsProperty = MenuItemsPropertyKey.BindableProperty;

		private IList<Element> _logicalChildren = new List<Element>();

		private ReadOnlyCollection<Element> _logicalChildrenReadOnly;

		private List<Page> _navStack = new List<Page> { null };

		public ShellSection()
		{
			((INotifyCollectionChanged)Items).CollectionChanged += ItemsCollectionChanged;
			((INotifyCollectionChanged)MenuItems).CollectionChanged += MenuItemsCollectionChanged;
			Navigation = new NavigationImpl(this);
		}

		public ShellContent CurrentItem
		{
			get { return (ShellContent)GetValue(CurrentItemProperty); }
			set { SetValue(CurrentItemProperty, value); }
		}

		public ShellContentCollection Items => (ShellContentCollection)GetValue(ItemsProperty);

		public MenuItemCollection MenuItems => (MenuItemCollection)GetValue(MenuItemsProperty);

		public IReadOnlyList<Page> Stack => _navStack;

		internal override ReadOnlyCollection<Element> LogicalChildrenInternal => _logicalChildrenReadOnly ?? (_logicalChildrenReadOnly = new ReadOnlyCollection<Element>(_logicalChildren));

		private Shell Shell => Parent?.Parent as Shell;

		private ShellItem ShellItem => Parent as ShellItem;

#if DEBUG
		[Obsolete("Please dont use this in core code... its SUPER hard to debug when this happens", true)]
#endif
		public static implicit operator ShellSection(ShellContent shellContent)
		{
			var shellSection = new ShellSection();

			var contentRoute = shellContent.Route;

			shellSection.Route = Routing.GenerateImplicitRoute(contentRoute);

			shellSection.Items.Add(shellContent);
			shellSection.SetBinding(TitleProperty, new Binding("Title", BindingMode.OneWay, source: shellContent));
			shellSection.SetBinding(IconProperty, new Binding("Icon", BindingMode.OneWay, source: shellContent));
			return shellSection;
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

		internal void SendStructureChanged()
		{
			if (Parent?.Parent is Shell shell)
			{
				shell.SendStructureChanged();
			}
		}

		protected virtual IReadOnlyList<Page> GetNavigationStack() => _navStack;

		protected override void OnChildAdded(Element child)
		{
			base.OnChildAdded(child);
			if (CurrentItem == null)
				SetValueFromRenderer(CurrentItemProperty, child);
		}

		protected override void OnChildRemoved(Element child)
		{
			base.OnChildRemoved(child);
			if (CurrentItem == child)
			{
				if (Items.Count == 0)
					ClearValue(CurrentItemProperty);
				else
					SetValueFromRenderer(CurrentItemProperty, Items[0]);
			}
		}

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
				CurrentItem,
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
				CurrentItem,
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
				CurrentItem,
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

		private static void OnCurrentItemChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var shellContent = (ShellSection)bindable;

			if (shellContent.Parent?.Parent is IShellController shell)
			{
				shell.UpdateCurrentState(ShellNavigationSource.ShellSectionChanged);
			}

			((IShellSectionController)bindable).UpdateChecked();
			shellContent.SendStructureChanged();
			((IShellController)shellContent?.Parent?.Parent)?.AppearanceChanged(shellContent, false);
		}

		private void AddPage(Page page)
		{
			_logicalChildren.Add(page);
			OnChildAdded(page);
		}

		private void ItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.NewItems != null)
			{
				foreach (Element element in e.NewItems)
					OnChildAdded(element);
			}

			if (e.OldItems != null)
			{
				foreach (Element element in e.OldItems)
					OnChildRemoved(element);
			}

			SendStructureChanged();
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
			private readonly ShellSection _owner;

			public NavigationImpl(ShellSection owner) => _owner = owner;

			protected override IReadOnlyList<Page> GetNavigationStack() => _owner.GetNavigationStack();

			protected override void OnInsertPageBefore(Page page, Page before) => _owner.OnInsertPageBefore(page, before);

			protected override Task<Page> OnPopAsync(bool animated) => _owner.OnPopAsync(animated);

			protected override Task OnPopToRootAsync(bool animated) => _owner.OnPopToRootAsync(animated);

			protected override Task OnPushAsync(Page page, bool animated) => _owner.OnPushAsync(page, animated);

			protected override void OnRemovePage(Page page) => _owner.OnRemovePage(page);
		}
	}
}