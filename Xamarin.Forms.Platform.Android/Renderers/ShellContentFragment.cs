using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V4.Widget;
using Android.Views;
using System;
using AView = Android.Views.View;
using AndroidAnimation = Android.Views.Animations.Animation;
using AnimationSet = Android.Views.Animations.AnimationSet;
using Android.Views.Animations;

namespace Xamarin.Forms.Platform.Android
{
	public class ShellContentFragment : Fragment, AndroidAnimation.IAnimationListener, IShellObservableFragment
	{
		private Page _page;
		private IVisualElementRenderer _renderer;
		private AView _root;
		private ShellTabItem _shellTabItem;
		private ShellPageContainer _shellPageContainer;

		public ShellContentFragment(ShellTabItem shellTabItem)
		{
			_shellTabItem = shellTabItem;
		}

		public ShellContentFragment(Page page)
		{
			_page = page;
		}

		public event EventHandler AnimationFinished;

		public Fragment Fragment => this;

		public override AndroidAnimation OnCreateAnimation(int transit, bool enter, int nextAnim)
		{
			var result = base.OnCreateAnimation(transit, enter, nextAnim);

			if (result == null && nextAnim != 0)
			{
				result = AnimationUtils.LoadAnimation(Context, nextAnim);
			}

			result.SetAnimationListener(this);
			var animSet = new AnimationSet(true);
			animSet.AddAnimation(result);

			return animSet;
		}

		public override AView OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			if (_shellTabItem != null)
			{
				_page = ((IShellTabItemController)_shellTabItem).GetOrCreateContent();
			}

			_root = inflater.Inflate(Resource.Layout.ShellContent, null).JavaCast<CoordinatorLayout>();
			var scrollview = _root.FindViewById<NestedScrollView>(Resource.Id.shellcontent_scrollview);

			_renderer = Platform.CreateRenderer(_page, Context);
			Platform.SetRenderer(_page, _renderer);

			_shellPageContainer = new ShellPageContainer(Context, _renderer);

			scrollview.AddView(_shellPageContainer);

			return _root;
		}

		// Use OnDestroy instead of OnDestroyView because OnDestroyView will be 
		// called before the animation completes. This causes tons of tiny issues.
		public override void OnDestroy()
		{
			base.OnDestroy();

			_shellPageContainer.RemoveAllViews();
			_renderer?.Dispose();
			_root?.Dispose();

			if (_shellTabItem != null)
			{
				((IShellTabItemController)_shellTabItem).RecyclePage(_page);
				_page.ClearValue(Platform.RendererProperty);
				_page = null;
			}

			_root = null;
			_renderer = null;
		}

		void AndroidAnimation.IAnimationListener.OnAnimationEnd(AndroidAnimation animation)
		{
			AnimationFinished?.Invoke(this, EventArgs.Empty);
		}

		void AndroidAnimation.IAnimationListener.OnAnimationRepeat(AndroidAnimation animation)
		{
		}

		void AndroidAnimation.IAnimationListener.OnAnimationStart(AndroidAnimation animation)
		{
		}
	}
}