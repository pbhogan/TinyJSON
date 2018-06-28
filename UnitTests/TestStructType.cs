using TinyJSON;
using NUnit.Framework;


// ReSharper disable InconsistentNaming


[TestFixture]
public class TestStructType
{
	public static bool LoadCallbackFired;

	struct TestStruct
	{
		public int x;
		public int y;

		[Exclude]
		public int z;

		[AfterDecode]
		// ReSharper disable once UnusedMember.Local
		public void OnLoad()
		{
			LoadCallbackFired = true;
		}
	}


	[Test]
	public void TestDumpStruct()
	{
		var testStruct = new TestStruct { x = 5, y = 7, z = 0 };

		Assert.AreEqual( "{\"" + ProxyObject.TypeHintKey + "\":\"" + testStruct.GetType().FullName + "\",\"x\":5,\"y\":7}", JSON.Dump( testStruct ) );
	}


	[Test]
	public void TestLoadStruct()
	{
		var testStruct = JSON.Load( "{\"x\":5,\"y\":7,\"z\":3}" ).Make<TestStruct>();

		Assert.AreEqual( 5, testStruct.x );
		Assert.AreEqual( 7, testStruct.y );
		Assert.AreEqual( 0, testStruct.z ); // should not get assigned

		Assert.IsTrue( LoadCallbackFired );
	}
}
