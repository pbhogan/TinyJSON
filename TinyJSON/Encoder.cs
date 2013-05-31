using System;
using System.Collections;
using System.Reflection;
using System.Text;


namespace TinyJSON
{
	public sealed class Encoder
	{
		const string INDENT = "\t";

		StringBuilder builder;
		bool prettyPrint;
		int indent;


		Encoder( bool prettyPrint = false )
		{
			this.prettyPrint = prettyPrint;

			builder = new StringBuilder();
			indent = 0;
		}


		public static string Encode( object obj, bool prettyPrint = false )
		{
			var instance = new Encoder( prettyPrint );
			instance.EncodeValue( obj );
			return instance.builder.ToString();
		}


		void AppendIndent()
		{
			for (int i = 0; i < indent; i++)
			{
				builder.Append( INDENT );
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
				builder.Append( value.ToString().ToLower());
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
				EncodeString( value.ToString());
			}
			else
			{
				EncodeOther( value );
			}
		}


		void EncodeObject( object value )
		{
			builder.Append( '{' );

			if (prettyPrint)
			{
				builder.Append( '\n' );
				indent++;
			}

			bool first = true;
			var fields = value.GetType().GetFields( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance );
			foreach (var field in fields)
			{
				if (!Attribute.GetCustomAttributes( field ).Any( attr => attr is Skip ))
				{
					if (!first)
					{
						builder.Append( ',' );

						if (prettyPrint)
						{
							builder.Append( '\n' );
						}
					}

					if (prettyPrint)
					{
						AppendIndent();
					}

					EncodeString( field.Name );

					builder.Append( ':' );

					if (prettyPrint)
					{
						builder.Append( ' ' );
					}

					EncodeValue( field.GetValue( value ));

					first = false;
				}
			}

			if (prettyPrint)
			{
				builder.Append( '\n' );
				indent--;
				AppendIndent();
			}

			builder.Append( '}' );
		}


		void EncodeDictionary( IDictionary value )
		{
			bool first = true;

			builder.Append( '{' );

			if (prettyPrint)
			{
				builder.Append( '\n' );
				indent++;
			}

			foreach (object e in value.Keys)
			{
				if (!first)
				{
					builder.Append( ',' );

					if (prettyPrint)
					{
						builder.Append( '\n' );
					}
				}

				if (prettyPrint)
				{
					AppendIndent();
				}

				EncodeString( e.ToString());

				builder.Append( ':' );

				if (prettyPrint)
				{
					builder.Append( ' ' );
				}

				EncodeValue( value[e] );

				first = false;
			}

			if (prettyPrint)
			{
				builder.Append( '\n' );
				indent--;
				AppendIndent();
			}

			builder.Append( '}' );
		}


		void EncodeList( IList value )
		{
			bool first = true;

			builder.Append( '[' );

			if (prettyPrint)
			{
				builder.Append( '\n' );
				indent++;
			}

			foreach (object obj in value)
			{
				if (!first)
				{
					builder.Append( ',' );

					if (prettyPrint)
					{
						builder.Append( '\n' );
					}
				}

				if (prettyPrint)
				{
					AppendIndent();
				}

				EncodeValue( obj );

				first = false;
			}

			if (prettyPrint)
			{
				builder.Append( '\n' );
				indent--;
				AppendIndent();
			}

			builder.Append( ']' );
		}


		void EncodeString( string value )
		{
			builder.Append( '\"' );

			char [] charArray = value.ToCharArray();
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
						builder.Append( "\\u" + Convert.ToString( codepoint, 16 ).PadLeft( 4, '0' ));
					}
					break;
				}
			}

			builder.Append( '\"' );
		}


		void EncodeOther( object value )
		{
			if (value is float  ||
			    value is double ||
			    value is int    ||
			    value is uint   ||
			    value is long   ||
			    value is sbyte  ||
			    value is byte   ||
			    value is short  ||
			    value is ushort ||
			    value is ulong  ||
			    value is decimal)
			{
				builder.Append( value.ToString());
			}
			else
			{
				EncodeObject( value );
			}
		}
	}
}

