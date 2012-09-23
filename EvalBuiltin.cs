using System;
using System.Collections.Generic;

namespace loki3.core
{
	internal class EvalBuiltin
	{
		/// <summary>
		/// Attempt to evaluate token as a bool, int, etc.  May throw.
		/// </summary>
		internal static Value Do(Token token)
		{
			string s = token.Value;

			// try as nil
			if (s == "nil")
				return new ValueNil();

			// try as bool
			if (s == "true")
				return new ValueBool(true);
			if (s == "false")
				return new ValueBool(false);

			// try as int
			int i;
			if (Int32.TryParse(s, out i))
				return new ValueInt(i);

			// try as float
			double d;
			if (Double.TryParse(s, out d))
				return new ValueFloat(d);

			// try as string
			if (s.StartsWith(":"))
				return new ValueString(s.Substring(1));

			// give up
			throw new UnrecognizedTokenException(token);
		}
	}
}
