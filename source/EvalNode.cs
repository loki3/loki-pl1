using System;
using System.Collections.Generic;

namespace loki3.core
{
	/// <summary>Used to request the previous or next nodes</summary>
	interface INodeRequestor
	{
		/// <summary>Returns null if previous value doesn't exist</summary>
		DelimiterNode GetPrevious();
		/// <summary>Returns null if next value doesn't exist</summary>
		DelimiterNode GetNext();
	}

	/// <summary>
	/// Evaluates a node, returning a value based on a function, variable,
	/// delimited set of nodes, or built-in value
	/// May request the previous or next tokens in the token list.
	/// </summary>
	internal class EvalNode
	{
		/// <summary>
		/// Evaluates a token, possibly requesting the previous and next values.
		/// Returns a value.
		/// </summary>
		/// <param name="token">token representing a function or variable</param>
		/// <param name="scope">used to request functions, variables, and delimiters</param>
		/// <param name="nodes">used to request previous and next nodes</param>
		internal static Value Do(DelimiterNode node, IScope scope, INodeRequestor nodes, ILineRequestor requestor)
		{
			if (node.Value != null)
			{	// node has already been evaluated
				return node.Value;
			}
			else if (node.Token != null)
			{	// function/variable or built-in
				Token token = node.Token;
				Value value = scope.GetValue(token);
				if (value is ValueFunction && nodes != null)
				{
					ValueFunction function = value as ValueFunction;
					// get previous & next nodes if needed
					DelimiterNode previous = (function.ConsumesPrevious ? nodes.GetPrevious() : null);
					DelimiterNode next = (function.ConsumesNext ? nodes.GetNext() : null);

					scope.FunctionName = token.Value;

					// evaluate
					try
					{
						return function.Eval(previous, next, scope, scope, nodes, requestor);
					}
					catch (Loki3Exception e)
					{	// this function is the correct context if there isn't already one there
						if (!e.Errors.ContainsKey(Loki3Exception.keyFunction))
							e.AddFunction(token.Value);
						if (!e.Errors.ContainsKey(Loki3Exception.keyScope))
							e.AddScope(scope);
						throw e;
					}
				}
				else if (value != null)
				{
					return value;
				}
				else
				{
					return EvalBuiltin.Do(token);
				}
			}
			else if (node.List != null)
			{	// delimited list of nodes
				Value value = null;
				DelimiterList list = node.List;
				DelimiterType type = list.Delimiter.DelimiterType;

				// get contents as a Value
				switch (type)
				{
					case DelimiterType.AsString:
						value = new ValueString(list.Original);
						break;
					case DelimiterType.AsValue:
						value = EvalList.Do(list.Nodes, scope);
						break;
					case DelimiterType.AsArray:
						List<Value> values = new List<Value>(list.Nodes.Count);
						foreach (DelimiterNode subnode in list.Nodes)
						{	// note: 'nodes' is null so functions don't get evaled
							Value subvalue = Do(subnode, scope, null, requestor);
							values.Add(subvalue);
						}
						value = new ValueArray(values);
						break;
					case DelimiterType.AsRaw:
						value = new ValueRaw(list);
						break;
				}

				// run contents through a function if specified
				ValueFunction function = list.Delimiter.Function;
				if (function == null)
					return value;
				DelimiterNode next = new DelimiterNodeValue(value);
				return function.Eval(null, next, scope, scope, nodes, requestor);
			}
			return new ValueNil();
		}
	}
}
