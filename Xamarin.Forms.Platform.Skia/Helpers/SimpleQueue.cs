using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Collections.Generic;
namespace System.Collections.Generic
{

	public class SimpleQueue<T>
	{
		readonly object privateLockObject = new object();

		readonly List<T> queue = new List<T>();

		public int Count => queue.Count;

		public T this[int i] => queue[i];

		public Action<T> OnDequeue { get; set; }
		public void Enqueue(T obj)
		{
			queue.Add(obj);
		}
		public bool Remove(T obj)
		{
			bool removed = true;
			lock (privateLockObject)
			{
				if (queue.Contains(obj))
					removed = queue.Remove(obj);
			}

			OnDequeue?.Invoke(obj);
			return removed;
		}

		public T Dequeue()
		{
			lock (privateLockObject)
			{
				if (Count == 0)
					return default(T);
				var item = queue[0];
				queue.RemoveAt(0);
				return item;
			}

		}

		public void Clear()
		{
			List<T> items;
			lock (privateLockObject)
			{
				items = queue.ToList();
			}
			items.ForEach((i) => Remove(i));
		}

		public override string ToString()
		{
			return string.Join(Environment.NewLine, queue.Select(x => x).Reverse());
		}

		public string ToString(Func<T, string> format)
		{
			return string.Join(Environment.NewLine, queue.Select(x => format(x)).Reverse());
		}
		public int IndexOf(T obj) => queue.IndexOf(obj);

		public bool TryGetIndex(int index, out T value)
		{
			try
			{
				if (index < 0 || index > queue.Count - 1)
				{
					value = default(T);
					return false;
				}
				value = queue[index];
				return true;

			}
			catch
			{
				value = default(T);
				return false;
			}

		}
	}
}
