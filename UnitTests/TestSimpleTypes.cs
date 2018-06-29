using System;
using System.Collections.Generic;
using System.Globalization;
using TinyJSON;
using NUnit.Framework;


// Disable field is never used warning.
#pragma warning disable 169

// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable InconsistentNaming


[TestFixture]
public class TestSimpleTypes
{
	[Test]
	public void TestDumpBool()
	{
		Assert.AreEqual( "true", JSON.Dump( true ) );
		Assert.AreEqual( "false", JSON.Dump( false ) );
	}


	[Test]
	public void TestLoadBool()
	{
		Assert.AreEqual( true, (Boolean) JSON.Load( "true" ) );
		Assert.AreEqual( false, (Boolean) JSON.Load( "false" ) );
	}


	[Test]
	public void TestDumpIntegerTypes()
	{
		Assert.AreEqual( "-12345", JSON.Dump( (Int16) (-12345) ) );
		Assert.AreEqual( "-12345", JSON.Dump( (Int32) (-12345) ) );
		Assert.AreEqual( "-12345", JSON.Dump( (Int64) (-12345) ) );

		Assert.AreEqual( "12345", JSON.Dump( (UInt16) 12345 ) );
		Assert.AreEqual( "12345", JSON.Dump( (UInt32) 12345 ) );
		Assert.AreEqual( "12345", JSON.Dump( (UInt64) 12345 ) );
	}


	[Test]
	public void TestLoadIntegerTypes()
	{
		Assert.AreEqual( -12345, (Int32) JSON.Load( "-12345" ) );
	}


	[Test]
	public void TestDumpFloatTypes()
	{
		Assert.AreEqual( "123.45", JSON.Dump( (Single) 123.45 ) );
		Assert.AreEqual( "123.45", JSON.Dump( (Double) 123.45 ) );
	}


	[Test]
	public void TestDumpFloatTypesForGermanCulture()
	{
		var currentCulture = CultureInfo.CurrentCulture;
		CultureInfo.CurrentCulture = new CultureInfo( "de", false );
		Assert.AreEqual( "123.45", JSON.Dump( (Single) 123.45 ) );
		Assert.AreEqual( "123.45", JSON.Dump( (Double) 123.45 ) );
		CultureInfo.CurrentCulture = currentCulture;
	}


	[Test]
	public void TestDumpDecimalType()
	{
		Assert.AreEqual( "79228162514264337593543950335", JSON.Dump( Decimal.MaxValue ) );
		Assert.AreEqual( "-79228162514264337593543950335", JSON.Dump( Decimal.MinValue ) );
	}


	[Test]
	public void TestLoadFloatTypes()
	{
		Assert.AreEqual( 123.45f, (Single) JSON.Load( "123.45" ) );
	}


	[Test]
	public void TestDumpString()
	{
		Assert.AreEqual( "\"OHAI! Can haz ball of strings?\"", JSON.Dump( "OHAI! Can haz ball of strings?" ) );
		Assert.AreEqual( "\"\\\"\"", JSON.Dump( "\"" ) );
		Assert.AreEqual( "\"\\\\\"", JSON.Dump( "\\" ) );
		Assert.AreEqual( "\"\\b\"", JSON.Dump( "\b" ) );
		Assert.AreEqual( "\"\\f\"", JSON.Dump( "\f" ) );
		Assert.AreEqual( "\"\\n\"", JSON.Dump( "\n" ) );
		Assert.AreEqual( "\"\\r\"", JSON.Dump( "\r" ) );
		Assert.AreEqual( "\"\\t\"", JSON.Dump( "\t" ) );
		Assert.AreEqual( "\"c\"", JSON.Dump( 'c' ) );
	}


	[Test]
	public void TestLoadString()
	{
		Assert.AreEqual( "OHAI! Can haz ball of strings?", (String) JSON.Load( "\"OHAI! Can haz ball of strings?\"" ) );
		Assert.AreEqual( "\"", (String) JSON.Load( "\"\\\"\"" ) );
		Assert.AreEqual( "\\", (String) JSON.Load( "\"\\\\\"" ) );
		Assert.AreEqual( "\b", (String) JSON.Load( "\"\\b\"" ) );
		Assert.AreEqual( "\f", (String) JSON.Load( "\"\\f\"" ) );
		Assert.AreEqual( "\n", (String) JSON.Load( "\"\\n\"" ) );
		Assert.AreEqual( "\r", (String) JSON.Load( "\"\\r\"" ) );
		Assert.AreEqual( "\t", (String) JSON.Load( "\"\\t\"" ) );
	}


	[Test]
	public void TestDumpNull()
	{
		List<int> list = null;
		Assert.AreEqual( "null", JSON.Dump( list ) );
		Assert.AreEqual( "null", JSON.Dump( null ) );
	}


	[Test]
	public void TestLoadNull()
	{
		Assert.AreEqual( null, JSON.Load( "null" ) );
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


	[Test]
	// ReSharper disable once InconsistentNaming
	public void TestAOTCompatibility()
	{
		ValueTypes item;
		const string json = "{\"i16\":1,\"u16\":2,\"i32\":3,\"u32\":4,\"i64\":5,\"u64\":6,\"s\":7,\"d\":8,\"m\":9,\"b\":true}";
		var data = JSON.Load( json );
		Assert.DoesNotThrow( () => data.Make<ValueTypes>() );
		Assert.DoesNotThrow( () => JSON.MakeInto( data, out item ) );
	}
}
