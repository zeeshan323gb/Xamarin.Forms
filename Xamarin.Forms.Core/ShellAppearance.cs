using System.Collections.Generic;
using System.ComponentModel;

namespace Xamarin.Forms
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class ShellAppearance : BindableObject, IShellAppearanceController
	{
		public Color? BackgroundColor { get; set; }

		public Color? DisabledColor { get; set; }

		public Color? ForegroundColor { get; set; }

		public Color? TabBarBackgroundColor { get; set; }

		public Color? TabBarDisabledColor { get; set; }

		public Color? TabBarForegroundColor { get; set; }

		public Color? TabBarTitleColor { get; set; }

		public Color? TabBarUnselectedColor { get; set; }

		public Color? TitleColor { get; set; }

		public Color? UnselectedColor { get; set; }

		Color IShellAppearanceController.EffectiveTabBarBackgroundColor =>
			TabBarBackgroundColor?.IsDefault == false ? TabBarBackgroundColor.Value : BackgroundColor.Value;

		Color IShellAppearanceController.EffectiveTabBarDisabledColor =>
			TabBarDisabledColor?.IsDefault == false ? TabBarDisabledColor.Value : DisabledColor.Value;

		Color IShellAppearanceController.EffectiveTabBarForegroundColor =>
			TabBarForegroundColor?.IsDefault == false ? TabBarForegroundColor.Value : ForegroundColor.Value;

		Color IShellAppearanceController.EffectiveTabBarTitleColor =>
			TabBarTitleColor?.IsDefault == false ? TabBarTitleColor.Value : TitleColor.Value;

		Color IShellAppearanceController.EffectiveTabBarUnselectedColor =>
			TabBarUnselectedColor?.IsDefault == false ? TabBarUnselectedColor.Value : UnselectedColor.Value;

		public void MakeComplete ()
		{
			if (!BackgroundColor.HasValue) BackgroundColor = Color.Default;
			if (!DisabledColor.HasValue) DisabledColor = Color.Default;
			if (!ForegroundColor.HasValue) ForegroundColor = Color.Default;
			if (!TabBarBackgroundColor.HasValue) TabBarBackgroundColor = Color.Default;
			if (!TabBarDisabledColor.HasValue) TabBarDisabledColor = Color.Default;
			if (!TabBarForegroundColor.HasValue) TabBarForegroundColor = Color.Default;
			if (!TabBarTitleColor.HasValue) TabBarTitleColor = Color.Default;
			if (!TabBarUnselectedColor.HasValue) TabBarUnselectedColor = Color.Default;
			if (!TitleColor.HasValue) TitleColor = Color.Default;
			if (!UnselectedColor.HasValue) UnselectedColor = Color.Default;
		}

		public override bool Equals(object obj)
		{
			var appearance = obj as ShellAppearance;
			return appearance != null &&
				   EqualityComparer<Color?>.Default.Equals(BackgroundColor, appearance.BackgroundColor) &&
				   EqualityComparer<Color?>.Default.Equals(DisabledColor, appearance.DisabledColor) &&
				   EqualityComparer<Color?>.Default.Equals(ForegroundColor, appearance.ForegroundColor) &&
				   EqualityComparer<Color?>.Default.Equals(TabBarBackgroundColor, appearance.TabBarBackgroundColor) &&
				   EqualityComparer<Color?>.Default.Equals(TabBarDisabledColor, appearance.TabBarDisabledColor) &&
				   EqualityComparer<Color?>.Default.Equals(TabBarForegroundColor, appearance.TabBarForegroundColor) &&
				   EqualityComparer<Color?>.Default.Equals(TabBarTitleColor, appearance.TabBarTitleColor) &&
				   EqualityComparer<Color?>.Default.Equals(TabBarUnselectedColor, appearance.TabBarUnselectedColor) &&
				   EqualityComparer<Color?>.Default.Equals(TitleColor, appearance.TitleColor) &&
				   EqualityComparer<Color?>.Default.Equals(UnselectedColor, appearance.UnselectedColor);
		}

		public override int GetHashCode()
		{
			var hashCode = -1988429770;
			hashCode = hashCode * -1521134295 + EqualityComparer<Color?>.Default.GetHashCode(BackgroundColor);
			hashCode = hashCode * -1521134295 + EqualityComparer<Color?>.Default.GetHashCode(DisabledColor);
			hashCode = hashCode * -1521134295 + EqualityComparer<Color?>.Default.GetHashCode(ForegroundColor);
			hashCode = hashCode * -1521134295 + EqualityComparer<Color?>.Default.GetHashCode(TabBarBackgroundColor);
			hashCode = hashCode * -1521134295 + EqualityComparer<Color?>.Default.GetHashCode(TabBarDisabledColor);
			hashCode = hashCode * -1521134295 + EqualityComparer<Color?>.Default.GetHashCode(TabBarForegroundColor);
			hashCode = hashCode * -1521134295 + EqualityComparer<Color?>.Default.GetHashCode(TabBarTitleColor);
			hashCode = hashCode * -1521134295 + EqualityComparer<Color?>.Default.GetHashCode(TabBarUnselectedColor);
			hashCode = hashCode * -1521134295 + EqualityComparer<Color?>.Default.GetHashCode(TitleColor);
			hashCode = hashCode * -1521134295 + EqualityComparer<Color?>.Default.GetHashCode(UnselectedColor);
			return hashCode;
		}

		public static bool operator ==(ShellAppearance appearance1, ShellAppearance appearance2)
		{
			return EqualityComparer<ShellAppearance>.Default.Equals(appearance1, appearance2);
		}

		public static bool operator !=(ShellAppearance appearance1, ShellAppearance appearance2)
		{
			return !(appearance1 == appearance2);
		}
	}
}