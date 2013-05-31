using System;
using System.Collections;
using System.Collections.Generic;


namespace TinyJSON
{
	public sealed class ProxyArray : Variant, IEnumerable<Variant>
	{
		private List<Variant> list;


		public ProxyArray()
		{
			list = new List<Variant>();
		}


//		protected override IEnumerator<Variant> _GetEnumerator()
//		{
//			return list.GetEnumerator();
//		}


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

