## Information

TinyJSON is a simple, more betterer JSON library for C#, based on MiniJSON.

## Features

* Transmogrify things into JSON and back again.
* Uses reflection to dump and load simple POD structures (including nested objects) automagically.
* Parsed data uses proxy variants that can be implicitly cast in most situations for cleaner code.
* Pretty printing for JSON output.

## How To

There are really only three calls you need to know:

```csharp
namespace TinyJSON
{
	public static class JSON
	{
		public static Variant Load( string json );
		public static string Dump( object data, bool prettyPrint = false );
		public static void MakeInto<T>( Variant data, out T item );
	}
}
```

`Load()` will load a string of JSON, returns `null` if invalid or a `Variant` proxy object if successful. The proxy allows for implicit casts and can convert between various C# numberic value types.

```csharp
var data = JSON.Load( "{\"foo\": 1, \"bar\": 2.34}" );
int i = data["foo"];
float f = data["bar"];
```

`Dump()` will take a C# object, list, dictionary or primitive value type and turn it into JSON.

```csharp
var data = new List<int>() { { 0 }, { 1 }, { 2 } };
Console.WriteLine( JSON.Dump( data ) ); // Output: [1,2,3]
```

But it is also much more powerful than this, handling nested objects and simple class types. Given:

```csharp
class Data
{
	public int i;
	public float f;
	public double d;
	public string s;
	public bool b;

	[Skip]
	public int h; // will be ignored

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
```

The following:

```csharp
var data = Data.New();
var json = JSON.Dump( data, true );
Console.WriteLine( json );
```

Will output:

```json
{
	"i": 5,
	"f": 3.14,
	"d": 1.23456789,
	"s": "OHAI!",
	"b": true,
	"l": [
		0,
		1,
		2
	]
}
```

Finally, `MakeInto()` can be used to reconstruct JSON data back into an object. Using the output above in `string json`:

```csharp
Data data;
JSON.MakeInto( JSON.Load( json ), out data );
```

There are also `Make()` methods on `Variant`, so either of the following will work:

```csharp
Data data;
JSON.Load( json ).Make( out data );
// or
var data = JSON.Load( json ).Make<Data>();
```

Go forth and be awesome.

## Note

This project was developed with pain elimination and lightweight size in mind. That said, it should be able able to handle reasonable amounts of reasonable data at reasonable speeds.

My primary use case for this library is with Mono and Unity3D version 4 and higher, so compatibility is focused there, though it should work with most modern C# environments.

## Meta

Developed by Patrick Hogan [[twitter](http://twitter.com/pbhogan) &bull; [github](http://github.com/pbhogan) &bull; [website](http://www.gallantgames.com)]

Based on [MiniJSON](https://gist.github.com/darktable/1411710) by Calvin Rien

