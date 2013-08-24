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
		string Name { get; }

		/// <summary>Returns scope token exists on in the scope chain, or null</summary>
		IScope Exists(string token);

		/// <summary>Get parent scope, if any</summary>
		IScope Parent { get; }

		/// <summary>Get current scope as a value</summary>
		Value AsValue { get; }

		/// <summary>Get current scope as a map</summary>
		Map AsMap { get; }

		/// <summary>Set the name of the child function called from this scope</summary>
		string FunctionName { set; }
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
			Init(parent, new Map());
		}

		/// <summary>Treat a map as a scope</summary>
		internal ScopeChain(Map map)
		{
			Init(null, map);
		}

		/// <summary>Create a scope with no parent</summary>
		internal ScopeChain()
		{
			Init(null, new Map());
		}

		private void Init(IScope parent, Map map)
		{
			m_parent = parent;
			m_values = map;
			m_valueMap = new ValueMap(m_values);
			m_valueMap.Scope = this;
			m_valueMap.WritableMetadata[keyParent] = (parent == null ? ValueNil.Nil : parent.AsValue);
			m_valueMap.WritableMetadata[Value.keyType] = s_type;
			m_valueMap.WritableMetadata[keyName] = s_defaultName;
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
			get { return m_valueMap.Metadata == null ? "" : m_valueMap.Metadata.GetOptionalT(keyName, ""); }
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

		public Value AsValue
		{
			get { return m_valueMap; }
		}

		public Map AsMap
		{
			get { return m_values; }
		}

		public string FunctionName
		{
			set { m_valueMap.WritableMetadata[keyCalledFunction] = new ValueString(value); }
		}

		#endregion

		/// <summary>Sets an optional function pointer for the scope</summary>
		public ValueFunction Function
		{
			set { m_valueMap.WritableMetadata[keyFunction] = value; }
		}

		#region Keys
		internal static string keyName = "l3.scope.name";
		internal static string keyFunction = "l3.scope.function";
		internal static string keyCalledFunction = "l3.scope.calledFunction";
		internal static string keyParent = "l3.scope.parent";
		internal static string keyModules = "l3.scope.modules";
		#endregion

		private static Value s_type = new ValueString("scope");
		private static Value s_defaultName = new ValueString("");

		private IScope m_parent;
		private Map m_values;
		private ValueMap m_valueMap;
	}
}
