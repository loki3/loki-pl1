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
				string s = "";
				do
				{
					Console.Write("loki3> ");
					s = Console.ReadLine();
					try
					{
						DelimiterList list = ParseLine.Do(s, scope);
						Value v = EvalList.Do(list.Nodes, scope);
						Console.WriteLine(v.ToString());
					}
					catch (Loki3Exception error)
					{
						Console.WriteLine(error.ToString());
					}
				} while (s != "");
			}
			catch (Loki3Exception error)
			{
				System.Diagnostics.Debug.WriteLine(error.ToString());
			}
		}

		static ScopeChain Bootstrap()
		{
			ScopeChain scope = new ScopeChain();
			AllBuiltins.RegisterAll(scope);
			EvalFile.Do("l3/bootstrap.l3", scope);
			return scope;
		}
	}
}
