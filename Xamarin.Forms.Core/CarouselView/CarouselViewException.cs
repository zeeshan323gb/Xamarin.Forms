using System;

namespace Xamarin.Forms
{
	public class CarouselViewException : Exception
	{
		public CarouselViewException()
		{
		}

		public CarouselViewException(string message)
			: base(message)
		{
		}

		public CarouselViewException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
