using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Xamarin.Forms;
using static Xamarin.Forms.Controls.Issues.Issue3408;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 3408, "System.ObjectDisposedException: from SwitchCellRenderer when changing ItemSource", PlatformAffected.iOS)]
	public class Issue3408 : TestContentPage
	{
		public static List<Recommendation> GetRecommendations(object e)
		{
			switch (e)
			{
				case List<RecommendationsViewModel> pc: return pc.First().Recommendations;
				case List<RecommendationsViewModel2> pc: return pc.First().Recommendations;
				default: return null;
			}
		}
		protected override void Init()
		{

			var grd = new Grid();

			var aacountListView = new RepeaterSwipeView();
			aacountListView.ItemTemplate = new AccountDetailsDataTemplateSelector();
			aacountListView.BindingContext = new List<RecommendationsViewModel> { new RecommendationsViewModel() };

			aacountListView.SetBinding(RepeaterSwipeView.ItemsSourceProperty, ".");
			var btn = new Button
			{
				Text = "Change Source",
				AutomationId = "btn1",
				Command = new Command(() =>
				{
					aacountListView.BindingContext = new List<RecommendationsViewModel2> { new RecommendationsViewModel2() };
				})
			};
			var btn2 = new Button
			{
				Text = "Change Property",
				AutomationId = "btn2",
				Command = new Command(() =>
				{

					foreach (var item in GetRecommendations(aacountListView.BindingContext))
					{
						item.Name = "New Item Name";
						item.IsBusy = !item.IsBusy;
					}

				})
			};
			grd.Children.Add(aacountListView);
			Grid.SetRow(aacountListView, 0);
			grd.Children.Add(btn);
			Grid.SetRow(btn, 1);
			grd.Children.Add(btn2);
			Grid.SetRow(btn2, 2);
			Content = grd;
		}

#if UITEST
		[Test]
		public void Issue3408Test ()
		{
			RunningApp.WaitForElement (q => q.Marked ("btn1"));
			RunningApp.WaitForElement (q => q.Marked ("Click to Change"));
			RunningApp.Tap(q => q.Marked("btn1"));
			RunningApp.WaitForElement(q => q.Marked("This should have changed"));
			RunningApp.Tap(q => q.Marked("btn2"));
			RunningApp.WaitForElement(q => q.Marked("New Item Name"));
		}
#endif

		[Preserve(AllMembers = true)]
		public class RecommendationsBaseViewModel : ViewModelBase
		{
			public string AccountName => $"";
			public List<Recommendation> Recommendations { get; set; }
		}

		[Preserve(AllMembers = true)]
		public class RecommendationsViewModel : RecommendationsBaseViewModel
		{
			public string AccountName => $"Recommendations";

			public RecommendationsViewModel()
			{
				Recommendations = new List<Recommendation>()
			{
					new Recommendation(){ Name = "Click to Change"},
					new Recommendation(){ Name = "Recommendations"},
					new Recommendation(){ Name = "Recommendations"},
			};
			}
		}

		[Preserve(AllMembers = true)]
		public class RecommendationsViewModel2 : RecommendationsBaseViewModel
		{
			public string AccountName => $"Recommendations 2";
			public RecommendationsViewModel2()
			{
				Recommendations = new List<Recommendation>()
			{
					new Recommendation(){ Name = "This should have changed"},
					new Recommendation(){ Name = "Recommendations 2"},
					new Recommendation(){ Name = "Recommendations 2", IsBusy = true },
			};
			}
		}

		[Preserve(AllMembers = true)]
		public class Recommendation : ViewModelBase
		{
			string _name;
			public string Name
			{
				get { return _name; }
				set
				{
					if (_name == value)
						return;
					_name = value;
					OnPropertyChanged();
				}
			}
		}

		[Preserve(AllMembers = true)]
		public class RepeaterView : StackLayout
		{
			public RepeaterView()
			{
				Dictionary<Element, IDisposable> activatedViews
					= new Dictionary<Element, IDisposable>();
			}

			public static readonly BindableProperty ItemsSourceProperty =
				BindableProperty.Create(
					"ItemsSource",
					typeof(IEnumerable),
					typeof(RepeaterView),
					defaultValue: null,
					defaultBindingMode: BindingMode.OneWay,
					propertyChanged: ItemsChanged);


			public static readonly BindableProperty ItemTemplateProperty =
				BindableProperty.Create(
					"ViewModel",
					typeof(DataTemplate),
					typeof(RepeaterView),
					defaultValue: null,
					defaultBindingMode: BindingMode.OneWay);


			bool waitingForBindingContext = false;
			public event RepeaterViewItemAddedEventHandler ItemCreated;

			public IEnumerable ItemsSource
			{
				get { return (IEnumerable)GetValue(ItemsSourceProperty); }
				set { SetValue(ItemsSourceProperty, value); }
			}

			public DataTemplate ItemTemplate
			{
				get { return (DataTemplate)GetValue(ItemTemplateProperty); }
				set { SetValue(ItemTemplateProperty, value); }
			}


			protected override void OnBindingContextChanged()
			{
				base.OnBindingContextChanged();

				if (BindingContext != null && waitingForBindingContext && ItemsSource != null)
				{
					ItemsChanged(this, null, ItemsSource);
				}
			}

			static void ItemsChanged(BindableObject bindable, object old, object newVal)
			{
				IEnumerable oldValue = old as IEnumerable;
				IEnumerable newValue = newVal as IEnumerable;

				var control = (RepeaterView)bindable;

				var oldObservableCollection = oldValue as INotifyCollectionChanged;

				if (oldObservableCollection != null)
				{
					oldObservableCollection.CollectionChanged -= control.OnItemsSourceCollectionChanged;
				}

				//HACK:SHANE
				if (control.BindingContext == null)
				{
					control.waitingForBindingContext = true;
					//this means this control has been removed from the visual tree
					//so don't update it other wise you get random null reference exceptions
					return;
				}

				control.waitingForBindingContext = false;

				var newObservableCollection = newValue as INotifyCollectionChanged;

				if (newObservableCollection != null)
				{
					newObservableCollection.CollectionChanged += control.OnItemsSourceCollectionChanged;
				}

				try
				{
					control.Children.Clear();

					if (newValue != null)
					{
						foreach (var item in newValue)
						{
							var view = control.CreateChildViewFor(item);
							control.Children.Add(view);
							control.OnItemCreated(view);
						}
					}

					control.UpdateChildrenLayout();
					control.InvalidateLayout();
				}
				catch (NullReferenceException)
				{
					try
					{
						Debug.WriteLine(
							String.Format($"RepeaterView: NullReferenceException Parent:{control.Parent} ParentView:{control.Parent} IsVisible:{control.IsVisible}")
						);
					}
					catch (Exception exc)
					{
						Debug.WriteLine($"NullReferenceException Logging Failed {exc}");
					}
				}
			}

			protected virtual void OnItemCreated(View view)
			{

				if (this.ItemCreated != null)
				{
					ItemCreated.Invoke(this, new RepeaterViewItemAddedEventArgs(view, view.BindingContext));
				}


			}

			void OnItemsSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
			{
				try
				{
					var invalidate = false;

					List<View> createdViews = new List<View>();
					if (e.Action == NotifyCollectionChangedAction.Reset)
					{
						var list = sender as IEnumerable;


						this.Children.SyncList(
							list,
							(item) =>
							{
								var view = this.CreateChildViewFor(item);
								createdViews.Add(view);
								return view;
							}, (item, view) => view.BindingContext == item,
							null);

						foreach (View view in createdViews)
						{
							OnItemCreated(view);
						}

						invalidate = true;
					}

					if (e.OldItems != null)
					{
						this.Children.RemoveAt(e.OldStartingIndex);
						invalidate = true;
					}

					if (e.NewItems != null)
					{
						for (var i = 0; i < e.NewItems.Count; ++i)
						{
							var item = e.NewItems[i];
							var view = this.CreateChildViewFor(item);

							this.Children.Insert(i + e.NewStartingIndex, view);
							OnItemCreated(view);
						}

						invalidate = true;
					}

					if (invalidate)
					{
						this.UpdateChildrenLayout();
						this.InvalidateLayout();
					}
				}
				catch (NullReferenceException)
				{
					try
					{
						Debug.WriteLine(
							$"RepeaterView.OnItemsSourceCollectionChanged: NullReferenceException Parent:{Parent} ParentView:{Parent} IsVisible:{IsVisible} BindingContext: {BindingContext}"
						);
					}
					catch (Exception exc)
					{
						Debug.WriteLine($"OnItemsSourceCollectionChanged: NullReferenceException Logging Failed {exc}");
					}
				}
			}

			View CreateChildViewFor(object item)
			{
				this.ItemTemplate.SetValue(BindableObject.BindingContextProperty, item);

				if (this.ItemTemplate is DataTemplateSelector)
				{
					var dts = (DataTemplateSelector)this.ItemTemplate;
					return (View)dts.SelectTemplate(item, null).CreateContent();
				}
				else
				{
					return (View)this.ItemTemplate.CreateContent();
				}
			}
		}

		[Preserve(AllMembers = true)]
		public delegate void RepeaterViewItemAddedEventHandler(object sender, RepeaterViewItemAddedEventArgs args);
		[Preserve(AllMembers = true)]
		public class RepeaterSwipeView : StackLayout
		{
			public event EventHandler SelectedIndexChanged;
			public int SelectedIndex
			{
				get { return _selectedIndex; }
				private set { _selectedIndex = value; SelectedIndexChanged?.Invoke(this, EventArgs.Empty); }
			}

			public RepeaterSwipeView()
			{
				Dictionary<Element, IDisposable> activatedViews
					= new Dictionary<Element, IDisposable>();

				Orientation = StackOrientation.Horizontal;
				Padding = new Thickness(0);
				Margin = new Thickness(0);
				if (Device.RuntimePlatform == Device.iOS)
					AddSwipeGestures();
			}

			public void RemoveSwipeGestures()
			{
				if (Device.RuntimePlatform == Device.iOS)
					return;

				this.GestureRecognizers.Clear();
			}


			public async void MoveForward()
			{
				var view = Children[SelectedIndex];
				if (this.Children.LastOrDefault() == view)
					return;

				RemoveSwipeGestures();
				SelectedIndex++;
				await this.TranslateTo(-this.Width, this.Y);
				await this.TranslateTo(0, this.Y, 0);
				this.UpdateChildrenLayout();
				this.InvalidateLayout();
			}

			public async void MoveBackwards()
			{
				var view = Children[SelectedIndex];
				if (this.Children.FirstOrDefault() == view)
					return;

				RemoveSwipeGestures();
				SelectedIndex--;
				await this.TranslateTo(Width, Y);
				await this.TranslateTo(0, Y, 0);
				this.UpdateChildrenLayout();
				this.InvalidateLayout();
			}

			public void AddSwipeGestures()
			{
				if (Device.RuntimePlatform == Device.iOS && this.GestureRecognizers.Any())
					return;

				RemoveSwipeGestures();
				var control = this;

				this.GestureRecognizers.Add(new SwipeGestureRecognizer()
				{
					Command = new Command(MoveBackwards),
					Direction = SwipeDirection.Right
				});

				this.GestureRecognizers.Add(new SwipeGestureRecognizer()
				{
					Command = new Command(MoveForward),
					Direction = SwipeDirection.Left
				});

			}

			public static readonly BindableProperty ItemsSourceProperty =
				BindableProperty.Create(
					"ItemsSource",
					typeof(IEnumerable),
					typeof(RepeaterSwipeView),
					defaultValue: null,
					defaultBindingMode: BindingMode.OneWay,
					propertyChanged: ItemsChanged);


			public static readonly BindableProperty ItemTemplateProperty =
				BindableProperty.Create(
					"ViewModel",
					typeof(DataTemplate),
					typeof(RepeaterSwipeView),
					defaultValue: null,
					defaultBindingMode: BindingMode.OneWay);


			bool waitingForBindingContext = false;
			private int _selectedIndex = 0;

			public event RepeaterViewItemAddedEventHandler ItemCreated;

			public IEnumerable ItemsSource
			{
				get { return (IEnumerable)GetValue(ItemsSourceProperty); }
				set { SetValue(ItemsSourceProperty, value); }
			}

			public DataTemplate ItemTemplate
			{
				get { return (DataTemplate)GetValue(ItemTemplateProperty); }
				set { SetValue(ItemTemplateProperty, value); }
			}


			protected override void OnBindingContextChanged()
			{
				base.OnBindingContextChanged();

				if (BindingContext != null && waitingForBindingContext && ItemsSource != null)
				{
					ItemsChanged(this, null, ItemsSource);
				}
			}

			private static void ItemsChanged(BindableObject bindable, object old, object newVal)
			{
				IEnumerable oldValue = old as IEnumerable;
				IEnumerable newValue = newVal as IEnumerable;

				var control = (RepeaterSwipeView)bindable;

				var oldObservableCollection = oldValue as INotifyCollectionChanged;

				if (oldObservableCollection != null)
				{
					oldObservableCollection.CollectionChanged -= control.OnItemsSourceCollectionChanged;
				}

				//HACK:SHANE
				if (control.BindingContext == null)
				{
					control.waitingForBindingContext = true;
					//this means this control has been removed from the visual tree
					//so don't update it other wise you get random null reference exceptions
					return;
				}

				control.waitingForBindingContext = false;

				var newObservableCollection = newValue as INotifyCollectionChanged;

				if (newObservableCollection != null)
				{
					newObservableCollection.CollectionChanged += control.OnItemsSourceCollectionChanged;
				}

				try
				{
					control.Children.Clear();

					if (newValue != null)
					{
						foreach (var item in newValue)
						{
							var view = control.CreateChildViewFor(item);
							view.WidthRequest = Application.Current.MainPage?.Width ?? 360.0;
							control.Children.Add(view);
							control.OnItemCreated(view);

						}
					}

					control.UpdateChildrenLayout();
					control.InvalidateLayout();
				}
				catch (NullReferenceException)
				{
					try
					{
						Debug.WriteLine(
							String.Format($"RepeaterView: NullReferenceException Parent:{control.Parent} ParentView:{control.Parent} IsVisible:{control.IsVisible}")
						);
					}
					catch (Exception exc)
					{
						Debug.WriteLine($"NullReferenceException Logging Failed {exc}");
					}
				}
			}

			public double GetTotalChildrenWidth()
			{
				double total = 0;
				foreach (var child in Children)
				{
					total += Application.Current.MainPage?.Width ?? 360.0;
				}

				return total;
			}

			public double GetDesiredOffset()
			{
				double total = 0;
				foreach (var child in Children)
				{
					if (Children.IndexOf(child) >= SelectedIndex)
						break;

					total += Application.Current.MainPage?.Width ?? 360.0;
				}

				return total;
			}


			protected override void LayoutChildren(double x, double y, double width, double height)
			{
				//TranslationX = 0;
				base.LayoutChildren(-GetDesiredOffset(), y, GetTotalChildrenWidth(), height);
				AddSwipeGestures();
			}

			protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
			{
				return base.OnMeasure(widthConstraint, heightConstraint);
			}


			protected virtual void OnItemCreated(View view)
			{
				if (this.ItemCreated != null)
				{
					ItemCreated.Invoke(this, new RepeaterViewItemAddedEventArgs(view, view.BindingContext));
				}
			}

			private void OnItemsSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
			{
				try
				{
					var invalidate = false;

					List<View> createdViews = new List<View>();
					if (e.Action == NotifyCollectionChangedAction.Reset)
					{
						var list = sender as IEnumerable;


						this.Children.SyncList(
							list,
							(item) =>
							{
								var view = this.CreateChildViewFor(item);
								createdViews.Add(view);
								return view;
							}, (item, view) => view.BindingContext == item,
							null);

						foreach (View view in createdViews)
						{
							OnItemCreated(view);
						}

						invalidate = true;
					}

					if (e.OldItems != null)
					{
						this.Children.RemoveAt(e.OldStartingIndex);
						invalidate = true;
					}

					if (e.NewItems != null)
					{
						for (var i = 0; i < e.NewItems.Count; ++i)
						{
							var item = e.NewItems[i];
							var view = this.CreateChildViewFor(item);

							this.Children.Insert(i + e.NewStartingIndex, view);
							OnItemCreated(view);
						}

						invalidate = true;
					}

					if (invalidate)
					{
						this.UpdateChildrenLayout();
						this.InvalidateLayout();
					}
				}
				catch (NullReferenceException)
				{
					try
					{
						Debug.WriteLine(
							$"RepeaterView.OnItemsSourceCollectionChanged: NullReferenceException Parent:{Parent} ParentView:{Parent} IsVisible:{IsVisible} BindingContext: {BindingContext}"
						);
					}
					catch (Exception exc)
					{
						Debug.WriteLine($"OnItemsSourceCollectionChanged: NullReferenceException Logging Failed {exc}");
					}
				}
			}

			private View CreateChildViewFor(object item)
			{
				this.ItemTemplate.SetValue(BindableObject.BindingContextProperty, item);

				if (this.ItemTemplate is DataTemplateSelector)
				{
					var dts = (DataTemplateSelector)this.ItemTemplate;
					return (View)dts.SelectTemplate(item, null).CreateContent();
				}
				else
				{
					return (View)this.ItemTemplate.CreateContent();
				}
			}
		}
	}
	[Preserve(AllMembers = true)]
	public class RepeaterViewItemAddedEventArgs : EventArgs
	{
		private readonly View view;
		private readonly object model;

		public RepeaterViewItemAddedEventArgs(View view, object model)
		{
			this.view = view;
			this.model = model;
		}

		public View View { get { return view; } }

		public object Model { get { return model; } }
	}
	[Preserve(AllMembers = true)]
	public class RecommendationsView : ContentView
	{
		public RecommendationsView()
		{
			Grid grd = new Grid();
			var lst = new ListView
			{
				ItemTemplate = new DataTemplate(() =>
				{
					var swittch = new SwitchCell();
					swittch.SetBinding(SwitchCell.TextProperty, new Binding("Name"));
					swittch.SetBinding(SwitchCell.OnProperty, new Binding("IsBusy"));
					//swittch.Text = new Binding("Text");
					return swittch;
				})

			};
			lst.SetBinding(ListView.ItemsSourceProperty, new Binding("Recommendations"));
			grd.Children.Add(lst);
			Content = grd;
		}
	}
	[Preserve(AllMembers = true)]
	public class AccountDetailsDataTemplateSelector : DataTemplateSelector
	{
		public Lazy<DataTemplate> ConsumptionDetailsViewDataTemplate { get; }
		public Lazy<DataTemplate> AfkCurrentWorkLoadsViewDataTemplate { get; }
		public Lazy<DataTemplate> RecommendationsViewDataTemplate { get; }
		public Lazy<DataTemplate> AccountDetailsViewDataTemplate { get; }


		public Lazy<View> RecommendationsView { get; } = new Lazy<View>(() => new RecommendationsView());


		public AccountDetailsDataTemplateSelector()
		{
			RecommendationsViewDataTemplate = new Lazy<DataTemplate>(() => new DataTemplate(() => RecommendationsView.Value));
		}

		protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
		{
			if (item is RecommendationsViewModel)
			{
				RecommendationsView.Value.BindingContext = item;
				return RecommendationsViewDataTemplate.Value;
			}

			if (item is RecommendationsViewModel2)
			{
				RecommendationsView.Value.BindingContext = item;
				return RecommendationsViewDataTemplate.Value;
			}

			throw new ArgumentException("Invalid ViewModel Type");
		}
	}
	[Preserve(AllMembers = true)]
	public static class IListMixIns
	{

		public static void SyncList<T>(
			this IList<T> This,
			IEnumerable<T> sourceList)
		{
			This.SyncList<T, T>(sourceList, x => x, (x, y) => x.Equals(y), null);
		}


		public static void SyncList<T>(
			this IList<T> This,
			IEnumerable sourceList,
			Func<object, T> selector,
			Func<object, T, bool> areEqual,
			Action<object, T> updateExisting,
			bool dontRemove = false)
		{
			var sourceListEnum = sourceList.OfType<Object>().ToList();

			//passengers
			foreach (T dest in This.ToList())
			{
				var match = sourceListEnum.FirstOrDefault(p => areEqual(p, dest));
				if (match != null)
				{
					if (updateExisting != null)
						updateExisting(match, dest);
				}
				else if (!dontRemove)
				{
					This.Remove(dest);
				}
			}

			sourceListEnum.Where(x => !This.Any(p => areEqual(x, p)))
				.ToList().ForEach(p =>
				{
					if (This.Count >= sourceListEnum.IndexOf(p))
						This.Insert(sourceListEnum.IndexOf(p), selector(p));
					else
					{
						var result = selector(p);
						if (!EqualityComparer<T>.Default.Equals(result, default(T)))
							This.Add(result);
					}
				});
		}

		public static bool IsEmpty<T>(this IEnumerable<T> list)
		{
			return !list.Any();
		}

		public static bool IsEmpty<T>(this Array list)
		{
			return list.Length == 0;
		}

		public static bool IsEmpty<T>(this List<T> list)
		{
			return list.Count == 0;
		}

		public static void ForEach<T>(this IEnumerable<T> list, Action<T> doMe)
		{
			foreach (var item in list)
			{
				doMe(item);
			}
		}

		public static void SyncList<T, Source>(
			this IList<T> This,
			IEnumerable<Source> sourceList,
			Func<Source, T> selector,
			Func<Source, T, bool> areEqual,
			Action<Source, T> updateExisting,
			bool dontRemove = false)
		{
			//passengers
			foreach (T dest in This.ToList())
			{
				var match = sourceList.FirstOrDefault(p => areEqual(p, dest));
				if (!EqualityComparer<Source>.Default.Equals(match, default(Source)))
				{
					updateExisting?.Invoke(match, dest);
				}
				else if (!dontRemove)
				{
					This.Remove(dest);
				}
			}

			sourceList.Where(x => !This.Any(p => areEqual(x, p)))
				.ToList().ForEach(p =>
				{
					var result = selector(p);
					if (!EqualityComparer<T>.Default.Equals(result, default(T)))
						This.Add(result);
				});
		}
	}
}