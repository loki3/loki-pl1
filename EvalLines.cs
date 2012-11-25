using System.Collections.Generic;

namespace loki3.core
{
	/// <summary>Evaluate a series of lines</summary>
	internal class EvalLines
	{
		/// <summary>Evaluate lines till we run out</summary>
		internal static Value Do(ILineRequestor requestor, IScope scope)
		{
			int lineNumber = 0;
			try
			{
				Value value = null;
				while (requestor.HasCurrent())
				{
					lineNumber = requestor.GetCurrentLineNumber();
					value = EvalLines.DoOne(requestor, scope);
				}
				return value;
			}
			catch (Loki3Exception e)
			{
				e.AddLineNumber(lineNumber);
				throw e;
			}
		}

		/// <summary>
		/// Evaluate line and body if needed.
		/// 'requestor' is positioned on the next line to eval.
		/// </summary>
		internal static Value DoOne(ILineRequestor requestor, IScope scope)
		{
			DelimiterList list = requestor.GetCurrentLine(scope);
			try
			{
				Value value = EvalList.Do(list.Nodes, scope, requestor);

				// if only item on line was a func, see if it needs a body
				ValueFunction func = value as ValueFunction;
				if (func != null && func.RequiresBody())
					// if line created a partial function that needs a body,
					// eval all subsequent indented lines
					value = EvalList.DoAddBody(func, scope, requestor);

				// if only item was a map w/ one value, see if it needs a body
				if (func == null && value is ValueMap)
				{
					Map map = value.AsMap;
					if (map.Raw.Count == 1)
					{
						string key = "";
						foreach (string k in map.Raw.Keys)
							key = k;
						func = map[key] as ValueFunction;
						if (func != null && func.RequiresBody())
							map[key] = EvalList.DoAddBody(func, scope, requestor);
					}
				}

				// if only item was an array, see if last item needs a body
				if (func == null && value is ValueArray)
				{
					List<Value> array = value.AsArray;
					if (array.Count > 0)
					{
						func = array[array.Count - 1] as ValueFunction;
						if (func != null && func.RequiresBody())
							array[array.Count - 1] = EvalList.DoAddBody(func, scope, requestor);
					}
				}

				requestor.Advance();

				return value;
			}
			catch (Loki3Exception e)
			{
				e.AddLine(list.ToString());
				throw e;
			}
		}
	}
}
