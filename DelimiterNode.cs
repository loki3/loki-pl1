using System;

namespace loki3
{
	internal abstract class DelimiterNode
	{
		internal virtual Token Token { get { return null; } }
		internal virtual DelimiterList List { get { return null; } }
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

	internal class DelimiterNodeList : DelimiterNode
	{
		internal DelimiterNodeList(DelimiterList tree)
		{
			m_list = tree;
		}

		internal override DelimiterList List { get { return m_list; } }

		private DelimiterList m_list;
	}
}
