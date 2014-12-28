using System;
using System.Collections.Generic;
using System.Globalization;
using TinyJSON;


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


	class BaseClass
	{
	}


	class TestClass : BaseClass
	{
		public string name;
		public TestEnum type;
		public List<TestStruct> data = new List<TestStruct>();

		#pragma warning disable 0414
		private int priv1;
		[Include] private int priv2;
		#pragma warning restore 0414

		int prop1 { get; set; }
		[Include] int prop2 { set { } }
		[Include] int prop3 { get; set; }

		[Exclude] public int _ignored;

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


	class Program
	{
		public static void Main( string[] args )
		{
			int[,,] array;
			var variant = JSON.Load( "[[[1,2],[3,4]],[[5,6],[7,8]],[[9,0],[1,2]]]" );
			JSON.MakeInto( variant, out array );
			Console.WriteLine( JSON.Dump( array ) );

			/*
			var array0 = new int[] { 1, 2, 3 };
			Console.WriteLine( JSON.Dump( array0 ) );

			var array1 = new int[,] { { 1, 2, 3 }, { 4, 5, 6 } };
			Console.WriteLine( JSON.Dump( array1 ) );

			var array2 = new int[,,] { { { 1, 2 }, { 3, 4 } }, { { 5, 6 }, { 7, 8 } }, { { 9, 0 }, { 1, 2 } } };
			Console.WriteLine( JSON.Dump( array2 ) );

			var array3 = new int[][] { new int[] { 1, 2, 3 }, new int[] { 4, 5, 6 } };
			Console.WriteLine( JSON.Dump( array3 ) );
			/**/

			/*
			var testClass = new TestClass();
			testClass.Init();
			testClass.data.Add( new TestStruct() { x = 1, y = 2 } );
			testClass.data.Add( new TestStruct() { x = 3, y = 4 } );
			testClass.data.Add( new TestStruct() { x = 5, y = 6 } );

			var testClassJson = JSON.Dump( (BaseClass) testClass, EncodeOptions.PrettyPrint );
			Console.WriteLine( testClassJson );

			testClass = JSON.Load( testClassJson ).Make<BaseClass>() as TestClass;
			Console.WriteLine( JSON.Dump( testClass, EncodeOptions.PrettyPrint ) );
			/**/

			/*
			var list = JSON.Load( "[1,2,3]" );
			foreach (var item in list as ProxyArray)
			{
				int number = item;
				Console.WriteLine( number );
			}

			var dict = JSON.Load( "{\"x\":1,\"y\":2}" );
			foreach (var pair in dict as ProxyObject)
			{
				float value = pair.Value;
				Console.WriteLine( pair.Key + " = " + value );
			}
			/**/
		}
	}
}

