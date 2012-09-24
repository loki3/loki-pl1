using System;
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
		}


		/// <summary>[a b] -> bool,  depending on if a and b are equal</summary>
		class IsEqual : ValueFunctionPre
		{
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
	}
}
