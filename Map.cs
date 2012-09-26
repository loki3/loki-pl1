using System;
using System.Collections.Generic;

namespace loki3.core
{
	internal class Map
	{
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
		internal T GetOptional<T>(string key, T ifmissing)
		{
			if (m_val == null)
				return ifmissing;
			Value value;
			if (!m_val.TryGetValue(key, out value))
				return ifmissing;
			ValueBase<T> valueBase = value as ValueBase<T>;
			return valueBase.GetValue();
		}

		/// <summary>True if the same key/value pairs are present in both</summary>
		internal bool Equals(Map v)
		{
			Map other = v as Map;
			if (other == null)
				return false;
			int count = m_val.Count;
			if (count != other.m_val.Count)
				return false;
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

		public override string ToString() { return m_val.ToString(); }

		private Dictionary<string, Value> m_val = null;
	}
}
