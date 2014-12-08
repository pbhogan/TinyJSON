using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;


namespace TinyJSON
{
	// Mark members that should not be dumped.
	public sealed class Skip : Attribute
	{
	}


	// Mark methods to be invoked after loading.
	public sealed class Load : Attribute
	{
	}


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


		public static string Dump( object data, EncodeOptions options = EncodeOptions.Default )
		{
			return Encoder.Encode( data, options );
		}


		public static void MakeInto<T>( Variant data, out T item )
		{
			item = DecodeType<T>( data );
		}


		private static Dictionary<string,Type> typeCache = new Dictionary<string,Type>();
		private static Type FindType( string fullName )
		{
			if (fullName == null)
			{
				return null;
			}

			Type type;
			if (typeCache.TryGetValue( fullName, out type ))
			{
				return type;
			}

			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				type = assembly.GetType( fullName );
				if (type != null)
				{
					typeCache.Add( fullName, type );
					return type;
				}
			}

			return null;
		}


		private static T DecodeType<T>( Variant data )
		{
			if (data == null)
			{
				return default(T);
			}

			var type = typeof(T);

			if (type.IsEnum)
			{
				return (T) Enum.Parse( type, data.ToString() );
			}

			if (type.IsPrimitive || type == typeof(string) || type == typeof(decimal))
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

			// At this point we should be dealing with a class or struct.
			T instance;
			var proxyObject = data as ProxyObject;
			if (proxyObject == null)
			{
				throw new InvalidCastException( "ProxyObject expected when decoding into '" + type.FullName + "'." );
			}

			// If there's a type hint, use it to create the instance.
			var typeHint = proxyObject.TypeHint;
			if (typeHint != null && typeHint != type.FullName)
			{
				var makeType = FindType( typeHint );
				if (makeType == null)
				{
					throw new TypeLoadException( "Could not load type '" + typeHint + "'." );
				}
				else
				{
					if (type.IsAssignableFrom( makeType ))
					{
						instance = (T) Activator.CreateInstance( makeType );
						type = makeType;
					}
					else
					{
						throw new InvalidCastException( "Cannot assign type '" + typeHint + "' to type '" + type.FullName + "'." );
					}
				}
			}
			else
			{
				// We don't have a type hint, so just instantiate the type we have.
				instance = Activator.CreateInstance<T>();
			}


			// Now decode each field, except for those tagged with [Skip] attribute.
			foreach (var pair in data as ProxyObject)
			{
				var field = type.GetField( pair.Key, instanceBindingFlags );
				if (field != null)
				{
					if (!Attribute.GetCustomAttributes( field ).AnyOfType( typeof(Skip) ))
					{
						var makeFunc = decodeTypeMethod.MakeGenericMethod( new Type[] { field.FieldType } );
						if (type.IsValueType)
						{
							// Type is a struct.
							var instanceRef = (object) instance;
							field.SetValue( instanceRef, makeFunc.Invoke( null, new object[] { pair.Value } ) );
							instance = (T) instanceRef;
						}
						else
						{
							// Type is a class.
							field.SetValue( instance, makeFunc.Invoke( null, new object[] { pair.Value } ) );
						}
					}
				}
			}

			// Invoke methods tagged with [Load] attribute.
			foreach (var method in type.GetMethods( instanceBindingFlags ))
			{
				if (method.GetCustomAttributes( false ).AnyOfType( typeof(Load) ))
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
				var k = (K) Convert.ChangeType( pair.Key, typeof(K) );
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
		private static MethodInfo decodeTypeMethod = typeof(JSON).GetMethod( "DecodeType", staticBindingFlags );
		private static MethodInfo decodeListMethod = typeof(JSON).GetMethod( "DecodeList", staticBindingFlags );
		private static MethodInfo decodeDictionaryMethod = typeof(JSON).GetMethod( "DecodeDictionary", staticBindingFlags );
		private static MethodInfo decodeArrayMethod = typeof(JSON).GetMethod( "DecodeArray", staticBindingFlags );


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
			DecodeDictionary<Decimal,T>( null );
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
			SupportTypeForAOT<Decimal>();
			SupportTypeForAOT<Boolean>();
			SupportTypeForAOT<String>();
		}
	}
}

