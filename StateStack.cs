using System;
using System.Collections.Generic;

namespace loki3
{
	/// <summary>Used to get a named function</summary>
	interface IStack : IParseLineDelimiters
	{
		/// <summary>Returns null if requested token doesn't exist</summary>
		Value GetValue(Token token);
	}

	/// <summary>
	/// Stores functions and delimiters.
	/// Returns its own functions first, deferring to parent if not found.
	/// </summary>
	internal class StateStack : IStack, IParseLineDelimiters
	{
		internal StateStack(IStack parent)
		{
			m_parent = parent;
		}

		internal void AddValue(string name, Value v)
		{
			m_values[name] = v;
		}

		#region IStack Members

		public Value GetValue(Token token)
		{
			Value val;
			if (m_values.TryGetValue(token.Value, out val))
				return val;
			return m_parent.GetValue(token);
		}

		public ValueDelimiter GetDelim(string start)
		{
			return null;
		}

		#endregion

		private IStack m_parent;
		private Dictionary<string, Value> m_values = null;
	}
}
