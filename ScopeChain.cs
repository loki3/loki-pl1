using System;
using System.Collections.Generic;

namespace loki3.core
{
	/// <summary>Used to get a named values</summary>
	interface IScope : IParseLineDelimiters
	{
		/// <summary>Returns null if requested token doesn't exist</summary>
		Value GetValue(Token token);

		/// <summary>Stores a value on a token</summary>
		void SetValue(string token, Value value);
	}

	/// <summary>
	/// Stores functions and delimiters.
	/// Returns its own functions first, deferring to parent if not found.
	/// </summary>
	internal class ScopeChain : IScope, IParseLineDelimiters
	{
		internal ScopeChain(IScope parent)
		{
			m_parent = parent;
		}

		#region IScope Members

		public void SetValue(string token, Value value)
		{
			m_values[token] = value;
		}

		public Value GetValue(Token token)
		{
			Value val;
			if (m_values.TryGetValue(token.Value, out val))
				return val;
			return (m_parent != null ? m_parent.GetValue(token) : null);
		}

		public ValueDelimiter GetDelim(string start)
		{
			Value val;
			if (m_values.TryGetValue(start, out val))
				return val as ValueDelimiter;
			return (m_parent != null ? m_parent.GetDelim(start) : null);
		}

		#endregion

		private IScope m_parent;
		private Dictionary<string, Value> m_values = new Dictionary<string,Value>();
	}
}
