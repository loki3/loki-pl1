using System;
using System.Collections.Generic;
using loki3.core;

namespace loki3.builtin
{
	class AllBuiltins
	{
		static void RegisterAll(IScope scope)
		{
			Math.Register(scope);
		}
	}
}
