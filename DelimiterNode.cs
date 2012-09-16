using System;

namespace loki3
{
	internal abstract class DelimiterNode
	{
		internal virtual Token Token { get { return null; } }
		internal virtual DelimiterTree Tree { get { return null; } }
	}

	internal class DelimiterNodeToken : DelimiterNode
	{
		internal DelimiterNodeToken(Token token)
		{
			m_token = token;
		}

		internal override Token Token { get { return m_token; } }

		private Token m_token;
	}

	internal class DelimiterNodeTree : DelimiterNode
	{
		internal DelimiterNodeTree(DelimiterTree tree)
		{
			m_tree = tree;
		}

		internal override DelimiterTree Tree { get { return m_tree; } }

		private DelimiterTree m_tree;
	}
}
