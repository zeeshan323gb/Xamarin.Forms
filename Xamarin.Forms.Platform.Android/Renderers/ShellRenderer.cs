using System;
using System.ComponentModel;
using Android.Content;
using Android.Views;
using AView = Android.Views.View;

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

		private void OnElementSizeChanged(object sender, EventArgs e)
		{
			int width = (int)AndroidContext.ToPixels(Element.Width);
			int height = (int)AndroidContext.ToPixels(Element.Height);
			_flyoutRenderer.AndroidView.Measure(MeasureSpecFactory.MakeMeasureSpec(width, MeasureSpecMode.Exactly), MeasureSpecFactory.MakeMeasureSpec(height, MeasureSpecMode.Exactly));
			_flyoutRenderer.AndroidView.Layout(0, 0, (int)width, (int)height);
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
			_flyoutRenderer.AttachFlyout(this, new AView(AndroidContext));
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
			return null;
			//return new ShellItemRenderer(this, AndroidContext);
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