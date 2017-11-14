using System;

namespace Xamarin.Forms.Alias
{
	public static class TextWrappingExtensions
	{
		public static LineBreakMode ToLineBreakMode(this TextWrapping self)
		{
			switch (self)
			{
				case TextWrapping.NoWrap:
					return LineBreakMode.NoWrap;
				case TextWrapping.Wrap:
					return LineBreakMode.CharacterWrap;
				case TextWrapping.WrapWholeWords:
					return LineBreakMode.WordWrap;
				default:
					throw new ArgumentOutOfRangeException(nameof(self), self, null);
			}
		}

		public static TextWrapping ToTextWrapping(this LineBreakMode self)
		{
			switch (self)
			{
				case LineBreakMode.NoWrap:
					return TextWrapping.NoWrap;
				case LineBreakMode.WordWrap:
					return TextWrapping.WrapWholeWords;
				case LineBreakMode.CharacterWrap:
					return TextWrapping.Wrap;
				case LineBreakMode.HeadTruncation:
					return TextWrapping.NoWrap;
				case LineBreakMode.TailTruncation:
					return TextWrapping.NoWrap;
				case LineBreakMode.MiddleTruncation:
					return TextWrapping.NoWrap;
				default:
					throw new ArgumentOutOfRangeException(nameof(self), self, null);
			}
		}
	}
}