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


	class TestClass
	{
		public int i;
		public float f;
		public double d;
		public string s;
		public bool b;

		[Skip]
		public int h; // ignored by TinyJSON

		public List<int> l;

		public static TestClass New()
		{
			var data = new TestClass();
			data.i = 5;
			data.f = 3.14f;
			data.d = 1.23456789;
			data.s = "OHAI!";
			data.b = true;
			data.h = 7;
			data.l = new List<int>() { { 0 }, { 1 }, { 2 } };
			return data;
		}

		[Load]
		private void OnLoad()
		{
			Console.WriteLine( "TestData.OnLoad() called!" );
		}
	}


	struct TestStruct
	{
		public int x;
		public int y;
	}


	class Program
	{
		public static void Main( string [] args )
		{
			{
				var variant = JSON.Load( "{\"foo\": 1, \"bar\": 2.34}" );
				int i = variant["foo"];
				float f = variant["bar"];
				Console.WriteLine( i );
				Console.WriteLine( f );
			}

			{
				var variant = new List<int> () { { 0 }, { 1 }, { 2 } };
				Console.WriteLine( JSON.Dump( variant ));
			}

			{
				var testClass = TestClass.New();
				var testClassJson = JSON.Dump( testClass, true );
				Console.WriteLine( testClassJson );
			
				JSON.Load( testClassJson ).Make( out testClass );
				Console.WriteLine( JSON.Dump( testClass ));

				testClass = JSON.Load( testClassJson ).Make<TestClass>();
				Console.WriteLine( JSON.Dump( testClass ));
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
				var variant = JSON.Load( "[1,2,3]" );
				JSON.MakeInto( variant, out list );
				Console.WriteLine( "[" + String.Join( ", ", list ) + "]" );
			}

			{
				TestEnum testEnum = JSON.Load( "\"Thing1\"" ).Make<TestEnum>();
				Console.WriteLine( testEnum );
			}

			{
				var testStruct = new TestStruct();
				testStruct.x = 1;
				testStruct.y = 2;

				var testStructJson = JSON.Dump( testStruct );
				Console.WriteLine( testStructJson );

				testStruct = JSON.Load( testStructJson ).Make<TestStruct>();
				Console.WriteLine( testStruct.x + " should be 1" );
				Console.WriteLine( testStruct.y + " should be 2" );
			}

			// Variant enumerators might not be a good idea and are not fully implemented yet.
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

