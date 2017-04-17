
using Android.Support.V4.View;

namespace Xamarin.Forms.Platform.Android
{

	public class DefaultTransformer : Java.Lang.Object, ViewPager.IPageTransformer
	{
		public void TransformPage(global::Android.Views.View page, float position)
		{
			page.TranslationX = page.Width * -position;
			float yPosition = position * page.Height;
			page.TranslationY = yPosition;
		}
	}
}