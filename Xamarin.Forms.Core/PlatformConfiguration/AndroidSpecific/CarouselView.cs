
namespace Xamarin.Forms.PlatformConfiguration.AndroidSpecific
{
	using FormsElement = Forms.CarouselView;

	public static class CarouselView
	{
		#region InterPageSpacingColor

		public static readonly BindableProperty InterPageSpacingColorProperty = BindableProperty.Create("InterPageSpacingColor", typeof(Color), typeof(CarouselView), Color.White);

		public static Color GetInterPageSpacingColor(BindableObject element)
		{
			return (Color)element.GetValue(InterPageSpacingColorProperty);
		}

		public static void SetInterPageSpacingColor(BindableObject element, Color InterPageSpacingColor)
		{
			element.SetValue(InterPageSpacingColorProperty, InterPageSpacingColor);
		}

		public static Color GetInterPageSpacingColor(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			return (Color)config.Element.GetValue(InterPageSpacingColorProperty);
		}

		public static IPlatformElementConfiguration<Android, FormsElement> SetInterPageSpacingColor(
			this IPlatformElementConfiguration<Android, FormsElement> config, Color value)
		{
			config.Element.SetValue(InterPageSpacingColorProperty, value);
			return config;
		}

		#endregion

		#region InterPageSpacing

		public static readonly BindableProperty InterPageSpacingProperty = BindableProperty.Create("InterPageSpacing", typeof(int), typeof(CarouselView), 0);

		public static int GetInterPageSpacing(BindableObject element)
		{
			return (int)element.GetValue(InterPageSpacingProperty);
		}

		public static void SetInterPageSpacing(BindableObject element, int InterPageSpacing)
		{
			element.SetValue(InterPageSpacingProperty, InterPageSpacing);
		}

		public static int GetInterPageSpacing(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			return (int)config.Element.GetValue(InterPageSpacingProperty);
		}

		public static IPlatformElementConfiguration<Android, FormsElement> SetInterPageSpacing(
			this IPlatformElementConfiguration<Android, FormsElement> config, int value)
		{
			config.Element.SetValue(InterPageSpacingProperty, value);
			return config;
		}

		#endregion
	}
}
