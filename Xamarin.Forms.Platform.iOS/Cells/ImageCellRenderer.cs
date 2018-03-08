using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Foundation;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public class ImageCellRenderer : TextCellRenderer
	{
		//UIColor defaultSelectedItemColor;

		public override UITableViewCell GetCell(Cell item, UITableViewCell reusableCell, UITableView tv)
		{
			var result = (CellTableViewCell)base.GetCell(item, reusableCell, tv);

			var imageCell = (ImageCell)item;

			//if (item.SelectedBackgroundColor != Color.Default)
			//{
			//	defaultSelectedItemColor = result.SelectedBackgroundView.BackgroundColor;
			//	result.SelectedBackgroundView = new UIView()
			//	{
			//		BackgroundColor = item.SelectedBackgroundColor.ToUIColor()
			//	};
			//}

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
			//else if (args.PropertyName == ImageCell.SelectedBackgroundColorProperty.PropertyName)
			//	UpdateSelectedItemBackgroundColor(imageCell, tvc);
		}

		//private void UpdateSelectedItemBackgroundColor(ImageCell cell, CellTableViewCell target)
		//{
		//	if (cell.SelectedBackgroundColor == Color.Default)
		//		target.SelectedBackgroundView.BackgroundColor = defaultSelectedItemColor;
		//	else
		//		target.SelectedBackgroundView.BackgroundColor = cell.SelectedBackgroundColor.ToUIColor();
		//}

		async void SetImage(ImageCell cell, CellTableViewCell target)
		{
			var source = cell.ImageSource;

			target.ImageView.Image = null;

			IImageSourceHandler handler;

			if (source != null && (handler = Internals.Registrar.Registered.GetHandlerForObject<IImageSourceHandler>(source)) != null)
			{
				UIImage uiimage;
				try
				{
					uiimage = await handler.LoadImageAsync(source).ConfigureAwait(false);
				}
				catch (TaskCanceledException)
				{
					uiimage = null;
				}

				NSRunLoop.Main.BeginInvokeOnMainThread(() =>
				{
					if (target.Cell != null)
					{
						target.ImageView.Image = uiimage;
						target.SetNeedsLayout();
					}
					else
						uiimage?.Dispose();
				});
			}
			else
				target.ImageView.Image = null;
		}
	}
}
