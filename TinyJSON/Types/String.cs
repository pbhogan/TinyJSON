using System;


namespace TinyJSON.Proxy
{
	public sealed class String : Variant
	{
		private string value;


		public String( string value )
		{
			this.value = value;
		}


		public override string ToString( IFormatProvider provider )
		{
			return value;
		}
	}
}

