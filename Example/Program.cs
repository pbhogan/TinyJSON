using System;
using System.Collections.Generic;
using TinyJSON;


// Disable non-accessed field warning.
#pragma warning disable 414

// Disable field is never used warning.
#pragma warning disable 169

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global


namespace TestProgram
{
	enum TestEnum
	{
		Thing1,
		Thing2,
		Thing3
	}


	struct TestStruct
	{
		public int x;
		public int y;
	}


	class BaseClass {}


	class TestClass : BaseClass
	{
		public string name;
		public TestEnum type;

		[TypeHint]
		public List<TestStruct> data = new List<TestStruct>();

		int priv1;

		[Include]
		int priv2;

		// ReSharper disable once UnusedAutoPropertyAccessor.Local
		int prop1 { get; set; }

		[Include]
		// ReSharper disable once MemberCanBeMadeStatic.Local
		int prop2
		{
			// ReSharper disable once ValueParameterNotUsed
			set {}
		}

		[Include]
		// ReSharper disable once UnusedAutoPropertyAccessor.Local
		int prop3 { get; set; }

		[Exclude]
		public int _ignored;

		public void Init()
		{
			priv1 = 1;
			priv2 = 2;

			prop1 = 1;
			prop2 = 2;
			prop3 = 3;

			name = "Rumpelstiltskin Jones";
			type = TestEnum.Thing2;
		}


		[AfterDecode]
		public void AfterDecode()
		{
			Console.WriteLine( "AfterDecode callback fired!" );
		}
	}


	class ValueTypes
	{
		public Int16 i16 = 1;
		public UInt16 u16 = 2;
		public Int32 i32 = 3;
		public UInt32 u32 = 4;
		public Int64 i64 = 5;
		public UInt64 u64 = 6;
		public Single s = 7;
		public Double d = 8;
		public Decimal m = 9;
		public Boolean b = true;
	}


	static class Program
	{
		static void TestEnumStringDictionary()
		{
			var dict = new Dictionary<TestEnum, String>();
			dict[TestEnum.Thing1] = "Thing1";
			dict[TestEnum.Thing2] = "Thing2";
			dict[TestEnum.Thing3] = "Thing3";
			Console.WriteLine( JSON.Dump( dict ) );

			const string json = "{\"Thing1\":\"Thing1\",\"Thing2\":\"Thing2\",\"Thing3\":\"Thing3\"}";
			Console.WriteLine( json );

			dict = JSON.Load( json ).Make<Dictionary<TestEnum, String>>();
			Console.WriteLine( JSON.Dump( dict ) );
		}


		static void TestClasses()
		{
			var testClass = new TestClass();
			testClass.Init();
			testClass.data.Add( new TestStruct { x = 1, y = 2 } );
			testClass.data.Add( new TestStruct { x = 3, y = 4 } );
			testClass.data.Add( new TestStruct { x = 5, y = 6 } );

			Console.WriteLine( JSON.Dump( testClass, EncodeOptions.PrettyPrint ) );

			var baseClass = (BaseClass) testClass;
			var baseClassJson = JSON.Dump( baseClass, EncodeOptions.PrettyPrint );
			Console.WriteLine( baseClassJson );

			testClass = JSON.Load( baseClassJson ).Make<BaseClass>() as TestClass;
			Console.WriteLine( JSON.Dump( testClass, EncodeOptions.PrettyPrint ) );
		}


		static void TestArrays()
		{
			const string json = "[[[1,2],[3,4]],[[5,6],[7,8]],[[9,0],[1,2]]]";
			Console.WriteLine( json );

			int[,,] array;
			JSON.MakeInto( JSON.Load( json ), out array );
			Console.WriteLine( JSON.Dump( array ) );

			var array0 = new[] { 1, 2, 3 };
			Console.WriteLine( JSON.Dump( array0 ) );

			var array1 = new[,] { { 1, 2, 3 }, { 4, 5, 6 } };
			Console.WriteLine( JSON.Dump( array1 ) );

			var array2 = new[,,] { { { 1, 2 }, { 3, 4 } }, { { 5, 6 }, { 7, 8 } }, { { 9, 0 }, { 1, 2 } } };
			Console.WriteLine( JSON.Dump( array2 ) );

			var array3 = new[] { new[] { 1, 2, 3 }, new[] { 4, 5, 6 } };
			Console.WriteLine( JSON.Dump( array3 ) );
		}


		static void TestIteratingProxyArray()
		{
			var list = JSON.Load( "[1,2,3]" );
			foreach (var item in (ProxyArray) list)
			{
				int number = item;
				Console.WriteLine( number );
			}
		}


		static void TestIteratingProxyObject()
		{
			var dict = JSON.Load( "{\"x\":1,\"y\":2}" );
			foreach (var pair in (ProxyObject) dict)
			{
				float value = pair.Value;
				Console.WriteLine( pair.Key + " = " + value );
			}
		}


		public static void Main()
		{
			// TestClasses();
			// TestArrays();
			TestEnumStringDictionary();
			// TestIteratingProxyArray();
			// TestIteratingProxyObject();
		}
	}
}
