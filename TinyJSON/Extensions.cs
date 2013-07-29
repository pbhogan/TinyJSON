using System;
using System.Collections.Generic;


namespace TinyJSON
{
	public static class Extensions
	{
		public delegate TResult Func<in T, out TResult>( T arg );


		public static bool Any<TSource>( this IEnumerable<TSource> source )
		{
			if (source == null)
			{
				throw new ArgumentNullException( "source" );
			}

			using (var iterator = source.GetEnumerator())
			{
				return iterator.MoveNext();
			}
		}


		public static bool Any<TSource>( this IEnumerable<TSource> source, Func<TSource, bool> predicate )
		{
			if (source == null)
			{
				throw new ArgumentNullException( "source" );
			}

			if (predicate == null)
			{
				throw new ArgumentNullException( "predicate" );
			}

			foreach (var item in source)
			{
				if (predicate( item ))
				{
					return true;
				}
			}

			return false;
		}
	}
}

