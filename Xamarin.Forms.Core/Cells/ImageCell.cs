using System;
using System.ComponentModel;

namespace Xamarin.Forms
{
	public class ImageCell : TextCell, IImageController
	{
		public static readonly BindableProperty ImageSourceProperty = BindableProperty.Create("ImageSource", typeof(ImageSource), typeof(ImageCell), null,
			propertyChanging: (bindable, oldvalue, newvalue) => ((ImageCell)bindable).OnSourcePropertyChanging((ImageSource)oldvalue, (ImageSource)newvalue),
			propertyChanged: (bindable, oldvalue, newvalue) => ((ImageCell)bindable).OnSourcePropertyChanged((ImageSource)oldvalue, (ImageSource)newvalue));

		internal static readonly BindablePropertyKey IsLoadingPropertyKey = BindableProperty.CreateReadOnly(nameof(IsLoading), typeof(bool), typeof(Image), default(bool));
		public static readonly BindableProperty IsLoadingProperty = IsLoadingPropertyKey.BindableProperty;

		public ImageCell()
		{
			Disappearing += (sender, e) =>
			{
				if (ImageSource == null)
					return;
				ImageSource.Cancel();
			};
		}

		[TypeConverter(typeof(ImageSourceConverter))]
		public ImageSource ImageSource
		{
			get { return (ImageSource)GetValue(ImageSourceProperty); }
			set { SetValue(ImageSourceProperty, value); }
		}

		public bool IsLoading => (bool)GetValue(IsLoadingProperty);

		protected override void OnBindingContextChanged()
		{
			if (ImageSource != null)
				SetInheritedBindingContext(ImageSource, BindingContext);

			base.OnBindingContextChanged();
		}

		void OnSourceChanged(object sender, EventArgs eventArgs)
		{
			OnPropertyChanged(ImageSourceProperty.PropertyName);
		}

		void OnSourcePropertyChanged(ImageSource oldvalue, ImageSource newvalue)
		{
			if (newvalue != null)
			{
				newvalue.SourceChanged += OnSourceChanged;
				SetInheritedBindingContext(newvalue, BindingContext);
			}
		}

		void OnSourcePropertyChanging(ImageSource oldvalue, ImageSource newvalue)
		{
			if (oldvalue != null)
				oldvalue.SourceChanged -= OnSourceChanged;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		void IImageController.SetIsLoading(bool isLoading)
		{
			SetValue(IsLoadingPropertyKey, isLoading);
		}

		ImageSource IImageController.Source => ImageSource;
	}
}