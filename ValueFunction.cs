using System;
using System.Collections.Generic;

namespace loki3
{
	/// <summary>
	/// Represents a constant value
	/// or a function that may take one or two values and returns a value
	/// </summary>
	internal class ValueFunction : Value
	{
		internal ValueFunction(bool bNeedsPrevious, bool bNeedsNext)
		{
			m_bNeedsPrevious = bNeedsPrevious;
			m_bNeedsNext = bNeedsNext;
		}

		#region Value
		internal override ValueType Type
		{
			get { return ValueType.Function; }
		}
		#endregion

		internal bool NeedsPrevious { get { return m_bNeedsPrevious; } }
		internal bool NeedsNext { get { return m_bNeedsNext; } }

		internal virtual Value Eval(Value prev, Value next)
		{
			return null;
		}

		private bool m_bNeedsPrevious;
		private bool m_bNeedsNext;
	}
}
