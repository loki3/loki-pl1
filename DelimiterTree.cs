using System;
using System.Collections.Generic;

namespace loki3
{
	/// <summary>
	/// Tree where every node is a token or subtree with an associated delimiter
	/// </summary>
	internal class DelimiterTree
	{
		internal DelimiterTree(ValueDelimiter delim, List<DelimiterNode> nodes)
		{
			m_delimiter = delim;
			m_nodes = nodes;
		}

		internal ValueDelimiter Delimiter { get { return m_delimiter; } }
		internal List<DelimiterNode> Nodes { get { return m_nodes; } }

		private ValueDelimiter m_delimiter;
		private List<DelimiterNode> m_nodes;
	}
}
