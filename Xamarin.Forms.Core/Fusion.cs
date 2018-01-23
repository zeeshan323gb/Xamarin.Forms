using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms.Core;

namespace Xamarin.Forms
{
	/*
	 * var centerFuse = new Fusion (mainLayout).Center.Add(20, new Fusion (label2).Height);
	 * new Fusion (mainLayout).Center.Add(20, new Fusion (label2).Height).Center
	 * new Fusion (mainLayout).Center.Center
	 * 
	 * 
var sizeFuse = new Fusion (otherLabel).Size.Add (20, 20);
FusedLayout.AddFusion (view2, FuseProperty.Center, centerFuse);
FusedLayout.AddFusion (view2, FuseProperty.Size, sizeFuse);
var sizeFuse = new Fusion (view2).Measure().Add (20, 20);(

		// view2 is the targetview    mainLayout is the source view
		FusedLayout.AddFusion(view2, FuseProperty.Center, new Fuse (mainLayout).Center.Add (20, 0));
 */


	public abstract class PointFusionBase : FusionBase
	{
		protected PointFusionBase(FusionBase parent) : base(parent)
		{
		}

		public PointFusion Add(Point point)
		{
			return new PointFusion(point, this);
		}
	}

	public class PointFusion : PointFusionBase
	{
		public PointFusion(Point addMe, FusionBase parent) : base(parent)
		{
			SourceElement = parent.SourceElement;
			SourceProperty = parent.SourceProperty;
			Addition = addMe;
		}

		public Point Addition
		{
			get;
		}


		public override double? Calculate(FuseProperty propertyXYHW)
		{
			var calculatedSize = ParentFusion.Calculate(propertyXYHW);
			if (!calculatedSize.HasValue) { return null; }
			switch (propertyXYHW)
			{
				case FuseProperty.X:
					calculatedSize += Addition.X;
					break;
				case FuseProperty.Y:
					calculatedSize += Addition.Y;
					break;
				default:
					throw new ArgumentException($"{propertyXYHW}");
			}

			return calculatedSize;

		}
	}


	public class CenterFusion : PointFusionBase
	{
		public CenterFusion(FusionBase parent) : base(parent)
		{
		}

		public override double? Calculate(FuseProperty propertyXYHW)
		{
			switch (propertyXYHW)
			{
				case FuseProperty.X:
					var parentX = ParentFusion.Calculate(FuseProperty.X);
					if (!parentX.HasValue) { return null; }

					var parentWidth = ParentFusion.Calculate(FuseProperty.Width);
					if (!parentWidth.HasValue) { return null; }

					return ((parentX.Value + parentWidth.Value) / 2);


				case FuseProperty.Y:
					var parentY = ParentFusion.Calculate(FuseProperty.Y);
					if (!parentY.HasValue) { return null; }

					var parentHeight = ParentFusion.Calculate(FuseProperty.Height);
					if (!parentHeight.HasValue) { return null; }

					return ((parentY.Value + parentHeight.Value) / 2);

				default:
					throw new ArgumentException($"{propertyXYHW}");
			}
		}
	}

	public abstract class FusionBase : BindableObject
	{

		protected FusionBase(FusionBase parent)
		{
			this.ParentFusion = parent;
		}
		 
		public FusionBase ParentFusion
		{
			get;
		}

		public VisualElement SourceElement { get; set; } // BP 

		public FuseProperty SourceProperty { get; set; } // BP 

		/// <summary>
		/// 
		/// </summary>
		/// <param name="targetView"></param>
		/// <param name="propertyXYHW">Needs a better name. This basically indicates this property is only going to be XYHW		
		/// </param>
		/// <returns></returns>
		public abstract double? Calculate(FuseProperty propertyXYHW);
	}


	/// <summary>
	/// Transforms the Source result to the target property
	/// </summary>
	public class TargetWrapper
	{
		public TargetWrapper(FusionBase fusion, FuseProperty targetProperty, View targetView)
		{
			Fuse = fusion;
			TargetProperty = targetProperty;
			TargetView = targetView;
		}


		string getErrorString(FuseProperty targetViewPropertyXYWH)
		{
			return $"{targetViewPropertyXYWH} - {TargetProperty}";
		}

		public bool Influences(FuseProperty targetViewPropertyXYWH)
		{
			switch (targetViewPropertyXYWH)
			{
				case FuseProperty.X:
					switch (TargetProperty)
					{
						case FuseProperty.X:
						case FuseProperty.Center:
							return true;
						case FuseProperty.Y:
							return false;
						default:
							throw new ArgumentException(getErrorString(targetViewPropertyXYWH));
					}
				case FuseProperty.Y:
					switch (TargetProperty)
					{
						case FuseProperty.Y:
						case FuseProperty.Center:
							return true;
						case FuseProperty.X:
							return false;
						default:
							throw new ArgumentException(getErrorString(targetViewPropertyXYWH));

					}
				case FuseProperty.Height:
					switch (TargetProperty)
					{
						case FuseProperty.Height:
							return true;
						case FuseProperty.Width:
						case FuseProperty.X:
						case FuseProperty.Center:
							return false;
						default:
							throw new ArgumentException(getErrorString(targetViewPropertyXYWH));

					}
				case FuseProperty.Width:
					switch (TargetProperty)
					{
						case FuseProperty.Height:
							return true;
						case FuseProperty.Width:
						case FuseProperty.X:
						case FuseProperty.Center:
							return false;
						default:
							throw new ArgumentException(getErrorString(targetViewPropertyXYWH));

					}
				default:
					throw new ArgumentException(getErrorString(targetViewPropertyXYWH));
			}
		}

		internal double? Calculate(FuseProperty targetViewPropertyXYWHCalculating)
		{			
			double? sourceResult = Fuse.Calculate(targetViewPropertyXYWHCalculating);
			if (!sourceResult.HasValue) { return null; }
			var targetSolve = FusedLayout.GetSolveView(TargetView);

			switch (TargetProperty)
			{
				case FuseProperty.X:
					return sourceResult;
				case FuseProperty.Center:
					switch(targetViewPropertyXYWHCalculating)
					{
						case FuseProperty.X:
							if (!targetSolve.Width.IsSolved) { return null; }
							return sourceResult.Value - targetSolve.Width.Value / 2;

						case FuseProperty.Y:
							if (!targetSolve.Height.IsSolved) { return null; }
							return sourceResult.Value - targetSolve.Height.Value / 2;

						default:
							throw new ArgumentException(getErrorString(targetViewPropertyXYWHCalculating));

					}
				default:
					throw new ArgumentException(getErrorString(targetViewPropertyXYWHCalculating));

			}

			throw new ArgumentException(getErrorString(targetViewPropertyXYWHCalculating));
		}

		public FuseProperty TargetProperty { get; set; } // BP 
		public FusionBase Fuse { get; set; }
		public View TargetView { get; set; } // BP 
	}


	public class ScalarOperationFusion : FusionBase
	{
		private readonly FuseOperator _operation;
		private readonly double _value;

		public ScalarOperationFusion(FusionBase parent, FuseOperator operation, double value) : base(parent)
		{
			SourceProperty = parent.SourceProperty;
			SourceElement = parent.SourceElement;
			_operation = operation;
			_value = value;
		}

		public override double? Calculate(FuseProperty propertyXYHW)
		{
			var calculated = ParentFusion.Calculate(propertyXYHW);
			if (!calculated.HasValue) { return null; }
			var returnValue = calculated.Value;

			switch(_operation)
			{
				case FuseOperator.Add:
					returnValue += _value;
					break;

				case FuseOperator.Subtract:
					returnValue -= _value;
					break;

				default:
					throw new ArgumentException($"{propertyXYHW}");

			}

			return returnValue;
		}
	}


	public class ScalarPropertyFusion : FusionBase
	{
		public ScalarPropertyFusion(
			FusionBase parent, 
			FuseProperty sourceProperty) : base(parent)
		{
			SourceProperty = sourceProperty;
			SourceElement = parent.SourceElement;
		}

		public ScalarOperationFusion Add(double value)
		{
			return new ScalarOperationFusion(this, FuseOperator.Add, value);
		}


		public override double? Calculate(FuseProperty propertyXYHW)
		{
			if (propertyXYHW != SourceProperty)
				throw new ArgumentException($"Invalid SourceProperty: {SourceProperty}");

			var dependentSourceSolve = FusedLayout.GetSolveView(SourceElement);
			switch (SourceProperty)
			{
				case FuseProperty.X:
					{
						return dependentSourceSolve.X.Value;
					}
				case FuseProperty.Y:
					{
						return dependentSourceSolve.Y.Value;
					}
				case FuseProperty.Height:
					{
						return dependentSourceSolve.Height.Value;
					}
				case FuseProperty.Width:
					{
						return dependentSourceSolve.Width.Value;
					}
			}

			throw new ArgumentException($"Invalid SourceProperty: {SourceProperty}");
		}
	}

	public class Fusion : FusionBase
	{
		public Fusion(VisualElement sourceElement) : base(null)
		{
			SourceElement = sourceElement;
		}

		public CenterFusion Center  => new CenterFusion(this);

		public ScalarPropertyFusion X => new ScalarPropertyFusion(this, FuseProperty.X);
		public ScalarPropertyFusion Y => new ScalarPropertyFusion(this, FuseProperty.Y);

		public override double? Calculate(FuseProperty propertyXYHW)
		{
			var dependentSourceSolve = FusedLayout.GetSolveView(SourceElement);
			switch (propertyXYHW)
			{
				case FuseProperty.X:
					{
						return dependentSourceSolve.X.Value;
					}
				case FuseProperty.Y:
					{
						return dependentSourceSolve.Y.Value;
					}
				case FuseProperty.Height:
					{
						return dependentSourceSolve.Height.Value;
					}
				case FuseProperty.Width:
					{
						return dependentSourceSolve.Width.Value;
					}
			}

			throw new ArgumentException($"Invalid SourceProperty: {SourceProperty}");
		}
	}

}
