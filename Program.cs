using System;
using loki3.core;
using loki3.builtin;

namespace loki_pl1
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				IScope scope = Bootstrap();
				Repl.Do(scope, "loki3>");
			}
			catch (Exception error)
			{
				System.Diagnostics.Debug.WriteLine(error.ToString());
			}
		}

		static ScopeChain Bootstrap()
		{
			ScopeChain scope = new ScopeChain();
			AllBuiltins.RegisterAll(scope);
			EvalFile.Do("l3/bootstrap.l3", scope);
			EvalFile.Do("l3/help.l3", scope);
			return scope;
		}
	}
}
