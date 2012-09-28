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
			internal IsEqual()
			{
				List<Value> list = new List<Value>();
				list.Add(PatternData.Single("a"));
				list.Add(PatternData.Single("b"));
				ValueArray array = new ValueArray(list);
				Init(array);
			}

			internal override Value Eval(DelimiterNode next, IScope scope, INodeRequestor nodes)
			{
				Value post = EvalNode.Do(next, scope, nodes);
				List<Value> list = post.AsArray;
				if (list.Count != 2)
					throw new WrongSizeArray(2, list.Count);

				Value v1 = list[0];
				Value v2 = list[1];
				return new ValueBool(v1.Equals(v2));
			}
		}

		/// <summary>[bool bool] -> bool,  a AND b</summary>
		class LogicalAnd : ValueFunctionPre
		{
			internal LogicalAnd()
			{
				List<Value> list = new List<Value>();
				list.Add(PatternData.Single("a", ValueType.Bool));
				list.Add(PatternData.Single("b", ValueType.Bool));
				ValueArray array = new ValueArray(list);
				Init(array);
			}

			internal override Value Eval(DelimiterNode next, IScope scope, INodeRequestor nodes)
			{
				Value post = EvalNode.Do(next, scope, nodes);
				List<Value> list = post.AsArray;
				if (list.Count != 2)
					throw new WrongSizeArray(2, list.Count);

				return new ValueBool(list[0].AsBool && list[1].AsBool);
			}
		}

		/// <summary>[bool bool] -> bool,  a OR b</summary>
		class LogicalOr : ValueFunctionPre
		{
			internal LogicalOr()
			{
				List<Value> list = new List<Value>();
				list.Add(PatternData.Single("a", ValueType.Bool));
				list.Add(PatternData.Single("b", ValueType.Bool));
				ValueArray array = new ValueArray(list);
				Init(array);
			}

			internal override Value Eval(DelimiterNode next, IScope scope, INodeRequestor nodes)
			{
				Value post = EvalNode.Do(next, scope, nodes);
				List<Value> list = post.AsArray;
				if (list.Count != 2)
					throw new WrongSizeArray(2, list.Count);

				return new ValueBool(list[0].AsBool || list[1].AsBool);
			}
		}

		/// <summary>bool -> bool,  NOT a</summary>
		class LogicalNot : ValueFunctionPre
		{
			internal LogicalNot() { Init(PatternData.Single("a", ValueType.Bool)); }

			internal override Value Eval(DelimiterNode next, IScope scope, INodeRequestor nodes)
			{
				Value post = EvalNode.Do(next, scope, nodes);
				return new ValueBool(!post.AsBool);
			}
		}
	}
}
