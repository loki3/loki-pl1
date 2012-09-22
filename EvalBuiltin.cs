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
			try
			{
				int i = Int32.Parse(s);
				return new ValueInt(i);
			}
			catch (Exception) { }

			// try as float
			try
			{
				double d = Double.Parse(s);
				return new ValueFloat(d);
			}
			catch (Exception) { }

			// give up
			throw new UnrecognizedTokenException(token);
		}
	}
}
