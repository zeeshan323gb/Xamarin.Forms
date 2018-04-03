namespace Xamarin.Forms
{
	public interface ICarouselViewController : IViewController
	{
		void NotifyPositionChanged(int newPosition);
	}
}
