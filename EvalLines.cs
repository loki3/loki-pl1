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

				ValueFunction func = value as ValueFunction;
				if (func != null && func.RequiresBody())
					// if line created a partial function that needs a body,
					// eval all subsequent indented lines
					value = EvalList.DoAddBody(func, scope, requestor);

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
