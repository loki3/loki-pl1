using System.Collections.Generic;

namespace loki3.core
{
	/// <summary>Evaluate a series of lines</summary>
	internal class EvalLines
	{
		/// <summary>Evaluate lines till we run out</summary>
		internal static Value Do(ILineRequestor requestor, IScope scope)
		{
			Value value = null;
			while (requestor.HasCurrent())
				value = EvalLines.DoOne(requestor, scope);
			return value;
		}

		/// <summary>
		/// Evaluate line and body if needed.
		/// 'requestor' is positioned on the next line to eval.
		/// </summary>
		internal static Value DoOne(ILineRequestor requestor, IScope scope)
		{
			string line = requestor.GetCurrentLine();
			DelimiterList list = ParseLine.Do(line, scope, requestor);
			Value value = EvalList.Do(list.Nodes, scope);

			// if line created a partial function that needs a body,
			// eval all subsequent indented lines
			if (value is ValueFunction /* && function requires body */)
			{
				ValueFunctionPre function = value as ValueFunctionPre;
				int parentIndent = Utility.CountIndent(line);
				List<Value> body = new List<Value>();
				while (requestor.HasCurrent())
				{
					requestor.Advance();
					string childLine = requestor.GetCurrentLine();
					int childIndent = (childLine == null ? -1 : Utility.CountIndent(childLine));
					if (childIndent <= parentIndent)
						break;	// now we have the body

					// keep adding to the body
					body.Add(new ValueString(childLine));
				}

				// we've built the entire body - now pass it to function
				if (body.Count == 0)
					throw new MissingBodyException(function);
				Map map = new Map();
				map[ValueFunction.keyBody] = new ValueArray(body);
				value = function.Eval(new ValueMap(map), new ScopeChain(scope));
			}
			else
			{
				requestor.Advance();
			}

			return value;
		}
	}
}
