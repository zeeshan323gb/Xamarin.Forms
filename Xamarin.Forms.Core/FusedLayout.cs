using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Xamarin.Forms.Core
{
	public class FusedLayout : Layout<View>, IElementConfiguration<FusedLayout>
	{
		public static readonly BindableProperty FusesProperty =
			BindableProperty.Create("FusesProperty", typeof(FuseCollection), typeof(FusedLayout), null);



		// Node List? or better to break out to SolveNodeX, SolveNodeYProperty etc...
		public static readonly BindableProperty SolveViewProperty =
			BindableProperty.Create("SolveViewProperty", typeof(SolveView), typeof(FusedLayout), null);


		readonly Lazy<PlatformConfigurationRegistry<FusedLayout>> _platformConfigurationRegistry;
		readonly FusedElementCollection _children;


		public FusedLayout()
		{
			_children = new FusedElementCollection(InternalChildren, this);
			_children.Parent = this;

			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<FusedLayout>>(() =>
				new PlatformConfigurationRegistry<FusedLayout>(this));
		}

		public new IFusedLayoutList<View> Children => _children;
		

		public static FuseCollection GetFuses(BindableObject bindable)
		{
			return (FuseCollection)bindable.GetValue(FusesProperty);
		}

		public static void SetFuses(BindableObject bindable, FuseCollection value)
		{
			bindable.SetValue(FusesProperty, value);
		}

		internal static SolveView GetSolveView(BindableObject bindable)
		{
			return (SolveView)bindable.GetValue(SolveViewProperty);
		}

		internal static void SetSolveView(BindableObject bindable, SolveView value)
		{
			bindable.SetValue(SolveViewProperty, value);
		}



		public static IFusionSolve AddFusion(View targetView, FuseProperty targetProperty, FusionBase source)
		{
			return AddFusion(targetView, new TargetWrapper(source, targetProperty, targetView));
		}

		public static IFusionSolve AddFusion(View targetView, FuseProperty targetProperty, double value)
		{ 
			return AddFusion(targetView, targetProperty, new ScalarOperationFusion(targetView, targetProperty, value)); 
		}

		public static IFusionSolve AddFusion(View targetView, IFusionSolve fusionSolve)
		{
			var currentFuses = GetFuses(targetView) ?? new FuseCollection();
			currentFuses.Add(fusionSolve);
			SetFuses(targetView, currentFuses);
			return fusionSolve;
		}

		public static void RemoveFusion(View targetView, IFusionSolve solve)
		{
			var currentFuses = GetFuses(targetView);
			currentFuses.Remove(solve);
			SetFuses(targetView, currentFuses);
		}


		public IPlatformElementConfiguration<T, FusedLayout> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}

		

		protected override void LayoutChildren(double x, double y, double width, double height)
		{

			// this setup could just be cached after the first run through
			// then subsequent run throughs would just reset the nodes
			// plus on a second run through the order would be more known
			//SolveNodeList unSolvedNodeList = new SolveNodeList();
			List<SolveView> unSolvedViews = new List<SolveView>();
			List<SolveView> solvedViews = new List<SolveView>();
			 
			for (int i = _children.Count - 1; i >= 0; i--)
			{
				var fusesToSolveView = GetFuses(_children[i]);
				//SolveNodeList viewNodes = new SolveNodeList();
				SolveView solveView = new SolveView(_children[i]);
				//unSolvedNodeList.AddRange(solveView.Nodes);
				SetSolveView(_children[i], solveView);
				unSolvedViews.Add(solveView);
				solveView.ResetSolves();
			}

			// in theory after the first run through this contains the order
			SetFuses(this, new FuseCollection());
			FusedLayout.AddFusion(this, FuseProperty.Height, height);
			FusedLayout.AddFusion(this, FuseProperty.Width, width);
			FusedLayout.AddFusion(this, FuseProperty.X, x);
			FusedLayout.AddFusion(this, FuseProperty.Y, y);

			var layoutSolveView = new SolveView(this);
			unSolvedViews.Add(layoutSolveView);
			SetSolveView(this, layoutSolveView);

			ValidateAndFillInFusions();
			bool iSolvedSomething = true;
			while (unSolvedViews.Count > 0 && iSolvedSomething)
			{
				iSolvedSomething = false;

				// solve is a single view being solved over
				for (int i = unSolvedViews.Count - 1; i >= 0; i--)
				{
					if (unSolvedViews[i].TrySolve())
					{
						iSolvedSomething = true;
					}

					if (unSolvedViews[i].IsSolved)
					{
						solvedViews.Add(unSolvedViews[i]);
						unSolvedViews.RemoveAt(i);
					}
				}
			}

			if (unSolvedViews.Count > 0)
			{
				throw new Exception("unsolved");
			}

			for (int i = 0; i < solvedViews.Count; i++)
			{
				if (solvedViews[i] == layoutSolveView) { continue; }
				solvedViews[i].Apply();
			}
		}
		public interface IFusedLayoutList<T> : IList<T> where T : View
		{
			void Add(T view, FuseProperty targetProperty, FusionBase fuse);
		}

		class FusedElementCollection : ElementCollection<View>, IFusedLayoutList<View>
		{
			public FusedElementCollection(ObservableCollection<Element> inner, FusedLayout parent) : base(inner)
			{
				Parent = parent;
			}
			internal FusedLayout Parent { get; set; }

			public void Add(View view, FuseProperty targetProperty, FusionBase fuse)
			{
				AddFusion(view, targetProperty, fuse);
				base.Add(view);
			}
		}


		static internal List<FuseProperty> xFuses =
			new List<FuseProperty> { FuseProperty.Width, FuseProperty.X, FuseProperty.Center, FuseProperty.CenterX, FuseProperty.Left, FuseProperty.Right, FuseProperty.Size };


		static internal List<FuseProperty> yFuses =
			new List <FuseProperty> { FuseProperty.Height, FuseProperty.Y, FuseProperty.Center, FuseProperty.CenterY, FuseProperty.Top, FuseProperty.Bottom, FuseProperty.Size };



		void logWarning(string warning)
		{
			Internals.Log.Warning(nameof(FusedLayout), warning);
		}
		
		/// <summary>
		/// If you have over constrained this throws an exception.
		/// If you have under constrainted this fills in the details by calling measure
		/// </summary>
		void ValidateAndFillInFusions()
		{
			for (int i = 0; i < _children.Count; i++)
			{
				View childView = _children[i];
				FuseCollection fuses = GetFuses(childView);
				SolveView solveView = GetSolveView(childView);

				List<FuseProperty> yAxisFuses = new List<FuseProperty>();
				List<FuseProperty> xAxisFuses = new List<FuseProperty>();


				if (fuses != null)
				{
					for (int j = 0; j < fuses.Count; j++)
					{ 
						xAxisFuses.AddRange(fuses[j].GetXFuseProperties());
						yAxisFuses.AddRange(fuses[j].GetYFuseProperties());
					}
				}


				if(xAxisFuses.Count > 2 || yAxisFuses.Count > 2)
				{					
					throw new ArgumentException($"{childView} is over constrained");
				}


				if (xAxisFuses.Count == 2)
				{
					if ((xAxisFuses[0] == FuseProperty.Left && xAxisFuses[1] == FuseProperty.X) ||
					    (xAxisFuses[0] == FuseProperty.X && xAxisFuses[1] == FuseProperty.Left) ||
						(xAxisFuses[0] == xAxisFuses[1]))
					{
						throw new ArgumentException($"{childView} is over constrained");
					}
				}
				if (yAxisFuses.Count == 2)
				{

					if ((yAxisFuses[0] == FuseProperty.Top && yAxisFuses[1] == FuseProperty.Y) ||
					    (yAxisFuses[0] == FuseProperty.Y && yAxisFuses[1] == FuseProperty.Top) ||
						(yAxisFuses[0] == yAxisFuses[1]))
					{
						throw new ArgumentException($"{childView} is over constrained");
					}
				}
				

				Size sizeRequest = SolveView.NullSize;
				Func<Size, Size> doMeasure = (sr) =>
				{
					if (!childView.IsVisible)
					{
						return new Size(0, 0);
					}
					else if (sr == SolveView.NullSize)
					{
						logWarning("Measure");
						return childView.Measure(double.PositiveInfinity, double.PositiveInfinity).Request;
					}

					return sr;
				};


				
				if (xAxisFuses.Count == 0)
				{
					sizeRequest = doMeasure(sizeRequest);
					solveView.Width = sizeRequest.Width;
					solveView.X = 0;
				}
				else if (xAxisFuses.Count == 1)
				{
					switch (xAxisFuses[0])
					{
						case FuseProperty.Width:
						case FuseProperty.Size:
							solveView.X = 0;
							break;
						case FuseProperty.X:
						case FuseProperty.Left:
							sizeRequest = doMeasure(sizeRequest);
							solveView.Width = sizeRequest.Width;
							break;
						case FuseProperty.Center:
						case FuseProperty.CenterX:
						case FuseProperty.Right:
							if (childView.WidthRequest > -1)
							{
								sizeRequest = doMeasure(sizeRequest);
								solveView.Width = sizeRequest.Width;
							}
							else
							{
								solveView.X = 0;	
							}
							
							break;
						default:
							throw new ArgumentException($"{xAxisFuses[0]}");
					}
				}

				if (yAxisFuses.Count == 0)
				{
					sizeRequest = doMeasure(sizeRequest);
					solveView.Height = sizeRequest.Height;
					solveView.Y = 0;
				}
				else if (yAxisFuses.Count == 1)
				{
					switch (yAxisFuses[0])
					{
						case FuseProperty.Height:
						case FuseProperty.Size:
							solveView.Y = 0;
							break;
						case FuseProperty.Y:
						case FuseProperty.Top:
							sizeRequest = doMeasure(sizeRequest);
							solveView.Height = sizeRequest.Height;
							break;
						case FuseProperty.Center:
						case FuseProperty.CenterY:
						case FuseProperty.Bottom:
							if (childView.WidthRequest > -1)
							{
								sizeRequest = doMeasure(sizeRequest);
								solveView.Height = sizeRequest.Height;
							}
							else
							{
								solveView.Y = 0;
							}
							break;
						default:
							throw new ArgumentException($"{yAxisFuses[0]}");
					}
				}
			}
		}



		// possibly hide these behind a specific contained solver you can ask questions of	

		public SolveView GetSolveFor(View element)
		{
			return FusedLayout.GetSolveView(element);
		}

		public class SolveView
		{
			public static Point NullPoint = new Point(-1, -1);
			public static Size NullSize = new Size(-1, -1);

			View TargetElement { get; set; }

			FuseCollection Fuses { get; set; } 

			public SolveView(View target)
			{
				TargetElement = target;
				Fuses = GetFuses(target);
				ResetSolves();
			}

			public double X { get; set; } 

			public double Width { get; set; } 
			public double Right { get; set; } 
			public double Left { get; set; } 


			public double Y { get; set; } 
			public double Height { get; set; } 
			public double Top { get; set; } 
			public double Bottom { get; set; } 


			public Point Center { get; set; } 

			public double CenterX { get; set; } 
			public double CenterY { get; set; } 

			public Size Size { get; set; } 

			internal void ResetSolves()
			{
				X  = double.NaN;
				Width  = double.NaN;
				Right  = double.NaN;
				Left  = double.NaN;

				Y  = double.NaN;
				Height  = double.NaN;
				Top  = double.NaN;
				Bottom  = double.NaN;

				Center  = NullPoint;
				CenterX  = double.NaN;
				CenterY  = double.NaN;
				Size  = NullSize;
			}



			internal bool IsPropertyNull(FuseProperty fuseProperty)
			{
				switch(fuseProperty)
				{
					case FuseProperty.X:
						return double.IsNaN(X);
					case FuseProperty.Bottom:
						return double.IsNaN(Bottom);
					case FuseProperty.CenterX:
						return double.IsNaN(CenterX);
					case FuseProperty.CenterY:
						return double.IsNaN(CenterY);
					case FuseProperty.Height:
						return double.IsNaN(Height);
					case FuseProperty.Left:
						return double.IsNaN(Left);
					case FuseProperty.Right:
						return double.IsNaN(Right);
					case FuseProperty.Top:
						return double.IsNaN(Top);
					case FuseProperty.Width:
						return double.IsNaN(Width);
					case FuseProperty.Y:
						return double.IsNaN(Y);
					case FuseProperty.Center:
						return Center == NullPoint;
					case FuseProperty.Size:
						return Size == NullSize;
					default:
						throw new ArgumentException($"{fuseProperty}");
				}
			}

			internal void SetPropertyValue(TargetWrapper fuseTarget)
			{
				FuseProperty fuseProperty = fuseTarget.TargetProperty;

				switch (fuseProperty)
				{
					case FuseProperty.X:
						X = ((IFusion<double>)fuseTarget.Fuse).GetPropertySolve();
						break;
					case FuseProperty.Bottom:
						Bottom = ((IFusion<double>)fuseTarget.Fuse).GetPropertySolve();
						break;
					case FuseProperty.CenterX:
						CenterX = ((IFusion<double>)fuseTarget.Fuse).GetPropertySolve();
						break;
					case FuseProperty.CenterY:
						CenterY = ((IFusion<double>)fuseTarget.Fuse).GetPropertySolve();
						break;
					case FuseProperty.Height:
						Height = ((IFusion<double>)fuseTarget.Fuse).GetPropertySolve();
						break;
					case FuseProperty.Left:
						Left = ((IFusion<double>)fuseTarget.Fuse).GetPropertySolve();
						break;
					case FuseProperty.Right:
						Right = ((IFusion<double>)fuseTarget.Fuse).GetPropertySolve();
						break;
					case FuseProperty.Top:
						Top = ((IFusion<double>)fuseTarget.Fuse).GetPropertySolve();
						break;
					case FuseProperty.Width:
						Width = ((IFusion<double>)fuseTarget.Fuse).GetPropertySolve();
						break;
					case FuseProperty.Y:
						Y = ((IFusion<double>)fuseTarget.Fuse).GetPropertySolve();
						break;
					case FuseProperty.Center:
						Center = ((IFusion<Point>)fuseTarget.Fuse).GetPropertySolve();
						break;
					case FuseProperty.Size:
						Size = ((IFusion<Size>)fuseTarget.Fuse).GetPropertySolve();
						break;
					default:
						throw new ArgumentException($"{fuseProperty}");
				}
			}


			public bool TrySolve()
			{
				bool somethingSolved = false;
				
				if (Fuses != null)
				{
					for (int i = 0; i < Fuses.Count; i++)
					{
						somethingSolved = somethingSolved || Fuses[i].SolveTargetProperty();
					}
				} 

				bool implicitValueSet = false;
				do
				{
					implicitValueSet = ProcessImplicitValues();
					somethingSolved = somethingSolved || implicitValueSet;
				} while (implicitValueSet);

				return somethingSolved;
			}


			bool ProcessImplicitValues()
			{
				bool somethingNewSet = false;

				if (!double.IsNaN(Left) && double.IsNaN(X))
				{
					X = Left;
					somethingNewSet = true;
				}
				else if (double.IsNaN(Left) && !double.IsNaN(X))
				{
					Left = X;
					somethingNewSet = true;
				}


				if (!double.IsNaN(Top) && double.IsNaN(Y))
				{
					Y = Top;
					somethingNewSet = true;
				}
				else if (double.IsNaN(Top) && !double.IsNaN(Y))
				{
					Top = Y;
					somethingNewSet = true;
				}


				//Center setters
				if (!double.IsNaN(X) && double.IsNaN(Width) &&
					Center != NullPoint)
				{
					Width = (Center.X - X) * 2;
					somethingNewSet = true;
				}

				if (double.IsNaN(X) && !double.IsNaN(Width) &&
					Center != NullPoint)
				{
					X = (Center.X - (Width / 2));
					somethingNewSet = true;
				}

				if (!double.IsNaN(Y) && double.IsNaN(Height) &&
					Center != NullPoint)
				{
					Height = (Center.Y - Y) * 2;
					somethingNewSet = true;
				}

				if (double.IsNaN(Y) && !double.IsNaN(Height) &&
					Center != NullPoint)
				{
					Y = (Center.Y - (Height / 2));
					somethingNewSet = true;
				}


				if (Center == NullPoint && !double.IsNaN(CenterX) && !double.IsNaN(CenterY))
				{
					Center = new Point(CenterX, CenterY);
					somethingNewSet = true;
				}

				if (Center != NullPoint && double.IsNaN(CenterX))
				{
					CenterX = Center.X;
					somethingNewSet = true;
				}

				if (Center != NullPoint && double.IsNaN(CenterY))
				{
					CenterY = Center.Y;
					somethingNewSet = true;
				}

				if (Size == NullSize && !double.IsNaN(Width) && !double.IsNaN(Height))
				{
					Size = new Size(Width, Height);
					somethingNewSet = true;
				}

				if (Size != NullSize && double.IsNaN(Height))
				{
					Height = Size.Height;
				}

				if (Size != NullSize && double.IsNaN(Width))
				{
					Width = Size.Width;
					somethingNewSet = true;
				}


				if (!double.IsNaN(X) && !double.IsNaN(Width) &&
					!double.IsNaN(Y) && !double.IsNaN(Height) &&
					Center == NullPoint)
				{
					Center = new Point(X + (Width / 2), Y + (Height / 2));
					somethingNewSet = true;
				}

				if (double.IsNaN(Right) && !double.IsNaN(X) && !double.IsNaN(Width))
				{
					Right = X + Width;
					somethingNewSet = true;
				}

				if (!double.IsNaN(Right) && double.IsNaN(X) && !double.IsNaN(Width))
				{
					X = Right - Width;
					somethingNewSet = true;
				}

				if (!double.IsNaN(Right) && !double.IsNaN(X) && double.IsNaN(Width))
				{
					Width = Right - X;
					somethingNewSet = true;
				}

				if (double.IsNaN(Bottom) && !double.IsNaN(Y) && !double.IsNaN(Height))
				{
					Bottom = Y + Height;
					somethingNewSet = true;
				}

				if (!double.IsNaN(Bottom) && double.IsNaN(Y) && !double.IsNaN(Height))
				{
					Y = Bottom - Height;
					somethingNewSet = true;
				}

				if (!double.IsNaN(Bottom) && !double.IsNaN(Y) && double.IsNaN(Height))
				{
					Height = Bottom - Y;
					somethingNewSet = true;
				}

				return somethingNewSet;
			}



			public void Apply()
			{
				if(!TargetElement.IsVisible)
				{
					return;
				}

				TargetElement.Layout(
					new Rectangle(
						X,
						Y,
						Width,
						Height
						)
					);
			}


			public bool IsSolved =>
				!double.IsNaN(X) &&
				!double.IsNaN(Y) &&
				!double.IsNaN(Height) &&
				!double.IsNaN(Width);
		}
	}





	public class FuseCollection : List<IFusionSolve>
	{

	}


	public enum FuseOperator
	{
		None = 0,
		Add = 1,
		Subtract = 2
	}

	public enum FuseProperty
	{
		None = 0,
		X,
		Y,
		Width,
		Height,
		Left,
		Top,
		Right,
		Bottom,
		Center,
		CenterX,
		CenterY,
		Size
	}
}