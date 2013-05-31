using System.Collections.Generic;
using System;
using TinyJSON;

namespace TinyJSON
{
	enum TestEnum
	{
		Thing1,
		Thing2,
		Thing3
	}


	class TestData
	{
		public int i;
		public float f;
		public double d;
		public string s;
		public bool b;

		[Skip]
		public int h; // ignored by TinyJSON

		public List<int> l;

		public static TestData New()
		{
			var data = new TestData();
			data.i = 5;
			data.f = 3.14f;
			data.d = 1.23456789;
			data.s = "OHAI!";
			data.b = true;
			data.h = 7;
			data.l = new List<int>() { { 0 }, { 1 }, { 2 } };
			return data;
		}
	}


	class Program
	{
		public static void Main( string [] args )
		{
			{
				var data = JSON.Load( "{\"foo\": 1, \"bar\": 2.34}" );
				int i = data["foo"];
				float f = data["bar"];
				Console.WriteLine( i );
				Console.WriteLine( f );
			}

			{
				var data = new List<int> () { { 0 }, { 1 }, { 2 } };
				Console.WriteLine( JSON.Dump( data ));
			}

			string json;
			{
				var data = TestData.New();
				json = JSON.Dump( data, true );
				Console.WriteLine( json );
			}

			{
				TestData data;
				JSON.Load( json ).Make( out data );
			}

			{
				var data = JSON.Load( json ).Make<TestData>();
				Console.WriteLine( JSON.Dump( data ));
			}

			{
				Dictionary<string, float> dict;
				var data = JSON.Load( "{\"foo\": 1, \"bar\": 2.34}" );
				JSON.MakeInto( data, out dict );
				Console.WriteLine( dict["foo"] );
				Console.WriteLine( dict["bar"] );
			}
			
			{
				List<int> list;
				var data = JSON.Load( "[1,2,3]" );
				JSON.MakeInto( data, out list );
				Console.WriteLine( "[" + String.Join( ", ", list ) + "]" );
			}

			{
				TestEnum testEnum = JSON.Load( "\"Thing1\"" ).Make<TestEnum>();
				Console.WriteLine( testEnum );
			}

			// Variant enumerators might not be a good idea and are not implemented yet.
//			{
//				var data = JSON.Load( "[1,2,3]" );
//				foreach (var item in data)
//				{
//					Console.WriteLine( item as string );
//				}
//			}
		}
	}
}

