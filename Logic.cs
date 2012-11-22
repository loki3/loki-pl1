using System.Collections.Generic;
using loki3.core;

namespace loki3.builtin
{
	/// <summary>
	/// Built-in logic functions, e.g. =, &&, !
	/// </summary>
	class Logic
	{
		/// <summary>
		/// Add built-in Value functions to the scope
		/// </summary>
		internal static void Register(IScope scope)
		{
			scope.SetValue("l3.equal?", new IsEqual());
			scope.SetValue("l3.and?", new LogicalAnd());
			scope.SetValue("l3.or?", new LogicalOr());
			scope.SetValue("l3.not?", new LogicalNot());
		}


		/// <summary>[a b] -> bool,  depending on if a and b are equal</summary>
		class IsEqual : ValueFunctionPre
		{
			internal override Value ValueCopy() { return new IsEqual(); }

			internal IsEqual()
			{
				SetDocString("If both elements in array are equal, return true, else false.");

				List<Value> list = new List<Value>();
				list.Add(PatternData.Single("a"));
				list.Add(PatternData.Single("b"));
				ValueArray array = new ValueArray(list);
				Init(array);
			}

			internal override Value Eval(Value arg, IScope scope)
			{
				List<Value> list = arg.AsArray;
				Value v1 = list[0];
				Value v2 = list[1];
				return new ValueBool(v1.Equals(v2));
			}
		}

		/// <summary>[bool bool] -> bool,  a AND b</summary>
		class LogicalAnd : ValueFunctionPre
		{
			internal override Value ValueCopy() { return new LogicalAnd(); }

			internal LogicalAnd()
			{
				SetDocString("Return true only if both values of array are true.");

				List<Value> list = new List<Value>();
				list.Add(PatternData.Single("a", ValueType.Bool));
				list.Add(PatternData.Single("b", ValueType.Bool));
				ValueArray array = new ValueArray(list);
				Init(array);
			}

			internal override Value Eval(Value arg, IScope scope)
			{
				List<Value> list = arg.AsArray;
				return new ValueBool(list[0].AsBool && list[1].AsBool);
			}
		}

		/// <summary>[bool bool] -> bool,  a OR b</summary>
		class LogicalOr : ValueFunctionPre
		{
			internal override Value ValueCopy() { return new LogicalOr(); }

			internal LogicalOr()
			{
				SetDocString("Return true if either value in array are true.");

				List<Value> list = new List<Value>();
				list.Add(PatternData.Single("a", ValueType.Bool));
				list.Add(PatternData.Single("b", ValueType.Bool));
				ValueArray array = new ValueArray(list);
				Init(array);
			}

			internal override Value Eval(Value arg, IScope scope)
			{
				List<Value> list = arg.AsArray;
				return new ValueBool(list[0].AsBool || list[1].AsBool);
			}
		}

		/// <summary>bool -> bool,  NOT a</summary>
		class LogicalNot : ValueFunctionPre
		{
			internal override Value ValueCopy() { return new LogicalNot(); }

			internal LogicalNot()
			{
				SetDocString("Return true if value is false, else false.");
				Init(PatternData.Single("a", ValueType.Bool));
			}

			internal override Value Eval(Value arg, IScope scope)
			{
				return new ValueBool(!arg.AsBool);
			}
		}
	}
}
