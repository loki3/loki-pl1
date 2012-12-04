using System.Collections.Generic;
using loki3.core;

namespace loki3.builtin
{
	/// <summary>
	/// Built-in input/output functions
	/// </summary>
	class IO
	{
		/// <summary>
		/// Add built-in IO functions to the scope
		/// </summary>
		internal static void Register(IScope scope)
		{
			scope.SetValue("l3.print", new Print());
		}


		/// <summary>{ :value } -> send string to stdout</summary>
		class Print : ValueFunctionPre
		{
			internal override Value ValueCopy() { return new Print(); }

			internal Print()
			{
				SetDocString("Print the string representation of a value, i.e. send it to stdout.");

				Map map = new Map();
				map["value"] = PatternData.Single("value");
				ValueMap vMap = new ValueMap(map);
				Init(vMap);
			}

			internal override Value Eval(Value arg, IScope scope)
			{
				Map map = arg.AsMap;
				Value val = map["value"];
				string s = val.ToString();
				if (s[s.Length-1] == '\n')
					System.Console.Write(s);
				else
					System.Console.WriteLine(s);
				return val;
			}
		}
	}
}
