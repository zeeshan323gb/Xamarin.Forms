namespace Xamarin.Forms.PlatformConfiguration.AndroidSpecific
{
	using FormsElement = Forms.ShellItem;

	public enum ShellTabBarLocation
	{
		Top,
		Bottom,
	}

	public static class ShellItem
	{
		#region TabBarLocation
		public static readonly BindableProperty TabBarLocationProperty = 
			BindableProperty.CreateAttached("TabBarLocation", typeof(ShellTabBarLocation), typeof(ShellItem), ShellTabBarLocation.Top);

		public static ShellTabBarLocation GetTabBarLocation(BindableObject element)
		{
			return (ShellTabBarLocation)element.GetValue(TabBarLocationProperty);
		}

		public static void SetTabBarLocation(BindableObject element, ShellTabBarLocation value)
		{
			element.SetValue(TabBarLocationProperty, value);
		}

		public static ShellTabBarLocation GetTabBarLocation(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			return GetTabBarLocation(config.Element);
		}

		public static IPlatformElementConfiguration<Android, FormsElement> SetTabBarLocation(this IPlatformElementConfiguration<Android, FormsElement> config, ShellTabBarLocation value)
		{
			SetTabBarLocation(config.Element, value);
			return config;
		}
		#endregion TabBarLocation
	}
}