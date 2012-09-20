using System;
using System.Collections.Generic;

namespace loki3
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

		internal override List<Value> AsArray { get { return m_values; } }
		#endregion


		private List<Value> m_values;
	}
}
