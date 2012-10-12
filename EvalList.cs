using System;
using System.Collections.Generic;

namespace loki3.core
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
			Function,	// fully resolved function
			Empty,		// node has been consumed by adjacent function
		}

		/// <summary>
		/// Tracks info on evaluating a node
		/// </summary>
		internal class NodeEval
		{
			internal NodeEval(DelimiterNode node, IScope scope)
			{
				m_node = node;

				Token token = node.Token;
				if (token != null)
				{
					Value value = scope.GetValue(token);
					m_func = value as ValueFunction;
					if (m_func != null)
					{	// function
						m_state = NodeState.Node;
						m_precedence = (int)m_func.Precedence;
					}
					else if (value != null)
					{	// variable
						m_value = value;
						m_state = NodeState.Value;
					}
					else
					{	// built-in
						try
						{
							m_value = EvalBuiltin.Do(token);
							m_state = NodeState.Value;
						}
						catch (Exception) { }
					}
				}
				else if (node.List != null)
				{
					m_state = NodeState.Node;
					m_precedence = 0;
				}
				else if (node.Value != null)
				{
					m_value = node.Value;
					m_precedence = (int)m_value.Precedence;
					m_func = m_value as ValueFunction;
					m_state = (m_func == null ? NodeState.Value : NodeState.Function);
				}
			}

			internal int Precedence { get { return m_precedence; } }
			internal bool IsEmpty { get { return m_state == NodeState.Empty; } }
			internal bool HasFunction { get { return m_state == NodeState.Function; } }
			internal bool HasValue { get { return m_state == NodeState.Value; } }
			internal bool CantEval { get { return m_state == NodeState.CantEval; } }
			internal Value Value { get { return m_value; } }
			internal DelimiterNode Node { get { return m_node; } }

			/// <summary>Evaluate this node, possibly consuming adjacent nodes</summary>
			internal void Eval(IScope scope, INodeRequestor nodes)
			{
				if (m_state != NodeState.Node && m_state != NodeState.Function)
					return;

				// get new value
				if (m_state == NodeState.Node)
				{	// hasn't been evaled at all
					m_value = EvalNode.Do(m_node, scope, nodes);
				}
				else if (m_state == NodeState.Function)
				{	// previously resolved to a function
					DelimiterNode previous = (m_func.ConsumesPrevious ? previous = nodes.GetPrevious() : null);
					DelimiterNode next = (m_func.ConsumesNext ? next = nodes.GetNext() : null);
					m_value = m_func.Eval(previous, next, scope, nodes);
				}

				if (m_value == null)
				{	// e.g. because node was a comment
					m_state = NodeState.Empty;
					return;
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
			internal ListEval(List<DelimiterNode> nodes, IScope scope)
			{
				m_nodes = new List<NodeEval>(nodes.Count);
				foreach (DelimiterNode node in nodes)
					m_nodes.Add(new NodeEval(node, scope));
				m_scope = scope;
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
				int empties = 0;
				for (int i = count - 1; i >= 0; --i)
				{
					NodeEval node = m_nodes[i];
					if (!node.HasValue /*&& !node.HasFunction*/ && !node.IsEmpty)
					{	// node hasn't been evaled or consumed yet
						int p = node.Precedence;
						if (p > max)
						{	// greater precedence than anything to the right
							max = p;
							m_evalIndex = i;
						}
					}
					if (node.IsEmpty)
						empties++;
				}

				// we're done if we didn't find anything
				if (max == -2)
					return false;
				// if the only thing left is a function, there's nothing further to do
				if (empties == count - 1 && m_nodes[m_evalIndex].HasFunction)
					return false;
				// if we found something we can't eval, it's an error
				if (m_nodes[m_evalIndex].CantEval)
					throw new Loki3Exception().AddBadToken(m_nodes[m_evalIndex].Node.Token);

				// eval the one we found
				m_nodes[m_evalIndex].Eval(m_scope, this);
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
					if (node.HasValue || node.HasFunction)
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
			private IScope m_scope;
			private int m_evalIndex;
		}

		/// <summary>
		/// Eval a list of DelimiterNodes and return a Value
		/// </summary>
		internal static Value Do(List<DelimiterNode> nodes, IScope scope)
		{
			ListEval eval = new ListEval(nodes, scope);
			while (eval.EvalNext())
				;
			return eval.GetValue();
		}
	}
}
