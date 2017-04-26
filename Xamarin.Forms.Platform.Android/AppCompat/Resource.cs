using Android.Runtime;

[assembly: ResourceDesigner("Xamarin.Forms.Platform.Android.Resource", IsApplication = false)]

namespace Xamarin.Forms.Platform.Android
{
	public class Resource
	{
		static Resource()
		{
			ResourceIdManager.UpdateIdValues();
		}

		public class Attribute
		{
			// aapt resource value: 0x7f0100a5
			// ReSharper disable once InconsistentNaming
			// Android is pretty insistent about this casing
			public static int actionBarSize = 2130772133;

			static Attribute()
			{
				ResourceIdManager.UpdateIdValues();
			}

			Attribute()
			{
			}
		}
		
		public partial class Id
		{
			
			// aapt resource value: 0x7f080000
			public static int horizontal = 2131230720;
			
			// aapt resource value: 0x7f080003
			public static int indicator = 2131230723;
			
			// aapt resource value: 0x7f080002
			public static int pager = 2131230722;
			
			// aapt resource value: 0x7f080001
			public static int vertical = 2131230721;
			
			static Id()
			{
				global::Android.Runtime.ResourceIdManager.UpdateIdValues();
			}
			
			private Id()
			{
			}
		}
		
		public partial class Layout
		{
			
			// aapt resource value: 0x7f020000
			public static int horizontal_viewpager = 2130837504;
			
			// aapt resource value: 0x7f020001
			public static int vertical_viewpager = 2130837505;
			
			static Layout()
			{
				global::Android.Runtime.ResourceIdManager.UpdateIdValues();
			}
			
			private Layout()
			{
			}
		}
	}
}