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

		public new IFusedLayoutList<View> Children
		{
			get { return _children; }
		}

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


		public static void AddFusion(View targetView, FuseProperty targetProperty, FusionBase source)
		{
			var currentFuses = GetFuses(targetView) ?? new FuseCollection();
			currentFuses.Add(new TargetWrapper(source, targetProperty, targetView));
			SetFuses(targetView, currentFuses);
		}

		public static void AddFusion(View targetView, FuseProperty targetProperty, double value)
		{
			AddFusion(targetView, targetProperty, new ScalarValueFusion(targetProperty, value));
		}


		public static void RemoveFusion(View targetView, FuseProperty targetProperty, FusionBase removeFuse)
		{
			var currentFuses = GetFuses(targetView);

			for(int i = currentFuses.Count - 1; i >= 0; i--)
			{
				if(currentFuses[i].TargetProperty == targetProperty && currentFuses[i].Fuse == removeFuse)
				{
					currentFuses.Remove(currentFuses[i]);
				}
			}

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

			// don't use foreach things
			for (int i = _children.Count - 1; i >= 0; i--)
			{
				var fusesToSolveView = GetFuses(_children[i]);
				//SolveNodeList viewNodes = new SolveNodeList();
				SolveView solveView = new SolveView(_children[i]);
				//unSolvedNodeList.AddRange(solveView.Nodes);
				SetSolveView(_children[i], solveView);
				unSolvedViews.Add(solveView);
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

					if(unSolvedViews[i].IsSolved)
					{
						solvedViews.Add(unSolvedViews[i]);
						unSolvedViews.RemoveAt(i);
					}
				}
			}

			if(unSolvedViews.Count > 0)
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

		public class SolveView
		{
			public View TargetElement { get; set; }
			public FuseCollection Fuses { get; private set; }
			//public SolveNodeList Nodes { get; private set; }

			public SolveView(View target)
			{
				TargetElement = target;
				Fuses = GetFuses(target);
			}

			public double X { get; set; } = double.NaN;
			public double Width { get; set; } = double.NaN;
			public double Right { get; set; } = double.NaN;
			public double Left { get; set; } = double.NaN;


			public double Y { get; set; } = double.NaN;
			public double Height { get; set; } = double.NaN;
			public double Top { get; set; } = double.NaN;
			public double Bottom { get; set; } = double.NaN;


			static public Point NullPoint = new Point(-1, -1);
			public Point Center { get; set; } = NullPoint;

			// need to fix to handle more then just the double type
			double SolveForMe(FuseProperty property, bool solveState, out bool solved)
			{
				for(int i = 0; i < Fuses.Count; i++)
				{
					double result = double.NaN;
					if (property == Fuses[i].TargetProperty)
					{
						var propertySolve = Fuses[i].Fuse.GetPropertySolve();
						if(propertySolve is double)
						{
							result = (double)propertySolve;
							if(!double.IsNaN(result))
							{
								solved = true;
								return result;
							}
						}
					}

					result = Fuses[i].Calculate(property);
					if(!double.IsNaN(result))
					{
						solved = true;
						return result;
					}
				}

				solved = false || solveState;
				return double.NaN;
			}

			public bool TrySolve()
			{
				bool somethingSolved = false;
				if (double.IsNaN(X)){ X = SolveForMe(FuseProperty.X, somethingSolved, out somethingSolved); }
				if (double.IsNaN(Y)) { Y = SolveForMe(FuseProperty.Y, somethingSolved, out somethingSolved); }
				if (double.IsNaN(Width)) { Width = SolveForMe(FuseProperty.Width, somethingSolved, out somethingSolved); }
				if (double.IsNaN(Height)) { Height = SolveForMe(FuseProperty.Height, somethingSolved, out somethingSolved); }
				if (double.IsNaN(Right)) { Right = SolveForMe(FuseProperty.Right, somethingSolved, out somethingSolved); }
				if (double.IsNaN(Left)) { Left = SolveForMe(FuseProperty.Left, somethingSolved, out somethingSolved); }
				if (double.IsNaN(Top)) { Top = SolveForMe(FuseProperty.Top, somethingSolved, out somethingSolved); }
				if (double.IsNaN(Bottom)) { Bottom = SolveForMe(FuseProperty.Bottom, somethingSolved, out somethingSolved); }
				// FIX CASE if (Center == nullPoint) { SolveForMe(FuseProperty.Center, out somethingSolved); }

				bool somethingNewSolved = false;

				do
				{
					somethingNewSolved = false;

					if(!double.IsNaN(Left) && double.IsNaN(X))
					{
						X = Left;
					}
					else if (double.IsNaN(Left) && !double.IsNaN(X))
					{
						Left = X;
					}

					if (!double.IsNaN(X) && !double.IsNaN(Width) &&
						!double.IsNaN(Y) && !double.IsNaN(Height) &&
						Center == NullPoint)
					{
						Center = new Point(X + (Width / 2), Y + (Height / 2));
						somethingNewSolved = true;
					}

					if (double.IsNaN(Right) && !double.IsNaN(X) && !double.IsNaN(Width))
					{
						Right = X + Width;
						somethingNewSolved = true;
					}

					if (!double.IsNaN(Right) && double.IsNaN(X) && !double.IsNaN(Width))
					{
						X = Right - Width;
						somethingNewSolved = true;
					}

					if (!double.IsNaN(Right) && !double.IsNaN(X) && double.IsNaN(Width))
					{
						Width = Right - X;
						somethingNewSolved = true;
					}

					somethingSolved = somethingNewSolved || somethingSolved;

				} while (somethingNewSolved);


				return somethingSolved;
			}


			/// <summary>
			/// Todo need to throw exception to user indicating solve was impossible given constraints
			/// </summary>
			public void ForceSolve()
			{
				double width = Width;
				double height = Height;

				if (!Double.IsNaN(width)) { width = double.PositiveInfinity; }
				if (!Double.IsNaN(height)) { height = double.PositiveInfinity; }

				if(Double.IsNaN(width) || Double.IsNaN(height))
				{
					var sizeRequest = TargetElement.Measure(width, height);

					Width = sizeRequest.Request.Width;
					Height = sizeRequest.Request.Height;
				}

				if (Double.IsNaN(X)) { X = 0; }
				if (Double.IsNaN(height)) { Y = 0; }


			}

			public void Apply()
			{
				if(!IsSolved)
				{
					ForceSolve();
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





	public class FuseCollection : List<TargetWrapper> 
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
		Size
	}
}