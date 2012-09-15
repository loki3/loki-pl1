using System;

namespace loki3
{
	internal abstract class DelimiterNode
	{
		abstract internal string Value { get; }
	}

	internal class DelimiterNodeToken : DelimiterNode
	{
		internal DelimiterNodeToken(Token token)
		{
			m_token = token;
		}

		internal Token Token { get { return m_token; } }
		internal override string Value { get { return m_token.Value; } }

		private Token m_token;
	}

	internal class DelimiterNodeTree : DelimiterNode
	{
		internal DelimiterNodeTree(DelimiterTree tree)
		{
			m_tree = tree;
		}

		internal DelimiterTree Tree { get { return m_tree; } }
		internal override string Value { get { return ""; } }

		private DelimiterTree m_tree;
	}
}
