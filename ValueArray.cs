using System;
using System.Collections.Generic;

namespace loki3.core
{
	/// <summary>
	/// Represents an array of values
	/// </summary>
	class ValueArray : ValueBase<List<Value>>
	{
		internal ValueArray(List<Value> values)
		{
			m_val = values;
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
			int count = m_val.Count;
			if (count != other.m_val.Count)
				return false;
			for (int i = 0; i < count; i++)
				if (!m_val[i].Equals(other.m_val[i]))
					return false;
			return true;
		}

		internal override List<Value> AsArray { get { return m_val; } }
		#endregion

		/// <summary>Combine a and b into a new array</summary>
		internal static ValueArray Combine(ValueArray a, ValueArray b)
		{
			List<Value> list = new List<Value>();
			foreach (Value v in a.AsArray)
				list.Add(v);
			foreach (Value v in b.AsArray)
				list.Add(v);
			return new ValueArray(list);
		}
	}
}
