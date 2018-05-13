namespace Xamarin.Forms
{
	public class ShellAppearance : BindableObject, IShellAppearanceController
	{
		#region AttachedProperties

		public static readonly BindableProperty NavBarVisibleProperty =
			BindableProperty.CreateAttached("NavBarVisible", typeof(bool), typeof(ShellAppearance), true);

		public static readonly BindableProperty TabBarVisibleProperty =
			BindableProperty.CreateAttached("TabBarVisible", typeof(bool), typeof(ShellAppearance), true);

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

		#endregion

		public static readonly BindableProperty BackgroundColorProperty =
			BindableProperty.Create(nameof(BackgroundColor), typeof(Color), typeof(ShellAppearance), Color.Default, BindingMode.OneTime);

		public static readonly BindableProperty DisabledColorProperty =
			BindableProperty.Create(nameof(DisabledColor), typeof(Color), typeof(ShellAppearance), Color.Default, BindingMode.OneTime);

		public static readonly BindableProperty ForegroundColorProperty =
					BindableProperty.Create(nameof(ForegroundColor), typeof(Color), typeof(ShellAppearance), Color.Default, BindingMode.OneTime);

		public static readonly BindableProperty TabBarBackgroundColorProperty =
			BindableProperty.Create(nameof(TabBarBackgroundColor), typeof(Color), typeof(ShellAppearance), Color.Default, BindingMode.OneTime);

		public static readonly BindableProperty TabBarDisabledColorProperty =
			BindableProperty.Create(nameof(TabBarDisabledColor), typeof(Color), typeof(ShellAppearance), Color.Default, BindingMode.OneTime);

		public static readonly BindableProperty TabBarForegroundColorProperty =
			BindableProperty.Create(nameof(TabBarForegroundColor), typeof(Color), typeof(ShellAppearance), Color.Default, BindingMode.OneTime);

		public static readonly BindableProperty TabBarTitleColorProperty = 
			BindableProperty.Create(nameof(TabBarTitleColor), typeof(Color), typeof(ShellAppearance), Color.Default, BindingMode.OneTime);

		public static readonly BindableProperty TabBarUnselectedColorProperty = 
			BindableProperty.Create(nameof(TabBarUnselectedColor), typeof(Color), typeof(ShellAppearance), Color.Default, BindingMode.OneTime);

		public static readonly BindableProperty TitleColorProperty =
			BindableProperty.Create(nameof(TitleColor), typeof(Color), typeof(ShellAppearance), Color.Default, BindingMode.OneTime);

		public static readonly BindableProperty UnselectedColorProperty =
			BindableProperty.Create(nameof(UnselectedColor), typeof(Color), typeof(ShellAppearance), Color.Default, BindingMode.OneTime);

		public Color BackgroundColor
		{
			get { return (Color)GetValue(BackgroundColorProperty); }
			set { SetValue(BackgroundColorProperty, value); }
		}

		public Color DisabledColor
		{
			get { return (Color)GetValue(DisabledColorProperty); }
			set { SetValue(DisabledColorProperty, value); }
		}

		public Color ForegroundColor
		{
			get { return (Color)GetValue(ForegroundColorProperty); }
			set { SetValue(ForegroundColorProperty, value); }
		}

		public Color TabBarBackgroundColor
		{
			get { return (Color)GetValue(TabBarBackgroundColorProperty); }
			set { SetValue(TabBarBackgroundColorProperty, value); }
		}

		public Color TabBarDisabledColor
		{
			get { return (Color)GetValue(TabBarDisabledColorProperty); }
			set { SetValue(TabBarDisabledColorProperty, value); }
		}

		public Color TabBarForegroundColor
		{
			get { return (Color)GetValue(TabBarForegroundColorProperty); }
			set { SetValue(TabBarForegroundColorProperty, value); }
		}

		public Color TabBarTitleColor
		{
			get { return (Color)GetValue(TabBarTitleColorProperty); }
			set { SetValue(TabBarTitleColorProperty, value); }
		}

		public Color TabBarUnselectedColor
		{
			get { return (Color)GetValue(TabBarUnselectedColorProperty); }
			set { SetValue(TabBarUnselectedColorProperty, value); }
		}

		public Color TitleColor
		{
			get { return (Color)GetValue(TitleColorProperty); }
			set { SetValue(TitleColorProperty, value); }
		}

		public Color UnselectedColor
		{
			get { return (Color)GetValue(UnselectedColorProperty); }
			set { SetValue(UnselectedColorProperty, value); }
		}

		Color IShellAppearanceController.EffectiveTabBarBackgroundColor =>
			TabBarBackgroundColor.IsDefault ? BackgroundColor : TabBarBackgroundColor;

		Color IShellAppearanceController.EffectiveTabBarDisabledColor =>
			TabBarDisabledColor.IsDefault ? DisabledColor : TabBarDisabledColor;

		Color IShellAppearanceController.EffectiveTabBarForegroundColor =>
			TabBarForegroundColor.IsDefault ? ForegroundColor : TabBarForegroundColor;

		Color IShellAppearanceController.EffectiveTabBarTitleColor =>
			TabBarTitleColor.IsDefault ? TitleColor : TabBarTitleColor;

		Color IShellAppearanceController.EffectiveTabBarUnselectedColor =>
			TabBarUnselectedColor.IsDefault ? UnselectedColor : TabBarUnselectedColor;
	}
}