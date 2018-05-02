using Android.Content;
using Android.Support.Design.Widget;
using System;
using System.Collections.Generic;
using static Android.Support.Design.Widget.NavigationView;
using AV = Android.Views;
using AView = Android.Views.View;

namespace Xamarin.Forms.Platform.Android
{
	public class ShellFlyoutContentRenderer : NavigationView, IShellFlyoutContentRenderer, IOnNavigationItemSelectedListener
	{
		#region IShellFlyoutContentRenderer

		public event EventHandler<ElementSelectedEventArgs> ElementSelected;

		AView IShellFlyoutContentRenderer.AndroidView => this;

		#endregion IShellFlyoutContentRenderer

		private readonly IShellContext _shellContext;
		private readonly Dictionary<AV.IMenuItem, Element> _lookupTable = new Dictionary<AV.IMenuItem, Element>();

		public ShellFlyoutContentRenderer(IShellContext shellContext, Context context) : base(context)
		{
			_shellContext = shellContext;

			SetNavigationItemSelectedListener(this);

			BuildMenu();
		}

		bool IOnNavigationItemSelectedListener.OnNavigationItemSelected(AV.IMenuItem menuItem)
		{
			if (_lookupTable.TryGetValue(menuItem, out var element))
			{
				ElementSelected?.Invoke(this, new ElementSelectedEventArgs { Element = element });
				return true;
			}
			return false;
		}

		protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
		{
			base.OnLayout(changed, left, top, right, bottom);
		}

		private void BuildMenu()
		{
			_lookupTable.Clear();

			var menu = Menu;
			menu.Clear();
			var shell = _shellContext.Shell;

			ShellItemGroupBehavior previous = ShellItemGroupBehavior.HideTabs;
			AV.ISubMenu section = null;
			foreach (var shellItem in shell.Items)
			{
				bool isCurrentShellItem = shell.CurrentItem == shellItem;
				var groupBehavior = shellItem.GroupBehavior;
				if (groupBehavior == ShellItemGroupBehavior.ShowTabs)
				{
					section = menu.AddSubMenu(new Java.Lang.String(shellItem.Title));
					foreach (var tabItem in shellItem.Items)
					{
						var item = section.Add(new Java.Lang.String(tabItem.Title));
						item.SetCheckable(true);
						_lookupTable[item] = tabItem;
						// when an item is selected we will display its menu items
						if (isCurrentShellItem && tabItem == shellItem.CurrentItem)
						{
							foreach (var menuItem in tabItem.MenuItems)
							{
								var subItem = section.Add(new Java.Lang.String(menuItem.Text));
								_lookupTable[subItem] = menuItem;
							}
						}
					}
				}
				else
				{
					var subItem = menu.Add(new Java.Lang.String(shellItem.Title));
					subItem.SetCheckable(true);
					_lookupTable[subItem] = shellItem;
				}

				previous = groupBehavior;
			}
			
			if (shell.MenuItems.Count > 0)
			{
				section = menu.AddSubMenu(null);
				foreach (var menuItem in shell.MenuItems)
				{
					var subItem = section.Add(new Java.Lang.String(menuItem.Text));
					_lookupTable[subItem] = menuItem;
				}
			}
		}
	}
}