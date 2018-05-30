using CoreGraphics;
using Foundation;
using System;
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
				return new CGSize(Label.SizeThatFits(size).Width + 30, 35);
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
		private UIView _bottomShadow;

		public ShellSectionRootHeader(ShellSection shellSection) : base (new UICollectionViewFlowLayout ())
		{
			_shellSection = shellSection;
		}

		protected virtual void OnShellSectionPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == ShellSection.CurrentItemProperty.PropertyName)
			{
				UpdateSelectedIndex();
			}
		}

		protected virtual void UpdateSelectedIndex (bool animated = false)
		{
			SelectedIndex = _shellSection.Items.IndexOf(_shellSection.CurrentItem);
			LayoutBar();
		}

		public double SelectedIndex { get; set; }

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();


			CollectionView.BackgroundColor = UIColor.FromRGB(104, 159, 57);
			CollectionView.ScrollsToTop = false;
			CollectionView.Bounces = false;
			CollectionView.AlwaysBounceHorizontal = false;
			CollectionView.ShowsHorizontalScrollIndicator = false;
			CollectionView.ClipsToBounds = false;

			_bar = new UIView(new CGRect(0, 0, 20, 20));
			_bar.BackgroundColor = UIColor.White;
			_bar.Layer.ZPosition = 9001; //its over 9000!
			CollectionView.AddSubview(_bar);

			_bottomShadow = new UIView(new CGRect(0, 0, 10, 1));
			_bottomShadow.BackgroundColor = Color.Black.MultiplyAlpha(0.3).ToUIColor();
			_bottomShadow.Layer.ZPosition = 9002;
			CollectionView.AddSubview(_bottomShadow);

			var flowLayout = Layout as UICollectionViewFlowLayout;
			flowLayout.ScrollDirection = UICollectionViewScrollDirection.Horizontal;
			flowLayout.MinimumInteritemSpacing = 0;
			flowLayout.MinimumLineSpacing = 0;
			flowLayout.EstimatedItemSize = new CGSize(70, 35);

			CollectionView.RegisterClassForCell(typeof(ShellSectionHeaderCell), CellId);

			UpdateSelectedIndex();
			_shellSection.PropertyChanged += OnShellSectionPropertyChanged;
		}

		public override void ViewDidLayoutSubviews()
		{
			base.ViewDidLayoutSubviews();

			LayoutBar();

			_bottomShadow.Frame = new CGRect(0, CollectionView.Frame.Bottom, CollectionView.Frame.Width, 0.5);
		}

		protected void LayoutBar()
		{
			var layout = CollectionView.GetLayoutAttributesForItem(NSIndexPath.FromItemSection((int)SelectedIndex, 0));

			var frame = layout.Frame;

			_bar.Frame = new CGRect(frame.X, frame.Bottom - 4, frame.Width, 4);
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

		public override void ItemSelected(UICollectionView collectionView, NSIndexPath indexPath)
		{
			var row = indexPath.Row;

			var item = _shellSection.Items[row];

			if (item != _shellSection.CurrentItem)
				_shellSection.SetValueFromRenderer(ShellSection.CurrentItemProperty, item);
		}
	}
}