using System;
using System.Collections.Generic;
using loki3.core;

namespace loki3.builtin
{
	internal class AllBuiltins
	{
		internal static void RegisterAll(IScope scope)
		{
			ArrayFunctions.Register(scope);
			Conditional.Register(scope);
			DebugFunctions.Register(scope);
			IO.Register(scope);
			Logic.Register(scope);
			Loop.Register(scope);
			Math.Register(scope);
			MapFunctions.Register(scope);
			Module.Register(scope);
			ScopeFunctions.Register(scope);
			String.Register(scope);
			Values.Register(scope);
		}
	}
}
