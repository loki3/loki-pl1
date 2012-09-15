using System;
using System.Collections.Generic;

namespace loki3
{
	/// <summary>
	/// Interface for asking simple questions about delimiters
	/// </summary>
	interface IParseLineDelimiters
	{
		/// <summary>
		/// If 'start' is a starting delimiter, returns ending delimiter
		/// else returns Char.MinValue
		/// </summary>
		char GetEndDelim(char start);

		/// <summary>
		/// If 'start' is a starting delimiter, returns ending delimiter
		/// else returns null
		/// </summary>
		string GetEndDelim(string start);
	}

	/// <summary>
	/// Interface for getting the following line(s) if needed
	/// </summary>
	interface ILineRequestor
	{
		string GetNextLine();
	}


	/// <summary>
	/// Parses a single line
	/// </summary>
	class ParseLine
	{
		/// <summary>
		/// Creates delimiter tree from string.  All delimiters must be closed.
		/// </summary>
		/// <param name="str">line to parse</param>
		/// <param name="delims">used to ask questions about delimiters</param>
		static DelimiterTree Do(string str, IParseLineDelimiters delims)
		{
			return Do(str, null);
		}

		/// <summary>
		/// Creates delimiter tree from string,
		/// can request lines if missing closing delimiter
		/// </summary>
		/// <param name="str">line to parse</param>
		/// <param name="requestor">used to ask for additional lines if needed, may be null</param>
		/// <param name="delims">used to ask questions about delimiters</param>
		static DelimiterTree Do(string str, IParseLineDelimiters delims, ILineRequestor requestor)
		{
			return null;
		}
	}
}
