namespace Xamarin.Forms.Alias
{
	public class ToggleSwitch : Switch
	{
		public static readonly BindableProperty IsOnProperty = IsToggledProperty;

		public bool IsOn
		{
			get { return IsToggled; }
			set { IsToggled = value; }
		}
	}
}