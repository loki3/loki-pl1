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
			scope.SetValue("l3.ifBody", new IfBody());
			scope.SetValue("l3.ifValue", new IfValue());
		}


		/// <summary>{ :do? :body } -> if do?, last value of body, else false</summary>
		class IfBody : ValueFunctionPre
		{
			internal IfBody()
			{
				SetDocString("If do?, evaluate body and return the last value in body.\nElse return false.");

				Map map = new Map();
				map["do?"] = PatternData.Single("do?", ValueType.Bool);
				map["body"] = PatternData.Body();
				ValueMap vMap = new ValueMap(map);
				Init(vMap);
			}

			internal override Value Eval(Value arg, IScope scope)
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

		/// <summary>{ :do? [:ifTrue] [:ifFalse] } -> if do?, returns ifTrue, else ifFalse</summary>
		class IfValue : ValueFunctionPre
		{
			internal IfValue()
			{
				SetDocString("If do?, return value of ifTrue, else value of ifFalse.");

				Map map = new Map();
				map["do?"] = PatternData.Single("do?", ValueType.Bool);
				map["ifTrue"] = PatternData.Single("ifTrue", ValueBool.True);
				map["ifFalse"] = PatternData.Single("ifFalse", ValueBool.False);
				ValueMap vMap = new ValueMap(map);
				Init(vMap);
			}

			internal override Value Eval(Value arg, IScope scope)
			{
				Map map = arg.AsMap;
				bool shouldDo = map["do?"].AsBool;
				if (shouldDo)
				{
					Value val = map["ifTrue"];
					ValueRaw raw = val as ValueRaw;
					if (raw == null)
						return val;
					return EvalList.Do(raw.GetValue().Nodes, scope);
				}
				else
				{
					Value val = map["ifFalse"];
					ValueRaw raw = val as ValueRaw;
					if (raw == null)
						return val;
					return EvalList.Do(raw.GetValue().Nodes, scope);
				}
			}
		}
	}
}
