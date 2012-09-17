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
			Dictionary<string, Value> meta = WritableMetadata;
			meta[needsPrevious] = new ValueBool(bNeedsPrevious);
			meta[needsNext] = new ValueBool(bNeedsNext);
		}

		#region Value
		internal override ValueType Type
		{
			get { return ValueType.Function; }
		}
		#endregion

		#region Keys
		internal static string needsPrevious = "needs-previous";
		internal static string needsNext = "needs-next";
		#endregion

		internal bool NeedsPrevious { get { return Metadata[needsPrevious].AsBool; } }
		internal bool NeedsNext { get { return Metadata[needsNext].AsBool; } }

		internal virtual Value Eval(Value prev, Value next)
		{
			return null;
		}
	}
}
