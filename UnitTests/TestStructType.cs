using System;
using TinyJSON;
using NUnit.Framework;
using System.Collections.Generic;


[TestFixture]
public class TestStructType
{
	public static bool loadCallbackFired = false;

	struct TestStruct
	{
		public int x;
		public int y;

		[Skip] 
		public int z;

		[Load]
		public void OnLoad()
		{
			TestStructType.loadCallbackFired = true;
		}
	}


	[Test]
	public void TestDumpStruct()
	{
		var testStruct = new TestStruct() { x = 5, y = 7, z = 0 };

		Assert.AreEqual( "{\"x\":5,\"y\":7}", JSON.Dump( testStruct ) );
	}


	[Test]
	public void TestLoadStruct()
	{
		TestStruct testStruct = JSON.Load( "{\"x\":5,\"y\":7,\"z\":3}" ).Make<TestStruct>();

		Assert.AreEqual( 5, testStruct.x );
		Assert.AreEqual( 7, testStruct.y );
		Assert.AreEqual( 0, testStruct.z ); // should not get assigned

		Assert.IsTrue( loadCallbackFired );
	}

}

