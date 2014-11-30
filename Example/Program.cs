using System.Collections.Generic;
using System;
using TinyJSON;


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


class TestClass
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

		var testClassJson = JSON.Dump( testClass, true );
		Console.WriteLine( testClassJson );

		testClass = JSON.Load( testClassJson ).Make<TestClass>();
		Console.WriteLine( JSON.Dump( testClass ) );


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
	}
}

