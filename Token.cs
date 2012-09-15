using System;

namespace loki3
{
	/// <summary>
	/// Unevaluated string
	/// </summary>
	class Token
	{
		Token(string token)
		{
			m_token = token;
		}

		string Value { get { return m_token; } }

		private string m_token;
	}
}
