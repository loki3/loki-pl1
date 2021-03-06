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
				scope.Exit();
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
			EvalFile.Do("bootstrap.l3", scope);
			EvalFile.Do("help.l3", scope);
			return scope;
		}
	}
}
