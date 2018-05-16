using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Xamarin.Forms
{
	[ContentProperty("Items")]
	public class ShellItem : BaseShellItem, IShellItemController
	{
		#region PropertyKeys

		private static readonly BindablePropertyKey ItemsPropertyKey = BindableProperty.CreateReadOnly(nameof(Items), typeof(ShellTabItemCollection), typeof(ShellItem), null,
				defaultValueCreator: bo => new ShellTabItemCollection { Inner = new ElementCollection<ShellTabItem>(((ShellItem)bo)._children) });

		#endregion PropertyKeys

		#region IShellItemController

		event EventHandler IShellItemController.StructureChanged
		{
			add { _structureChanged += value; }
			remove { _structureChanged -= value; }
		}

		void IShellItemController.UpdateChecked()
		{
			var shell = Parent as Shell;
			bool isChecked = shell?.CurrentItem == this;
			if (isChecked)
			{
				SetValue(IsCheckedPropertyKey, true);
				foreach (var tab in Items)
					tab.SetValue(IsCheckedPropertyKey, tab == CurrentItem);
			}
			else
			{
				SetValue(IsCheckedPropertyKey, false);
				foreach (var tab in Items)
					tab.SetValue(IsCheckedPropertyKey, false);
			}
		}

		private event EventHandler _structureChanged;

		#endregion IShellItemController

		public static readonly BindableProperty CurrentItemProperty =
			BindableProperty.Create(nameof(CurrentItem), typeof(ShellTabItem), typeof(ShellItem), null, BindingMode.TwoWay,
				propertyChanged: OnCurrentItemChanged);

		public static readonly BindableProperty GroupBehaviorProperty =
			BindableProperty.Create(nameof(GroupBehavior), typeof(ShellItemGroupBehavior), typeof(ShellItem), ShellItemGroupBehavior.HideTabs, BindingMode.OneTime);

		public static readonly BindableProperty ItemsProperty = ItemsPropertyKey.BindableProperty;

		private readonly ObservableCollection<Element> _children = new ObservableCollection<Element>();
		private ReadOnlyCollection<Element> _logicalChildren;

		public ShellItem()
		{
			((INotifyCollectionChanged)Items).CollectionChanged += ItemsCollectionChanged;
		}

		public ShellTabItem CurrentItem
		{
			get { return (ShellTabItem)GetValue(CurrentItemProperty); }
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

		public ShellTabItemCollection Items => (ShellTabItemCollection)GetValue(ItemsProperty);

		internal override ReadOnlyCollection<Element> LogicalChildrenInternal => _logicalChildren ?? (_logicalChildren = new ReadOnlyCollection<Element>(_children));

		internal void SendStructureChanged()
		{
			_structureChanged?.Invoke(this, EventArgs.Empty);
			if (Parent is Shell shell)
			{
				shell.SendStructureChanged();
			}
		}

#if DEBUG
		[Obsolete ("Please dont use this in core code... its SUPER hard to debug when this happens", true)]
#endif
		public static implicit operator ShellItem(ShellTabItem tab)
		{
			var result = new ShellItem();
			result.Items.Add(tab);
			result.SetBinding(TitleProperty, new Binding("Title", BindingMode.OneWay, source: tab));
			result.SetBinding(IconProperty, new Binding("Icon", BindingMode.OneWay, source: tab));
			return result;
		}

#if DEBUG
		[Obsolete("Please dont use this in core code... its SUPER hard to debug when this happens", true)]
#endif
		public static implicit operator ShellItem(TemplatedPage page) => (ShellTabItem)page;

#if DEBUG
		[Obsolete("Please dont use this in core code... its SUPER hard to debug when this happens", true)]
#endif
		public static implicit operator ShellItem(MenuItem menuItem) => new MenuShellItem(menuItem);

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

		private static void OnCurrentItemChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var shellItem = (ShellItem)bindable;

			if (shellItem.Parent is IShellController shell)
			{
				shell.UpdateCurrentState(ShellNavigationSource.ShellTabItemChanged);
			}

			((IShellItemController)bindable).UpdateChecked();
			shellItem.SendStructureChanged();
			((IShellAppearanceTracker)shellItem).AppearanceChanged(shellItem, false);
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

		public class MenuShellItem : ShellItem
		{
			internal MenuShellItem(MenuItem menuItem)
			{
				MenuItem = menuItem;

				SetBinding(TitleProperty, new Binding("Text", BindingMode.OneWay, source: menuItem));
				SetBinding(IconProperty, new Binding("Icon", BindingMode.OneWay, source: menuItem));
			}

			public MenuItem MenuItem { get; }
		}
	}
}