using TinyJSON;
using NUnit.Framework;


// Disable field is assigned by never used warning.
#pragma warning disable 414

// ReSharper disable UnusedAutoPropertyAccessor.Local


[TestFixture]
public class TestEnforceHeirarchyOrder
{
	class ClassA
	{
		public int FieldA;
		public int PropertyA { get; set; }
	}

	class ClassB : ClassA
	{
		public int FieldB;
		public int PropertyB { get; set; }
	}

	class ClassC : ClassB
	{
		public int FieldC;
		public int PropertyC { get; set; }
	}


	[Test]
	public void TestEncodeWithEnforceHeirarchyOrderEnabled()
	{
		var testClass = new ClassC { FieldA = 1, FieldB = 2, FieldC = 3, PropertyA = 4, PropertyB = 5, PropertyC = 6 };
		var json = JSON.Dump( testClass, EncodeOptions.NoTypeHints | EncodeOptions.IncludePublicProperties | EncodeOptions.EnforceHeirarchyOrder );
		Assert.AreEqual( "{\"FieldA\":1,\"FieldB\":2,\"FieldC\":3,\"PropertyA\":4,\"PropertyB\":5,\"PropertyC\":6}", json );
	}
}
