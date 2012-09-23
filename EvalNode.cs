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
		internal static Value Do(DelimiterNode node, IScope scope, INodeRequestor nodes)
		{
			if (node.Value != null)
			{	// node has already been evaluated
				return node.Value;
			}
			else if (node.Token != null)
			{	// function/variable or built-in
				Token token = node.Token;
				Value value = scope.GetValue(token);
				if (value is ValueFunction)
				{
					ValueFunction function = value as ValueFunction;
					// get previous & next nodes if needed
					DelimiterNode previous = null;
					if (function.ConsumesPrevious)
					{
						previous = nodes.GetPrevious();
						if (previous == null)
							throw new MissingAdjacentValueException(token, true/*bPrevious*/);
					}
					DelimiterNode next = null;
					if (function.ConsumesNext)
					{
						next = nodes.GetNext();
						if (next == null)
							throw new MissingAdjacentValueException(token, false/*bPrevious*/);
					}

					// evaluate
					return function.Eval(previous, next, scope, nodes);
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
				DelimiterList list = node.List;
				DelimiterType type = list.Delimiter.DelimiterType;
				switch (type)
				{
					case DelimiterType.AsString:
						return new ValueString(list.Nodes[0].Token.Value);
					case DelimiterType.AsValue:
						return EvalList.Do(list.Nodes, scope);
					case DelimiterType.AsArray:
						List<Value> values = new List<Value>(list.Nodes.Count);
						foreach (DelimiterNode subnode in list.Nodes)
						{
							Value value = Do(subnode, scope, nodes);
							values.Add(value);
						}
						return new ValueArray(values);
				}
			}
			return new ValueNil();
		}
	}
}
