using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using System;
using LP = Android.Views.ViewGroup.LayoutParams;

namespace Xamarin.Forms.Platform.Android.AppCompat
{
	internal class ShellFragmentContainer : FragmentContainer
	{
		private Page _page;

		public ShellTabItem ShellTabItem { get; private set; }

		public ShellFragmentContainer(ShellTabItem tabItem) : base()
		{
			ShellTabItem = tabItem;
		}

		protected ShellFragmentContainer(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
		}

		public override Page Page => _page;

		protected override PageContainer CreatePageContainer(Context context, IVisualElementRenderer child, bool inFragment)
		{
			return new ShellPageContainer(context, child, inFragment)
			{
				LayoutParameters = new LP(LP.MatchParent, LP.MatchParent)
			};
		}

		public override global::Android.Views.View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			_page = ((IShellTabItemController)ShellTabItem).GetOrCreateContent();
			return base.OnCreateView(inflater, container, savedInstanceState);
		}

		public override void OnDestroyView()
		{
			base.OnDestroyView();
			((IShellTabItemController)ShellTabItem).RecyclePage(_page);
			_page = null;
		}
	}
}