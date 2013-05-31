using System.Collections.Generic;
using System.IO;
using System.Text;
using System;


namespace TinyJSON
{
	public sealed class Decoder : IDisposable
	{
		const string WHITE_SPACE = " \t\n\r";
		const string WORD_BREAK  = " \t\n\r{}[],:\"";

		enum TOKEN
		{
			NONE,
			CURLY_OPEN,
			CURLY_CLOSE,
			SQUARED_OPEN,
			SQUARED_CLOSE,
			COLON,
			COMMA,
			STRING,
			NUMBER,
			TRUE,
			FALSE,
			NULL
		};


		StringReader json;


		Decoder( string jsonString )
		{
			json = new StringReader( jsonString );
		}


		public static Variant Decode( string jsonString )
		{
			using (var instance = new Decoder( jsonString ))
			{
				return instance.DecodeValue();
			}
		}


		public void Dispose()
		{
			json.Dispose();
			json = null;
		}


		ProxyObject DecodeObject()
		{
			ProxyObject obj = new ProxyObject();

			// ditch opening brace
			json.Read();

			// {
			while (true)
			{
				switch (NextToken)
				{
				case TOKEN.NONE:
					return null;

				case TOKEN.COMMA:
					continue;

				case TOKEN.CURLY_CLOSE:
					return obj;

				default:
					// name
					string name = DecodeString();
					if (name == null)
					{
						return null;
					}

					// :
					if (NextToken != TOKEN.COLON)
					{
						return null;
					}
					json.Read();

					obj.Add( name, DecodeValue() );
					break;
				}
			}
		}


		ProxyArray DecodeArray()
		{
			ProxyArray array = new ProxyArray();

			// ditch opening bracket
			json.Read();

			// [
			var parsing = true;
			while (parsing)
			{
				TOKEN nextToken = NextToken;

				switch (nextToken)
				{
				case TOKEN.NONE:
					return null;

				case TOKEN.COMMA:
					continue;

				case TOKEN.SQUARED_CLOSE:
					parsing = false;
					break;

				default:
					array.Add( DecodeByToken( nextToken ) );
					break;
				}
			}

			return array;
		}


		Variant DecodeValue()
		{
			TOKEN nextToken = NextToken;
			return DecodeByToken( nextToken );
		}


		Variant DecodeByToken( TOKEN token )
		{
			switch (token)
			{
			case TOKEN.STRING:
				return DecodeString();

			case TOKEN.NUMBER:
				return DecodeNumber();

			case TOKEN.CURLY_OPEN:
				return DecodeObject();

			case TOKEN.SQUARED_OPEN:
				return DecodeArray();

			case TOKEN.TRUE:
				return new ProxyBoolean( true );

			case TOKEN.FALSE:
				return new ProxyBoolean( false );

			case TOKEN.NULL:
				return null;

			default:
				return null;
			}
		}


		Variant DecodeString()
		{
			StringBuilder s = new StringBuilder();
			char c;

			// ditch opening quote
			json.Read();

			bool parsing = true;
			while (parsing)
			{
				if (json.Peek() == -1)
				{
					parsing = false;
					break;
				}

				c = NextChar;
				switch (c)
				{
				case '"':
					parsing = false;
					break;

				case '\\':
					if (json.Peek() == -1)
					{
						parsing = false;
						break;
					}

					c = NextChar;
					switch (c)
					{
					case '"':
					case '\\':
					case '/':
						s.Append( c );
						break;

					case 'b':
						s.Append( '\b' );
						break;

					case 'f':
						s.Append( '\f' );
						break;

					case 'n':
						s.Append( '\n' );
						break;

					case 'r':
						s.Append( '\r' );
						break;

					case 't':
						s.Append( '\t' );
						break;

					case 'u':
						var hex = new StringBuilder();

						for (int i = 0; i < 4; i++)
						{
							hex.Append( NextChar );
						}

						s.Append(( char ) Convert.ToInt32( hex.ToString(), 16 ));
						break;
					}
					break;

				default:
					s.Append( c );
					break;
				}
			}

			return new ProxyString( s.ToString() );
		}


		Variant DecodeNumber()
		{
			return new ProxyNumber( NextWord );
		}


		void ConsumeWhiteSpace()
		{
			while (WHITE_SPACE.IndexOf( PeekChar ) != -1)
			{
				json.Read();

				if (json.Peek() == -1)
				{
					break;
				}
			}
		}


		char PeekChar
		{
			get
			{
				return Convert.ToChar( json.Peek());
			}
		}

		char NextChar
		{
			get
			{
				return Convert.ToChar( json.Read());
			}
		}

		string NextWord
		{
			get
			{
				StringBuilder word = new StringBuilder();

				while (WORD_BREAK.IndexOf( PeekChar ) == -1)
				{
					word.Append( NextChar );

					if (json.Peek() == -1)
					{
						break;
					}
				}

				return word.ToString();
			}
		}

		TOKEN NextToken
		{
			get
			{
				ConsumeWhiteSpace();

				if (json.Peek() == -1)
				{
					return TOKEN.NONE;
				}

				switch (PeekChar)
				{
				case '{':
					return TOKEN.CURLY_OPEN;

				case '}':
					json.Read();
					return TOKEN.CURLY_CLOSE;

				case '[':
					return TOKEN.SQUARED_OPEN;

				case ']':
					json.Read();
					return TOKEN.SQUARED_CLOSE;

				case ',':
					json.Read();
					return TOKEN.COMMA;

				case '"':
					return TOKEN.STRING;

				case ':':
					return TOKEN.COLON;

				case '0':
				case '1':
				case '2':
				case '3':
				case '4':
				case '5':
				case '6':
				case '7':
				case '8':
				case '9':
				case '-':
					return TOKEN.NUMBER;
				}

				string word = NextWord;

				switch (word)
				{
				case "false":
					return TOKEN.FALSE;

				case "true":
					return TOKEN.TRUE;

				case "null":
					return TOKEN.NULL;
				}

				return TOKEN.NONE;
			}
		}
	}
}

