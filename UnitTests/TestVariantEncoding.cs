using TinyJSON;
using NUnit.Framework;


[TestFixture]
public class TestVariantEncoding
{
	[Test]
	public void TestDumpProxyBoolean()
	{
		Assert.AreEqual( "true", new ProxyBoolean( true ).ToJSON() );
		Assert.AreEqual( "false", new ProxyBoolean( false ).ToJSON() );
	}


	[Test]
	public void TestDumpProxyNumber()
	{
		Assert.AreEqual( "12345", new ProxyNumber( 12345 ).ToJSON() );
		Assert.AreEqual( "12.34", new ProxyNumber( 12.34 ).ToJSON() );
	}


	[Test]
	public void TestDumpProxyString()
	{
		Assert.AreEqual( "\"string\"", new ProxyString( "string" ).ToJSON() );
	}


	[Test]
	public void TestDumpProxyArray()
	{
		Assert.AreEqual( "[1,true,\"three\"]", JSON.Load( "[1,true,\"three\"]" ).ToJSON() );
	}


	[Test]
	public void TestDumpProxyObject()
	{
		Assert.AreEqual( "{\"x\":1,\"y\":2}", JSON.Load( "{\"x\":1,\"y\":2}" ).ToJSON() );
	}
}
