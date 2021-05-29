using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.IO;


namespace TinyJSON
{
	public sealed class Encoder
	{
		static readonly Type includeAttrType = typeof(Include);
		static readonly Type excludeAttrType = typeof(Exclude);
		static readonly Type typeHintAttrType = typeof(TypeHint);

		readonly StringBuilder builder;
		readonly EncodeOptions options;
		int indent;
		readonly bool writeToFile;
		readonly string filePath;
		StringBuilder buffer;
		const int writeThreshold = 4096; //arbitrary

		Encoder( EncodeOptions options , bool writeToFile, string filePath)
		{
			this.options = options;
			builder = new StringBuilder();
			indent = 0;
			this.writeToFile = writeToFile;
			this.filePath = filePath;
			buffer = new StringBuilder(writeThreshold * 2);
		}


		// ReSharper disable once UnusedMember.Global
		public static string Encode( object obj )
		{
			return Encode( obj, EncodeOptions.None );
		}


		public static string Encode( object obj, EncodeOptions options )
		{
			var instance = new Encoder( options , false , null );
			instance.EncodeValue( obj, false );
			return instance.builder.ToString();
		}


		public static void EncodeToFile( object obj , string filePath )
		{
			EncodeToFile(obj, filePath, EncodeOptions.None);
		}


		public static void EncodeToFile( object obj , string filePath , EncodeOptions options )
		{
			string directory = Path.GetDirectoryName(filePath);
			if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
			File.Create(filePath).Close();
			var instance = new Encoder(options, true, filePath);
			instance.EncodeValue(obj, false);
			instance.WriteBufferToFile();
		}


		bool PrettyPrintEnabled
		{
			get
			{
				return (options & EncodeOptions.PrettyPrint) == EncodeOptions.PrettyPrint;
			}
		}


		bool TypeHintsEnabled
		{
			get
			{
				return (options & EncodeOptions.NoTypeHints) != EncodeOptions.NoTypeHints;
			}
		}


		bool IncludePublicPropertiesEnabled
		{
			get
			{
				return (options & EncodeOptions.IncludePublicProperties) == EncodeOptions.IncludePublicProperties;
			}
		}


		bool EnforceHierarchyOrderEnabled
		{
			get
			{
				return (options & EncodeOptions.EnforceHierarchyOrder) == EncodeOptions.EnforceHierarchyOrder;
			}
		}


		void Append(char character)
		{
			if (writeToFile)
			{
				buffer.Append(character);
				if (buffer.Length > writeThreshold) WriteBufferToFile();
			}
			else builder.Append(character);
		}


		void Append(string text)
		{
			if (writeToFile)
			{
				buffer.Append(text);
				if (buffer.Length > writeThreshold) WriteBufferToFile();
			}
			else builder.Append(text);
		}


		void WriteBufferToFile()
		{
			System.IO.File.AppendAllText(filePath, buffer.ToString());
			buffer = new StringBuilder(writeThreshold * 2);
		}


		void EncodeValue( object value, bool forceTypeHint )
		{
			if (value == null)
			{
				Append( "null" );
				return;
			}

			if (value is string)
			{
				EncodeString( (string) value );
				return;
			}

			if (value is ProxyString)
			{
				EncodeString( ((ProxyString) value).ToString( CultureInfo.InvariantCulture ) );
				return;
			}

			if (value is char)
			{
				EncodeString( value.ToString() );
				return;
			}

			if (value is bool)
			{
				Append( (bool) value ? "true" : "false" );
				return;
			}

			if (value is Enum)
			{
				EncodeString( value.ToString() );
				return;
			}

			if (value is Array)
			{
				EncodeArray( (Array) value, forceTypeHint );
				return;
			}

			if (value is IList)
			{
				EncodeList( (IList) value, forceTypeHint );
				return;
			}

			if (value is IDictionary)
			{
				EncodeDictionary( (IDictionary) value, forceTypeHint );
				return;
			}

			if (value is Guid)
			{
				EncodeString( value.ToString() );
				return;
			}

			if (value is ProxyArray)
			{
				EncodeProxyArray( (ProxyArray) value );
				return;
			}

			if (value is ProxyObject)
			{
				EncodeProxyObject( (ProxyObject) value );
				return;
			}

			if (value is float ||
			    value is double ||
			    value is int ||
			    value is uint ||
			    value is long ||
			    value is sbyte ||
			    value is byte ||
			    value is short ||
			    value is ushort ||
			    value is ulong ||
			    value is decimal ||
			    value is ProxyBoolean ||
			    value is ProxyNumber)
			{
				Append( Convert.ToString( value, CultureInfo.InvariantCulture ) );
				return;
			}

			EncodeObject( value, forceTypeHint );
		}


		IEnumerable<FieldInfo> GetFieldsForType( Type type )
		{
			if (EnforceHierarchyOrderEnabled)
			{
				var types = new Stack<Type>();
				while (type != null)
				{
					types.Push( type );
					type = type.BaseType;
				}

				var fields = new List<FieldInfo>();
				while (types.Count > 0)
				{
					fields.AddRange( types.Pop().GetFields( BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance ) );
				}

				return fields;
			}

			return type.GetFields( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance );
		}


		IEnumerable<PropertyInfo> GetPropertiesForType( Type type )
		{
			if (EnforceHierarchyOrderEnabled)
			{
				var types = new Stack<Type>();
				while (type != null)
				{
					types.Push( type );
					type = type.BaseType;
				}

				var properties = new List<PropertyInfo>();
				while (types.Count > 0)
				{
					properties.AddRange( types.Pop().GetProperties( BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance ) );
				}

				return properties;
			}

			return type.GetProperties( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance );
		}


		void EncodeObject( object value, bool forceTypeHint )
		{
			var type = value.GetType();

			AppendOpenBrace();

			forceTypeHint = forceTypeHint || TypeHintsEnabled;

			var includePublicProperties = IncludePublicPropertiesEnabled;

			var firstItem = !forceTypeHint;
			if (forceTypeHint)
			{
				if (PrettyPrintEnabled)
				{
					AppendIndent();
				}

				EncodeString( ProxyObject.TypeHintKey );
				AppendColon();
				EncodeString( type.FullName );

				// ReSharper disable once RedundantAssignment
				firstItem = false;
			}

			foreach (var field in GetFieldsForType( type ))
			{
				var shouldTypeHint = false;
				var shouldEncode = field.IsPublic;
				foreach (var attribute in field.GetCustomAttributes( true ))
				{
					if (excludeAttrType.IsInstanceOfType( attribute ))
					{
						shouldEncode = false;
					}

					if (includeAttrType.IsInstanceOfType( attribute ))
					{
						shouldEncode = true;
					}

					if (typeHintAttrType.IsInstanceOfType( attribute ))
					{
						shouldTypeHint = true;
					}
				}

				if (shouldEncode)
				{
					AppendComma( firstItem );
					EncodeString( field.Name );
					AppendColon();
					EncodeValue( field.GetValue( value ), shouldTypeHint );
					firstItem = false;
				}
			}

			foreach (var property in GetPropertiesForType( type ))
			{
				if (property.CanRead)
				{
					var shouldTypeHint = false;
					var shouldEncode = includePublicProperties;

					foreach (var attribute in property.GetCustomAttributes( true ))
					{
						if (excludeAttrType.IsInstanceOfType( attribute ))
						{
							shouldEncode = false;
						}

						if (includeAttrType.IsInstanceOfType( attribute ))
						{
							shouldEncode = true;
						}

						if (typeHintAttrType.IsInstanceOfType( attribute ))
						{
							shouldTypeHint = true;
						}
					}

					if (shouldEncode)
					{
						AppendComma( firstItem );
						EncodeString( property.Name );
						AppendColon();
						EncodeValue( property.GetValue( value, null ), shouldTypeHint );
						firstItem = false;
					}
				}
			}

			AppendCloseBrace();
		}


		void EncodeProxyArray( ProxyArray value )
		{
			if (value.Count == 0)
			{
				Append( "[]" );
			}
			else
			{
				AppendOpenBracket();

				var firstItem = true;
				foreach (var obj in value)
				{
					AppendComma( firstItem );
					EncodeValue( obj, false );
					firstItem = false;
				}

				AppendCloseBracket();
			}
		}


		void EncodeProxyObject( ProxyObject value )
		{
			if (value.Count == 0)
			{
				Append( "{}" );
			}
			else
			{
				AppendOpenBrace();

				var firstItem = true;
				foreach (var e in value.Keys)
				{
					AppendComma( firstItem );
					EncodeString( e );
					AppendColon();
					EncodeValue( value[e], false );
					firstItem = false;
				}

				AppendCloseBrace();
			}
		}


		void EncodeDictionary( IDictionary value, bool forceTypeHint )
		{
			if (value.Count == 0)
			{
				Append( "{}" );
			}
			else
			{
				AppendOpenBrace();

				var firstItem = true;
				foreach (var e in value.Keys)
				{
					AppendComma( firstItem );
					EncodeString( e.ToString() );
					AppendColon();
					EncodeValue( value[e], forceTypeHint );
					firstItem = false;
				}

				AppendCloseBrace();
			}
		}


		// ReSharper disable once SuggestBaseTypeForParameter
		void EncodeList( IList value, bool forceTypeHint )
		{
			if (value.Count == 0)
			{
				Append( "[]" );
			}
			else
			{
				AppendOpenBracket();

				var firstItem = true;
				foreach (var obj in value)
				{
					AppendComma( firstItem );
					EncodeValue( obj, forceTypeHint );
					firstItem = false;
				}

				AppendCloseBracket();
			}
		}


		void EncodeArray( Array value, bool forceTypeHint )
		{
			if (value.Rank == 1)
			{
				EncodeList( value, forceTypeHint );
			}
			else
			{
				var indices = new int[value.Rank];
				EncodeArrayRank( value, 0, indices, forceTypeHint );
			}
		}


		void EncodeArrayRank( Array value, int rank, int[] indices, bool forceTypeHint )
		{
			AppendOpenBracket();

			var min = value.GetLowerBound( rank );
			var max = value.GetUpperBound( rank );

			if (rank == value.Rank - 1)
			{
				for (var i = min; i <= max; i++)
				{
					indices[rank] = i;
					AppendComma( i == min );
					EncodeValue( value.GetValue( indices ), forceTypeHint );
				}
			}
			else
			{
				for (var i = min; i <= max; i++)
				{
					indices[rank] = i;
					AppendComma( i == min );
					EncodeArrayRank( value, rank + 1, indices, forceTypeHint );
				}
			}

			AppendCloseBracket();
		}


		void EncodeString( string value )
		{
			Append( '\"' );

			var charArray = value.ToCharArray();
			foreach (var c in charArray)
			{
				switch (c)
				{
					case '"':
						Append( "\\\"" );
						break;

					case '\\':
						Append( "\\\\" );
						break;

					case '\b':
						Append( "\\b" );
						break;

					case '\f':
						Append( "\\f" );
						break;

					case '\n':
						Append( "\\n" );
						break;

					case '\r':
						Append( "\\r" );
						break;

					case '\t':
						Append( "\\t" );
						break;

					default:
						var codepoint = Convert.ToInt32( c );
						if ((codepoint >= 32) && (codepoint <= 126))
						{
							Append( c );
						}
						else
						{
							Append( "\\u" + Convert.ToString( codepoint, 16 ).PadLeft( 4, '0' ) );
						}

						break;
				}
			}

			Append( '\"' );
		}


		#region Helpers

		void AppendIndent()
		{
			for (var i = 0; i < indent; i++)
			{
				Append( '\t' );
			}
		}


		void AppendOpenBrace()
		{
			Append( '{' );

			if (PrettyPrintEnabled)
			{
				Append( '\n' );
				indent++;
			}
		}


		void AppendCloseBrace()
		{
			if (PrettyPrintEnabled)
			{
				Append( '\n' );
				indent--;
				AppendIndent();
			}

			Append( '}' );
		}


		void AppendOpenBracket()
		{
			Append( '[' );

			if (PrettyPrintEnabled)
			{
				Append( '\n' );
				indent++;
			}
		}


		void AppendCloseBracket()
		{
			if (PrettyPrintEnabled)
			{
				Append( '\n' );
				indent--;
				AppendIndent();
			}

			Append( ']' );
		}


		void AppendComma( bool firstItem )
		{
			if (!firstItem)
			{
				Append( ',' );

				if (PrettyPrintEnabled)
				{
					Append( '\n' );
				}
			}

			if (PrettyPrintEnabled)
			{
				AppendIndent();
			}
		}


		void AppendColon()
		{
			Append( ':' );

			if (PrettyPrintEnabled)
			{
				Append( ' ' );
			}
		}

		#endregion
	}
}
