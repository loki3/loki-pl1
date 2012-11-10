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

		internal static Value Do(List<string> rawLines, IScope parent)
		{
			// turn strings into lists ready to run
			List<DelimiterList> parsedLines = new List<DelimiterList>(rawLines.Count);
			foreach (string s in rawLines)
			{
				DelimiterList line = ParseLine.Do(s, parent);
				parsedLines.Add(line);
			}

			return Do(parsedLines, parent);
		}

		internal static Value Do(List<DelimiterList> parsedLines, IScope scope)
		{
			// run each line, returning value of last line
			Value retval = null;
			foreach (DelimiterList line in parsedLines)
				retval = EvalList.Do(line.Nodes, scope);
			return (retval == null ? new ValueNil() : retval);
		}
	}
}
