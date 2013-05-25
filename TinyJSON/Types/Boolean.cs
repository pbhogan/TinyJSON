using System;


namespace TinyJSON.Proxy
{
	public sealed class Boolean : Variant
	{
		private bool value;


		public Boolean( bool value )
		{
			this.value = value;
		}


		public override bool ToBoolean( IFormatProvider provider )
		{
			return value;
		}
	}
}

