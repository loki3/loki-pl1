using System;
using System.Collections.Generic;

namespace loki3.core
{
	internal class Map
	{
		internal Map() { }
		internal Map(Dictionary<string, Value> dict) { m_val = dict; }

		internal bool ContainsKey(string key)
		{
			return m_val != null && m_val.ContainsKey(key);
		}
		internal int Count
		{
			get { return (m_val == null ? 0 : m_val.Count); }
		}
		internal Dictionary<string, Value> Raw
		{
			get { return m_val; }
		}

		/// <summary>Get a value by key</summary>
		internal Value this[string key]
		{
			get { return m_val[key]; }
			set
			{
				if (m_val == null)
					m_val = new Dictionary<string, Value>();
				m_val[key] = value;
			}
		}

		internal bool TryGetValue(string p, out Value val)
		{
			if (m_val == null)
			{
				val = null;
				return false;
			}
			return m_val.TryGetValue(p, out val);
		}

		/// <summary>Return value if present, else return a default</summary>
		internal Value GetOptional(string key, Value ifmissing)
		{
			if (m_val == null)
				return ifmissing;
			Value value;
			if (!m_val.TryGetValue(key, out value))
				return ifmissing;
			return value;
		}

		/// <summary>Return value if present, else return a default</summary>
		internal T GetOptionalT<T>(string key, T ifmissing)
		{
			if (m_val == null)
				return ifmissing;
			Value value;
			if (!m_val.TryGetValue(key, out value))
				return ifmissing;
			ValueBase<T> valueBase = value as ValueBase<T>;
			if (valueBase == null)
				return ifmissing;
			return valueBase.GetValue();
		}

		/// <summary>True if the same key/value pairs are present in both</summary>
		internal bool Equals(Map v)
		{
			Map other = v as Map;
			if (other == null)
				return false;
			int count = (m_val == null ? 0 : m_val.Count);
			int otherCount = (other.m_val == null ? 0 : other.m_val.Count);
			if (count != otherCount)
				return false;
			if (count == 0)
				return true;
			Dictionary<string, Value>.KeyCollection keys = m_val.Keys;
			foreach (string key in keys)
			{
				Value val;
				if (!other.m_val.TryGetValue(key, out val))
					return false;
				if (!m_val[key].Equals(val))
					return false;
			}
			return true;
		}

		internal Map Copy()
		{
			Map map = new Map();
			map.m_val = new Dictionary<string,Value>(m_val);
			return map;
		}

		public override string ToString()
		{
			string s = "{ ";
			if (m_val != null)
			{
				Dictionary<string, Value>.KeyCollection keys = m_val.Keys;
				bool bFirst = true;
				foreach (string key in keys)
				{
					if (bFirst)
						bFirst = false;
					else
						s += " , ";
					s += ":" + key + " ";

					ValueMap checkMap = m_val[key] as ValueMap;
					if (checkMap != null && checkMap.AsMap == this)
						s += "<self>";	// can't print self
					else
						s += m_val[key].ToFinalString();
				}
			}
			s += " }";
			return s;
		}

		private Dictionary<string, Value> m_val = null;
	}
}
