using System;
using System.Collections;
using System.Collections.Generic;


namespace TinyJSON
{
	public sealed class ProxyObject : Variant, IEnumerable<KeyValuePair<string, Variant>>
	{
		private Dictionary<string, Variant> dict;


		public ProxyObject()
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


		public int Count
		{
			get { return dict.Count; }
		}
	}
}

