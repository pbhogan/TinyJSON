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

			if (type.IsArray)
			{
				var makeFunc = decodeArrayMethod.MakeGenericMethod( new Type[] { type.GetElementType() } );
				return (T) makeFunc.Invoke( null, new object[] { data } );
			}

			if (typeof(IList).IsAssignableFrom( type ))
			{
				var makeFunc = decodeListMethod.MakeGenericMethod( type.GetGenericArguments() );
				return (T) makeFunc.Invoke( null, new object[] { data } );
			}

			if (typeof(IDictionary).IsAssignableFrom( type ))
			{
				var makeFunc = decodeDictionaryMethod.MakeGenericMethod( type.GetGenericArguments() );
				return (T) makeFunc.Invoke( null, new object[] { data } );
			}

			// This is a class or struct, so instantiate it with the default constructor.
			var instance = Activator.CreateInstance<T>();

			// Now decode each field, except for those tagged with [Skip] attribute.
			foreach (var pair in data as ProxyObject)
			{
				var field = type.GetField( pair.Key, instanceBindingFlags );
				if (field != null)
				{
					if (!Attribute.GetCustomAttributes( field ).AnyOfType( typeof( Skip ) ))
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
			foreach (var method in type.GetMethods( instanceBindingFlags ))
			{
				if (method.GetCustomAttributes( false ).AnyOfType( typeof( Load ) ))
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


		private static Dictionary<K,V> DecodeDictionary<K,V>( Variant data )
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


		private static T[] DecodeArray<T>( Variant data )
		{
			var arrayData = data as ProxyArray;
			var arraySize = arrayData.Count;
			var array = new T[arraySize];

			int i = 0;
			foreach (var item in data as ProxyArray)
			{
				array[i++] = DecodeType<T>( item );
			}

			return array;
		}


		private static BindingFlags instanceBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
		private static BindingFlags staticBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
		private static MethodInfo decodeTypeMethod = typeof( JSON ).GetMethod( "DecodeType", staticBindingFlags );
		private static MethodInfo decodeListMethod = typeof( JSON ).GetMethod( "DecodeList", staticBindingFlags );
		private static MethodInfo decodeDictionaryMethod = typeof( JSON ).GetMethod( "DecodeDictionary", staticBindingFlags );
		private static MethodInfo decodeArrayMethod = typeof( JSON ).GetMethod( "DecodeArray", staticBindingFlags );


		private static void SupportTypeForAOT<T>()
		{
			DecodeType<T>( null );
			DecodeList<T>( null );
			DecodeArray<T>( null );
			DecodeDictionary<Int16,T>( null );
			DecodeDictionary<UInt16,T>( null );
			DecodeDictionary<Int32,T>( null );
			DecodeDictionary<UInt32,T>( null );
			DecodeDictionary<Int64,T>( null );
			DecodeDictionary<UInt64,T>( null );
			DecodeDictionary<Single,T>( null );
			DecodeDictionary<Double,T>( null );
			DecodeDictionary<Boolean,T>( null );
			DecodeDictionary<String,T>( null );
		}


		private static void SupportValueTypesForAOT()
		{
			SupportTypeForAOT<Int16>();
			SupportTypeForAOT<UInt16>();
			SupportTypeForAOT<Int32>();
			SupportTypeForAOT<UInt32>();
			SupportTypeForAOT<Int64>();
			SupportTypeForAOT<UInt64>();
			SupportTypeForAOT<Single>();
			SupportTypeForAOT<Double>();
			SupportTypeForAOT<Boolean>();
			SupportTypeForAOT<String>();
		}
	}
}

