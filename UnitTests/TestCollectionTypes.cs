using System;
using TinyJSON;
using NUnit.Framework;
using System.Collections.Generic;


[TestFixture]
public class TestCollectionTypes
{
	[Test]
	public void TestDumpArray()
	{
		var array = new int[] { 3, 1, 4 };
		Assert.AreEqual( "[3,1,4]", JSON.Dump( array ) );
	}


	[Test]
	public void TestLoadArray()
	{
		int[] array;
		var variant = JSON.Load( "[3,1,4]" );
		JSON.MakeInto( variant, out array );
		Assert.AreNotEqual( null, array );

		Assert.AreEqual( 3, array.Length );
		Assert.AreEqual( 3, array[0] );
		Assert.AreEqual( 1, array[1] );
		Assert.AreEqual( 4, array[2] );
	}


	[Test]
	public void TestDumpList()
	{
		var list = new List<int> () { { 3 }, { 1 }, { 4 } };
		Assert.AreEqual( "[3,1,4]", JSON.Dump( list ) );
	}


	[Test]
	public void TestLoadList()
	{
		List<int> list;
		var variant = JSON.Load( "[3,1,4]" );
		JSON.MakeInto( variant, out list );
		Assert.AreNotEqual( null, list );

		Assert.AreEqual( 3, list.Count );
		Assert.AreEqual( 3, list[0] );
		Assert.AreEqual( 1, list[1] );
		Assert.AreEqual( 4, list[2] );
	}


	[Test]
	public void TestDumpDict()
	{
		var dict = new Dictionary<string, float>();
		dict["foo"] = 1337f;
		dict["bar"] = 3.14f;

		Assert.AreEqual( "{\"foo\":1337,\"bar\":3.14}", JSON.Dump( dict ) );
	}


	[Test]
	public void TestLoadDict()
	{
		var variant = JSON.Load( "{\"foo\":1337,\"bar\":3.14}" );

		Dictionary<string, float> dict;
		JSON.MakeInto( variant, out dict );

		Assert.AreNotEqual( null, dict );
		Assert.AreEqual( 2, dict.Count );
		Assert.AreEqual( 1337f, (float) dict["foo"] );
		Assert.AreEqual( 3.14f, (float) dict["bar"] );
	}


	[Test]
	public void TestLoadDictIntoProxy()
	{
		var variant = JSON.Load( "{\"foo\":1337,\"bar\":3.14}" );
		var proxy = variant as ProxyObject;

		Assert.AreNotEqual( null, proxy );
		Assert.AreEqual( 2, proxy.Count );
		Assert.AreEqual( 1337f, (float) proxy["foo"] );
		Assert.AreEqual( 3.14f, (float) proxy["bar"] );
	}


	enum TestEnum
	{
		Thing1,
		Thing2,
		Thing3
	}

	[Test]
	public void TestDumpEnum()
	{
		var testEnum = TestEnum.Thing2;
		Assert.AreEqual( "\"Thing2\"", JSON.Dump( testEnum ) );
	}


	public void TestLoadEnum()
	{
		TestEnum testEnum;

		testEnum = JSON.Load( "\"Thing2\"" ).Make<TestEnum>();
		Assert.AreEqual( TestEnum.Thing2, testEnum );

		testEnum = JSON.Load( "\"Thing4\"" ).Make<TestEnum>();
		Assert.AreEqual( null, testEnum );
	}

}

