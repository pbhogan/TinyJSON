using System;


namespace TinyJSON
{
	[Flags]
	public enum EncodeOptions
	{
		None = 0,
		PrettyPrint = 1,
		TypeHint = 2,
		Default = TypeHint
	}
}

