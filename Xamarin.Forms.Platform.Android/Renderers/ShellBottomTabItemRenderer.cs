using Android.Content;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Support.Design.Internal;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;
using System;
using AColor = Android.Graphics.Color;
using AView = Android.Views.View;
using ColorStateList = Android.Content.Res.ColorStateList;
using IMenu = Android.Views.IMenu;
using LP = Android.Views.ViewGroup.LayoutParams;
using Orientation = Android.Widget.Orientation;
using R = Android.Resource;
using Typeface = Android.Graphics.Typeface;
using TypefaceStyle = Android.Graphics.TypefaceStyle;

namespace Xamarin.Forms.Platform.Android
{
	public class ShellBottomTabItemRenderer : ShellItemRendererBase, BottomNavigationView.IOnNavigationItemSelectedListener, IAppearanceObserver
	{
		#region IOnNavigationItemSelectedListener

		bool BottomNavigationView.IOnNavigationItemSelectedListener.OnNavigationItemSelected(IMenuItem item)
		{
			return OnItemSelected(item);
		}

		#endregion IOnNavigationItemSelectedListener

		#region IAppearanceObserver

		void IAppearanceObserver.OnAppearanceChanged(ShellAppearance appearance)
		{
			if (appearance != null)
				ApplyAppearance(appearance);
			else
				ResetAppearance();
		}

		#endregion IAppearanceObserver

		protected const int MoreTabId = 99;
		private BottomNavigationView _bottomView;
		private ColorStateList _defaultList;
		private Color _lastColor = Color.Default;
		private FrameLayout _navigationArea;

		public ShellBottomTabItemRenderer(IShellContext shellContext) : base(shellContext)
		{
		}

		public override AView OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			var outerLayout = inflater.Inflate(Resource.Layout.BottomTabLayout, null);
			_bottomView = outerLayout.FindViewById<BottomNavigationView>(Resource.Id.bottomtab_tabbar);
			_navigationArea = outerLayout.FindViewById<FrameLayout>(Resource.Id.bottomtab_navarea);
			_navigationArea.SetBackgroundColor(Color.Black.ToAndroid());

			_bottomView.SetBackgroundColor(Color.White.ToAndroid());
			_bottomView.SetOnNavigationItemSelectedListener(this);

			HookEvents(ShellItem);
			SetupMenu(_bottomView.Menu, _bottomView.MaxItemCount, ShellItem);

			((IShellController)ShellContext.Shell).AddAppearanceObserver(this, ShellItem);

			return outerLayout;
		}

		// Use OnDestory become OnDestroyView may fire before events are completed.
		public override void OnDestroy()
		{
			UnhookEvents(ShellItem);
			if (_bottomView != null)
			{
				_bottomView.SetOnNavigationItemSelectedListener(null);
				_bottomView.Dispose();
				_bottomView = null;
			}

			((IShellController)ShellContext.Shell).RemoveAppearanceObserver(this);

			base.OnDestroy();
		}

		protected virtual void ApplyAppearance(ShellAppearance appearance)
		{
			if (_defaultList == null)
			{
				_defaultList = _bottomView.ItemTextColor;
			}

			IShellAppearanceController controller = appearance;
			var background = controller.EffectiveTabBarBackgroundColor;
			var foreground = controller.EffectiveTabBarForegroundColor;
			var disabled = controller.EffectiveTabBarDisabledColor;
			var unselected = controller.EffectiveTabBarUnselectedColor;
			var title = controller.EffectiveTabBarTitleColor;

			var colorStateList = MakeColorStateList(title, disabled, unselected);
			_bottomView.ItemTextColor = colorStateList;
			_bottomView.ItemIconTintList = colorStateList;

			SetBackgroundColor(background);
		}

		protected override IShellObservableFragment CreateFragmentForPage(Page page)
		{
			return new ShellContentFragment(ShellContext, page);
		}

		protected virtual Drawable CreateItemBackgroundDrawable()
		{
			var stateList = ColorStateList.ValueOf(Color.Black.MultiplyAlpha(0.2).ToAndroid());
			return new RippleDrawable(stateList, new ColorDrawable(AColor.White), null);
		}

		protected virtual BottomSheetDialog CreateMoreBottomSheet(Action<ShellTabItem, BottomSheetDialog> selectCallback)
		{
			var bottomSheetDialog = new BottomSheetDialog(Context);
			var bottomSheetLayout = new LinearLayout(Context);
			bottomSheetLayout.LayoutParameters = new LP(LP.MatchParent, LP.WrapContent);
			bottomSheetLayout.Orientation = Orientation.Vertical;
			// handle the more tab
			for (int i = 4; i < ShellItem.Items.Count; i++)
			{
				var tab = ShellItem.Items[i];

				var innerLayout = new LinearLayout(Context);
				innerLayout.ClipToOutline = true;
				innerLayout.SetBackground(CreateItemBackgroundDrawable());
				innerLayout.SetPadding(0, (int)Context.ToPixels(6), 0, (int)Context.ToPixels(6));
				innerLayout.Orientation = Orientation.Horizontal;
				innerLayout.LayoutParameters = new LP(LP.MatchParent, LP.WrapContent);

				// technically the unhook isn't needed
				// we dont even unhook the events that dont fire
				void clickCallback(object s, EventArgs e)
				{
					selectCallback(tab, bottomSheetDialog);
					innerLayout.Click -= clickCallback;
				}
				innerLayout.Click += clickCallback;

				var image = new ImageView(Context);
				image.LayoutParameters = new LinearLayout.LayoutParams((int)Context.ToPixels(32), (int)Context.ToPixels(32))
				{
					LeftMargin = (int)Context.ToPixels(20),
					RightMargin = (int)Context.ToPixels(20),
					TopMargin = (int)Context.ToPixels(6),
					BottomMargin = (int)Context.ToPixels(6),
					Gravity = GravityFlags.Center
				};
				image.ImageTintList = ColorStateList.ValueOf(Color.Black.MultiplyAlpha(0.6).ToAndroid());
				SetImage(image, tab.Icon);

				innerLayout.AddView(image);

				var text = new TextView(Context);
				text.SetTypeface(Typeface.Create("sans-serif-medium", TypefaceStyle.Normal), TypefaceStyle.Normal);
				text.SetTextColor(AColor.Black);
				text.Text = tab.Title;
				text.LayoutParameters = new LinearLayout.LayoutParams(0, LP.WrapContent)
				{
					Gravity = GravityFlags.Center,
					Weight = 1
				};

				innerLayout.AddView(text);

				bottomSheetLayout.AddView(innerLayout);
			}

			bottomSheetDialog.SetContentView(bottomSheetLayout);

			return bottomSheetDialog;
		}

		protected override ViewGroup GetNavigationTarget() => _navigationArea;

		protected override IShellObservableFragment GetOrCreateFragmentForTab(ShellTabItem tab)
		{
			return new ShellContentFragment(ShellContext, tab);
		}

		protected override void OnCurrentTabItemChanged()
		{
			base.OnCurrentTabItemChanged();

			var index = ShellItem.Items.IndexOf(CurrentTabItem);
			index = Math.Min(index, _bottomView.Menu.Size() - 1);
			if (index < 0)
				return;
			var menuItem = _bottomView.Menu.GetItem(index);
			menuItem.SetChecked(true);
		}

		protected virtual bool OnItemSelected(IMenuItem item)
		{
			var id = item.ItemId;
			if (id == MoreTabId)
			{
				var bottomSheetDialog = CreateMoreBottomSheet(OnMoreItemSelected);
				bottomSheetDialog.Show();
				bottomSheetDialog.DismissEvent += OnMoreSheetDismissed;
			}
			else
			{
				if (!item.IsChecked)
				{
					var shellTabItem = ShellItem.Items[id];
					ShellItem.SetValueFromRenderer(ShellItem.CurrentItemProperty, shellTabItem);
				}
			}

			return true;
		}

		protected virtual void OnMoreItemSelected(ShellTabItem tabItem, BottomSheetDialog dialog)
		{
			ShellItem.SetValueFromRenderer(ShellItem.CurrentItemProperty, tabItem);

			dialog.Dismiss();
			dialog.Dispose();
		}

		protected virtual void OnMoreSheetDismissed(object sender, EventArgs e)
		{
			OnCurrentTabItemChanged();
		}

		protected virtual void ResetAppearance()
		{
			if (_defaultList != null)
			{
				_bottomView.ItemTextColor = _defaultList;
				_bottomView.ItemIconTintList = _defaultList;
			}

			SetBackgroundColor(Color.White);
		}

		protected virtual void SetBackgroundColor(Color color)
		{
			if (_lastColor.IsDefault)
				_lastColor = color;

			var menuView = _bottomView.GetChildAt(0) as BottomNavigationMenuView;

			if (menuView == null)
			{
				_bottomView.SetBackground(new ColorDrawable(color.ToAndroid()));
			}
			else
			{
				var index = ShellItem.Items.IndexOf(CurrentTabItem);
				index = Math.Min(index, _bottomView.Menu.Size() - 1);

				var child = menuView.GetChildAt(index);
				var touchPoint = new Point(child.Left + (child.Right - child.Left) / 2, child.Top + (child.Bottom - child.Top) / 2);

				_bottomView.SetBackground(new ColorChangeRevealDrawable(_lastColor.ToAndroid(), color.ToAndroid(), touchPoint));
				_lastColor = color;
			}
		}

		protected virtual void SetupMenu(IMenu menu, int maxBottomItems, ShellItem shellItem)
		{
			bool showMore = ShellItem.Items.Count > maxBottomItems;

			int end = showMore ? maxBottomItems - 1 : ShellItem.Items.Count;

			for (int i = 0; i < end; i++)
			{
				var item = shellItem.Items[i];
				var menuItem = menu.Add(0, i, 0, new Java.Lang.String(item.Title));
				SetMenuItemIcon(menuItem, item.Icon);
				if (item == CurrentTabItem)
				{
					menuItem.SetChecked(true);
				}
			}

			if (showMore)
			{
				var menuItem = menu.Add(0, MoreTabId, 0, new Java.Lang.String("More"));
				menuItem.SetIcon(Resource.Drawable.abc_ic_menu_overflow_material);
			}

			_bottomView.Visibility = end == 1 ? ViewStates.Gone : ViewStates.Visible;

			_bottomView.SetShiftMode(false, false);
		}

		private ColorStateList MakeColorStateList(Color titleColor, Color disabledColor, Color unselectedColor)
		{
			var states = new int[][] {
				new int[] { -R.Attribute.StateEnabled },
				new int[] {R.Attribute.StateChecked },
				new int[] { }
			};

			var disabledInt = disabledColor.IsDefault ?
				_defaultList.GetColorForState(new[] { -R.Attribute.StateEnabled }, AColor.Gray) :
				disabledColor.ToAndroid().ToArgb();

			var checkedInt = titleColor.IsDefault ?
				_defaultList.GetColorForState(new[] { R.Attribute.StateChecked }, AColor.Black) :
				titleColor.ToAndroid().ToArgb();

			var defaultColor = unselectedColor.IsDefault ?
				_defaultList.GetColorForState(new int[0], AColor.Black) :
				unselectedColor.ToAndroid().ToArgb();

			var colors = new[] { disabledInt, checkedInt, defaultColor };

			return new ColorStateList(states, colors);
		}

		private async void SetImage(ImageView image, ImageSource source)
		{
			image.SetImageDrawable(await Context.GetFormsDrawable(source));
		}

		private async void SetMenuItemIcon(IMenuItem menuItem, ImageSource source)
		{
			if (source == null)
				return;
			var drawable = await Context.GetFormsDrawable(source);
			menuItem.SetIcon(drawable);
		}
	}
}