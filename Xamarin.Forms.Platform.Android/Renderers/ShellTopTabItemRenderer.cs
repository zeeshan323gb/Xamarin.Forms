using Android.OS;
using Android.Views;
using Android.Widget;
using AView = Android.Views.View;
using LP = Android.Views.ViewGroup.LayoutParams;

namespace Xamarin.Forms.Platform.Android
{
	public class ShellTopTabItemRenderer : ShellItemRendererBase
	{
		private FrameLayout _navigationArea;
		private IShellObservableFragment _rootFragment;

		public ShellTopTabItemRenderer(IShellContext shellContext) : base(shellContext)
		{
		}

		public override AView OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			_navigationArea = new FrameLayout(Context)
			{
				LayoutParameters = new LP(LP.MatchParent, LP.MatchParent),
				Id = Platform.GenerateViewId()
			};
			_navigationArea.SetBackgroundColor(global::Android.Graphics.Color.Black);

			HookEvents(ShellItem);

			return _navigationArea;
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
			UnhookEvents(ShellItem);

			_navigationArea?.Dispose();

			_navigationArea = null;
			CurrentTabItem = null;
		}

		protected override IShellObservableFragment CreateFragmentForPage(Page page)
		{
			return new ShellContentFragment(ShellContext, page);
		}

		protected override ViewGroup GetNavigationTarget() => _navigationArea;

		protected override IShellObservableFragment GetOrCreateFragmentForTab(ShellTabItem tab)
		{
			return _rootFragment ?? (_rootFragment = new ShellTopTabFragment(ShellContext) { ShellItem = ShellItem });
		}
	}
}