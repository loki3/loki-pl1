using System;

namespace loki3
{
	internal abstract class DelimiterNode
	{
	}

	internal class DelimiterNodeToken : DelimiterNode
	{
		internal DelimiterNodeToken(Token token)
		{
			m_token = token;
		}

		internal Token Token { get { return m_token; } }

		private Token m_token;
	}

	internal class DelimiterNodeTree : DelimiterNode
	{
		internal DelimiterNodeTree(DelimiterTree tree)
		{
			m_tree = tree;
		}

		internal DelimiterTree Tree { get { return m_tree; } }

		private DelimiterTree m_tree;
	}
}
