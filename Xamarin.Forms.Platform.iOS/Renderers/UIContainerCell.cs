using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public class UIContainerCell : UITableViewCell
	{
		private IVisualElementRenderer _renderer;

		public UIContainerCell(string cellId, View view) : base(UITableViewCellStyle.Default, cellId)
		{
			View = view;

			SelectionStyle = UITableViewCellSelectionStyle.None;

			_renderer = Platform.CreateRenderer(view);
			Platform.SetRenderer(view, _renderer);

			AddSubview(_renderer.NativeView);
		}

		public View View { get; }

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();
			View.Layout(Bounds.ToRectangle());
		}
	}
}