using Android.Content;
using Android.Runtime;
using System;
using LP = Android.Views.ViewGroup.LayoutParams;

namespace Xamarin.Forms.Platform.Android.AppCompat
{
	internal class ShellFragmentContainer : FragmentContainer
	{
		public ShellFragmentContainer()
		{
		}

		public ShellFragmentContainer(Page page) : base(page)
		{
		}

		protected ShellFragmentContainer(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
		}

		protected override PageContainer CreatePageContainer(Context context, IVisualElementRenderer child, bool inFragment)
		{
			return new ShellPageContainer(context, child, inFragment)
			{
				LayoutParameters = new LP(LP.MatchParent, LP.MatchParent)
			};
		}
	}
}