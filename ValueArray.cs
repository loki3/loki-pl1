using System;
using System.Collections.Generic;

namespace loki3.core
{
	/// <summary>
	/// Represents an array of values
	/// </summary>
	class ValueArray : Value
	{
		internal ValueArray(List<Value> values)
		{
			m_values = values;
		}

		#region Value
		internal override ValueType Type
		{
			get { return ValueType.Array; }
		}

		internal override bool Equals(Value v)
		{
			ValueArray other = v as ValueArray;
			if (other == null)
				return false;
			int count = m_values.Count;
			if (count != other.m_values.Count)
				return false;
			for (int i = 0; i < count; i++)
				if (!m_values[i].Equals(other.m_values[i]))
					return false;
			return true;
		}

		internal override List<Value> AsArray { get { return m_values; } }
		#endregion


		private List<Value> m_values;
	}
}
