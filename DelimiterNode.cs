using System;

namespace loki3.core
{
	internal abstract class DelimiterNode
	{
		internal virtual Token Token { get { return null; } }
		internal virtual DelimiterList List { get { return null; } }
		internal virtual Value Value { get { return null; } }
	}

	/// <summary>Unevaluated token</summary>
	internal class DelimiterNodeToken : DelimiterNode
	{
		internal DelimiterNodeToken(Token token)
		{
			m_token = token;
		}

		internal override Token Token { get { return m_token; } }

		public  override string ToString() { return m_token.Value; }

		private Token m_token;
	}

	/// <summary>Unevaluated list of nodes</summary>
	internal class DelimiterNodeList : DelimiterNode
	{
		internal DelimiterNodeList(DelimiterList tree)
		{
			m_list = tree;
		}

		internal override DelimiterList List { get { return m_list; } }

		private DelimiterList m_list;
	}

	/// <summary>Value that's already been evaluated</summary>
	internal class DelimiterNodeValue : DelimiterNode
	{
		internal DelimiterNodeValue(Value value)
		{
			m_value = value;
		}

		internal override Value Value { get { return m_value; } }

		private Value m_value;
	}
}
