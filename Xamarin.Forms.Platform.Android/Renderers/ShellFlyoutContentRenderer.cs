using Android.Content;
using Android.Graphics.Drawables;
using Android.Support.Design.Widget;
using Android.Views;
using System;
using System.Collections.Generic;
using static Android.Support.Design.Widget.NavigationView;
using AView = Android.Views.View;

namespace Xamarin.Forms.Platform.Android
{
	public class ShellFlyoutContentRenderer : NavigationView, IShellFlyoutContentRenderer, IOnNavigationItemSelectedListener
	{
		#region IShellFlyoutContentRenderer

		public event EventHandler<ElementSelectedEventArgs> ElementSelected;

		AView IShellFlyoutContentRenderer.AndroidView => this;

		#endregion IShellFlyoutContentRenderer

		private AView _headerView;
		private readonly Dictionary<IMenuItem, Element> _lookupTable = new Dictionary<IMenuItem, Element>();
		private IShellContext _shellContext;
		private bool _disposed;

		public ShellFlyoutContentRenderer(IShellContext shellContext, Context context) : base(context)
		{
			_shellContext = shellContext;

			((IShellController)_shellContext.Shell).StructureChanged += OnShellStructureChanged;

			SetNavigationItemSelectedListener(this);

			BuildMenu();

			_headerView = new ContainerView(context, ((IShellController)shellContext.Shell).FlyoutHeader)
			{
				MatchWidth = true
			};

			AddHeaderView(_headerView);
		}

		bool IOnNavigationItemSelectedListener.OnNavigationItemSelected(IMenuItem menuItem)
		{
			if (_lookupTable.TryGetValue(menuItem, out var element))
			{
				ElementSelected?.Invoke(this, new ElementSelectedEventArgs { Element = element });
				return true;
			}
			return false;
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing && !_disposed)
			{
				_disposed = true;

				((IShellController)_shellContext.Shell).StructureChanged -= OnShellStructureChanged;
				_lookupTable.Clear();
				_headerView.Dispose();
				_headerView = null;
				_shellContext = null;
			}
		}

		private void BuildMenu()
		{
			_lookupTable.Clear();

			var menu = Menu;
			menu.Clear();

			var groups = ((IShellController)_shellContext.Shell).GenerateFlyoutGrouping();

			int gid = 0;
			int id = 0;

			foreach (var group in groups)
			{
				foreach (var element in group)
				{
					string title = null;
					ImageSource icon = null;
					if (element is BaseShellItem shellItem)
					{
						title = shellItem.Title;
						icon = shellItem.FlyoutIcon;
					}
					else if (element is MenuItem menuItem)
					{
						title = menuItem.Text;
						icon = menuItem.Icon;
					}

					var item = menu.Add(gid, id++, 0, new Java.Lang.String(title));
					if (icon != null)
						SetMenuItemIcon(item, icon);
				}
				gid++;
			}

			//var shell = _shellContext.Shell;

			//int gid = 0;
			//int id = 0;

			//FlyoutDisplayOptions previous = FlyoutDisplayOptions.AsSingleItem;
			//foreach (var shellItem in shell.Items)
			//{
			//	bool isCurrentShellItem = shell.CurrentItem == shellItem;
			//	var groupBehavior = shellItem.FlyoutDisplayOptions;
			//	if (groupBehavior == FlyoutDisplayOptions.AsMultipleItems)
			//	{
			//		IMenu section = null;
			//		if (string.IsNullOrEmpty(shellItem.Title))
			//		{
			//			gid++;
			//			section = menu;
			//		}
			//		else
			//		{
			//			section = menu.AddSubMenu(new Java.Lang.String(shellItem.Title));
			//		}

			//		foreach (var shellSection in shellItem.Items)
			//		{
			//			var item = section.Add(gid, id, 0, new Java.Lang.String(shellSection.Title));
			//			item.SetEnabled(shellSection.IsEnabled);
			//			if (shellSection.Icon != null)
			//			{
			//				SetMenuItemIcon(item, shellSection.Icon);
			//			}
			//			item.SetCheckable(true);
			//			_lookupTable[item] = shellSection;
			//			// when an item is selected we will display its menu items
			//			if (isCurrentShellItem && shellSection == shellItem.CurrentItem)
			//			{
			//				item.SetChecked(true);
			//				foreach (var menuItem in shellSection.MenuItems)
			//				{
			//					var subItem = section.Add(gid, id, 0, new Java.Lang.String(menuItem.Text));
			//					subItem.SetEnabled(menuItem.IsEnabled);
			//					if (menuItem.Icon != null)
			//					{
			//						SetMenuItemIcon(subItem, menuItem.Icon);
			//						subItem.SetCheckable(false);
			//					}
			//					_lookupTable[subItem] = menuItem;
			//				}
			//			}
			//		}
			//		gid++;
			//	}
			//	else
			//	{
			//		var subItem = menu.Add(gid, id, 0, new Java.Lang.String(shellItem.Title));
			//		subItem.SetEnabled(shellItem.IsEnabled);
			//		if (shellItem.Icon != null)
			//		{
			//			SetMenuItemIcon(subItem, shellItem.Icon);
			//		}
			//		subItem.SetCheckable(true);
			//		if (isCurrentShellItem)
			//			subItem.SetChecked(true);
			//		_lookupTable[subItem] = shellItem;
			//	}

			//	previous = groupBehavior;
			//}

			//if (shell.MenuItems.Count > 0)
			//{
			//	gid++;
			//	foreach (var menuItem in shell.MenuItems)
			//	{
			//		var subItem = menu.Add(gid, id, 0, new Java.Lang.String(menuItem.Text));
			//		subItem.SetEnabled(menuItem.IsEnabled);
			//		subItem.SetCheckable(false);
			//		if (menuItem.Icon != null)
			//		{
			//			SetMenuItemIcon(subItem, menuItem.Icon);
			//		}
			//		_lookupTable[subItem] = menuItem;
			//	}
			//}
		}

		private void OnShellStructureChanged(object sender, EventArgs e)
		{
			BuildMenu();
		}

		private async void SetMenuItemIcon(IMenuItem menuItem, ImageSource source)
		{
			var drawable = await Context.GetFormsDrawable(source);
			menuItem.SetIcon(drawable);
		}
	}
}