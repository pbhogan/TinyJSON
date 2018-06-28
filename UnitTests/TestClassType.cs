using System;
using TinyJSON;
using NUnit.Framework;
using System.Collections.Generic;


[TestFixture]
public class TestClassType
{
	public static bool afterDecodeCallbackFired = false;
	public static bool beforeEncodeCallbackFired = false;


	class TestClass
	{
		public int x;
		public int y;

		[Exclude]
		public int z;

		public List<int> list;

		public int p1 { get; set; }
		public int p2 { get; private set; }
		public int p3 { get; }


		public TestClass()
		{
			p1 = 1;
			p2 = 2;
			p3 = 3;
		}


		[AfterDecode]
		public void AfterDecode()
		{
			TestClassType.afterDecodeCallbackFired = true;
		}


		[BeforeEncode]
		public void BeforeDecode()
		{
			TestClassType.beforeEncodeCallbackFired = true;
		}
	}


	[Test]
	public void TestDumpClass()
	{
		var testClass = new TestClass() { x = 5, y = 7, z = 0 };
		testClass.list = new List<int>() { { 3 }, { 1 }, { 4 } };

		Assert.AreEqual( "{\"@type\":\"" + testClass.GetType().FullName + "\",\"x\":5,\"y\":7,\"list\":[3,1,4]}", JSON.Dump( testClass ) );

		Assert.IsTrue( beforeEncodeCallbackFired );
	}


	[Test]
	public void TestDumpClassNoTypeHint()
	{
		var testClass = new TestClass() { x = 5, y = 7, z = 0 };
		testClass.list = new List<int>() { { 3 }, { 1 }, { 4 } };

		Assert.AreEqual( "{\"x\":5,\"y\":7,\"list\":[3,1,4]}", JSON.Dump( testClass, EncodeOptions.NoTypeHints ) );
	}


	[Test]
	public void TestDumpClassPrettyPrint()
	{
		var testClass = new TestClass() { x = 5, y = 7, z = 0 };
		testClass.list = new List<int>() { { 3 }, { 1 }, { 4 } };

		Assert.AreEqual( @"{
	""x"": 5,
	""y"": 7,
	""list"": [
		3,
		1,
		4
	]
}", JSON.Dump( testClass, EncodeOptions.PrettyPrint | EncodeOptions.NoTypeHints ) );
	}


	[Test]
	public void TestDumpClassIncludePublicProperties()
	{
		var testClass = new TestClass() { x = 5, y = 7, z = 0 };
		Console.WriteLine( JSON.Dump( testClass, EncodeOptions.NoTypeHints | EncodeOptions.IncludePublicProperties ) );
		Assert.AreEqual( "{\"x\":5,\"y\":7,\"list\":null,\"p1\":1,\"p2\":2,\"p3\":3}", JSON.Dump( testClass, EncodeOptions.NoTypeHints | EncodeOptions.IncludePublicProperties ) );
	}


	[Test]
	public void TestLoadClass()
	{
		TestClass testClass = JSON.Load( "{\"x\":5,\"y\":7,\"z\":3,\"list\":[3,1,4],\"p1\":1,\"p2\":2,\"p3\":3}" ).Make<TestClass>();

		Assert.AreEqual( 5, testClass.x );
		Assert.AreEqual( 7, testClass.y );
		Assert.AreEqual( 0, testClass.z ); // should not get assigned

		Assert.AreEqual( 3, testClass.list.Count );
		Assert.AreEqual( 3, testClass.list[0] );
		Assert.AreEqual( 1, testClass.list[1] );
		Assert.AreEqual( 4, testClass.list[2] );

		Assert.AreEqual( 1, testClass.p1 );
		Assert.AreEqual( 2, testClass.p2 );
		Assert.AreEqual( 3, testClass.p3 );

		Assert.IsTrue( afterDecodeCallbackFired );
	}


	class InnerClass
	{
	}

	class OuterClass
	{
		[TypeHint]
		public InnerClass inner;
	}

	[Test]
	public void TestDumpOuterNoTypeHint()
	{
		var outerClass = new OuterClass();
		outerClass.inner = new InnerClass();
		Assert.AreEqual( "{\"inner\":{\"@type\":\"" + typeof( InnerClass ).FullName + "\"}}", JSON.Dump( outerClass, EncodeOptions.NoTypeHints ) );
	}


	class AliasData
	{
		[DecodeAlias("numberFieldAlias")]
		public int numberField;

		[Include]
		[DecodeAlias("NumberPropertyAlias")]
		public int NumberProperty { get; set; }

		[DecodeAlias("anotherNumberFieldAliasOne", "anotherNumberFieldAliasTwo")]
		public int anotherNumberField;

		[DecodeAlias("AnotherNumberPropertyAliasOne")]
		[DecodeAlias("AnotherNumberPropertyAliasTwo")]
		public int AnotherNumberProperty;
	}

	[Test]
	public void TestLoadAlias() {
		var json = "{ \"numberFieldAlias\" : 1, \"NumberPropertyAlias\" : 2, \"anotherNumberFieldAliasOne\" : 3, \"anotherNumberFieldAliasTwo\" : 4, \"AnotherNumberPropertyAliasOne\" : 5, \"AnotherNumberPropertyAliasTwo\" : 6 }";
		var aliasData = JSON.Load( json ).Make<AliasData>();

		Assert.AreEqual( 1, aliasData.numberField );
		Assert.AreEqual( 2, aliasData.NumberProperty );
		Assert.IsTrue( aliasData.anotherNumberField == 3 || aliasData.anotherNumberField == 4 );
		Assert.IsTrue( aliasData.AnotherNumberProperty == 5 || aliasData.AnotherNumberProperty == 6 );
	}
}

