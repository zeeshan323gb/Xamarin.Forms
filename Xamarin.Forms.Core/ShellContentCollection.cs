using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Xamarin.Forms
{
	public sealed class ShellContentCollection :  IList<ShellContent>, INotifyCollectionChanged
	{
		event NotifyCollectionChangedEventHandler INotifyCollectionChanged.CollectionChanged
		{
			add { ((INotifyCollectionChanged)Inner).CollectionChanged += value; }
			remove { ((INotifyCollectionChanged)Inner).CollectionChanged -= value; }
		}

		public int Count => Inner.Count;
		public bool IsReadOnly => Inner.IsReadOnly;
		internal IList<ShellContent> Inner { get; set; }

		public ShellContent this[int index]
		{
			get => Inner[index];
			set => Inner[index] = value;
		}

		public void Add(ShellContent item) => Inner.Add(item);

		public void Clear() => Inner.Clear();

		public bool Contains(ShellContent item) => Inner.Contains(item);

		public void CopyTo(ShellContent[] array, int arrayIndex) => Inner.CopyTo(array, arrayIndex);

		public IEnumerator<ShellContent> GetEnumerator() => Inner.GetEnumerator();

		public int IndexOf(ShellContent item) => Inner.IndexOf(item);

		public void Insert(int index, ShellContent item) => Inner.Insert(index, item);

		public bool Remove(ShellContent item) => Inner.Remove(item);

		public void RemoveAt(int index) => Inner.RemoveAt(index);

		IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Inner).GetEnumerator();
	}
}