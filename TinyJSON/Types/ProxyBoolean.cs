using System;


namespace TinyJSON
{
	public sealed class ProxyBoolean : Variant
	{
		private bool value;


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

