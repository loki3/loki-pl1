using System;
using System.Collections.Generic;

namespace loki3
{
	/// <summary>
	/// Tree where every node is a token or subtree with an associated delimiter
	/// </summary>
	class DelimiterTree
	{
		DelimiterTree(Delimiter delim, List<DelimiterNode> nodes)
		{
			m_delimiter = delim;
			m_nodes = nodes;
		}

		Delimiter Delimiter { get { return m_delimiter; } }
		List<DelimiterNode> Nodes { get { return m_nodes; } }

		Delimiter m_delimiter;
		List<DelimiterNode> m_nodes;
	}
}
