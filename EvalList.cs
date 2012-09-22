using System;
using System.Collections.Generic;

namespace loki3
{
	/// <summary>
	/// Eval a list of DelimiterNodes and return a Value
	/// </summary>
	internal class EvalList
	{
		enum NodeState
		{
			CantEval,	// not a function or sub-list
			Node,		// function or list that hasn't been evaled
			Value,		// fully resolved value
			Function,	// fully resolve function
			Empty,		// node has been consumed by adjacent function
		}

		/// <summary>
		/// Tracks info on evaluating a node
		/// </summary>
		internal class NodeEval
		{
			internal NodeEval(DelimiterNode node, IFunctionRequestor functions)
			{
				m_node = node;

				Token token = node.Token;
				if (token != null)
				{
					m_func = functions.Get(token);
					if (m_func != null)
					{
						m_state = NodeState.Node;
						m_precedence = (int)m_func.Precedence;
					}
				}
				else if (node.List != null)
				{
					m_state = NodeState.Node;
					m_precedence = 0;
				}
			}

			internal int Precedence { get { return m_precedence; } }
			internal bool IsEmpty { get { return m_state == NodeState.Empty; } }
			internal bool HasValue { get { return m_state == NodeState.Value; } }
			internal Value Value { get { return m_value; } }
			internal DelimiterNode Node { get { return m_node; } }

			/// <summary>Evaluate this node, possibly consuming adjacent nodes</summary>
			internal void Eval(IFunctionRequestor functions, INodeRequestor nodes)
			{
				if (m_state != NodeState.Node && m_state != NodeState.Function)
					return;

				// get new value
				if (m_state == NodeState.Node)
				{	// hasn't been evaled at all
					m_value = EvalNode.Do(m_node, functions, nodes);
				}
				else if (m_state == NodeState.Function)
				{	// previously resolved to a function
					DelimiterNode previous = (m_func.ConsumesPrevious ? previous = nodes.GetPrevious() : null);
					DelimiterNode next = (m_func.ConsumesNext ? next = nodes.GetNext() : null);
					m_value = m_func.Eval(previous, next, functions, nodes);
				}

				// store new info about this node
				m_node = new DelimiterNodeValue(m_value);
				m_precedence = (int)m_value.Precedence;
				if (m_value.Type == ValueType.Function)
				{
					m_state = NodeState.Function;
					m_func = m_value as ValueFunction;
				}
				else
				{
					m_state = NodeState.Value;
				}
			}

			/// <summary>Mark this node as consumed</summary>
			internal void Consumed()
			{
				m_state = NodeState.Empty;
			}

			private const int m_unknownPrecedence = -1;

			private DelimiterNode m_node;
			private ValueFunction m_func = null;
			private int m_precedence = m_unknownPrecedence;
			private Value m_value = null;
			private NodeState m_state = NodeState.CantEval;
		}

		/// <summary>
		/// Tracks info on evaluating a list of nodes
		/// </summary>
		internal class ListEval : INodeRequestor
		{
			internal ListEval(List<DelimiterNode> nodes, IFunctionRequestor functions)
			{
				m_nodes = new List<NodeEval>(nodes.Count);
				foreach (DelimiterNode node in nodes)
					m_nodes.Add(new NodeEval(node, functions));
				m_functions = functions;
			}

			/// <summary>
			/// Evaluate the right-most node with the highest precendence.
			/// Return false when there are no more to eval.
			/// </summary>
			internal bool EvalNext()
			{
				// find right-most node with highest precedence
				m_evalIndex = -1;
				int max = -2;
				int count = m_nodes.Count;
				for (int i = count - 1; i >= 0; --i)
				{
					NodeEval node = m_nodes[i];
					if (!node.HasValue && !node.IsEmpty)
					{	// node hasn't been evaled or consumed yet
						int p = node.Precedence;
						if (p > max)
						{	// greater precedence than anything to the right
							max = p;
							m_evalIndex = i;
						}
					}
				}
				// we're done if we didn't find anything
				if (max == -2)
					return false;
				// eval the one we found
				m_nodes[m_evalIndex].Eval(m_functions, this);
				return true;
			}

			/// <summary>
			/// The right-most value is the value of the list
			/// </summary>
			internal Value GetValue()
			{
				int count = m_nodes.Count;
				for (int i = count - 1; i >= 0; --i)
				{
					NodeEval node = m_nodes[i];
					if (node.HasValue)
						return node.Value;
				}
				return new ValueNil();
			}

			#region INodeRequestor Members

			public DelimiterNode GetPrevious()
			{
				// step back til we find a non-empty node
				for (int i = m_evalIndex - 1; i >= 0; --i)
				{
					NodeEval node = m_nodes[i];
					if (!node.IsEmpty)
					{
						node.Consumed();
						return node.Node;
					}
				}
				// none found
				return null;
			}

			public DelimiterNode GetNext()
			{
				// step forward til we find a non-empty node
				int count = m_nodes.Count;
				for (int i = m_evalIndex + 1; i < count; i++)
				{
					NodeEval node = m_nodes[i];
					if (!node.IsEmpty)
					{
						node.Consumed();
						return node.Node;
					}
				}
				// none found
				return null;
			}

			#endregion

			private List<NodeEval> m_nodes;
			private IFunctionRequestor m_functions;
			private int m_evalIndex;
		}

		/// <summary>
		/// Eval a list of DelimiterNodes and return a Value
		/// </summary>
		internal static Value Do(List<DelimiterNode> nodes, IFunctionRequestor functions)
		{
			ListEval eval = new ListEval(nodes, functions);
			while (eval.EvalNext())
				;
			return eval.GetValue();
		}
	}
}
