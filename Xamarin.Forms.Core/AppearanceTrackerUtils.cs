namespace Xamarin.Forms
{
	internal static class AppearanceTrackerUtils
	{
		public static void AppearanceChanged(Element element, Element source)
		{
			if (element.Parent is IShellAppearanceTracker tracker)
			{
				tracker.AppearanceChanged(source);
			}
		}
	}
}