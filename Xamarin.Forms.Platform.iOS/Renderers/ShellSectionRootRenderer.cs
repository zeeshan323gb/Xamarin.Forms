using CoreGraphics;
using Foundation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public class ShellSectionRootHeader : UICollectionViewController
	{
		public class ShellSectionHeaderCell : UICollectionViewCell
		{
			[Export("initWithFrame:")]
			public ShellSectionHeaderCell(CGRect frame) : base(frame)
			{
				Label = new UILabel();
				Label.TextAlignment = UITextAlignment.Center;
				Label.Font = UIFont.BoldSystemFontOfSize(14);
				Label.TextColor = UIColor.White;
				ContentView.AddSubview(Label);
			}

			public override CGSize SizeThatFits(CGSize size)
			{
				return new CGSize(Label.SizeThatFits(size).Width + 200, 35);
			}

			public override void LayoutSubviews()
			{
				base.LayoutSubviews();

				Label.Frame = Bounds;
			}

			public UILabel Label { get; }
		}

		private static readonly NSString CellId = new NSString("HeaderCell");

		private readonly ShellSection _shellSection;
		private UIView _bar;

		public ShellSectionRootHeader(ShellSection shellSection) : base (new UICollectionViewFlowLayout ())
		{
			_shellSection = shellSection;
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();


			CollectionView.BackgroundColor = UIColor.FromRGB(104, 159, 57);

			_bar = new UIView(new CGRect(0, 0, 20, 20));
			_bar.BackgroundColor = UIColor.White;
			_bar.Layer.ZPosition = 9001; //its over 9000!
			CollectionView.AddSubview(_bar);

			CollectionView.Bounces = false;
			CollectionView.AlwaysBounceHorizontal = false;

			CollectionView.ShowsHorizontalScrollIndicator = false;

			var flowLayout = Layout as UICollectionViewFlowLayout;
			flowLayout.ScrollDirection = UICollectionViewScrollDirection.Horizontal;
			flowLayout.MinimumInteritemSpacing = 0;
			flowLayout.MinimumLineSpacing = 0;
			flowLayout.EstimatedItemSize = new CGSize(70, 35);

			CollectionView.RegisterClassForCell(typeof(ShellSectionHeaderCell), CellId);
		}

		public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
		{
			var headerCell = (ShellSectionHeaderCell)collectionView.DequeueReusableCell(CellId, indexPath);

			var shellContent = _shellSection.Items[indexPath.Row];
			headerCell.Label.Text = shellContent.Title;
			headerCell.Label.SetNeedsDisplay();

			return headerCell;
		}

		public override nint NumberOfSections(UICollectionView collectionView)
		{
			return 1;
		}

		public override nint GetItemsCount(UICollectionView collectionView, nint section)
		{
			return _shellSection.Items.Count;
		}

		public override bool CanMoveItem(UICollectionView collectionView, NSIndexPath indexPath)
		{
			return false;
		}
	}

	public class ShellSectionRootRenderer : UIViewController, IShellSectionRootRenderer
	{
		#region IShellSectionRootRenderer

		bool IShellSectionRootRenderer.ShowNavBar => Shell.GetNavBarVisible(((IShellContentController)ShellSection.CurrentItem).GetOrCreateContent());

		UIViewController IShellSectionRootRenderer.ViewController => this;

		#endregion IShellSectionRootRenderer

		private readonly IShellContext _shellContext;
		private Dictionary<ShellContent, IVisualElementRenderer> _renderers = new Dictionary<ShellContent, IVisualElementRenderer>();
		private UIView _containerArea;
		private IShellPageRendererTracker _tracker;
		private ShellSectionRootHeader _header;

		public ShellSectionRootRenderer(ShellSection shellSection, IShellContext shellContext)
		{
			ShellSection = shellSection ?? throw new ArgumentNullException(nameof(shellSection));
			_shellContext = shellContext;
		}

		private ShellSection ShellSection { get; set; }

		public override void ViewDidLayoutSubviews()
		{
			base.ViewDidLayoutSubviews();

			_containerArea.Frame = View.Bounds;

			LayoutRenderers();

			LayoutHeader();
		}

		private void LayoutHeader()
		{
			_header.View.Frame = new CGRect(View.Bounds.X, View.SafeAreaInsets.Top, View.Bounds.Width, 35);
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			_containerArea = new UIView();
			_containerArea.InsetsLayoutMarginsFromSafeArea = false;

			View.AddSubview(_containerArea);

			LoadRenderers();

			_tracker = _shellContext.CreatePageRendererTracker();
			_tracker.IsRootPage = true;
			_tracker.ViewController = this;
			_tracker.Page = ((IShellContentController)ShellSection.CurrentItem).Page;

			ShellSection.PropertyChanged += OnShellSectionPropertyChanged;

			_header = new ShellSectionRootHeader(ShellSection);
			AddChildViewController(_header);
			View.AddSubview(_header.View);
		}

		public override void ViewSafeAreaInsetsDidChange()
		{
			base.ViewSafeAreaInsetsDidChange();

			LayoutHeader();
		}

		protected virtual void LayoutRenderers()
		{
			var items = ShellSection.Items;
			int i;
			for (i = 0; i < items.Count; i++)
			{
				var shellContent = items[i];
				if (_renderers.TryGetValue(shellContent, out var renderer))
				{
					var view = renderer.NativeView;
					view.Frame = new CGRect(i * View.Bounds.Width, 0, View.Bounds.Width, View.Bounds.Height);
				}
			}

			//_scrollView.ContentSize = new CGSize(View.Bounds.Width * i, View.Bounds.Height);
		}

		protected virtual void LoadRenderers()
		{
			foreach (var item in ShellSection.Items)
			{
				var page = ((IShellContentController)item).GetOrCreateContent();
				var renderer = Platform.CreateRenderer(page);
				Platform.SetRenderer(page, renderer);

				AddChildViewController(renderer.ViewController);
				_containerArea.AddSubview(renderer.NativeView);

				_renderers[item] = renderer;
			}
		}

		protected virtual void OnShellSectionPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == ShellSection.CurrentItemProperty.PropertyName)
			{
				_tracker.Page = ((IShellContentController)ShellSection.CurrentItem).Page;
			}
		}
	}
}