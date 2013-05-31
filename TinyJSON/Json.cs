using System;
using System.Collections;
using System.Collections.Generic;


namespace TinyJSON
{
	public sealed class Skip : Attribute {}


	public static class JSON
	{
		public static Variant Load( string json )
		{
			if (json == null)
			{
				return null;
			}

			return Decoder.Decode( json );
		}


		public static string Dump( object data, bool prettyPrint = false ) 
		{
			return Encoder.Encode( data, prettyPrint );
		}


		public static void MakeInto<T>( Variant data, out T item )
		{
			item = DecodeType<T>( data );
		}


		public static T DecodeType<T>( Variant data )
		{
			var type = typeof( T );

			if (type.IsEnum)
			{
				return (T) Enum.Parse( type, data.ToString() );
			}

			if (type.IsValueType || type == typeof(string))
			{
				return (T) Convert.ChangeType( data, type );
			}

			if (typeof(IList).IsAssignableFrom( type ))
			{
				var makeFunc = typeof(JSON).GetMethod( "DecodeList" ).MakeGenericMethod( type.GetGenericArguments() );
				return (T) makeFunc.Invoke( null, new object[] { data } );
			}

			if (typeof(IDictionary).IsAssignableFrom( type ))
			{
				var makeFunc = typeof(JSON).GetMethod( "DecodeDict" ).MakeGenericMethod( type.GetGenericArguments() );
				return (T) makeFunc.Invoke( null, new object[] { data } );
			}

			var instance = Activator.CreateInstance<T>();
			foreach (var pair in data as ProxyObject)
			{
				var field = type.GetField( pair.Key );
				if (!Attribute.GetCustomAttributes( field ).Any( attr => attr is Skip ))
				{
					var makeFunc = typeof(JSON).GetMethod( "DecodeType" ).MakeGenericMethod( new Type[] { field.FieldType } );
					field.SetValue( instance, makeFunc.Invoke( null, new object[] { pair.Value } ) );
				}
			}

			return instance;
		}


		public static List<T> DecodeList<T>( Variant data )
		{
			var list = new List<T>();

			foreach (var item in data as ProxyArray)
			{
				list.Add( DecodeType<T>( item ) );
			}

			return list;
		}


		public static Dictionary<K,V> DecodeDict<K,V>( Variant data )
		{
			var dict = new Dictionary<K,V>();

			foreach (var pair in data as ProxyObject)
			{
				var k = (K) Convert.ChangeType( pair.Key, typeof( K ) );
				var v = DecodeType<V>( pair.Value );
				dict.Add( k, v );
			}

			return dict;
		}
	}
}

