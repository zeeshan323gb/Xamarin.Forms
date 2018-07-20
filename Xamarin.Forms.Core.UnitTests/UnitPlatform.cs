using System;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Core.UnitTests
{
	public class UnitPlatform 
	{
		readonly bool useRealisticLabelMeasure;

		public UnitPlatform (Func<VisualElement, double, double, SizeRequest> getNativeSizeFunc = null, bool useRealisticLabelMeasure = false)
		{
			this.useRealisticLabelMeasure = useRealisticLabelMeasure;
		}

		
	}
	
}
