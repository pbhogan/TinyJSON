using TinyJSON;
using NUnit.Framework;


// Disable field is never assigned warning.
#pragma warning disable 649

// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local


[TestFixture]
public class TestDecodeAlias
{
	class AliasData
	{
		[DecodeAlias( "numberFieldAlias" )]
		public int NumberField;

		[Include]
		[DecodeAlias( "NumberPropertyAlias" )]
		public int NumberProperty { get; set; }

		[DecodeAlias( "anotherNumberFieldAliasOne", "anotherNumberFieldAliasTwo" )]
		public int AnotherNumberField;

		[DecodeAlias( "AnotherNumberPropertyAliasOne" )]
		[DecodeAlias( "AnotherNumberPropertyAliasTwo" )]
		public int AnotherNumberProperty;
	}


	[Test]
	public void TestLoadAlias()
	{
		const string json = "{ \"numberFieldAlias\" : 1, \"NumberPropertyAlias\" : 2, \"anotherNumberFieldAliasOne\" : 3, \"anotherNumberFieldAliasTwo\" : 4, \"AnotherNumberPropertyAliasOne\" : 5, \"AnotherNumberPropertyAliasTwo\" : 6 }";
		var aliasData = JSON.Load( json ).Make<AliasData>();

		Assert.AreEqual( 1, aliasData.NumberField );
		Assert.AreEqual( 2, aliasData.NumberProperty );
		Assert.IsTrue( aliasData.AnotherNumberField == 3 || aliasData.AnotherNumberField == 4 );
		Assert.IsTrue( aliasData.AnotherNumberProperty == 5 || aliasData.AnotherNumberProperty == 6 );
	}
}
