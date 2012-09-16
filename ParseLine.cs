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
		/// If 'start' is a starting delimiter, returns delimiter, else null
		/// </summary>
		Delimiter GetDelim(char start);

		/// <summary>
		/// If 'start' is a starting delimiter, returns delimiter, else null
		/// </summary>
		Delimiter GetDelim(string start);
	}

	/// <summary>
	/// Interface for getting the following line(s) if needed
	/// </summary>
	interface ILineRequestor
	{
		string GetNextLine();
	}


	/// <summary>
	/// Parses a single line of tokens separated by white space.
	/// Single char delimiters can be part of a token.
	/// Multiple char delimiters must be stand alone tokens.
	/// </summary>
	internal class ParseLine
	{
		/// <summary>
		/// Creates delimiter tree from string.  All delimiters must be closed.
		/// </summary>
		/// <param name="str">line to parse</param>
		/// <param name="delims">used to ask questions about delimiters</param>
		internal static DelimiterTree Do(string str, IParseLineDelimiters delims)
		{
			return Do(str, delims, null);
		}

		/// <summary>
		/// Creates delimiter tree from string,
		/// can request lines if missing closing delimiter
		/// </summary>
		/// <param name="str">line to parse</param>
		/// <param name="requestor">used to ask for additional lines if needed, may be null</param>
		/// <param name="delims">used to ask questions about delimiters</param>
		internal static DelimiterTree Do(string str, IParseLineDelimiters delims, ILineRequestor requestor)
		{
			char[] separators = { ' ', '\n', '\r', '\t' };
			string[] strs = str.Split(separators, StringSplitOptions.RemoveEmptyEntries);
			int end;
			return Do(strs, 0, Delimiter.Line, delims, requestor, out end);
		}

		private static DelimiterTree Do(string[] strs, int iStart, Delimiter thisDelim,
			IParseLineDelimiters delims, ILineRequestor requestor, out int iEnd)
		{
			List<DelimiterNode> nodes = new List<DelimiterNode>();
			for (int i = iStart; i < strs.Length; i++)
			{
				string s = strs[i];

				// is this the end of current set of delimited tokens?
				if (s == thisDelim.End)
				{	// end delimiter
					iEnd = i;
					return new DelimiterTree(thisDelim, nodes);
				}

				// is it a stand alone starting delimiter?
				Delimiter subDelim = (delims == null ? null : delims.GetDelim(s));
				if (subDelim != null)
				{	// start delimiter
					int end;
					DelimiterTree subtree = Do(strs, i + 1, subDelim, delims, requestor, out end);
					DelimiterNodeTree node = new DelimiterNodeTree(subtree);
					nodes.Add(node);
					i = end;	// skip past the subtree
				}
				else
				{	// stand alone token
					Token token = new Token(s);
					DelimiterNode node = new DelimiterNodeToken(token);
					nodes.Add(node);
				}
			}

			// didn't find closing delimiter, eventually we'll request the next line
			if (thisDelim != Delimiter.Line)
				throw new UndelimitedException(thisDelim);

			iEnd = strs.Length;
			return new DelimiterTree(thisDelim, nodes);
		}
	}
}
