using System;
using System.Collections.Generic;

namespace loki3
{
	/// <summary>Used to get a named function</summary>
	interface IFunctionRequestor
	{
		/// <summary>Returns null if requested token doesn't exist</summary>
		ValueFunction Get(Token token);
	}

	/// <summary>Used to request the previous or next values</summary>
	interface IValueRequestor
	{
		/// <summary>Returns null if previous value doesn't exist</summary>
		Value GetPrevious();
		/// <summary>Returns null if next value doesn't exist</summary>
		Value GetNext();
	}

	/// <summary>
	/// Evaluates a token, returning a value based on a function or variable.
	/// May request the previous or next tokens in the token list.
	/// </summary>
	internal class EvalToken
	{
		/// <summary>
		/// Evaluates a token, possibly requesting the previous and next values.
		/// Returns a value.
		/// </summary>
		/// <param name="token">token representing a function or variable</param>
		/// <param name="functions">used to request a function</param>
		/// <param name="values">used to request previous and next values</param>
		internal static Value Do(Token token, IFunctionRequestor functions, IValueRequestor values)
		{
			ValueFunction function = functions.Get(token);
			if (function == null)
				throw new MissingFunctionException(token);

			// get previous & next values if needed
			Value previous = null;
			if (function.NeedsPrevious)
			{
				previous = values.GetPrevious();
				if (previous == null)
					throw new MissingAdjacentValueException(token, true/*bPrevious*/);
			}
			Value next = null;
			if (function.NeedsNext)
			{
				next = values.GetNext();
				if (next == null)
					throw new MissingAdjacentValueException(token, false/*bPrevious*/);
			}

			// evaluate
			return function.Eval(previous, next);
		}
	}
}
