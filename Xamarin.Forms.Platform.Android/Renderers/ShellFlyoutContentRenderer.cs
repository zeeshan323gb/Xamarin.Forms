using Android.Content;
using Android.Support.Design.Widget;
using AView = Android.Views.View;
using System.Collections.Generic;
using AV = Android.Views;

namespace Xamarin.Forms.Platform.Android
{
	public class ShellFlyoutContentRenderer : NavigationView, IShellFlyoutContentRenderer
	{
		#region IShellFlyoutContentRenderer

		AView IShellFlyoutContentRenderer.AndroidView => this;

		#endregion IShellFlyoutContentRenderer

		private readonly IShellContext _shellContext;

		public ShellFlyoutContentRenderer(IShellContext shellContext, Context context) : base (context)
		{
			_shellContext = shellContext;

			BuidMenu();
		}

		protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
		{
			base.OnLayout(changed, left, top, right, bottom);
		}

		private void BuidMenu ()
		{
			var menu = Menu;
			menu.Clear();
			var shell = _shellContext.Shell;

			ShellItemGroupBehavior previous = ShellItemGroupBehavior.HideTabs;
			//int groupIndex = 0;
			//int itemIndex = 0;
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
						// when an item is selected we will display its menu items
						if (isCurrentShellItem && tabItem == shellItem.CurrentItem)
						{
							foreach (var menuItem in tabItem.MenuItems)
								section.Add(new Java.Lang.String(menuItem.Text));
						}
					}
				}
				else
				{
					menu.Add(new Java.Lang.String(shellItem.Title));
				}

				previous = groupBehavior;
			}

			if (shell.MenuItems.Count > 0)
			{
				section = menu.AddSubMenu(null);
				foreach (var menuItem in shell.MenuItems)
					section.Add(new Java.Lang.String(menuItem.Text));
			}
		}
	}
}