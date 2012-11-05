using System.Collections.Generic;
using loki3.core;

namespace loki3.builtin
{
	/// <summary>
	/// Built-in loop functions
	/// </summary>
	class Loop
	{
		/// <summary>
		/// Add built-in If functions to the scope
		/// </summary>
		internal static void Register(IScope scope)
		{
			scope.SetValue("l3.loop", new BasicLoop());
		}


		/// <summary>{ :check :body [checkFirst?] } -> keep running body until !check</summary>
		class BasicLoop : ValueFunctionPre
		{
			internal BasicLoop()
			{
				SetDocString("Repeat body until a condition is no longer true. Return value is last value of body.");

				Map map = new Map();
				map["check"] = PatternData.Single("check", ValueType.Raw);
				map["checkFirst?"] = PatternData.Single("checkFirst?", ValueType.Bool, ValueBool.True);
				map["body"] = PatternData.Body();
				ValueMap vMap = new ValueMap(map);
				Init(vMap);
			}

			internal override Value Eval(Value arg, IScope scope)
			{
				Map map = arg.AsMap;

				List<DelimiterNode> check = (map["check"] as ValueRaw).GetValue().Nodes;
				bool checkFirst = map["checkFirst?"].AsBool;
				List<Value> valueBody = map["l3.func.body"].AsArray;

				bool isFirst = true;
				Value result = ValueBool.False;
				while (true)
				{
					if (!isFirst || checkFirst)
					{
						Value retval = EvalList.Do(check, scope);
						if (!retval.AsBool)
							return result;	// last value of body
					}
					else
					{
						isFirst = false;
					}
					result = EvalBody.Do(valueBody, scope);
				}
			}
		}

	}
}
