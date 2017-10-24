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


		/// <summary>{ :do? :body } -> if do?, return [last value of body, true], else [false, false]</summary>
		class IfBody : ValueFunctionPre
		{
			internal override Value ValueCopy() { return new IfBody(); }

			internal IfBody()
			{
				SetDocString("If do?, evaluate body and return [last value in body, true].\nElse return [false, false].");

				Map map = new Map();
				map["do?"] = PatternData.Single("do?", ValueType.Bool);
				map["body"] = PatternData.Body();
				ValueMap vMap = new ValueMap(map);
				Init(vMap);
			}

			internal override Value Eval(Value arg, IScope scope)
			{
				Map map = arg.AsMap;
				List<Value> newarray = new List<Value>(2);

				// if !do?, return
				if (map["do?"].AsBool)
				{   // if do?, eval body
					Value valueBody = map["body"];
					newarray.Add(EvalBody.Do(valueBody, scope));
					newarray.Add(new ValueBool(true));
				}
				else
				{
					newarray.Add(ValueBool.False);
					newarray.Add(ValueBool.False);
				}
				return new ValueArray(newarray);
			}
		}

		/// <summary>{ :do? [:ifTrue] [:ifFalse] } -> if do?, returns ifTrue, else ifFalse</summary>
		class IfValue : ValueFunctionPre
		{
			internal override Value ValueCopy() { return new IfValue(); }

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
					IScope theScope = (raw.Scope != null ? raw.Scope : scope);
					return EvalList.Do(raw.GetValue().Nodes, theScope);
				}
				else
				{
					Value val = map["ifFalse"];
					ValueRaw raw = val as ValueRaw;
					if (raw == null)
						return val;
					IScope theScope = (raw.Scope != null ? raw.Scope : scope);
					return EvalList.Do(raw.GetValue().Nodes, theScope);
				}
			}
		}
	}
}
