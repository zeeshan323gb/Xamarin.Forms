using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using CoreGraphics;
using Foundation;
using UIKit;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.iOS
{
	// TODO hartez 2018/05/30 08:58:42 This follows the same basic scheme as RecyclerView.Adapter; you should be able to reuse the same wrapper class for the IEnumerable	
	//// TODO hartez 2018/05/30 09:05:38 Think about whether this Controller and/or the new Adapter should be internal or public
	public class CollectionViewRenderer : ViewRenderer<CollectionView, UICollectionView>
	{
		CollectionViewController _collectionViewController;

		public override UIViewController ViewController => _collectionViewController;

		// TODO hartez 2018/05/31 11:50:26 Add a property for scroll direction	
		// TODO hartez 2018/05/31 11:50:43 Add a property for the fixed width/height and use it in ItemSize	
		internal class ListViewLayout : UICollectionViewFlowLayout
		{
			public ListViewLayout()
			{
				Initialize();
			}

			void Initialize()
			{
				EstimatedItemSize = new CGSize(200, 40);
				ScrollDirection = UICollectionViewScrollDirection.Vertical;
			}

			public override CGSize ItemSize {
				get
				{
					// TODO hartez 2018/05/30 12:32:16 This itemheight is very obviously not what we want	
					var x = new CGSize(200, 40);
					return x;
				}
				set { base.ItemSize = value; }
			}
		}

		public override SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			return Control.GetSizeRequest(widthConstraint, heightConstraint, 0, 0);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<CollectionView> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				var layout = new ListViewLayout();
				_collectionViewController = new CollectionViewController(e.NewElement.ItemsSource, layout);
				SetNativeControl(_collectionViewController.CollectionView);
			}
		}
	}

	internal class CollectionViewController : UICollectionViewController
	{
		readonly IEnumerable _itemsSource;

		public CollectionViewController(IEnumerable itemsSource, UICollectionViewLayout layout) : base(layout)
		{
			_itemsSource = itemsSource;
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			CollectionView.RegisterClassForCell (typeof(DefaultCell), DefaultCell.DefaultCellId);
		}

		public override nint GetItemsCount (UICollectionView collectionView, nint section)
		{
			return (_itemsSource as IList).Count;
		}

		public override UICollectionViewCell GetCell (UICollectionView collectionView, NSIndexPath indexPath)
		{
			// TODO hartez 2018/05/31 11:53:51 Find guidance on what this should do if it can't dequeue a cell	(Probably throw?)
			var cell = collectionView.DequeueReusableCell(DefaultCell.DefaultCellId, indexPath) as DefaultCell;

			if (_itemsSource is IList list)
			{
				cell.Label.Text = list[indexPath.Row].ToString();
			}

			return cell;
		}
	}

	internal class DefaultCell : UICollectionViewCell
	{
		public static NSString DefaultCellId = new NSString("DefaultCell");

		public UILabel Label { get; }

		[Export("initWithFrame:")]
		public DefaultCell(CGRect frame) : base(frame)
		{
			// TODO hartez 2018/05/31 11:52:33 Move all this into an overrideable init method	
			ContentView.BackgroundColor = UIColor.White;

			Label = new UILabel(Bounds)
			{
				TextColor = UIColor.Black
			};
			
			ContentView.AddSubview(Label);
		}
	}
}