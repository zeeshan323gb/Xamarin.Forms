using System;
using AView = Android.Views.View;
using AV = Android.Views;
using LP = Android.Views.ViewGroup.LayoutParams;
using Android.Support.V7.Widget;
using Android.Widget;
using Android.Views;
using Android.Support.Design.Widget;

namespace Xamarin.Forms.Platform.Android
{
	public class ShellFlyoutTemplatedContentRenderer : IShellFlyoutContentRenderer
	{
		#region IShellFlyoutContentRenderer

		public AView AndroidView { get; set; }

		public event EventHandler<ElementSelectedEventArgs> ElementSelected;

		#endregion IShellFlyoutContentRenderer

		private bool _disposed;
		private readonly IShellContext _shellContext;
		private ContainerView _headerView;

		public ShellFlyoutTemplatedContentRenderer(IShellContext shellContext)
		{
			_shellContext = shellContext;

			AndroidView = LoadView(shellContext);
		}

		protected void OnElementSelected (Element element)
		{
			ElementSelected?.Invoke(this, new ElementSelectedEventArgs
			{
				Element = element
			});
		}

		protected virtual AView LoadView(IShellContext shellContext)
		{
			var context = shellContext.AndroidContext;
			var coordinator = LayoutInflater.FromContext(context).Inflate(Resource.Layout.FlyoutContent, null);
			var recycler = coordinator.FindViewById<RecyclerView>(Resource.Id.flyoutcontent_recycler);
			var appBar = coordinator.FindViewById<AppBarLayout>(Resource.Id.flyoutcontent_appbar);

			_headerView = new ContainerView(context, ((IShellController)shellContext.Shell).FlyoutHeader);
			_headerView.LayoutParameters = new AppBarLayout.LayoutParams(LP.MatchParent, LP.WrapContent)
			{
				ScrollFlags = AppBarLayout.LayoutParams.ScrollFlagScroll
			};
			appBar.AddView(_headerView);

			var adapter = new ShellFlyoutRecyclerAdapter(shellContext, OnElementSelected);
			recycler.SetBackgroundColor(Color.White.ToAndroid());
			recycler.SetLayoutManager(new LinearLayoutManager(context, (int)Orientation.Vertical, false));
			recycler.SetAdapter(adapter);

			return coordinator;
		}

		#region IDisposable
		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
				}

				_disposed = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}
		#endregion IDisposable
	}
}