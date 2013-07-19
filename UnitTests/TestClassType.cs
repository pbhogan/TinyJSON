using System;
using TinyJSON;
using NUnit.Framework;
using System.Collections.Generic;


[TestFixture]
public class TestClassType
{
	public static bool loadCallbackFired = false;

	class TestClass
	{
		public int x;
		public int y;

		[Skip] 
		public int z;

		public List<int> list;


		[Load]
		public void OnLoad()
		{
			TestClassType.loadCallbackFired = true;
		}
	}


	[Test]
	public void TestDumpClass()
	{
		var testClass = new TestClass() { x = 5, y = 7, z = 0 };
		testClass.list = new List<int>() { { 3 }, { 1 }, { 4 } };

		Assert.AreEqual( "{\"x\":5,\"y\":7,\"list\":[3,1,4]}", JSON.Dump( testClass ) );
	}


	[Test]
	public void TestLoadStruct()
	{
		TestClass testClass = JSON.Load( "{\"x\":5,\"y\":7,\"z\":3,\"list\":[3,1,4]}}" ).Make<TestClass>();

		Assert.AreEqual( 5, testClass.x );
		Assert.AreEqual( 7, testClass.y );
		Assert.AreEqual( 0, testClass.z ); // should not get assigned

		Assert.AreEqual( 3, testClass.list.Count );
		Assert.AreEqual( 3, testClass.list[0] );
		Assert.AreEqual( 1, testClass.list[1] );
		Assert.AreEqual( 4, testClass.list[2] );

		Assert.IsTrue( loadCallbackFired );
	}

}

