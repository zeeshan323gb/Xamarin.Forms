namespace Xamarin.Forms.Alias
{
	public class ProgressBar : Xamarin.Forms.ProgressBar
	{
		public static readonly BindableProperty ValueProperty = ProgressProperty;

		public double Value
		{
			get { return Progress; }
			set { Progress = value; }
		}
	}
}