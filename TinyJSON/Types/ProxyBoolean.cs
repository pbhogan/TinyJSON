using System;


namespace TinyJSON
{
	public sealed class ProxyBoolean : Variant
	{
		readonly bool value;


		public ProxyBoolean( bool value )
		{
			this.value = value;
		}


		public override bool ToBoolean( IFormatProvider provider )
		{
			return value;
		}
	}
}
