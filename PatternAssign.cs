
namespace loki3.core
{
	internal class PatternAssign
	{
		/// <summary>
		/// Create an object that can be reused for pattern matching against
		/// a pattern and filling in variables appropriately
		/// </summary>
		internal PatternAssign(Map paramMap, IScope scope, bool bCreate)
		{
			Init(paramMap, scope, bCreate);
		}
		internal PatternAssign(Map paramMap, IScope scope)
		{
			bool bCreate = paramMap["create?"].AsBool;
			Init(paramMap, scope, bCreate);
		}

		/// <param name="paramMap">map of parameters, including :key and optionally :scope or :map</param>
		/// <param name="scope">owning scope</param>
		/// <param name="bCreate">whether new variable should be created or existing one reused</param>
		void Init(Map paramMap, IScope scope, bool bCreate)
		{
			// key must be a type that can be used for pattern matching
			Value key = paramMap["key"];
			if (key.Type != ValueType.String && key.Type != ValueType.Array && key.Type != ValueType.Map)
				throw new Loki3Exception().AddWrongType(ValueType.String, key.Type);

			m_key = key;
			m_bCreate = bCreate;

			// scope we're going to modify
			m_scope = Utility.GetScopeToModify(paramMap, scope, true/*bIncludeMap*/);
		}

		/// <summary>
		/// Pattern match against value and fill in appropriate variables
		/// </summary>
		/// <param name="value">value to pattern match against</param>
		/// <returns>origina value</returns>
		internal Value Assign(Value value)
		{
			// find the matches in the pattern, ignoring the leftover
			Value match, leftover;
			PatternChecker.Do(value, m_key, true/*bShortPat*/, out match, out leftover);

			// add/modify scope
			if (match != null)
				Utility.AddToScope(m_key, match, m_scope, m_bCreate);
			else if (m_key is ValueArray && value.IsNil)
			{	// special case: initialize every token in array to nil
				foreach (Value v in m_key.AsArray)
					Utility.AddToScope(v, value, m_scope, m_bCreate);
			}
			return value;
		}

		private Value m_key;
		private IScope m_scope;
		private bool m_bCreate;
	}
}
