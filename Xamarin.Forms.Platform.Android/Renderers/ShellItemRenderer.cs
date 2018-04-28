using Android.Content;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Views;
using AView = Android.Views.View;

namespace Xamarin.Forms.Platform.Android
{
	public class ShellItemRenderer : IShellItemRenderer
	{
		#region IShellItemRenderer

		AView IShellItemRenderer.AndroidView => _rootView;

		ShellItem IShellItemRenderer.ShellItem
		{
			get { return _shellItem; }
			set { _shellItem = value; }
		}

		#endregion IShellItemRenderer

		private bool _disposed = false;
		private ShellItem _shellItem;
		private AView _rootView = null;
		private readonly IShellContext _shellContext;
		private readonly Context _androidContext;

		public ShellItemRenderer(IShellContext shellContext, Context androidContext)
		{
			_shellContext = shellContext;
			_androidContext = androidContext;
		}

		private void BuildLayout ()
		{
			//var root = new CoordinatorLayout(_androidContext);
			//root.LayoutParameters = new LP(LP.MatchParent, LP.MatchParent);
			//root.SetFitsSystemWindows(true);

			//var appBarLayout = new AppBarLayout(_androidContext);
			//appBarLayout.LayoutParameters = new LP(LP.MatchParent, (int)_androidContext.ToPixels(300));
			//appBarLayout.SetFitsSystemWindows(true);

			var inflator = LayoutInflater.From(_androidContext);
			var root = inflator.Inflate(Resource.Layout.RootLayout, null).JavaCast<CoordinatorLayout>();

			_rootView = root;
		}

		#region IDisposable Support
		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					// TODO: dispose managed state (managed objects).
				}

				// TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
				// TODO: set large fields to null.

				_disposed = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}
		#endregion
	}
}