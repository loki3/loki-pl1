
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
			Init(paramMap, scope, bCreate, false/*bOverload*/);
		}
		internal PatternAssign(Map paramMap, IScope scope)
		{
			bool bCreate = paramMap["create?"].AsBool;
			bool bOverload = paramMap["overload?"].AsBool;
			Init(paramMap, scope, bCreate, bOverload);
		}

		/// <param name="paramMap">map of parameters, including :key and optionally :scope or :map</param>
		/// <param name="scope">owning scope</param>
		/// <param name="bCreate">whether new variable should be created or existing one reused</param>
		void Init(Map paramMap, IScope scope, bool bCreate, bool bOverload)
		{
			// key must be a type that can be used for pattern matching
			Value key = paramMap["key"];

			m_key = key;
			m_bCreate = bCreate;
			m_bOverload = bOverload;

			// scope we're going to modify
			m_scope = Utility.GetScopeToModify(paramMap, scope, true/*bIncludeMap*/);
		}

		/// <summary>
		/// Pattern match against value and fill in appropriate variables
		/// </summary>
		/// <param name="value">value to pattern match against</param>
		/// <param name="bInitOnly">if true and key already exists, don't change value</param>
		/// <returns>true if values were assigned</returns>
		internal bool Assign(Value value, bool bInitOnly)
		{
			// find the matches in the pattern, ignoring the leftover
			Value match, leftover;
			bool bMatch = PatternChecker.Do(value, m_key, true/*bShortPat*/, out match, out leftover);

			// if PatternChecker says it was a match, but there aren't any details,
			// it was a trivial match - report success
			if (bMatch && match == null && leftover == null)
				return true;

			// add/modify scope
			if (match != null && leftover == null)
				return Utility.AddToScope(m_key, match, m_scope, m_bCreate, m_bOverload, bInitOnly);
			if (m_key is ValueArray && value.IsNil)
			{	// special case: initialize every token in array to nil
				foreach (Value v in m_key.AsArray)
					Utility.AddToScope(v, value, m_scope, m_bCreate, m_bOverload, bInitOnly);
				return m_key.AsArray.Count > 0;
			}
			return false;
		}
		internal bool Assign(Value value)
		{
			return Assign(value, false/*bInitOnly*/);
		}

		private Value m_key;
		private IScope m_scope;
		private bool m_bCreate;
		private bool m_bOverload;
	}
}
