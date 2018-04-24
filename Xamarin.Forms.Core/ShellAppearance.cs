namespace Xamarin.Forms
{
	public class ShellAppearance : BindableObject
	{
		public static readonly BindableProperty BackgroundColorProperty =
			BindableProperty.Create(nameof(BackgroundColor), typeof(Color), typeof(ShellAppearance), Color.Default, BindingMode.OneTime);

		public static readonly BindableProperty ForegroundColorProperty =
			BindableProperty.Create(nameof(ForegroundColor), typeof(Color), typeof(ShellAppearance), Color.Default, BindingMode.OneTime);

		public static readonly BindableProperty NavBarVisibleProperty =
			BindableProperty.CreateAttached("NavBarVisible", typeof(bool), typeof(ShellAppearance), true);

		public static readonly BindableProperty TabBarVisibleProperty = 
			BindableProperty.CreateAttached("TabBarVisible", typeof(bool), typeof(ShellAppearance), true);

		public Color BackgroundColor
		{
			get { return (Color)GetValue(BackgroundColorProperty); }
			set { SetValue(BackgroundColorProperty, value); }
		}

		public Color ForegroundColor
		{
			get { return (Color)GetValue(ForegroundColorProperty); }
			set { SetValue(ForegroundColorProperty, value); }
		}

		public static bool GetNavBarVisible(BindableObject obj)
		{
			return (bool)obj.GetValue(NavBarVisibleProperty);
		}

		public static bool GetTabBarVisible(BindableObject obj)
		{
			return (bool)obj.GetValue(TabBarVisibleProperty);
		}

		public static void SetNavBarVisible(BindableObject obj, bool value)
		{
			obj.SetValue(NavBarVisibleProperty, value);
		}

		public static void SetTabBarVisible(BindableObject obj, bool value)
		{
			obj.SetValue(TabBarVisibleProperty, value);
		}
	}
}