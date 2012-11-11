using System.Collections.Generic;
using loki3.core;

namespace loki3.builtin
{
	/// <summary>
	/// Built-in string functions
	/// </summary>
	class String
	{
		/// <summary>
		/// Add built-in functions to the scope
		/// </summary>
		internal static void Register(IScope scope)
		{
			scope.SetValue("l3.stringConcat", new StringConcat());
		}


		/// <summary>[ :a :b ] -> add together two strings</summary>
		class StringConcat : ValueFunctionPre
		{
			internal StringConcat()
			{
				SetDocString("Concatenate two strings.");

				List<Value> list = new List<Value>();
				list.Add(PatternData.Single("a", ValueType.String));
				list.Add(PatternData.Single("b", ValueType.String));
				ValueArray array = new ValueArray(list);
				Init(array);
			}

			internal override Value Eval(Value arg, IScope scope)
			{
				List<Value> list = arg.AsArray;
				string s1 = list[0].AsString;
				string s2 = list[1].AsString;
				return new ValueString(s1 + s2);
			}
		}
	}
}
