using System;
using System.Collections;
using System.Collections.Generic;


namespace TinyJSON.Proxy
{
	public sealed class Array : Variant, IEnumerable<Variant>
	{
		private List<Variant> list;


		public Array()
		{
			list = new List<Variant>();
		}


		IEnumerator<Variant> IEnumerable<Variant>.GetEnumerator()
		{
			return list.GetEnumerator();
		}


		IEnumerator IEnumerable.GetEnumerator()
		{
			return list.GetEnumerator();
		}


		public void Add( Variant item )
		{
			list.Add( item );
		}


		public override Variant this[ int index ]
		{
			get { return list[index]; }
			set { list[index] = value; }
		}
	}
}

