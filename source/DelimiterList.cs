using System;
using System.Collections.Generic;

namespace loki3.core
{
	/// <summary>
	/// Tree where every node is a token or subtree with an associated delimiter
	/// </summary>
	internal class DelimiterList
	{
		internal DelimiterList(ValueDelimiter delim, List<DelimiterNode> nodes, int indent, string startDelim, string original, IScope scope)
		{
			m_delimiter = delim;
			m_nodes = nodes;
			m_indent = indent;
			m_startDelim = startDelim;	// if known
			m_original = original;
			m_scope = scope;
		}

		internal ValueDelimiter Delimiter { get { return m_delimiter; } }
		internal List<DelimiterNode> Nodes { get { return m_nodes; } }
		internal int Indent { get { return m_indent; } }
		internal string Original { get { return m_original; } }

		/// <summary>Scope to use when evaluating, might be null</summary>
		internal IScope Scope
		{
			get { return m_scope; }
			set { m_scope = value; }
		}

		public override string ToString()
		{
			string s = new string('\t', m_indent);
			s += m_startDelim + " ";
			foreach (DelimiterNode node in m_nodes)
				s += node.ToString() + " ";
			if (m_delimiter.End.Length > 0)
				s += " " + m_delimiter.End;
			return s;
		}

		private ValueDelimiter m_delimiter;
		private List<DelimiterNode> m_nodes;
		private int m_indent;
		private string m_startDelim;
		private string m_original;
		private IScope m_scope;
	}

	/// <summary>
	/// A value that represents a parsed line
	/// </summary>
	internal class ValueLine : Value
	{
		internal ValueLine(List<DelimiterList> list, IScope scope)
		{
			m_list = list;
			m_scope = scope;
		}

		internal List<DelimiterList> Line { get { return m_list; } }
		internal IScope Scope { get { return m_scope; } }

#region Value
		internal override List<DelimiterList> AsLine
		{
			get { return m_list; }
		}

		internal override ValueType Type
		{
			get { return ValueType.RawList; }
		}

		internal override bool Equals(Value v)
		{
			return this == v;
		}

		internal override Value ValueCopy() { return new ValueLine(m_list, m_scope); }

		internal override int Count { get { return m_list.Count; } }
#endregion

		private List<DelimiterList> m_list;
		private IScope m_scope;
	}
}
