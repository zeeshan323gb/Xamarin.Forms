using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Xamarin.Forms
{

	[ContentProperty("Items")]
	public class ShellItem : FrameworkElement, IShellItemController
	{
		static readonly BindablePropertyKey ItemsPropertyKey =
			BindableProperty.CreateReadOnly(nameof(Items), typeof(ShellTabItemCollection), typeof(ShellItem), null,
				defaultValueCreator: bo => new ShellTabItemCollection { Inner = new ElementCollection<ShellTabItem>(((ShellItem)bo)._children) });

		public static readonly BindableProperty CurrentItemProperty =
			BindableProperty.Create(nameof(CurrentItem), typeof(object), typeof(ShellItem), null, BindingMode.TwoWay);

		public static readonly BindableProperty GroupBehaviorProperty =
			BindableProperty.Create(nameof(GroupBehavior), typeof(ShellItemGroupBehavior), typeof(ShellItem), ShellItemGroupBehavior.HideTabs, BindingMode.OneTime);

		public static readonly BindableProperty IconProperty =
			BindableProperty.Create(nameof(Icon), typeof(ImageSource), typeof(ShellItem), null, BindingMode.OneTime);

		public static readonly BindableProperty IsEnabledProperty =
			BindableProperty.Create(nameof(IsEnabled), typeof(bool), typeof(ShellItem), true, BindingMode.OneWay);

		public static readonly BindableProperty ItemsProperty = ItemsPropertyKey.BindableProperty;

		public static readonly BindableProperty ItemsSourceProperty =
			BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable), typeof(ShellItem), null, BindingMode.OneTime);

		public static readonly BindableProperty ItemTemplateProperty =
			BindableProperty.Create(nameof(ItemTemplate), typeof(DataTemplate), typeof(ShellItem), null, BindingMode.OneTime);

		public static readonly BindableProperty ShellAppearanceProperty =
			BindableProperty.Create(nameof(ShellAppearance), typeof(ShellAppearance), typeof(ShellItem), null, BindingMode.OneTime);

		public static readonly BindableProperty TitleProperty =
			BindableProperty.Create(nameof(Title), typeof(string), typeof(ShellItem), null, BindingMode.OneTime);

		ObservableCollection<Element> _children = new ObservableCollection<Element>();
		ReadOnlyCollection<Element> _logicalChildren;

		public static ShellAppearance GetShellAppearance(BindableObject obj)
		{
			return (ShellAppearance)obj.GetValue(ShellAppearanceProperty);
		}

		public static void SetShellAppearance(BindableObject obj, ShellAppearance value)
		{
			obj.SetValue(ShellAppearanceProperty, value);
		}

		public ShellItem()
		{
			((INotifyCollectionChanged)Items).CollectionChanged += ItemsCollectionChanged;
		}

		internal override ReadOnlyCollection<Element> LogicalChildrenInternal => _logicalChildren ?? (_logicalChildren = new ReadOnlyCollection<Element>(_children));

		public string Route
		{
			get { return Router.GetRoute(this); }
			set { Router.SetRoute(this, value); }
		}

		public object CurrentItem
		{
			get { return GetValue(CurrentItemProperty); }
			set { SetValue(CurrentItemProperty, value); }
		}

		public ShellItemGroupBehavior GroupBehavior
		{
			get { return (ShellItemGroupBehavior)GetValue(GroupBehaviorProperty); }
			set { SetValue(GroupBehaviorProperty, value); }
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

		public ShellTabItemCollection Items => (ShellTabItemCollection)GetValue(ItemsProperty);

		public IEnumerable ItemsSource
		{
			get { return (IEnumerable)GetValue(ItemsSourceProperty); }
			set { SetValue(ItemsSourceProperty, value); }
		}

		public DataTemplate ItemTemplate
		{
			get { return (DataTemplate)GetValue(ItemTemplateProperty); }
			set { SetValue(ItemTemplateProperty, value); }
		}

		public ShellAppearance ShellAppearance
		{
			get { return (ShellAppearance)GetValue(ShellAppearanceProperty); }
			set { SetValue(ShellAppearanceProperty, value); }
		}

		public string Title
		{
			get { return (string)GetValue(TitleProperty); }
			set { SetValue(TitleProperty, value); }
		}

		public static implicit operator ShellItem(ShellTabItem tab)
		{
			throw new NotImplementedException();
		}

		public static implicit operator ShellItem(TemplatedPage page)
		{
			throw new NotImplementedException();
		}

		public static implicit operator ShellItem(MenuItem menuItem)
		{
			throw new NotImplementedException();
		}

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

		private void ItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.NewItems != null)
				foreach (Element element in e.NewItems)
					OnChildAdded(element);

			if (e.OldItems != null)
				foreach (Element element in e.OldItems)
					OnChildRemoved(element);
		}
	}
}