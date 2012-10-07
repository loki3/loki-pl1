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

			bool requiresBody = false;
			if (value is ValueFunction)
			{
				ValueFunction func = value as ValueFunction;
				requiresBody = func.RequiresBody();
			}

			// if line created a partial function that needs a body,
			// eval all subsequent indented lines
			if (requiresBody)
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
					throw new Loki3Exception().AddMissingBody(function);
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
