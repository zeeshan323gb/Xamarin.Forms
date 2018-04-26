using System;
using System.Collections;
using System.Windows.Input;

namespace Xamarin.Forms
{
	public class SearchHandler : BindableObject, ISearchHandlerController
	{
		#region ISearchHandlerController

		void ISearchHandlerController.ClearPlaceholderClicked()
		{
			OnClearPlaceholderClicked();
		}

		void ISearchHandlerController.QueryConfirmed()
		{
			OnQueryConfirmed();
		}

		#endregion ISearchHandlerController

		public static readonly BindableProperty ClearIconProperty =
			BindableProperty.Create(nameof(ClearIcon), typeof(ImageSource), typeof(SearchHandler), null, BindingMode.OneTime);

		public static readonly BindableProperty ClearPlaceholderCommandParameterProperty =
			BindableProperty.Create(nameof(ClearPlaceholderCommandParameter), typeof(object), typeof(SearchHandler), null, BindingMode.OneTime,
				propertyChanged: OnClearPlaceholderCommandParameterChanged);

		public static readonly BindableProperty ClearPlaceholderCommandProperty =
					BindableProperty.Create(nameof(ClearPlaceholderCommand), typeof(ICommand), typeof(SearchHandler), null, BindingMode.OneTime,
				propertyChanged: OnClearPlaceholderCommandChanged);

		public static readonly BindableProperty ClearPlaceholderEnabledProperty =
			BindableProperty.Create(nameof(ClearPlaceholderEnabled), typeof(bool), typeof(SearchHandler), false, BindingMode.OneWay);

		public static readonly BindableProperty ClearPlaceholderIconProperty =
			BindableProperty.Create(nameof(ClearPlaceholderIcon), typeof(ImageSource), typeof(SearchHandler), null, BindingMode.OneTime);

		public static readonly BindableProperty CommandParameterProperty =
			BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(SearchHandler), null, BindingMode.OneTime,
				propertyChanged: OnCommandParameterChanged);

		public static readonly BindableProperty CommandProperty =
			BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(SearchHandler), null, BindingMode.OneTime,
				propertyChanged: OnCommandChanged);

		public static readonly BindableProperty DisplayMemberNameProperty =
			BindableProperty.Create(nameof(DisplayMemberName), typeof(string), typeof(SearchHandler), null, BindingMode.OneTime);

		public static readonly BindableProperty IsSearchEnabledProperty =
			BindableProperty.Create(nameof(IsSearchEnabled), typeof(bool), typeof(SearchHandler), true, BindingMode.OneWay);

		public static readonly BindableProperty ItemsSourceProperty =
			BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable), typeof(SearchHandler), null, BindingMode.OneTime);

		public static readonly BindableProperty ItemTemplateProperty =
			BindableProperty.Create(nameof(ItemTemplate), typeof(DataTemplate), typeof(SearchHandler), null, BindingMode.OneTime);

		public static readonly BindableProperty PlaceholderProperty =
			BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(SearchHandler), null, BindingMode.OneTime);

		public static readonly BindableProperty QueryIconProperty =
			BindableProperty.Create(nameof(QueryIcon), typeof(ImageSource), typeof(SearchHandler), null, BindingMode.OneTime);

		public static readonly BindableProperty QueryProperty =
			BindableProperty.Create(nameof(Query), typeof(string), typeof(SearchHandler), null, BindingMode.TwoWay,
				propertyChanged: OnQueryChanged);

		public static readonly BindableProperty SearchBoxVisibilityProperty =
			BindableProperty.Create(nameof(SearchBoxVisibility), typeof(SearchBoxVisiblity), typeof(SearchHandler), SearchBoxVisiblity.Expanded, BindingMode.OneWay);

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

		public object ClearPlaceholderCommandParameter
		{
			get { return GetValue(ClearPlaceholderCommandParameterProperty); }
			set { SetValue(ClearPlaceholderCommandParameterProperty, value); }
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

		public ICommand Command
		{
			get { return (ICommand)GetValue(CommandProperty); }
			set { SetValue(CommandProperty, value); }
		}

		public object CommandParameter
		{
			get { return (object)GetValue(CommandParameterProperty); }
			set { SetValue(CommandParameterProperty, value); }
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

		public string Query
		{
			get { return (string)GetValue(QueryProperty); }
			set { SetValue(QueryProperty, value); }
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

		private bool ClearPlaceholderEnabledCore { set => SetValueCore(ClearPlaceholderEnabledProperty, value); }

		private bool IsSearchEnabledCore { set => SetValueCore(IsSearchEnabledProperty, value); }

		protected virtual void OnClearPlaceholderClicked()
		{
			var command = ClearPlaceholderCommand;
			var commandParameter = ClearPlaceholderCommandParameter;
			if (command != null && command.CanExecute(commandParameter))
			{
				command.Execute(commandParameter);
			}
		}

		protected virtual void OnQueryChanged(string oldValue, string newValue)
		{
		}

		protected virtual void OnQueryConfirmed()
		{
			var command = Command;
			var commandParameter = CommandParameter;
			if (command != null && command.CanExecute(commandParameter))
			{
				command.Execute(commandParameter);
			}
		}

		private static void OnClearPlaceholderCommandChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var self = (SearchHandler)bindable;
			var oldCommand = (ICommand)oldValue;
			var newCommand = (ICommand)newValue;
			self.OnClearPlaceholderCommandChanged(oldCommand, newCommand);
		}

		private static void OnClearPlaceholderCommandParameterChanged(BindableObject bindable, object oldValue, object newValue)
		{
			((SearchHandler)bindable).OnClearPlaceholderCommandParameterChanged();
		}

		private static void OnCommandChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var self = (SearchHandler)bindable;
			var oldCommand = (ICommand)oldValue;
			var newCommand = (ICommand)newValue;
			self.OnCommandChanged(oldCommand, newCommand);
		}

		private static void OnCommandParameterChanged(BindableObject bindable, object oldValue, object newValue)
		{
			((SearchHandler)bindable).OnCommandParameterChanged();
		}

		private static void OnQueryChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var searchHandler = (SearchHandler)bindable;
			searchHandler.OnQueryChanged((string)oldValue, (string)newValue);
		}

		private void CanExecuteChanged(object sender, EventArgs e)
		{
			IsSearchEnabledCore = Command.CanExecute(CommandParameter);
		}

		private void ClearPlaceholderCanExecuteChanged(object sender, EventArgs e)
		{
			ClearPlaceholderEnabledCore = ClearPlaceholderCommand.CanExecute(ClearPlaceholderCommandParameter);
		}

		private void OnClearPlaceholderCommandChanged(ICommand oldCommand, ICommand newCommand)
		{
			if (oldCommand != null)
			{
				oldCommand.CanExecuteChanged -= ClearPlaceholderCanExecuteChanged;
			}

			if (newCommand != null)
			{
				newCommand.CanExecuteChanged += ClearPlaceholderCanExecuteChanged;
				ClearPlaceholderEnabledCore = ClearPlaceholderCommand.CanExecute(ClearPlaceholderCommandParameter);
			}
			else
			{
				ClearPlaceholderEnabledCore = true;
			}
		}

		private void OnClearPlaceholderCommandParameterChanged()
		{
			if (ClearPlaceholderCommand != null)
				ClearPlaceholderEnabledCore = ClearPlaceholderCommand.CanExecute(CommandParameter);
		}

		private void OnCommandChanged(ICommand oldCommand, ICommand newCommand)
		{
			if (oldCommand != null)
			{
				oldCommand.CanExecuteChanged -= CanExecuteChanged;
			}

			if (newCommand != null)
			{
				newCommand.CanExecuteChanged += CanExecuteChanged;
				IsSearchEnabledCore = Command.CanExecute(CommandParameter);
			}
			else
			{
				IsSearchEnabledCore = true;
			}
		}

		private void OnCommandParameterChanged()
		{
			if (Command != null)
				IsSearchEnabledCore = Command.CanExecute(CommandParameter);
		}
	}
}