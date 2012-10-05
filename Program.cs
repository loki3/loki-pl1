using System;
using loki3.core;
using loki3.builtin;

namespace loki_pl1
{
	class Program
	{
		static void Main(string[] args)
		{
			IScope scope = Bootstrap();
		}

		static ScopeChain Bootstrap()
		{
			ScopeChain scope = new ScopeChain(null);
			AllBuiltins.RegisterAll(scope);
			EvalFile.Do("l3/bootstrap.l3", scope);
			return scope;
		}
	}
}
