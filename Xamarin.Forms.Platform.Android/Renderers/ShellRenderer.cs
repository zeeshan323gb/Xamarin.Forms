using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using AView = Android.Views.View;
using AViews = Android.Views;
using LP = Android.Views.ViewGroup.LayoutParams;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace Xamarin.Forms.Platform.Android
{
	public class ShellRenderer : IVisualElementRenderer, IShellContext
	{
		#region IVisualElementRenderer
		VisualElement IVisualElementRenderer.Element => Element;

		VisualElementTracker IVisualElementRenderer.Tracker => null;

		ViewGroup IVisualElementRenderer.ViewGroup => _flyoutRenderer.AndroidView as ViewGroup;

		AView IVisualElementRenderer.View => _flyoutRenderer.AndroidView;

		event EventHandler<VisualElementChangedEventArgs> IVisualElementRenderer.ElementChanged
		{
			add { _elementChanged += value; }
			remove { _elementChanged -= value; }
		}

		event EventHandler<PropertyChangedEventArgs> IVisualElementRenderer.ElementPropertyChanged
		{
			add { _elementPropertyChanged += value; }
			remove { _elementPropertyChanged -= value; }
		}

		SizeRequest IVisualElementRenderer.GetDesiredSize(int widthConstraint, int heightConstraint)
		{
			return new SizeRequest(new Size(100, 100));
		}

		void IVisualElementRenderer.SetElement(VisualElement element)
		{
			if (Element != null)
				throw new NotSupportedException("Reuse of the Shell Renderer is not supported");
			Element = (Shell)element;
			Element.SizeChanged += OnElementSizeChanged;
			OnElementSet(Element);

			Element.PropertyChanged += OnElementPropertyChanged;
			_elementChanged?.Invoke(this, new VisualElementChangedEventArgs(null, Element));
		}

		void IVisualElementRenderer.SetLabelFor(int? id)
		{
			
		}

		void IVisualElementRenderer.UpdateLayout()
		{
		}
		#endregion IVisualElementRenderer

		#region IShellContext

		Shell IShellContext.Shell => Element;

		Context IShellContext.AndroidContext => _androidContext;

		IShellFlyoutContentRenderer IShellContext.CreateShellFlyoutContentRenderer()
		{
			return CreateShellFlyoutContentRenderer();
		}

		IShellItemRenderer IShellContext.CreateShellItemRenderer()
		{
			return CreateShellItemRenderer();
		}

		#endregion IShellContext

		private event EventHandler<PropertyChangedEventArgs> _elementPropertyChanged;
		private event EventHandler<VisualElementChangedEventArgs> _elementChanged;

		private bool _disposed = false;
		private readonly Context _androidContext;
		private IShellFlyoutRenderer _flyoutRenderer;

		public ShellRenderer(Context context)
		{
			_androidContext = context;
		}

		protected Shell Element { get; private set; }

		protected Context AndroidContext => _androidContext;

		protected virtual void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			_elementPropertyChanged?.Invoke(sender, e);
		}

		protected virtual void OnElementSet (Shell shell)
		{
			_flyoutRenderer = CreateShellFlyoutRenderer();

			var inflator = LayoutInflater.From(_androidContext);
			var root = inflator.Inflate(Resource.Layout.RootLayout, null).JavaCast<CoordinatorLayout>();

			var appBar = root.FindViewById<AppBarLayout>(Resource.Id.main_appbar);
			var ctl = root.FindViewById<CollapsingToolbarLayout>(Resource.Id.main_collapsing);
			var backdrop = root.FindViewById<ImageView>(Resource.Id.main_backdrop);
			var toolbar = root.FindViewById<Toolbar>(Resource.Id.main_toolbar);
			var scrollView = root.FindViewById<NestedScrollView>(Resource.Id.main_scrollview);

			scrollView.Background = new ColorDrawable(Color.Purple.ToAndroid());

			var textView = new TextView(_androidContext)
			{
				LayoutParameters = new LP(LP.MatchParent, LP.WrapContent),
				Text = @"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Aliquam mattis tincidunt nisl eu semper. Suspendisse potenti. Proin quis pulvinar massa, a faucibus lectus. Nunc orci odio, imperdiet id sapien consequat, ullamcorper accumsan tortor. Ut rhoncus mauris tristique elementum imperdiet. Mauris cursus orci eu tortor imperdiet, non tincidunt risus semper. Morbi consequat ex sed faucibus volutpat. Pellentesque lacus dui, hendrerit at sapien et, vulputate pulvinar libero.

Aliquam nec vestibulum nisl. In nec condimentum quam, non vehicula leo. Aenean varius dictum finibus. Nam risus velit, sagittis sed pretium at, volutpat vel purus. Donec a mi a dolor venenatis maximus a vitae urna. Etiam vitae scelerisque magna. Aenean dapibus blandit dolor sed mattis. In id erat eget lectus eleifend condimentum eget ut urna. Integer pellentesque iaculis justo. Cras id placerat augue. In auctor est ac ipsum fringilla, et egestas ex accumsan. Morbi a est quis eros gravida pretium. Aenean suscipit fringilla consequat. Suspendisse tincidunt tincidunt elementum. Maecenas suscipit quam nunc, sed accumsan mi suscipit at. Praesent rhoncus lacus nisi, a porttitor arcu molestie efficitur.

Aenean in commodo dolor. Sed et hendrerit justo. Donec libero lacus, volutpat sit amet tortor viverra, tempor facilisis leo. Nam consectetur, libero non venenatis consectetur, purus tortor iaculis nisl, a mattis lacus ex eget nibh. Nullam in lorem et justo finibus dignissim non ullamcorper purus. Cras elit sem, feugiat nec blandit ut, consequat non urna. Aenean eget ex vel ligula vehicula pellentesque. Mauris finibus vitae magna ut tincidunt. Duis cursus tristique velit, a tempor lorem aliquet non. Maecenas quis cursus neque.

Aenean a est sed sem accumsan pellentesque pellentesque vel elit. Vivamus velit mauris, suscipit vestibulum ante a, dapibus feugiat lorem. Sed ac elementum augue. Donec in lacinia diam. Nullam malesuada ac erat a suscipit. Vestibulum egestas purus in felis ullamcorper ultricies sit amet id dui. Maecenas id massa id leo imperdiet mollis. Cras eget tortor non nisl elementum aliquam id id diam. Mauris eu lorem gravida, fringilla purus eu, tempus purus. Maecenas auctor nibh nec augue tempus, in tincidunt lacus pulvinar. Nullam porta ullamcorper erat et accumsan. Duis sodales facilisis mauris.

Curabitur congue enim id dolor aliquam suscipit. Pellentesque quis est eu nulla pellentesque pretium. Fusce sit amet lobortis enim. Aliquam placerat turpis quis justo iaculis, vel euismod felis semper. Etiam tristique bibendum sagittis. Cras laoreet orci nisi, ac pharetra tellus dapibus ut. Donec condimentum magna vel neque fermentum feugiat. Vestibulum tincidunt vulputate tincidunt. Quisque at urna vel justo cursus egestas. Nullam sagittis mauris quis ipsum interdum, quis ultrices ex fringilla. Donec volutpat, massa in condimentum feugiat, felis enim posuere odio, eget tincidunt diam nisi quis massa. Ut porta finibus nulla, nec tincidunt erat vulputate at. Aenean sit amet nisi posuere, elementum justo a, viverra urna.

Pellentesque sed dui at lorem sagittis ullamcorper sit amet nec erat. In mollis odio id diam consequat, vel tempus enim accumsan. Nam tempor rutrum tincidunt. Aenean ac tincidunt felis. Quisque bibendum eu tortor at eleifend. Nulla facilisi. Praesent tempor nisi sit amet quam maximus fermentum. Nunc aliquet sed augue at varius. Nulla eget est risus. Sed sit amet mi suscipit, cursus turpis non, molestie augue. Sed sed ornare felis. Aenean laoreet tempor maximus. Ut at tellus ut urna placerat elementum non eu ante.

Fusce ultrices nisl felis, quis vestibulum est fermentum at. Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas. Aenean aliquam nisi non scelerisque luctus. Maecenas tempor aliquet felis, id lobortis leo cursus id. Aenean condimentum venenatis felis, sed commodo lorem placerat quis. Ut ut bibendum purus, a tincidunt est. Vestibulum et sapien vitae mauris iaculis pharetra quis consequat dui. Pellentesque malesuada facilisis congue. Quisque turpis nisi, commodo eu imperdiet nec, dignissim in ipsum. Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. Aliquam facilisis maximus nibh vitae pulvinar. Sed elementum laoreet lacus ut hendrerit."
			};

			scrollView.AddView(textView);

			toolbar.Title = "Testing 123";


			_flyoutRenderer.AttachFlyout(this, root);
			scrollView.RequestLayout();
		}

		protected virtual IShellFlyoutRenderer CreateShellFlyoutRenderer ()
		{
			return new ShellFlyoutRenderer(this, AndroidContext);
		}

		protected virtual IShellFlyoutContentRenderer CreateShellFlyoutContentRenderer()
		{
			return new ShellFlyoutContentRenderer(this, AndroidContext);
		}

		protected virtual IShellItemRenderer CreateShellItemRenderer()
		{
			return new ShellItemRenderer(this, AndroidContext);
		}

		private void OnElementSizeChanged(object sender, EventArgs e)
		{
			int width = (int)AndroidContext.ToPixels(Element.Width);
			int height = (int)AndroidContext.ToPixels(Element.Height);
			_flyoutRenderer.AndroidView.Measure(MeasureSpecFactory.MakeMeasureSpec(width, MeasureSpecMode.Exactly), MeasureSpecFactory.MakeMeasureSpec(height, MeasureSpecMode.Exactly));
			_flyoutRenderer.AndroidView.Layout(0, 0, (int)width, (int)height);
		}

		#region IDisposable Support


		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					Element.PropertyChanged -= OnElementPropertyChanged;
				}

				// TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
				// TODO: set large fields to null.

				_disposed = true;
			}
		}

		void IDisposable.Dispose()
		{
			Dispose(true);
		}
		#endregion

	}
}