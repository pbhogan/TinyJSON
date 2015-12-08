using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;


namespace TinyJSON
{
	/// <summary>
	/// Mark members that should be included. 
	/// Public fields are included by default.
	/// </summary>
	[AttributeUsage( AttributeTargets.Field | AttributeTargets.Property )]
	public sealed class Include : Attribute
	{
	}


	/// <summary>
	/// Mark members that should be excluded.
	/// Private fields and all properties are excluded by default.
	/// </summary>
	[AttributeUsage( AttributeTargets.Field | AttributeTargets.Property )]
	public class Exclude : Attribute
	{
	}


	/// <summary>
	/// Mark methods to be called after an object is decoded.
	/// </summary>
	[AttributeUsage( AttributeTargets.Method )]
	public class AfterDecode : Attribute
	{
	}


	/// <summary>
	/// Mark methods to be called before an object is encoded.
	/// </summary>
	[AttributeUsage( AttributeTargets.Method )]
	public class BeforeEncode : Attribute
	{
	}


	/// <summary>
	/// Mark members to force type hinting even when EncodeOptions.NoTypeHints is set.
	/// </summary>
	[AttributeUsage( AttributeTargets.Field | AttributeTargets.Property )]
	public class TypeHint : Attribute
	{
	}


	[Obsolete( "Use the Exclude attribute instead." )]
	public sealed class Skip : Exclude
	{
	}


	[Obsolete( "Use the AfterDecode attribute instead." )]
	public sealed class Load : AfterDecode
	{
	}


	public sealed class DecodeException : Exception
	{
		public DecodeException( string message )
			: base( message )
		{
		}


		public DecodeException( string message, Exception innerException )
			: base( message, innerException )
		{
		}
	}


	public static class JSON
	{
		static readonly Type includeAttrType = typeof(Include);
		static readonly Type excludeAttrType = typeof(Exclude);


		public static Variant Load( string json )
		{
			if (json == null)
			{
				throw new ArgumentNullException( "json" );
			}

			return Decoder.Decode( json );
		}


		public static string Dump( object data )
		{
			return Dump( data, EncodeOptions.None );
		}


		public static string Dump( object data, EncodeOptions options )
		{
			// Invoke methods tagged with [BeforeEncode] attribute.
			if (data != null)
			{
				var type = data.GetType();
				if (!(type.IsEnum || type.IsPrimitive || type.IsArray))
				{
					foreach (var method in type.GetMethods( instanceBindingFlags ))
					{
						if (method.GetCustomAttributes( false ).AnyOfType( typeof(BeforeEncode) ))
						{
							if (method.GetParameters().Length == 0)
							{
								method.Invoke( data, null );
							}
						}
					}
				}
			}

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
				if (type.GetArrayRank() == 1)
				{
					var makeFunc = decodeArrayMethod.MakeGenericMethod( new Type[] { type.GetElementType() } );
					return (T) makeFunc.Invoke( null, new object[] { data } );
				}
				else
				{
					var arrayData = data as ProxyArray;
					var arrayRank = type.GetArrayRank();
					var rankLengths = new int[arrayRank];
					if (arrayData.CanBeMultiRankArray( rankLengths ))
					{
						var array = Array.CreateInstance( type.GetElementType(), rankLengths ); 
						var makeFunc = decodeMultiRankArrayMethod.MakeGenericMethod( new Type[] { type.GetElementType() } );
						try
						{
							makeFunc.Invoke( null, new object[] { arrayData, array, 1, rankLengths } );
						}
						catch (Exception e)
						{
							throw new DecodeException( "Error decoding multidimensional array. Did you try to decode into an array of incompatible rank or element type?", e );
						}
						return (T) Convert.ChangeType( array, typeof(T) );
					}
					throw new DecodeException( "Error decoding multidimensional array; JSON data doesn't seem fit this structure." );
					#pragma warning disable 0162
					return default(T);
					#pragma warning restore 0162
				}
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


			// Now decode fields and properties.
			foreach (var pair in data as ProxyObject)
			{
				var field = type.GetField( pair.Key, instanceBindingFlags );
				if (field != null)
				{
					var shouldDecode = field.IsPublic;
					foreach (var attribute in field.GetCustomAttributes( true ))
					{
						if (excludeAttrType.IsAssignableFrom( attribute.GetType() ))
						{
							shouldDecode = false;
						}

						if (includeAttrType.IsAssignableFrom( attribute.GetType() ))
						{
							shouldDecode = true;
						}
					}

					if (shouldDecode)
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

				var property = type.GetProperty( pair.Key, instanceBindingFlags );
				if (property != null)
				{
					if (property.CanWrite && property.GetCustomAttributes( false ).AnyOfType( includeAttrType ))
					{
						var makeFunc = decodeTypeMethod.MakeGenericMethod( new Type[] { property.PropertyType } );
						if (type.IsValueType)
						{
							// Type is a struct.
							var instanceRef = (object) instance;
							property.SetValue( instanceRef, makeFunc.Invoke( null, new object[] { pair.Value } ), null );
							instance = (T) instanceRef;
						}
						else
						{
							// Type is a class.
							property.SetValue( instance, makeFunc.Invoke( null, new object[] { pair.Value } ), null );
						}
					}
				}
			}

			// Invoke methods tagged with [AfterDecode] attribute.
			foreach (var method in type.GetMethods( instanceBindingFlags ))
			{
				if (method.GetCustomAttributes( false ).AnyOfType( typeof(AfterDecode) ))
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
			var type = typeof(K);

			foreach (var pair in data as ProxyObject)
			{
				var k = (K) (type.IsEnum ? Enum.Parse( type, pair.Key ) : Convert.ChangeType( pair.Key, type ));
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


		private static void DecodeMultiRankArray<T>( ProxyArray arrayData, Array array, int arrayRank, int[] indices )
		{
			var count = arrayData.Count;
			for (int i = 0; i < count; i++)
			{
				indices[arrayRank - 1] = i;
				if (arrayRank < array.Rank)
				{
					DecodeMultiRankArray<T>( arrayData[i] as ProxyArray, array, arrayRank + 1, indices );
				}
				else
				{
					array.SetValue( DecodeType<T>( arrayData[i] ), indices );
				}
			}
		}


		private static BindingFlags instanceBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
		private static BindingFlags staticBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
		private static MethodInfo decodeTypeMethod = typeof(JSON).GetMethod( "DecodeType", staticBindingFlags );
		private static MethodInfo decodeListMethod = typeof(JSON).GetMethod( "DecodeList", staticBindingFlags );
		private static MethodInfo decodeDictionaryMethod = typeof(JSON).GetMethod( "DecodeDictionary", staticBindingFlags );
		private static MethodInfo decodeArrayMethod = typeof(JSON).GetMethod( "DecodeArray", staticBindingFlags );
		private static MethodInfo decodeMultiRankArrayMethod = typeof(JSON).GetMethod( "DecodeMultiRankArray", staticBindingFlags );


		public static void SupportTypeForAOT<T>()
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

