using System.Collections.Generic;

namespace loki3.core
{
	internal class EvalLines
	{
		/// <summary>Evaluate lines till we run out</summary>
		internal static Value Do(ILineRequestor requestor, IScope scope)
		{
			Value value = null;
			string line = requestor.GetNextLine();
			while (line != null)
			{
				bool bGetNext = true;

				DelimiterList list = ParseLine.Do(line, scope, requestor);
				value = EvalList.Do(list.Nodes, scope);

				// if line created a partial function that needs a body,
				// eval all subsequent indented lines
				if (value is ValueFunction /* && function requires body */)
				{
					ValueFunctionPre function = value as ValueFunctionPre;
					int parentIndent = Utility.CountIndent(line);
					List<Value> body = new List<Value>();
					while (true)
					{
						string childLine = requestor.GetNextLine();
						int childIndent = (childLine == null ? -1 : Utility.CountIndent(childLine));
						if (childIndent <= parentIndent)
						{	// we've built the entire body - now pass it to function
							if (body.Count == 0)
								throw new MissingBodyException(function);
							Map map = new Map();
							map[ValueFunction.keyBody] = new ValueArray(body);
							value = function.Eval(new ValueMap(map), new ScopeChain(scope));

							// the last line we got was immediately after the body,
							// so we still need to use it (or it could be null)
							line = childLine;
							bGetNext = false;
							break;
						}

						// keep adding to the body
						body.Add(new ValueString(childLine));
					}
				}

				if (bGetNext)
					line = requestor.GetNextLine();
			}
			return value;
		}
	}
}
