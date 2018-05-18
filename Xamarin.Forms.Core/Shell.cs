using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms
{
	[ContentProperty("Items")]
	public class Shell : Page, IShellController, IShellAppearanceTracker
	{
		#region Attached Properties

		public static readonly BindableProperty NavBarVisibleProperty =
			BindableProperty.CreateAttached("NavBarVisible", typeof(bool), typeof(Shell), true);

		public static readonly BindableProperty TabBarVisibleProperty =
			BindableProperty.CreateAttached("TabBarVisible", typeof(bool), typeof(Shell), true);

		public static bool GetNavBarVisible(BindableObject obj) => (bool)obj.GetValue(NavBarVisibleProperty);

		public static bool GetTabBarVisible(BindableObject obj) => (bool)obj.GetValue(TabBarVisibleProperty);

		public static void SetNavBarVisible(BindableObject obj, bool value) => obj.SetValue(NavBarVisibleProperty, value);

		public static void SetTabBarVisible(BindableObject obj, bool value) => obj.SetValue(TabBarVisibleProperty, value);

		#region Appearance Properties

		public static readonly BindableProperty ShellBackgroundColorProperty =
			BindableProperty.CreateAttached("ShellBackgroundColor", typeof(Color), typeof(Shell), Color.Default,
				propertyChanged: OnShellColorValueChanged);

		public static readonly BindableProperty ShellDisabledColorProperty =
			BindableProperty.CreateAttached("ShellDisabledColor", typeof(Color), typeof(Shell), Color.Default,
				propertyChanged: OnShellColorValueChanged);

		public static readonly BindableProperty ShellForegroundColorProperty =
			BindableProperty.CreateAttached("ShellForegroundColor", typeof(Color), typeof(Shell), Color.Default,
				propertyChanged: OnShellColorValueChanged);

		public static readonly BindableProperty ShellTabBarBackgroundColorProperty = 
			BindableProperty.CreateAttached("ShellTabBarBackgroundColor", typeof(Color), typeof(Shell), Color.Default,
				propertyChanged: OnShellColorValueChanged);

		public static readonly BindableProperty ShellTabBarDisabledColorProperty = 
			BindableProperty.CreateAttached("ShellTabBarDisabledColor", typeof(Color), typeof(Shell), Color.Default,
				propertyChanged: OnShellColorValueChanged);

		public static readonly BindableProperty ShellTabBarForegroundColorProperty = 
			BindableProperty.CreateAttached("ShellTabBarForegroundColor", typeof(Color), typeof(Shell), Color.Default,
				propertyChanged: OnShellColorValueChanged);

		public static readonly BindableProperty ShellTabBarTitleColorProperty = 
			BindableProperty.CreateAttached("ShellTabBarTitleColor", typeof(Color), typeof(Shell), Color.Default,
				propertyChanged: OnShellColorValueChanged);

		public static readonly BindableProperty ShellTabBarUnselectedColorProperty = 
			BindableProperty.CreateAttached("ShellTabBarUnselectedColor", typeof(Color), typeof(Shell), Color.Default,
				propertyChanged: OnShellColorValueChanged);

		public static readonly BindableProperty ShellTitleColorProperty = 
			BindableProperty.CreateAttached("ShellTitleColor", typeof(Color), typeof(Shell), Color.Default,
				propertyChanged: OnShellColorValueChanged);

		public static readonly BindableProperty ShellUnselectedColorProperty = 
			BindableProperty.CreateAttached("ShellUnselectedColor", typeof(Color), typeof(Shell), Color.Default,
				propertyChanged: OnShellColorValueChanged);

		public static Color GetShellBackgroundColor(BindableObject obj) => (Color)obj.GetValue(ShellBackgroundColorProperty);

		public static Color GetShellDisabledColor(BindableObject obj) => (Color)obj.GetValue(ShellDisabledColorProperty);

		public static Color GetShellForegroundColor(BindableObject obj) => (Color)obj.GetValue(ShellForegroundColorProperty);

		public static Color GetShellTabBarBackgroundColor(BindableObject obj) => (Color)obj.GetValue(ShellTabBarBackgroundColorProperty);

		public static Color GetShellTabBarDisabledColor(BindableObject obj) => (Color)obj.GetValue(ShellTabBarDisabledColorProperty);

		public static Color GetShellTabBarForegroundColor(BindableObject obj) => (Color)obj.GetValue(ShellTabBarForegroundColorProperty);

		public static Color GetShellTabBarTitleColor(BindableObject obj) => (Color)obj.GetValue(ShellTabBarTitleColorProperty);

		public static Color GetShellTabBarUnselectedColor(BindableObject obj) => (Color)obj.GetValue(ShellTabBarUnselectedColorProperty);

		public static Color GetShellTitleColor(BindableObject obj) => (Color)obj.GetValue(ShellTitleColorProperty);

		public static Color GetShellUnselectedColor(BindableObject obj) => (Color)obj.GetValue(ShellUnselectedColorProperty);

		public static void SetShellBackgroundColor(BindableObject obj, Color value) => obj.SetValue(ShellBackgroundColorProperty, value);

		public static void SetShellDisabledColor(BindableObject obj, Color value) => obj.SetValue(ShellDisabledColorProperty, value);

		public static void SetShellForegroundColor(BindableObject obj, Color value) => obj.SetValue(ShellForegroundColorProperty, value);

		public static void SetShellTabBarBackgroundColor(BindableObject obj, Color value) => obj.SetValue(ShellTabBarBackgroundColorProperty, value);

		public static void SetShellTabBarDisabledColor(BindableObject obj, Color value) => obj.SetValue(ShellTabBarDisabledColorProperty, value);

		public static void SetShellTabBarForegroundColor(BindableObject obj, Color value) => obj.SetValue(ShellTabBarForegroundColorProperty, value);

		public static void SetShellTabBarTitleColor(BindableObject obj, Color value) => obj.SetValue(ShellTabBarTitleColorProperty, value);

		public static void SetShellTabBarUnselectedColor(BindableObject obj, Color value) => obj.SetValue(ShellTabBarUnselectedColorProperty, value);

		public static void SetShellTitleColor(BindableObject obj, Color value) => obj.SetValue(ShellTitleColorProperty, value);

		public static void SetShellUnselectedColor(BindableObject obj, Color value) => obj.SetValue(ShellUnselectedColorProperty, value);

		private static void OnShellColorValueChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var item = (Element)bindable;

			if (item.Parent is IShellAppearanceTracker tracker)
			{
				tracker.AppearanceChanged(item, true);
			}
		}

		#endregion Appearance Properties

		public static readonly BindableProperty BackButtonBehaviorProperty =
			BindableProperty.CreateAttached("BackButtonBehavior", typeof(BackButtonBehavior), typeof(Shell), null, BindingMode.OneTime);

		public static readonly BindableProperty FlyoutBehaviorProperty =
			BindableProperty.CreateAttached("FlyoutBehavior", typeof(FlyoutBehavior), typeof(Shell), FlyoutBehavior.Popover);

		public static readonly BindableProperty SearchHandlerProperty =
			BindableProperty.CreateAttached("SearchHandler", typeof(SearchHandler), typeof(Shell), null, BindingMode.OneTime);

		public static readonly BindableProperty SetPaddingInsetsProperty =
			BindableProperty.CreateAttached("SetPaddingInsets", typeof(bool), typeof(Shell), false);

		public static BackButtonBehavior GetBackButtonBehavior(BindableObject obj) => (BackButtonBehavior)obj.GetValue(BackButtonBehaviorProperty);

		public static FlyoutBehavior GetFlyoutBehavior(BindableObject obj) => (FlyoutBehavior)obj.GetValue(FlyoutBehaviorProperty);

		public static SearchHandler GetSearchHandler(BindableObject obj) => (SearchHandler)obj.GetValue(SearchHandlerProperty);

		public static bool GetSetPaddingInsets(BindableObject obj) => (bool)obj.GetValue(SetPaddingInsetsProperty);

		public static void SetBackButtonBehavior(BindableObject obj, BackButtonBehavior behavior) => obj.SetValue(BackButtonBehaviorProperty, behavior);

		public static void SetFlyoutBehavior(BindableObject obj, FlyoutBehavior value) => obj.SetValue(FlyoutBehaviorProperty, value);

		public static void SetSearchHandler(BindableObject obj, SearchHandler handler) => obj.SetValue(SearchHandlerProperty, handler);

		public static void SetSetPaddingInsets(BindableObject obj, bool value) => obj.SetValue(SetPaddingInsetsProperty, value);

		#endregion Attached Properties

		#region PropertyKeys

		private static readonly BindablePropertyKey CurrentStatePropertyKey =
			BindableProperty.CreateReadOnly(nameof(CurrentState), typeof(ShellNavigationState), typeof(Shell), null);

		private static readonly BindablePropertyKey ItemsPropertyKey = BindableProperty.CreateReadOnly(nameof(Items), typeof(ShellItemCollection), typeof(Shell), null,
				defaultValueCreator: bo => new ShellItemCollection { Inner = new ElementCollection<ShellItem>(((Shell)bo).InternalChildren) });

		private static readonly BindablePropertyKey MenuItemsPropertyKey =
			BindableProperty.CreateReadOnly(nameof(MenuItems), typeof(MenuItemCollection), typeof(Shell), null, defaultValueCreator: bo => new MenuItemCollection());

		private static readonly BindablePropertyKey ShellNavigationStatePropertyKey =
			BindableProperty.CreateReadOnly(nameof(ShellNavigationState), typeof(ShellNavigationState), typeof(Shell), null);

		#endregion PropertyKeys

		#region IShellController

		private List<Tuple<IAppearanceObserver, Element>> _observers = new List<Tuple<IAppearanceObserver, Element>>();

		event EventHandler IShellController.HeaderChanged
		{
			add { _headerChanged += value; }
			remove { _headerChanged -= value; }
		}

		event EventHandler IShellController.StructureChanged
		{
			add { _structureChanged += value; }
			remove { _structureChanged -= value; }
		}

		private event EventHandler _headerChanged;

		private event EventHandler _structureChanged;

		View IShellController.FlyoutHeader
		{
			get { return FlyoutHeaderView; }
		}

		void IShellController.AddAppearanceObserver(IAppearanceObserver observer, Element pivot)
		{
			_observers.Add(Tuple.Create(observer, pivot));
			observer.OnAppearanceChanged(GetShellAppearanceForPivot(pivot));
		}

		ShellNavigationState IShellController.GetNavigationState(ShellItem item, ShellTabItem tab, bool includeStack = true)
		{
			return GetNavigationState(item, tab, includeStack ? tab.Stack.ToList() : null);
		}

		bool IShellController.ProposeNavigation(ShellNavigationSource source, ShellItem item, ShellTabItem tab, IList<Page> stack, bool canCancel)
		{
			var proposedState = GetNavigationState(item, tab, stack);
			return ProposeNavigation(source, proposedState, canCancel);
		}

		bool IShellController.RemoveAppearanceObserver(IAppearanceObserver observer)
		{
			for (int i = 0; i < _observers.Count; i++)
			{
				if (_observers[i].Item1 == observer)
				{
					_observers.RemoveAt(i);
					return true;
				}
			}
			return false;
		}

		void IShellController.UpdateCurrentState(ShellNavigationSource source)
		{
			var oldState = CurrentState;
			var shellItem = CurrentItem;
			var tab = shellItem?.CurrentItem;
			var stack = tab?.Stack;
			var result = GetNavigationState(shellItem, tab, stack.ToList());

			SetValueFromRenderer(CurrentStatePropertyKey, result);

			OnNavigated(new ShellNavigatedEventArgs(oldState, CurrentState, source));
		}

		#endregion IShellController

		#region IShellAppearanceTracker

		void IShellAppearanceTracker.AppearanceChanged(Element source, bool appearanceSet)
		{
			// here we wish to notify every element whose "pivot line" contains the source
			// To do that we first need to find the leaf node in the line, and then walk up
			// to see if we find the source on the way up.

			// If this is not an appearanceSet event but just a structural change, then we only
			// need walk up from the source to look for the pivot as items below the change
			// can't be affected by it

			foreach (var t in _observers)
			{
				var observer = t.Item1;
				var pivot = t.Item2;

				Element target;
				Element leaf;
				if (appearanceSet)
				{
					leaf = pivot;
					if (leaf is Shell shell)
					{
						leaf = shell.CurrentItem;
					}
					if (leaf is ShellItem shellItem)
					{
						leaf = shellItem.CurrentItem;
					}
					if (leaf is IShellTabItemController shellTabItem)
					{
						// this is the same as .Last but easier and will add in the root if not null
						// it generally wont be null but this is just in case
						leaf = shellTabItem.CurrentPage ?? leaf;
					}

					target = source;
				}
				else
				{
					leaf = source;
					target = pivot;
				}

				while (!Application.IsApplicationOrNull(leaf))
				{
					if (leaf == target)
					{
						observer.OnAppearanceChanged(GetShellAppearanceForPivot(pivot));
						break;
					}
					leaf = leaf.Parent;
				}
			}
		}

		#endregion IShellAppearanceTracker

		public static readonly BindableProperty CurrentItemProperty =
			BindableProperty.Create(nameof(CurrentItem), typeof(ShellItem), typeof(Shell), null, BindingMode.TwoWay,
				propertyChanged: OnCurrentItemChanged);

		public static readonly BindableProperty CurrentStateProperty = CurrentStatePropertyKey.BindableProperty;

		public static readonly BindableProperty FlyoutBackgroundColorProperty =
				BindableProperty.Create(nameof(FlyoutBackgroundColor), typeof(Color), typeof(Shell), Color.Default, BindingMode.OneTime);

		public static readonly BindableProperty FlyoutHeaderBehaviorProperty =
			BindableProperty.Create(nameof(FlyoutHeaderBehavior), typeof(FlyoutHeaderBehavior), typeof(Shell), FlyoutHeaderBehavior.Default, BindingMode.OneTime);

		public static readonly BindableProperty FlyoutHeaderProperty =
			BindableProperty.Create(nameof(FlyoutHeader), typeof(object), typeof(Shell), null, BindingMode.OneTime,
				propertyChanged: OnFlyoutHeaderChanged);

		public static readonly BindableProperty FlyoutHeaderTemplateProperty =
			BindableProperty.Create(nameof(FlyoutHeaderTemplate), typeof(DataTemplate), typeof(Shell), null, BindingMode.OneTime,
				propertyChanged: OnFlyoutHeaderTemplateChanged);

		public static readonly BindableProperty FlyoutIsPresentedProperty = BindableProperty.Create(nameof(FlyoutIsPresented), typeof(bool), typeof(Shell), false, BindingMode.TwoWay);

		public static readonly BindableProperty GroupHeaderTemplateProperty =
			BindableProperty.Create(nameof(GroupHeaderTemplate), typeof(DataTemplate), typeof(Shell), null, BindingMode.OneTime);

		public static readonly BindableProperty ItemsProperty = ItemsPropertyKey.BindableProperty;

		public static readonly BindableProperty ItemTemplateProperty =
			BindableProperty.Create(nameof(ItemTemplate), typeof(DataTemplate), typeof(Shell), null, BindingMode.OneTime);

		public static readonly BindableProperty MenuItemsProperty = MenuItemsPropertyKey.BindableProperty;

		public static readonly BindableProperty MenuItemTemplateProperty =
			BindableProperty.Create(nameof(MenuItemTemplate), typeof(DataTemplate), typeof(Shell), null, BindingMode.OneTime);

		public static readonly BindableProperty ShellNavigationStateProperty = ShellNavigationStatePropertyKey.BindableProperty;

		private ShellNavigatedEventArgs _accumulatedEvent;
		private bool _accumulateNavigatedEvents;
		private View _flyoutHeaderView;

		public Shell()
		{
			((INotifyCollectionChanged)Items).CollectionChanged += (s, e) => SendStructureChanged();
		}

		public event EventHandler<ShellNavigatedEventArgs> Navigated;

		public event EventHandler<ShellNavigatingEventArgs> Navigating;

		public ShellItem CurrentItem
		{
			get { return (ShellItem)GetValue(CurrentItemProperty); }
			set { SetValue(CurrentItemProperty, value); }
		}

		public ShellNavigationState CurrentState => (ShellNavigationState)GetValue(CurrentStateProperty);

		public Color FlyoutBackgroundColor
		{
			get { return (Color)GetValue(FlyoutBackgroundColorProperty); }
			set { SetValue(FlyoutBackgroundColorProperty, value); }
		}

		public FlyoutBehavior FlyoutBehavior
		{
			get { return (FlyoutBehavior)GetValue(FlyoutBehaviorProperty); }
			set { SetValue(FlyoutBehaviorProperty, value); }
		}

		public object FlyoutHeader
		{
			get { return GetValue(FlyoutHeaderProperty); }
			set { SetValue(FlyoutHeaderProperty, value); }
		}

		public FlyoutHeaderBehavior FlyoutHeaderBehavior
		{
			get { return (FlyoutHeaderBehavior)GetValue(FlyoutHeaderBehaviorProperty); }
			set { SetValue(FlyoutHeaderBehaviorProperty, value); }
		}

		public DataTemplate FlyoutHeaderTemplate
		{
			get { return (DataTemplate)GetValue(FlyoutHeaderTemplateProperty); }
			set { SetValue(FlyoutHeaderTemplateProperty, value); }
		}

		public bool FlyoutIsPresented
		{
			get { return (bool)GetValue(FlyoutIsPresentedProperty); }
			set { SetValue(FlyoutIsPresentedProperty, value); }
		}

		public DataTemplate GroupHeaderTemplate
		{
			get { return (DataTemplate)GetValue(GroupHeaderTemplateProperty); }
			set { SetValue(GroupHeaderTemplateProperty, value); }
		}

		public ShellItemCollection Items => (ShellItemCollection)GetValue(ItemsProperty);

		public DataTemplate ItemTemplate
		{
			get { return (DataTemplate)GetValue(ItemTemplateProperty); }
			set { SetValue(ItemTemplateProperty, value); }
		}

		public MenuItemCollection MenuItems => (MenuItemCollection)GetValue(MenuItemsProperty);

		public DataTemplate MenuItemTemplate
		{
			get { return (DataTemplate)GetValue(MenuItemTemplateProperty); }
			set { SetValue(MenuItemTemplateProperty, value); }
		}

		public string Route
		{
			get { return Routing.GetRoute(this); }
			set { Routing.SetRoute(this, value); }
		}

		public string RouteHost { get; set; }

		public string RouteScheme { get; set; } = "app";

		public ShellNavigationState ShellNavigationState => (ShellNavigationState)GetValue(ShellNavigationStateProperty);

		private View FlyoutHeaderView
		{
			get { return _flyoutHeaderView; }
			set
			{
				if (_flyoutHeaderView == value)
					return;

				if (_flyoutHeaderView != null)
					OnChildRemoved(_flyoutHeaderView);
				_flyoutHeaderView = value;
				if (_flyoutHeaderView != null)
					OnChildAdded(_flyoutHeaderView);
				_headerChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		public bool AddToBackStack(ShellNavigationState state)
		{
			throw new NotImplementedException();
		}

		public async Task GoToAsync(ShellNavigationState state, bool animate = true)
		{
			// FIXME: This should not be none, we need to compute the delta and set flags correctly
			var accept = ProposeNavigation(ShellNavigationSource.Unknown, state, true);
			if (!accept)
				return;

			_accumulateNavigatedEvents = true;

			var uri = state.Location;
			var queryString = uri.Query;
			var queryData = ParseQueryString(queryString);
			var path = uri.AbsolutePath;

			path = path.TrimEnd('/');

			var parts = path.Substring(1).Split('/');

			if (path.Length < 2)
				throw new InvalidOperationException("Path must be at least 2 items long in Shell navigation");

			var shellRoute = parts[0];
			var shellItemRoute = parts[1];
			var shellTabItemRoute = parts.Length > 2 ? parts[2] : null;

			var expectedShellRoute = Routing.GetRoute(this) ?? string.Empty;
			if (expectedShellRoute != shellRoute)
				throw new NotImplementedException();

			ApplyQueryAttributes(this, queryData, false);

			bool changedTab = false;
			// Find ShellItem
			foreach (var shellItem in Items)
			{
				if (Routing.GetRoute(shellItem) == shellItemRoute)
				{
					ApplyQueryAttributes(shellItem, queryData, parts.Length == 2);
					if (CurrentItem != shellItem)
					{
						changedTab = true;
						SetValueFromRenderer(CurrentItemProperty, shellItem);
					}

					break;
				}
			}

			// Find ShellTabItem
			ShellTabItem selectedTabItem = null;
			if (shellTabItemRoute != null)
			{
				var shellItem = CurrentItem;
				foreach (var tabItem in shellItem.Items)
				{
					if (Routing.GetRoute(tabItem) == shellTabItemRoute)
					{
						ApplyQueryAttributes(tabItem, queryData, parts.Length == 3);
						if (shellItem.CurrentItem != tabItem)
						{
							changedTab = true;
							shellItem.SetValueFromRenderer(ShellItem.CurrentItemProperty, tabItem);
						}

						selectedTabItem = tabItem;
						break;
					}
				}
			}

			// Send out navigation to ShellTabItem
			if (selectedTabItem != null)
			{
				List<string> navParts = new List<string>();
				if (parts.Length > 3)
				{
					for (int i = 3; i < parts.Length; i++)
					{
						navParts.Add(parts[i]);
					}
				}
				await selectedTabItem.GoToAsync(navParts, queryData, !changedTab);
			}

			_accumulateNavigatedEvents = false;

			// this can be null in the event that no navigation actually took place!
			if (_accumulatedEvent != null)
				OnNavigated(_accumulatedEvent);
		}

		internal static void ApplyQueryAttributes(Element element, IDictionary<string, string> query, bool isLastItem)
		{
			if (query.Count == 0)
				return;

			string prefix = "";
			if (!isLastItem)
			{
				var route = Routing.GetRoute(element);
				if (string.IsNullOrEmpty(route))
					return;
				prefix = route + ".";
			}

			var typeInfo = element.GetType().GetTypeInfo();
			object[] effectAttributes = typeInfo.GetCustomAttributes(typeof(QueryPropertyAttribute), true).ToArray();

			if (effectAttributes.Length == 0)
				return;

			foreach (var a in effectAttributes)
			{
				if (a is QueryPropertyAttribute attrib)
				{
					if (query.TryGetValue(prefix + attrib.QueryId, out var value))
					{
						PropertyInfo prop = null;

						while (prop == null && typeInfo != null)
						{
							prop = typeInfo.GetDeclaredProperty(attrib.Name);
							typeInfo = typeInfo.BaseType.GetTypeInfo();
						}

						if (prop != null && prop.CanWrite && prop.SetMethod.IsPublic)
						{
							prop.SetValue(element, value);
						}
					}
				}
			}
		}

		internal void SendStructureChanged()
		{
			_structureChanged?.Invoke(this, EventArgs.Empty);
		}

		protected override bool OnBackButtonPressed()
		{
			var currentTabItem = CurrentItem?.CurrentItem;
			if (currentTabItem != null && currentTabItem.Stack.Count > 1)
			{
				currentTabItem.Navigation.PopAsync();
				return true;
			}
			return false;
		}

		protected override void OnChildAdded(Element child)
		{
			base.OnChildAdded(child);

			if (child is ShellItem shellItem && CurrentItem == null && !(child is ShellItem.MenuShellItem))
			{
				SetValueFromRenderer(CurrentItemProperty, shellItem);
			}
		}

		protected override void OnChildRemoved(Element child)
		{
			base.OnChildRemoved(child);

			if (child == CurrentItem)
			{
				if (Items.Count > 0)
					SetValueFromRenderer(CurrentItemProperty, Items[0]);
			}
		}

		protected virtual void OnNavigated(ShellNavigatedEventArgs args)
		{
			if (_accumulateNavigatedEvents)
			{
				_accumulatedEvent = args;
			}
			else
			{
				Navigated?.Invoke(this, args);
				System.Diagnostics.Debug.WriteLine("Navigated: " + args.Current.Location);
			}
		}

		protected virtual void OnNavigating(ShellNavigatingEventArgs args)
		{
			Navigating?.Invoke(this, args);
		}

		private static string GenerateQueryString(Dictionary<string, string> queryData)
		{
			if (queryData.Count == 0)
				return string.Empty;
			string result = "?";

			bool addAnd = false;
			foreach (var kvp in queryData)
			{
				if (addAnd)
					result += "&";
				result += kvp.Key + "=" + kvp.Value;
				addAnd = true;
			}

			return result;
		}

		private static void GetQueryStringData(Element element, bool isLastItem, Dictionary<string, string> result)
		{
			string prefix = string.Empty;
			if (!isLastItem)
			{
				var route = Routing.GetRoute(element);
				if (string.IsNullOrEmpty(route))
					return;
				prefix = route + ".";
			}

			var typeInfo = element.GetType().GetTypeInfo();
			object[] effectAttributes = typeInfo.GetCustomAttributes(typeof(QueryPropertyAttribute), true).ToArray();

			if (effectAttributes.Length == 0)
				return;

			foreach (var a in effectAttributes)
			{
				if (a is QueryPropertyAttribute attrib)
				{
					PropertyInfo prop = null;

					while (prop == null && typeInfo != null)
					{
						prop = typeInfo.GetDeclaredProperty(attrib.Name);
						typeInfo = typeInfo.BaseType.GetTypeInfo();
					}

					if (prop != null && prop.CanRead && prop.GetMethod.IsPublic)
					{
						var val = (string)prop.GetValue(element);
						var key = isLastItem ? prefix + attrib.QueryId : attrib.QueryId;
						result[key] = val;
					}
				}
			}
		}

		private static void OnCurrentItemChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var oldItem = (IShellItemController)oldValue;
			var newItem = (IShellItemController)newValue;
			oldItem?.UpdateChecked();
			newItem?.UpdateChecked();

			var shell = (Shell)bindable;
			((IShellAppearanceTracker)shell).AppearanceChanged(shell, false);
			((IShellController)shell).UpdateCurrentState(ShellNavigationSource.ShellItemChanged);
		}

		private static void OnFlyoutHeaderChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var shell = (Shell)bindable;
			shell.OnFlyoutHeaderChanged(oldValue, newValue);
		}

		private static void OnFlyoutHeaderTemplateChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var shell = (Shell)bindable;
			shell.OnFlyoutHeaderTemplateChanged((DataTemplate)oldValue, (DataTemplate)newValue);
		}

		private static Dictionary<string, string> ParseQueryString(string query)
		{
			if (query.StartsWith("?"))
				query = query.Substring(1);
			Dictionary<string, string> lookupDict = new Dictionary<string, string>();
			if (query == null)
				return lookupDict;
			foreach (var part in query.Split('&'))
			{
				var p = part.Split('=');
				if (p.Length != 2)
					continue;
				lookupDict[p[0]] = p[1];
			}

			return lookupDict;
		}

		private ShellNavigationState GetNavigationState(ShellItem item, ShellTabItem tab, IList<Page> stack)
		{
			var state = RouteScheme + "://" + RouteHost + "/" + Route + "/";
			Dictionary<string, string> queryData = new Dictionary<string, string>();

			GetQueryStringData(this, false, queryData);

			if (item != null)
			{
				state += Routing.GetRouteStringForElement(item);
				state += "/";

				GetQueryStringData(item, tab == null, queryData);

				if (tab != null)
				{
					state += Routing.GetRouteStringForElement(tab);
					state += "/";

					GetQueryStringData(tab, stack == null || stack.Count <= 1, queryData);

					if (stack != null)
					{
						for (int i = 1; i < stack.Count; i++)
						{
							var page = stack[i];
							state += Routing.GetRouteStringForElement(page);
							GetQueryStringData(page, i == stack.Count - 1, queryData);
							if (i < stack.Count - 1)
								state += "/";
						}
					}
				}
			}

			var queryString = GenerateQueryString(queryData);

			return state + queryString;
		}

		private ShellAppearance GetShellAppearanceForPivot(Element pivot)
		{
			// this algorithm is pretty simple
			// 1) Get the "CurrentPage" by walking down from the pivot
			//		Walking down goes Shell -> ShellItem -> ShellTabItem -> ShellTabItem.Stack.Last
			// 2) Walk up from the pivot to the root Shell. Stop walking as soon as you find a ShellAppearance and return
			// 3) If nothing found, return null

			if (pivot is Shell shell)
			{
				pivot = shell.CurrentItem;
			}
			if (pivot is ShellItem shellItem)
			{
				pivot = shellItem.CurrentItem;
			}
			if (pivot is IShellTabItemController shellTabItem)
			{
				// this is the same as .Last but easier and will add in the root if not null
				// it generally wont be null but this is just in case
				pivot = shellTabItem.CurrentPage ?? pivot;
			}

			bool anySet = false;
			ShellAppearance result = new ShellAppearance();
			// Now we walk up
			while (!Application.IsApplicationOrNull(pivot))
			{
				if (result.Ingest(pivot))
					anySet = true;

				pivot = pivot.Parent;
			}
			
			if (anySet)
			{
				result.MakeComplete();
				return result;
			}
			return null;
		}

		private void OnFlyoutHeaderChanged(object oldVal, object newVal)
		{
			if (FlyoutHeaderTemplate == null)
			{
				if (newVal is View newFlyoutHeader)
					FlyoutHeaderView = newFlyoutHeader;
				else
					FlyoutHeaderView = null;
			}
			else
			{
				FlyoutHeaderView.BindingContext = newVal;
			}
		}

		private void OnFlyoutHeaderTemplateChanged(DataTemplate oldValue, DataTemplate newValue)
		{
			if (newValue == null)
			{
				if (FlyoutHeader is View flyoutHeaderView)
					FlyoutHeaderView = flyoutHeaderView;
				else
					FlyoutHeaderView = null;
			}
			else
			{
				var newHeaderView = (View)newValue.CreateContent(FlyoutHeader, this);
				newHeaderView.BindingContext = FlyoutHeader;
				FlyoutHeaderView = newHeaderView;
			}
		}

		private bool ProposeNavigation(ShellNavigationSource source, ShellNavigationState proposedState, bool canCancel)
		{
			if (_accumulateNavigatedEvents)
				return true;

			var navArgs = new ShellNavigatingEventArgs(CurrentState, proposedState, source, canCancel);

			OnNavigating(navArgs);
			System.Diagnostics.Debug.WriteLine("Proposed: " + proposedState.Location);
			return !navArgs.Cancelled;
		}
	}
}