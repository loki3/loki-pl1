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
			// need a list of strings, not values
			List<string> rawLines = new List<string>(valueLines.Count);
			foreach (Value v in valueLines)
				rawLines.Add(v.AsString);

			return Do(rawLines, parent);
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

		internal static Value Do(List<DelimiterList> parsedLines, IScope parent)
		{
			ScopeChain scope = new ScopeChain(parent);

			// run each line, returning value of last line
			Value retval = null;
			foreach (DelimiterList line in parsedLines)
				retval = EvalList.Do(line.Nodes, scope);
			return (retval == null ? new ValueNil() : retval);
		}
	}
}
