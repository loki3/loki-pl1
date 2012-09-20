using System;
using System.Collections.Generic;

namespace loki3
{
	/// <summary>Used to get a named function</summary>
	interface IFunctionRequestor
	{
		/// <summary>Returns null if requested token doesn't exist</summary>
		ValueFunction Get(Token token);
	}

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
		/// <param name="functions">used to request a function</param>
		/// <param name="nodes">used to request previous and next nodes</param>
		internal static Value Do(DelimiterNode node, IFunctionRequestor functions, INodeRequestor nodes)
		{
			Token token = node.Token;
			if (token != null)
			{	// function/variable or built-in
				ValueFunction function = functions.Get(token);
				if (function != null)
				{
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
					return function.Eval(previous, next, functions);
				}
				else
				{
					return EvalBuiltin.Do(token);
				}
			}
			else
			{	// delimited list of nodes
				DelimiterTree tree = node.Tree;
				DelimiterType type = tree.Delimiter.DelimiterType;
				switch (type)
				{
					case DelimiterType.AsString:
						return new ValueString(tree.Nodes[0].Token.Value);
					case DelimiterType.AsValue:
						break;
					case DelimiterType.AsArray:
						List<Value> values = new List<Value>(tree.Nodes.Count);
						foreach (DelimiterNode subnode in tree.Nodes)
						{
							Value value = Do(subnode, functions, nodes);
							values.Add(value);
						}
						return new ValueArray(values);
				}
				return new ValueNil();
			}
		}
	}
}
