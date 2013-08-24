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
			int indent = -1;
			for (int i = 0; i < count; i++)
			{
				DelimiterList line = parsedLines[i];
				if (indent == -1)
					indent = line.Indent;
				if (indent == line.Indent)
				{	// only run lines that are at the original indentation
					ILineRequestor requestor = Utility.GetSubLineRequestor(parsedLines, i, out i);
					try
					{
						retval = EvalList.Do(line.Nodes, scope, requestor);
					}
					catch (Loki3Exception e)
					{	// this line is the correct context if there isn't already one there
						if (!e.Errors.ContainsKey(Loki3Exception.keyLineContents))
							e.AddLine(line.Original);
						if (requestor != null && !e.Errors.ContainsKey(Loki3Exception.keyLineNumber))
							e.AddLineNumber(requestor.GetCurrentLineNumber());
						throw e;
					}
				}
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
