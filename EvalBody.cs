using System;
using System.Collections.Generic;

namespace loki3.core
{
	/// <summary>
	/// Evaluate a series of lines
	/// </summary>
	internal class EvalBody
	{
		internal static Value Do(List<Value> valueLines, IScope parent)
		{
			// run each line, returning value of last line
			Value retval = null;
			int count = valueLines.Count;
			for (int i = 0; i < count; i++)
			{
				Value v = valueLines[i];
				if (v is ValueString)
				{
					ILineRequestor requestor = Utility.GetSubLineRequestor(valueLines, i, out i);
					DelimiterList line = ParseLine.Do(v.AsString, parent);
					retval = EvalList.Do(line.Nodes, parent, requestor);
				}
				else if (v is ValueRaw)
				{
					ValueRaw raw = v as ValueRaw;
					retval = EvalList.Do(raw.GetValue().Nodes, parent);
				}
				else
				{
					throw new Loki3Exception().AddBadLine(v);
				}
			}
			return (retval == null ? new ValueNil() : retval);
		}

		internal static Value Do(List<DelimiterList> parsedLines, IScope scope)
		{
			// run each line, returning value of last line
			Value retval = null;
			int count = parsedLines.Count;
			for (int i = 0; i < count; i++)
			{
				DelimiterList line = parsedLines[i];
				ILineRequestor requestor = Utility.GetSubLineRequestor(parsedLines, i, out i);
				retval = EvalList.Do(line.Nodes, scope, requestor);
			}
			return (retval == null ? new ValueNil() : retval);
		}

		internal static Value Do(Value lines, IScope scope)
		{
			if (lines is ValueArray)
				return Do(lines.AsArray, scope);
			return Do(lines.AsLine, scope);
		}
	}
}
