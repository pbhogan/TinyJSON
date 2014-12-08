using System;
using System.Collections;
using System.Reflection;
using System.Text;


namespace TinyJSON
{
	public sealed class Encoder
	{
		StringBuilder builder;
		EncodeOptions options;
		int indent;


		Encoder( EncodeOptions options )
		{
			this.options = options;
			builder = new StringBuilder();
			indent = 0;
		}


		public static string Encode( object obj, EncodeOptions options = EncodeOptions.Default )
		{
			var instance = new Encoder( options );
			instance.EncodeValue( obj );
			return instance.builder.ToString();
		}


		bool PrettyPrint
		{
			get
			{
				return ((options & EncodeOptions.PrettyPrint) == EncodeOptions.PrettyPrint);
			}
		}


		bool TypeHint
		{
			get
			{
				return ((options & EncodeOptions.TypeHint) == EncodeOptions.TypeHint);
			}
		}


		void EncodeValue( object value )
		{
			IList asList;
			IDictionary asDict;
			string asString;

			if (value == null)
			{
				builder.Append( "null" );
			}
			else
			if ((asString = value as string) != null)
			{
				EncodeString( asString );
			}
			else
			if (value is bool)
			{
				builder.Append( value.ToString().ToLower() );
			}
			else
			if (value is Enum)
			{
				EncodeString( value.ToString() );
			}
			else
			if ((asList = value as IList) != null)
			{
				EncodeList( asList );
			}
			else
			if ((asDict = value as IDictionary) != null)
			{
				EncodeDictionary( asDict );
			}
			else
			if (value is char)
			{
				EncodeString( value.ToString() );
			}
			else
			{
				EncodeOther( value );
			}
		}


		void EncodeObject( object value )
		{
			AppendOpenBrace();

			var type = value.GetType();

			var firstItem = !TypeHint;
			if (TypeHint)
			{
				if (PrettyPrint)
				{
					AppendIndent();
				}
				EncodeString( ProxyObject.TypeHintName );
				AppendColon();
				EncodeString( type.FullName );
				firstItem = false;
			}

			var fields = type.GetFields( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance );
			foreach (var field in fields)
			{
				if (!Attribute.GetCustomAttributes( field ).AnyOfType( typeof(Skip) ))
				{
					AppendComma( firstItem );
					EncodeString( field.Name );
					AppendColon();
					EncodeValue( field.GetValue( value ) );
					firstItem = false;
				}
			}

			AppendCloseBrace();
		}


		void EncodeDictionary( IDictionary value )
		{
			if (value.Count == 0)
			{
				builder.Append( "{}" );
			}
			else
			{
				AppendOpenBrace();

				var firstItem = true;
				foreach (object e in value.Keys)
				{
					AppendComma( firstItem );
					EncodeString( e.ToString() );
					AppendColon();
					EncodeValue( value[e] );
					firstItem = false;
				}

				AppendCloseBrace();
			}
		}


		void EncodeList( IList value )
		{
			if (value.Count == 0)
			{
				builder.Append( "[]" );
			}
			else
			{
				AppendOpenBracket();

				var firstItem = true;
				foreach (object obj in value)
				{
					AppendComma( firstItem );
					EncodeValue( obj );
					firstItem = false;
				}

				AppendCloseBracket();
			}
		}


		void EncodeString( string value )
		{
			builder.Append( '\"' );

			char[] charArray = value.ToCharArray();
			foreach (var c in charArray)
			{
				switch (c)
				{
					case '"':
						builder.Append( "\\\"" );
						break;

					case '\\':
						builder.Append( "\\\\" );
						break;

					case '\b':
						builder.Append( "\\b" );
						break;

					case '\f':
						builder.Append( "\\f" );
						break;

					case '\n':
						builder.Append( "\\n" );
						break;

					case '\r':
						builder.Append( "\\r" );
						break;

					case '\t':
						builder.Append( "\\t" );
						break;

					default:
						int codepoint = Convert.ToInt32( c );
						if ((codepoint >= 32) && (codepoint <= 126))
						{
							builder.Append( c );
						}
						else
						{
							builder.Append( "\\u" + Convert.ToString( codepoint, 16 ).PadLeft( 4, '0' ) );
						}
						break;
				}
			}

			builder.Append( '\"' );
		}


		void EncodeOther( object value )
		{
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
			    value is decimal)
			{
				builder.Append( value.ToString() );
			}
			else
			{
				EncodeObject( value );
			}
		}


		#region Helpers

		void AppendIndent()
		{
			for (int i = 0; i < indent; i++)
			{
				builder.Append( '\t' );
			}
		}


		void AppendOpenBrace()
		{
			builder.Append( '{' );

			if (PrettyPrint)
			{
				builder.Append( '\n' );
				indent++;
			}
		}


		void AppendCloseBrace()
		{
			if (PrettyPrint)
			{
				builder.Append( '\n' );
				indent--;
				AppendIndent();
			}

			builder.Append( '}' );
		}


		void AppendOpenBracket()
		{
			builder.Append( '[' );

			if (PrettyPrint)
			{
				builder.Append( '\n' );
				indent++;
			}
		}


		void AppendCloseBracket()
		{
			if (PrettyPrint)
			{
				builder.Append( '\n' );
				indent--;
				AppendIndent();
			}

			builder.Append( ']' );
		}


		void AppendComma( bool firstItem )
		{
			if (!firstItem)
			{
				builder.Append( ',' );

				if (PrettyPrint)
				{
					builder.Append( '\n' );
				}
			}

			if (PrettyPrint)
			{
				AppendIndent();
			}
		}


		void AppendColon()
		{
			builder.Append( ':' );

			if (PrettyPrint)
			{
				builder.Append( ' ' );
			}
		}

		#endregion
	}
}

