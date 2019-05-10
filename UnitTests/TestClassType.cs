using System;
using System.Collections.Generic;
using TinyJSON;
using NUnit.Framework;


// ReSharper disable NotAccessedField.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable MemberCanBePrivate.Local
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local
// ReSharper disable InconsistentNaming


[TestFixture]
public class TestClassType
{
	public static bool AfterDecodeCallbackFired;
	public static bool BeforeEncodeCallbackFired;


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
			AfterDecodeCallbackFired = true;
		}


		[BeforeEncode]
		public void BeforeDecode()
		{
			BeforeEncodeCallbackFired = true;
		}
	}


	[Test]
	public void TestDumpClass()
	{
		var testClass = new TestClass { x = 5, y = 7, z = 0 };
		testClass.list = new List<int> { 3, 1, 4 };

		Assert.AreEqual( "{\"" + ProxyObject.TypeHintKey + "\":\"" + testClass.GetType().FullName + "\",\"x\":5,\"y\":7,\"list\":[3,1,4]}", JSON.Dump( testClass ) );

		Assert.IsTrue( BeforeEncodeCallbackFired );
	}


	[Test]
	public void TestDumpClassNoTypeHint()
	{
		var testClass = new TestClass { x = 5, y = 7, z = 0 };
		testClass.list = new List<int> { 3, 1, 4 };

		Assert.AreEqual( "{\"x\":5,\"y\":7,\"list\":[3,1,4]}", JSON.Dump( testClass, EncodeOptions.NoTypeHints ) );
	}


	[Test]
	public void TestDumpClassPrettyPrint()
	{
		var testClass = new TestClass { x = 5, y = 7, z = 0 };
		testClass.list = new List<int> { 3, 1, 4 };

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
		var testClass = new TestClass { x = 5, y = 7, z = 0 };
		//Console.WriteLine( JSON.Dump( testClass, EncodeOptions.NoTypeHints | EncodeOptions.IncludePublicProperties ) );
		Assert.AreEqual( "{\"x\":5,\"y\":7,\"list\":null,\"p1\":1,\"p2\":2,\"p3\":3}", JSON.Dump( testClass, EncodeOptions.NoTypeHints | EncodeOptions.IncludePublicProperties ) );
	}


	[Test]
	public void TestLoadClass()
	{
		var testClass = JSON.Load( "{\"x\":5,\"y\":7,\"z\":3,\"list\":[3,1,4],\"p1\":1,\"p2\":2,\"p3\":3}" ).Make<TestClass>();

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

		Assert.IsTrue( AfterDecodeCallbackFired );
	}


	class InnerClass {}

	class OuterClass
	{
		public InnerClass inner;
	}

	[Test]
	public void TestDumpOuterClassWithNoTypeHintPropagatesToInnerClasses()
	{
		var outerClass = new OuterClass();
		outerClass.inner = new InnerClass();
		Assert.AreEqual( "{\"inner\":{}}", JSON.Dump( outerClass, EncodeOptions.NoTypeHints ) );
	}


	class OuterClassForceInnerTypeHint
	{
		[TypeHint]
		public InnerClass inner;
	}

	[Test]
	public void TestDumpOuterClassWithForcedInnerTypeHint()
	{
		var outerClass = new OuterClassForceInnerTypeHint();
		outerClass.inner = new InnerClass();
		Assert.AreEqual( "{\"inner\":{\"" + ProxyObject.TypeHintKey + "\":\"" + typeof(InnerClass).FullName + "\"}}", JSON.Dump( outerClass, EncodeOptions.NoTypeHints ) );
	}
}
