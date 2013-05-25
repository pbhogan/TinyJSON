using System;
using System.Globalization;


namespace TinyJSON.Proxy
{
	public sealed class Number : Variant
	{
		private IConvertible value;


		public Number( IConvertible value )
		{
			if (value is string)
			{
				this.value = Parse( value as string );
			}
			else
			{
				this.value = value;
			}
		}


		private IConvertible Parse( string value )
		{
			if (value.IndexOf( '.' ) == -1)
			{
				if (value.IndexOf( '-' ) == -1)
				{
					UInt64 parsedValue;
					UInt64.TryParse( value, out parsedValue );
					return parsedValue;
				}
				else
				{
					Int64 parsedValue;
					Int64.TryParse( value, out parsedValue );
					return parsedValue;
				}
			}
			else
			{
				Double parsedValue;
				Double.TryParse( value, out parsedValue );
				return parsedValue;
			}
		}


		public override bool ToBoolean( IFormatProvider provider )
		{
			return value.ToBoolean( provider );
		}


		public override byte ToByte( IFormatProvider provider )
		{
			return value.ToByte( provider );
		}


		public override char ToChar( IFormatProvider provider )
		{
			return value.ToChar( provider );
		}


		public override decimal ToDecimal( IFormatProvider provider )
		{
			return value.ToDecimal( provider );
		}


		public override double ToDouble( IFormatProvider provider )
		{
			return value.ToDouble( provider );
		}


		public override short ToInt16( IFormatProvider provider )
		{
			return value.ToInt16( provider );
		}


		public override int ToInt32( IFormatProvider provider )
		{
			return value.ToInt32( provider );
		}


		public override long ToInt64( IFormatProvider provider )
		{
			return value.ToInt64( provider );
		}


		public override sbyte ToSByte( IFormatProvider provider )
		{
			return value.ToSByte( provider );
		}


		public override float ToSingle( IFormatProvider provider )
		{
			return value.ToSingle( provider );
		}


		public override string ToString( IFormatProvider provider )
		{
			return value.ToString( provider );
		}


		public override ushort ToUInt16( IFormatProvider provider )
		{
			return value.ToUInt16( provider );
		}


		public override uint ToUInt32( IFormatProvider provider )
		{
			return value.ToUInt32( provider );
		}


		public override ulong ToUInt64( IFormatProvider provider )
		{
			return value.ToUInt64( provider );
		}
	}
}

