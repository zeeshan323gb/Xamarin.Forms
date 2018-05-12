namespace Xamarin.Forms
{
	public class MaterialShellAppearance : ShellAppearance
	{
		public static readonly BindableProperty TabLocationProperty =
			BindableProperty.Create(nameof(TabLocation), typeof(ShellTabLocation), typeof(MaterialShellAppearance), ShellTabLocation.Top, BindingMode.OneTime);

		public ShellTabLocation TabLocation
		{
			get { return (ShellTabLocation)GetValue(TabLocationProperty); }
			set { SetValue(TabLocationProperty, value); }
		}
	}
}