using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Xamarin.Forms
{
	[ContentProperty("Items")]
	public class ShellItem : BaseShellItem, IShellItemController, IElementConfiguration<ShellItem>
	{
		#region PropertyKeys

		private static readonly BindablePropertyKey ItemsPropertyKey = BindableProperty.CreateReadOnly(nameof(Items), typeof(ShellContentCollection), typeof(ShellItem), null,
				defaultValueCreator: bo => new ShellContentCollection { Inner = new ElementCollection<ShellContent>(((ShellItem)bo)._children) });

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
				foreach (var content in Items)
					content.SetValue(IsCheckedPropertyKey, content == CurrentItem);
			}
			else
			{
				SetValue(IsCheckedPropertyKey, false);
				foreach (var content in Items)
					content.SetValue(IsCheckedPropertyKey, false);
			}
		}

		private event EventHandler _structureChanged;

		#endregion IShellItemController

		public static readonly BindableProperty CurrentItemProperty =
			BindableProperty.Create(nameof(CurrentItem), typeof(ShellContent), typeof(ShellItem), null, BindingMode.TwoWay,
				propertyChanged: OnCurrentItemChanged);

		public static readonly BindableProperty GroupBehaviorProperty =
			BindableProperty.Create(nameof(GroupBehavior), typeof(ShellItemGroupBehavior), typeof(ShellItem), ShellItemGroupBehavior.HideTabs, BindingMode.OneTime);

		public static readonly BindableProperty ItemsProperty = ItemsPropertyKey.BindableProperty;

		private readonly ObservableCollection<Element> _children = new ObservableCollection<Element>();
		private ReadOnlyCollection<Element> _logicalChildren;
		private Lazy<PlatformConfigurationRegistry<ShellItem>> _platformConfigurationRegistry;

		public ShellItem()
		{
			((INotifyCollectionChanged)Items).CollectionChanged += ItemsCollectionChanged;
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<ShellItem>>(() => new PlatformConfigurationRegistry<ShellItem>(this));
		}

		public ShellContent CurrentItem
		{
			get { return (ShellContent)GetValue(CurrentItemProperty); }
			set { SetValue(CurrentItemProperty, value); }
		}

		public ShellItemGroupBehavior GroupBehavior
		{
			get { return (ShellItemGroupBehavior)GetValue(GroupBehaviorProperty); }
			set { SetValue(GroupBehaviorProperty, value); }
		}

		public ShellContentCollection Items => (ShellContentCollection)GetValue(ItemsProperty);

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
		public static implicit operator ShellItem(ShellContent content)
		{
			var result = new ShellItem();
			result.Items.Add(content);
			result.SetBinding(TitleProperty, new Binding("Title", BindingMode.OneWay, source: content));
			result.SetBinding(IconProperty, new Binding("Icon", BindingMode.OneWay, source: content));
			return result;
		}

#if DEBUG
		[Obsolete("Please dont use this in core code... its SUPER hard to debug when this happens", true)]
#endif
		public static implicit operator ShellItem(TemplatedPage page) => (ShellContent)page;

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
				shell.UpdateCurrentState(ShellNavigationSource.ShellContentChanged);
			}

			((IShellItemController)bindable).UpdateChecked();
			shellItem.SendStructureChanged();
			((IShellController)shellItem?.Parent)?.AppearanceChanged(shellItem, false);
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

		public IPlatformElementConfiguration<T, ShellItem> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
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