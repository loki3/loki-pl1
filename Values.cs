using System;
using System.Collections.Generic;
using loki3.core;

namespace loki3.builtin
{
	/// <summary>
	/// Built-in functions for dealing with Value objects
	/// </summary>
	class Values
	{
		/// <summary>
		/// Add built-in Value functions to the stack
		/// </summary>
		internal static void Register(IStack stack)
		{
			stack.SetValue("l3.setValue", new SetValue());
		}


		/// <summary>[key value] -> value  and stores the key value pair on the current stack</summary>
		class SetValue : ValueFunctionPre
		{
			internal override Value Eval(DelimiterNode next, IStack stack, INodeRequestor nodes)
			{
				Value post = EvalNode.Do(next, stack, nodes);
				List<Value> list = post.AsArray;
				if (list.Count != 2)
					throw new WrongSizeArray(2, list.Count);

				string key = list[0].AsString;
				Value value = list[1];
				stack.SetValue(key, value);
				return value;
			}
		}
	}
}
