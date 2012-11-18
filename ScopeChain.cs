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

		/// <summary>Optional name for the scope</summary>
		string Name { get; set; }

		/// <summary>Returns scope token exists on in the scope chain, or null</summary>
		IScope Exists(string token);

		/// <summary>Get parent scope, if any</summary>
		IScope Parent { get; }

		/// <summary>Get current scope as a map</summary>
		Map AsMap { get; }
	}

	/// <summary>
	/// Stores functions and delimiters.
	/// Returns its own functions first, deferring to parent if not found.
	/// </summary>
	internal class ScopeChain : IScope
	{
		/// <summary>Chain a new scope off a parent scope</summary>
		internal ScopeChain(IScope parent)
		{
			m_parent = parent;
			m_values = new Map();
		}

		/// <summary>Treat a map as a scope</summary>
		internal ScopeChain(Map map)
		{
			m_parent = null;
			m_values = map;
		}

		/// <summary>Create a scope with no parent</summary>
		internal ScopeChain()
		{
			m_parent = null;
			m_values = new Map();
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

		public string Name
		{
			get { return m_name; }
			set { m_name = value; }
		}

		public ValueDelimiter GetDelim(string start)
		{
			Value val;
			if (m_values.TryGetValue(start, out val))
				return val as ValueDelimiter;
			return (m_parent != null ? m_parent.GetDelim(start) : null);
		}

		public IScope Exists(string token)
		{
			if (m_values.ContainsKey(token))
				return this;
			return (m_parent == null ? null : m_parent.Exists(token));
		}

		public IScope Parent
		{
			get { return m_parent; }
		}

		public Map AsMap
		{
			get { return m_values; }
		}

		#endregion

		private IScope m_parent;
		private Map m_values = new Map();
		private string m_name = "";
	}
}
