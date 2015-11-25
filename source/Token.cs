using System;

namespace loki3.core
{
	/// <summary>
	/// Unevaluated string
	/// </summary>
	internal class Token
	{
		internal Token(string token)
		{
			m_token = token;
		}

		internal string Value { get { return m_token; } }

		public override string ToString() { return m_token; }

		private string m_token;
	}
}
