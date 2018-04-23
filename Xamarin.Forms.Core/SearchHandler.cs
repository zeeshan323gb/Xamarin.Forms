using System.Collections;
using System.Windows.Input;

namespace Xamarin.Forms
{
	public abstract class SearchHandler : BindableObject
	{
		public static readonly BindableProperty ClearIconProperty =
			BindableProperty.Create(nameof(ClearIcon), typeof(ImageSource), typeof(SearchHandler), null, BindingMode.OneTime);

		public static readonly BindableProperty ClearPlaceholderCommandProperty =
			BindableProperty.Create(nameof(ClearPlaceholderCommand), typeof(ICommand), typeof(SearchHandler), null, BindingMode.OneTime);

		public static readonly BindableProperty ClearPlaceholderEnabledProperty =
			BindableProperty.Create(nameof(ClearPlaceholderEnabled), typeof(bool), typeof(SearchHandler), true, BindingMode.OneWay);

		public static readonly BindableProperty ClearPlaceholderIconProperty =
			BindableProperty.Create(nameof(ClearPlaceholderIcon), typeof(ImageSource), typeof(SearchHandler), null, BindingMode.OneTime);

		public static readonly BindableProperty DisplayMemberNameProperty =
			BindableProperty.Create(nameof(DisplayMemberName), typeof(string), typeof(SearchHandler), null, BindingMode.OneTime);

		public static readonly BindableProperty IsSearchEnabledProperty =
			BindableProperty.Create(nameof(IsSearchEnabled), typeof(bool), typeof(SearchHandler), true, BindingMode.OneWay);

		public static readonly BindableProperty IsSearchingProperty =
			BindableProperty.Create(nameof(IsSearching), typeof(bool), typeof(SearchHandler), false, BindingMode.OneWay);

		public static readonly BindableProperty ItemsSourceProperty =
			BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable), typeof(SearchHandler), null, BindingMode.OneTime);

		public static readonly BindableProperty ItemTemplateProperty =
			BindableProperty.Create(nameof(ItemTemplate), typeof(DataTemplate), typeof(SearchHandler), null, BindingMode.OneTime);

		public static readonly BindableProperty PlaceholderProperty =
			BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(SearchHandler), null, BindingMode.OneTime);

		public static readonly BindableProperty QueryIconProperty =
			BindableProperty.Create(nameof(QueryIcon), typeof(ImageSource), typeof(SearchHandler), null, BindingMode.OneTime);

		public static readonly BindableProperty SearchBoxVisibilityProperty =
			BindableProperty.Create(nameof(SearchBoxVisibility), typeof(SearchBoxVisiblity), typeof(SearchHandler), SearchBoxVisiblity.Collapsed, BindingMode.OneWay);

		public ImageSource ClearIcon
		{
			get { return (ImageSource)GetValue(ClearIconProperty); }
			set { SetValue(ClearIconProperty, value); }
		}

		public ICommand ClearPlaceholderCommand
		{
			get { return (ICommand)GetValue(ClearPlaceholderCommandProperty); }
			set { SetValue(ClearPlaceholderCommandProperty, value); }
		}

		public bool ClearPlaceholderEnabled
		{
			get { return (bool)GetValue(ClearPlaceholderEnabledProperty); }
			set { SetValue(ClearPlaceholderEnabledProperty, value); }
		}

		public ImageSource ClearPlaceholderIcon
		{
			get { return (ImageSource)GetValue(ClearPlaceholderIconProperty); }
			set { SetValue(ClearPlaceholderIconProperty, value); }
		}

		public string DisplayMemberName
		{
			get { return (string)GetValue(DisplayMemberNameProperty); }
			set { SetValue(DisplayMemberNameProperty, value); }
		}

		public bool IsSearchEnabled
		{
			get { return (bool)GetValue(IsSearchEnabledProperty); }
			set { SetValue(IsSearchEnabledProperty, value); }
		}

		public bool IsSearching
		{
			get { return (bool)GetValue(IsSearchingProperty); }
			set { SetValue(IsSearchingProperty, value); }
		}

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

		public string Placeholder
		{
			get { return (string)GetValue(PlaceholderProperty); }
			set { SetValue(PlaceholderProperty, value); }
		}

		public ImageSource QueryIcon
		{
			get { return (ImageSource)GetValue(QueryIconProperty); }
			set { SetValue(QueryIconProperty, value); }
		}

		public SearchBoxVisiblity SearchBoxVisibility
		{
			get { return (SearchBoxVisiblity)GetValue(SearchBoxVisibilityProperty); }
			set { SetValue(SearchBoxVisibilityProperty, value); }
		}

		protected virtual void OnClearPlaceholderPressed()
		{
		}

		protected virtual void OnSearchChanged(string oldValue, string newValue)
		{
		}

		protected virtual void OnSearchConfirmed()
		{
		}
	}
}