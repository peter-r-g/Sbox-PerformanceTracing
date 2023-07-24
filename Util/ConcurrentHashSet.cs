using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace PerformanceTracing.Util;

internal sealed class ConcurrentHashSet<T> : ICollection<T> where T : notnull
{
	public int Count => Inner.Count;

	private ConcurrentDictionary<T, byte> Inner { get; }

	public ConcurrentHashSet()
	{
		Inner = new ConcurrentDictionary<T, byte>();
	}

	public ConcurrentHashSet( IEqualityComparer<T> comparer )
	{
		Inner = new ConcurrentDictionary<T, byte>( comparer );
	}

	public bool TryAdd( T item ) => Inner.TryAdd( item, 0 );
	public bool TryRemove( T item ) => Inner.TryRemove( item, out _ );
	public bool Contains( T item ) => Inner.ContainsKey( item );
	public void Clear() => Inner.Clear();
	public IEnumerator<T> GetEnumerator() => Inner.Keys.GetEnumerator();

	bool ICollection<T>.IsReadOnly => false;
	void ICollection<T>.Add( T item ) => TryAdd( item );
	bool ICollection<T>.Remove( T item ) => TryRemove( item );
	void ICollection<T>.CopyTo( T[] array, int arrayIndex ) => Inner.Keys.CopyTo( array, arrayIndex );
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

