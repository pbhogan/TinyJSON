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


		public static bool AnyOfType<TSource>( this IEnumerable<TSource> source, Type expectedType )
		{
			if (source == null)
			{
				throw new ArgumentNullException( "source" );
			}

			if (expectedType == null)
			{
				throw new ArgumentNullException( "expectedType" );
			}

			foreach (var item in source)
			{
				if ( expectedType.IsAssignableFrom( item.GetType() ) )
				{
					return true;
				}
			}

			return false;
		}
	}
}

