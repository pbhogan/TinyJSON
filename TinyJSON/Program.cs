using System;
using System.Collections.Generic;
using System.Reflection;
using System.Globalization;
using System.Collections;
using System.Linq;
using System.Text;


namespace TinyJSON
{
	class Data
	{
		public int i;
		public float f;
		public double d;
		public string s;
		public bool b;

		[Skip]
		public int h; // ignored by TinyJSON

		public List<int> l;

		public static Data New()
		{
			var data = new Data();
			data.i = 5;
			data.f = 3.14f;
			data.d = 1.23456789;
			data.s = "OHAI!";
			data.b = true;
			data.h = 7;
			data.l = new List<int>() { { 0 }, { 1 }, { 2 } };
			return data;
		}
	}


	class MainClass
	{
		public static void Main( string[] args )
		{
//			var data = JSON.Load( "{\"foo\": 1, \"bar\": 2.34}" );
//			int i = data["foo"];
//			float f = data["bar"];
//			Console.WriteLine( i );
//			Console.WriteLine( f );

//			var data = new List<int>() { { 0 }, { 1 }, { 2 } };
//			Console.WriteLine( JSON.Dump( data ) );
		
			var data = Data.New();
			var json = JSON.Dump( data, true );
			Console.WriteLine( json );

//			Data newData;
//			JSON.Load( json ).Make( out newData );
			var newData = JSON.Load( json ).Make<Data>();
			Console.WriteLine( JSON.Dump( newData ) );

//			Dictionary<string,float> dict;
//			var data = JSON.Load( "{\"foo bar\": 1, \"3.14\": 2.34}" );
//			Make( data, out dict );
//			Console.WriteLine( dict.Count );


//			List<int> list;
//			var data = JSON.Load( "[1,2,3]" );
//			Make( data, out list );
//			Console.WriteLine( "[" + String.Join( ", ", list ) + "]" );

			/*
			var jsonString = "{ \"array\": [1.44,2,3], " +
			                 "\"object\": {\"key1\":\"value1\", \"key2\":256}, " +
			                 "\"string\": \"The quick brown fox \\\"jumps\\\" over the lazy dog \", " +
			                 "\"unicode\": \"\\u3041 Men\u00fa sesi\u00f3n\", " +
			                 "\"int\": 65536, " +
			                 "\"float\": 3.1415926, " +
			                 "\"bool\": true, " +
			                 "\"null\": null }";

			var dict = JSON.Load( jsonString );

			Console.WriteLine( "dict['array'][0]: " + dict["array"][0] );
			Console.WriteLine( "dict['string']: " + dict["string"] );
			Console.WriteLine( "dict['float']: " + dict["float"] ); // floats come out as doubles
			Console.WriteLine( "dict['int']: " + dict["int"] ); // ints come out as longs
			Console.WriteLine( "dict['unicode']: " + dict["unicode"] );

			var str = JSON.Dump( dict, true );

			Console.WriteLine( "serialized: " + str );
			/**/
		}
	}
}
