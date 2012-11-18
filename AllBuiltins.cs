using System;
using System.Collections.Generic;
using loki3.core;

namespace loki3.builtin
{
	internal class AllBuiltins
	{
		internal static void RegisterAll(IScope scope)
		{
			Conditional.Register(scope);
			IO.Register(scope);
			Logic.Register(scope);
			Loop.Register(scope);
			Math.Register(scope);
			Module.Register(scope);
			ScopeFunctions.Register(scope);
			String.Register(scope);
			Values.Register(scope);
		}
	}
}
