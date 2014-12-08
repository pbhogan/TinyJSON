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

		[Skip] 
		public int _ignored;

		[Load]
		public void OnLoad()
		{
			Console.WriteLine( "Load callback fired!" );
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
			var testClass = new TestClass();
			testClass.name = "Rumpelstiltskin Jones";
			testClass.type = TestEnum.Thing2;
			testClass.data.Add( new TestStruct() { x = 1, y = 2 } );
			testClass.data.Add( new TestStruct() { x = 3, y = 4 } );
			testClass.data.Add( new TestStruct() { x = 5, y = 6 } );

			var testClassJson = JSON.Dump( (BaseClass) testClass, EncodeOptions.PrettyPrint | EncodeOptions.TypeHint );
			Console.WriteLine( testClassJson );

			testClass = JSON.Load( testClassJson ).Make<BaseClass>() as TestClass;
			Console.WriteLine( JSON.Dump( testClass, EncodeOptions.PrettyPrint | EncodeOptions.TypeHint ) );

			/*
			// Iterating over variants:

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

