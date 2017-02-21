
namespace Xamarin.Forms.PlatformConfiguration.iOSSpecific
{
	using FormsElement = Forms.CarouselView;

	public static class CarouselView
	{
		#region Bounces
		public static readonly BindableProperty BouncesProperty = BindableProperty.Create("Bounces", typeof(bool), typeof(CarouselView), true);

		public static bool GetBounces(BindableObject element)
		{
			return (bool)element.GetValue(BouncesProperty);
		}

		public static void SetBounces(BindableObject element, bool Bounces)
		{
			element.SetValue(BouncesProperty, Bounces);
		}

		public static bool GetBounces(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return (bool)config.Element.GetValue(BouncesProperty);
		}

		public static IPlatformElementConfiguration<iOS, FormsElement> SetBounces(
			this IPlatformElementConfiguration<iOS, FormsElement> config, bool value)
		{
			config.Element.SetValue(BouncesProperty, value);
			return config;
		}
		#endregion

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

		public static Color GetInterPageSpacingColor(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return (Color)config.Element.GetValue(InterPageSpacingColorProperty);
		}

		public static IPlatformElementConfiguration<iOS, FormsElement> SetInterPageSpacingColor(
			this IPlatformElementConfiguration<iOS, FormsElement> config, Color value)
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

		public static int GetInterPageSpacing(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return (int)config.Element.GetValue(InterPageSpacingProperty);
		}

		public static IPlatformElementConfiguration<iOS, FormsElement> SetInterPageSpacing(
			this IPlatformElementConfiguration<iOS, FormsElement> config, int value)
		{
			config.Element.SetValue(InterPageSpacingProperty, value);
			return config;
		}

		#endregion
	}
}
