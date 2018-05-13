using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Xamarin.Forms
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class MaterialShellItem : ShellItem
	{
		public static readonly BindableProperty TabLocationProperty = 
			BindableProperty.Create(nameof(TabLocation), typeof(ShellTabLocation), typeof(MaterialShellItem), ShellTabLocation.Top, BindingMode.OneTime);

		public ShellTabLocation TabLocation
		{
			get { return (ShellTabLocation)GetValue(TabLocationProperty); }
			set { SetValue(TabLocationProperty, value); }
		}
	}
}