using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms
{
	[ContentProperty("Items")]
	public class Shell : Page, IShellController
	{
		#region Attached Properties
		public static readonly BindableProperty SearchHandlerProperty =
			BindableProperty.CreateAttached("SearchHandler", typeof(SearchHandler), typeof(Shell), null, BindingMode.OneTime);

		public static readonly BindableProperty BackButtonBehaviorProperty =
			BindableProperty.CreateAttached("BackButtonBehavior", typeof(BackButtonBehavior), typeof(Shell), null, BindingMode.OneTime);

		public static readonly BindableProperty FlyoutBehaviorProperty = 
			BindableProperty.CreateAttached("FlyoutBehavior", typeof(FlyoutBehavior), typeof(Shell), FlyoutBehavior.Popover);

		public static SearchHandler GetSearchHandler(BindableObject obj)
		{
			return (SearchHandler)obj.GetValue(SearchHandlerProperty);
		}

		public static void SetSearchHandler(BindableObject obj, SearchHandler handler)
		{
			obj.SetValue(SearchHandlerProperty, handler);
		}

		public static BackButtonBehavior GetBackButtonBehavior(BindableObject obj)
		{
			return (BackButtonBehavior)obj.GetValue(BackButtonBehaviorProperty);
		}

		public static void SetBackButtonBehavior(BindableObject obj, BackButtonBehavior behavior)
		{
			obj.SetValue(BackButtonBehaviorProperty, behavior);
		}

		public static FlyoutBehavior GetFlyoutBehavior(BindableObject obj)
		{
			return (FlyoutBehavior)obj.GetValue(FlyoutBehaviorProperty);
		}

		public static void SetFlyoutBehavior(BindableObject obj, FlyoutBehavior value)
		{
			obj.SetValue(FlyoutBehaviorProperty, value);
		}
		#endregion

		#region Static Methods
		public static void RegisterRoute(string route, Func<TemplatedPage> factory)
		{

		}

		public static void RegisterRoute(string route, Type pageType)
		{

		}
		#endregion

		static readonly BindablePropertyKey ItemsPropertyKey =
			BindableProperty.CreateReadOnly(nameof(Items), typeof(ShellItemCollection), typeof(Shell), null,
				defaultValueCreator: bo => new ShellItemCollection { Inner = new ElementCollection<ShellItem>(((Shell)bo).InternalChildren) });

		static readonly BindablePropertyKey MenuItemsPropertyKey =
			BindableProperty.CreateReadOnly(nameof(MenuItems), typeof(MenuItemCollection), typeof(Shell), null, defaultValueCreator: bo => new MenuItemCollection());

		static readonly BindablePropertyKey ShellNavigationStatePropertyKey =
			BindableProperty.CreateReadOnly(nameof(ShellNavigationState), typeof(ShellNavigationState), typeof(Shell), null);

		public static readonly BindableProperty CurrentItemProperty =
			BindableProperty.Create(nameof(CurrentItem), typeof(ShellItem), typeof(Shell), null, BindingMode.TwoWay);

		public static readonly BindableProperty ItemsProperty = ItemsPropertyKey.BindableProperty;

		public static readonly BindableProperty ShellNavigationStateProperty = ShellNavigationStatePropertyKey.BindableProperty;

		public static readonly BindableProperty MenuItemsProperty = MenuItemsPropertyKey.BindableProperty;

		public static readonly BindableProperty FlyoutHeaderProperty = 
			BindableProperty.Create(nameof(FlyoutHeader), typeof(object), typeof(Shell), null, BindingMode.OneTime,
				propertyChanged: OnFlyoutHeaderChanged);

		public static readonly BindableProperty FlyoutHeaderTemplateProperty = 
			BindableProperty.Create(nameof(FlyoutHeaderTemplate), typeof(DataTemplate), typeof(Shell), null, BindingMode.OneTime,
				propertyChanged: OnFlyoutHeaderTemplateChanged);

		public static readonly BindableProperty FlyoutHeaderBehaviorProperty = 
			BindableProperty.Create(nameof(FlyoutHeaderBehavior), typeof(FlyoutHeaderBehavior), typeof(Shell), FlyoutHeaderBehavior.Default, BindingMode.OneTime);

		public static readonly BindableProperty ItemTemplateProperty = 
			BindableProperty.Create(nameof(ItemTemplate), typeof(DataTemplate), typeof(Shell), null, BindingMode.OneTime);

		public static readonly BindableProperty GroupHeaderTemplateProperty = 
			BindableProperty.Create(nameof(GroupHeaderTemplate), typeof(DataTemplate), typeof(Shell), null, BindingMode.OneTime);

		public static readonly BindableProperty MenuItemsSourceProperty = 
			BindableProperty.Create(nameof(MenuItemsSource), typeof(IEnumerable), typeof(Shell), null, BindingMode.OneTime);

		public static readonly BindableProperty MenuItemTemplateProperty = 
			BindableProperty.Create(nameof(MenuItemTemplate), typeof(DataTemplate), typeof(Shell), null, BindingMode.OneTime);

		private static void OnFlyoutHeaderTemplateChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var shell = (Shell)bindable;
			shell.OnFlyoutHeaderTemplateChanged((DataTemplate)oldValue, (DataTemplate)newValue);
		}

		private static void OnFlyoutHeaderChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var shell = (Shell)bindable;
			shell.OnFlyoutHeaderChanged(oldValue, newValue);
		}

		public event EventHandler<ShellNavigatingEventArgs> Navigating;

		public event EventHandler<ShellNavigatedEventArgs> Navigated;

		View _flyoutHeaderView;

		public ShellItem CurrentItem
		{
			get { return (ShellItem)GetValue(CurrentItemProperty); }
			set { SetValue(CurrentItemProperty, value); }
		}

		public FlyoutBehavior FlyoutBehavior
		{
			get { return (FlyoutBehavior)GetValue(FlyoutBehaviorProperty); }
			set { SetValue(FlyoutBehaviorProperty, value); }
		}

		public FlyoutHeaderBehavior FlyoutHeaderBehavior
		{
			get { return (FlyoutHeaderBehavior)GetValue(FlyoutHeaderBehaviorProperty); }
			set { SetValue(FlyoutHeaderBehaviorProperty, value); }
		}

		public object FlyoutHeader
		{
			get { return GetValue(FlyoutHeaderProperty); }
			set { SetValue(FlyoutHeaderProperty, value); }
		}

		public DataTemplate FlyoutHeaderTemplate
		{
			get { return (DataTemplate)GetValue(FlyoutHeaderTemplateProperty); }
			set { SetValue(FlyoutHeaderTemplateProperty, value); }
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

		public IEnumerable MenuItemsSource
		{
			get { return (IEnumerable)GetValue(MenuItemsSourceProperty); }
			set { SetValue(MenuItemsSourceProperty, value); }
		}

		public DataTemplate MenuItemTemplate
		{
			get { return (DataTemplate)GetValue(MenuItemTemplateProperty); }
			set { SetValue(MenuItemTemplateProperty, value); }
		}

		public string Route
		{
			get { return Router.GetRoute(this); }
			set { Router.SetRoute(this, value); }
		}

		public ShellNavigationState ShellNavigationState => (ShellNavigationState)GetValue(ShellNavigationStateProperty);

		private event EventHandler _headerChanged;
		event EventHandler IShellController.HeaderChanged
		{
			add { _headerChanged += value; }
			remove { _headerChanged -= value; }
		}
		
		View IShellController.FlyoutHeader
		{
			get { return FlyoutHeaderView; }
		}

		View FlyoutHeaderView
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
			throw new NotImplementedException();
		}

		protected override void OnChildAdded(Element child)
		{
			base.OnChildAdded(child);

			if (child is ShellItem shellItem && CurrentItem == null)
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
					CurrentItem = Items[0];
			}
		}

		protected virtual void OnNavigating(ShellNavigatingEventArgs args)
		{
			Navigating?.Invoke(this, args);
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
	}
}