using System.ComponentModel;
using System.Threading.Tasks;
using Foundation;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public class ImageCellRenderer : TextCellRenderer
	{
		public override UITableViewCell GetCell(Cell item, UITableViewCell reusableCell, UITableView tv)
		{
			var result = (CellTableViewCell)base.GetCell(item, reusableCell, tv);

			var imageCell = (ImageCell)item;

			WireUpForceUpdateSizeRequested(item, result, tv);

			SetImage(imageCell, result);

			return result;
		}

		protected override void HandlePropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			var tvc = (CellTableViewCell)sender;
			var imageCell = (ImageCell)tvc.Cell;

			base.HandlePropertyChanged(sender, args);

			if (args.PropertyName == ImageCell.ImageSourceProperty.PropertyName)
				SetImage(imageCell, tvc);
		}

		protected async void SetImage(ImageCell cell, CellTableViewCell target)
		{
			await ImageElementManager.SetImage(cell, target);
		}
	}
}
