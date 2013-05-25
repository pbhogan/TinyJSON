using System;
using System.Collections;
using System.Collections.Generic;


namespace TinyJSON.Proxy
{
	public sealed class Object : Variant, IEnumerable<KeyValuePair<string, Variant>>
	{
		private Dictionary<string, Variant> dict;


		public Object()
		{
			dict = new Dictionary<string, Variant>();
		}


		IEnumerator<KeyValuePair<string, Variant>> IEnumerable<KeyValuePair<string, Variant>>.GetEnumerator()
		{
			return dict.GetEnumerator();
		}


		IEnumerator IEnumerable.GetEnumerator()
		{
			return dict.GetEnumerator();
		}


		public void Add( string key, Variant item )
		{
			dict.Add( key, item );
		}


		public override Variant this[ string key ]
		{
			get { return dict[key]; }
			set { dict[key] = value; }
		}
	}
}

