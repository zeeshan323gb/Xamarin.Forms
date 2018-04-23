using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Xamarin.Forms
{
	public sealed class ShellTabItemCollection : IEnumerable<ShellTabItem>, IList<ShellTabItem>, INotifyCollectionChanged
	{
		event NotifyCollectionChangedEventHandler INotifyCollectionChanged.CollectionChanged
		{
			add { ((INotifyCollectionChanged)Inner).CollectionChanged += value; }
			remove { ((INotifyCollectionChanged)Inner).CollectionChanged -= value; }
		}

		public int Count => Inner.Count;
		public bool IsReadOnly => ((IList<ShellTabItem>)Inner).IsReadOnly;
		internal IList<ShellTabItem> Inner { get; set; }

		public ShellTabItem this[int index]
		{
			get => Inner[index];
			set => Inner[index] = value;
		}

		public void Add(ShellTabItem item) => Inner.Add(item);

		public void Clear() => Inner.Clear();

		public bool Contains(ShellTabItem item) => Inner.Contains(item);

		public void CopyTo(ShellTabItem[] array, int arrayIndex) => Inner.CopyTo(array, arrayIndex);

		public IEnumerator<ShellTabItem> GetEnumerator() => Inner.GetEnumerator();

		public int IndexOf(ShellTabItem item) => Inner.IndexOf(item);

		public void Insert(int index, ShellTabItem item) => Inner.Insert(index, item);

		public bool Remove(ShellTabItem item) => Inner.Remove(item);

		public void RemoveAt(int index) => Inner.RemoveAt(index);

		IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Inner).GetEnumerator();
	}
}