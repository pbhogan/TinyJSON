using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;


namespace TinyJSON
{
	public sealed class Skip : Attribute {} // Mark members that should not be dumped.
	public sealed class Load : Attribute {} // Mark methods to be invoked after loading.


	public static class JSON
	{
		public static Variant Load( string json )
		{
			if (json == null)
			{
				throw new ArgumentNullException( "json" );
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


		private static T DecodeType<T>( Variant data )
		{
			var type = typeof( T );

			if (type.IsEnum)
			{
				return (T) Enum.Parse( type, data.ToString() );
			}

			if (type.IsPrimitive || type == typeof(string))
			{
				return (T) Convert.ChangeType( data, type );
			}

			if (typeof(IList).IsAssignableFrom( type ))
			{
				var makeFunc = decodeListMethod.MakeGenericMethod( type.GetGenericArguments() );
				return (T) makeFunc.Invoke( null, new object[] { data } );
			}

			if (typeof(IDictionary).IsAssignableFrom( type ))
			{
				var makeFunc = decodeDictMethod.MakeGenericMethod( type.GetGenericArguments() );
				return (T) makeFunc.Invoke( null, new object[] { data } );
			}

			// This is a class or struct, so instantiate it with the default constructor.
			var instance = Activator.CreateInstance<T>();

			// Now decode each field, except for those tagged with [Skip] attribute.
			foreach (var pair in data as ProxyObject)
			{
				var field = type.GetField( pair.Key );
				if (field != null)
				{
					if (!Attribute.GetCustomAttributes( field ).Any( attr => attr is Skip ))
					{
						var makeFunc = decodeTypeMethod.MakeGenericMethod( new Type[] { field.FieldType } );
						if (type.IsValueType)
						{
							// Type is a struct
							var instanceRef = (object) instance;
							field.SetValue( instanceRef, makeFunc.Invoke( null, new object[] { pair.Value } ) );
							instance = (T) instanceRef;
						}
						else
						{
							// Type is a class
							field.SetValue( instance, makeFunc.Invoke( null, new object[] { pair.Value } ) );
						}
					}
				}
			}

			// Invoke methods tagged with [Load] attribute.
			foreach (var method in type.GetMethods( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance ))
			{
				if (method.GetCustomAttributes( false ).Any( attr => attr is Load ))
				{
					if (method.GetParameters().Length == 0)
					{
						method.Invoke( instance, null );
					}
					else
					{
						method.Invoke( instance, new object[] { data } );
					}
				}
			}

			return instance;
		}


		private static List<T> DecodeList<T>( Variant data )
		{
			var list = new List<T>();

			foreach (var item in data as ProxyArray)
			{
				list.Add( DecodeType<T>( item ) );
			}

			return list;
		}


		private static Dictionary<K,V> DecodeDict<K,V>( Variant data )
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


		private static MethodInfo decodeTypeMethod = typeof( JSON ).GetMethod( "DecodeType", BindingFlags.NonPublic | BindingFlags.Static );
		private static MethodInfo decodeListMethod = typeof( JSON ).GetMethod( "DecodeList", BindingFlags.NonPublic | BindingFlags.Static );
		private static MethodInfo decodeDictMethod = typeof( JSON ).GetMethod( "DecodeDict", BindingFlags.NonPublic | BindingFlags.Static );
	}
}

