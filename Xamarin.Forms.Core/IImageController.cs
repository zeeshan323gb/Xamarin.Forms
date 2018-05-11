using Xamarin.Forms.Internals;

namespace Xamarin.Forms
{
	public interface IImageController
	{
		void NativeSizeChanged();
		void SetIsLoading(bool isLoading);
		ImageSource Source { get; }
		void InvalidateMeasure(InvalidationTrigger trigger);
	}
}