namespace Xamarin.Forms.Alias
{
	public class ProgressRing : ActivityIndicator
	{
		public static readonly BindableProperty IsActiveProperty = IsRunningProperty;

		public bool IsActive
		{
			get { return IsRunning; }
			set { IsRunning = value; }
		}
	}
}