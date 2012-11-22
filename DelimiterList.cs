using System;
using System.Collections.Generic;

namespace loki3.core
{
	/// <summary>
	/// Tree where every node is a token or subtree with an associated delimiter
	/// </summary>
	internal class DelimiterList
	{
		internal DelimiterList(ValueDelimiter delim, List<DelimiterNode> nodes, int indent)
		{
			m_delimiter = delim;
			m_nodes = nodes;
			m_indent = indent;
		}

		internal ValueDelimiter Delimiter { get { return m_delimiter; } }
		internal List<DelimiterNode> Nodes { get { return m_nodes; } }
		internal int Indent { get { return m_indent; } }

		public override string ToString()
		{
			string s = new string('\t', m_indent);
			s += m_delimiter.Start + " ";
			foreach (DelimiterNode node in m_nodes)
				s += node.ToString() + " ";
			if (m_delimiter.End.Length > 0)
				s += " " + m_delimiter.End;
			return s;
		}

		private ValueDelimiter m_delimiter;
		private List<DelimiterNode> m_nodes;
		private int m_indent;
	}

	/// <summary>
	/// A value that represents a parsed line
	/// </summary>
	internal class ValueLine : Value
	{
		internal ValueLine(List<DelimiterList> list)
		{
			m_list = list;
		}

		internal List<DelimiterList> Line { get { return m_list; } }

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

		internal override Value ValueCopy() { return new ValueLine(m_list); }
#endregion

		private List<DelimiterList> m_list;
	}
}
