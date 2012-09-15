using System;

namespace loki3
{
	abstract class DelimiterNode
	{
	}

	class DelimiterNodeToken
	{
		DelimiterNodeToken(Token token)
		{
			m_token = token;
		}

		Token Token { get { return m_token; } }

		private Token m_token;
	}

	class DelimiterNodeTree
	{
		DelimiterNodeTree(DelimiterTree tree)
		{
			m_tree = tree;
		}

		DelimiterTree Tree { get { return m_tree; } }

		private DelimiterTree m_tree;
	}
}
