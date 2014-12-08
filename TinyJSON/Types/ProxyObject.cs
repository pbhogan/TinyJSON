using System;
using System.Collections;
using System.Collections.Generic;


namespace TinyJSON
{
	public sealed class ProxyObject : Variant, IEnumerable<KeyValuePair<string, Variant>>
	{
		internal const string TypeHintName = "@type";
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


		public bool TryGetValue( string key, out Variant item )
		{
			return dict.TryGetValue( key, out item );
		}


		public string TypeHint
		{
			get
			{
				Variant item;
				if (TryGetValue( TypeHintName, out item ))
				{
					return item.ToString();
				}
				return null;
			}
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

