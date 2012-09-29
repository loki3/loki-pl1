using System.Collections.Generic;
using loki3.core;

namespace loki3.builtin
{
	/// <summary>
	/// Built-in conditional functions
	/// </summary>
	class Conditional
	{
		/// <summary>
		/// Add built-in If functions to the scope
		/// </summary>
		internal static void Register(IScope scope)
		{
			scope.SetValue("l3.if", new If());
		}


		/// <summary>{ :do? :body } -> if do?, last value of body, else false</summary>
		class If : ValueFunctionPre
		{
			internal If()
			{
				Map map = new Map();
				map["do?"] = PatternData.Single("do?", ValueType.Bool);
				map["body"] = PatternData.Body();
				ValueMap vMap = new ValueMap(map);
				Init(vMap);
			}

			protected override Value Eval(Value arg, IScope scope)
			{
				Map map = arg.AsMap;

				// if !do?, return
				bool shouldDo = map["do?"].AsBool;
				if (!shouldDo)
					return new ValueBool(false);

				// if do?, eval body
				List<Value> valueBody = map["body"].AsArray;
				return EvalBody.Do(valueBody, scope);
			}
		}
	}
}
