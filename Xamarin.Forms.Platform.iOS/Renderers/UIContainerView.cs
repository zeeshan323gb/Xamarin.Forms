using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public class UIContainerView : UIView
	{
		private readonly View _view;
		private IVisualElementRenderer _renderer;

		public UIContainerView(View view)
		{
			_view = view;

			_renderer = Platform.CreateRenderer(view);
			Platform.SetRenderer(view, _renderer);

			AddSubview(_renderer.NativeView);
		}

		public override void LayoutSubviews()
		{
			_view.Layout(Bounds.ToRectangle());
		}
	}
}