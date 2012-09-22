using System;
using System.Collections.Generic;

namespace loki3.core
{
	/// <summary>
	/// Interface for asking simple questions about delimiters
	/// </summary>
	interface IParseLineDelimiters
	{
		/// <summary>
		/// If 'start' is a starting delimiter, returns delimiter, else null
		/// </summary>
		ValueDelimiter GetDelim(string start);
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
		/// Creates delimiter list from string.  All delimiters must be closed.
		/// </summary>
		/// <param name="str">line to parse</param>
		/// <param name="delims">used to ask questions about delimiters</param>
		internal static DelimiterList Do(string str, IParseLineDelimiters delims)
		{
			return Do(str, delims, null);
		}

		/// <summary>
		/// Creates delimiter list from string,
		/// can request lines if missing closing delimiter
		/// </summary>
		/// <param name="str">line to parse</param>
		/// <param name="requestor">used to ask for additional lines if needed, may be null</param>
		/// <param name="delims">used to ask questions about delimiters</param>
		internal static DelimiterList Do(string str, IParseLineDelimiters delims, ILineRequestor requestor)
		{
			char[] separators = { ' ', '\n', '\r', '\t' };
			string[] strs = str.Split(separators, StringSplitOptions.RemoveEmptyEntries);
			int end;
			return Do(strs, 0, ValueDelimiter.Line, delims, requestor, out end);
		}

		private static DelimiterList Do(string[] strs, int iStart, ValueDelimiter thisDelim,
			IParseLineDelimiters delims, ILineRequestor requestor, out int iEnd)
		{
			List<DelimiterNode> nodes = new List<DelimiterNode>();

			DelimiterType type = thisDelim.DelimiterType;
			if (type == DelimiterType.AsComment)
			{	// ignore everything up to the end delimiter
				for (int i = iStart; i < strs.Length; i++)
				{
					if (strs[i] == thisDelim.End)
					{
						iEnd = i;
						return null;
					}
				}
			}
			else if (type == DelimiterType.AsString)
			{	// simply search for end and stuff everything in the middle into a single token
				string all = "";
				for (int i = iStart; i < strs.Length; i++)
				{
					string s = strs[i];
					if (s == thisDelim.End)
					{	// end - wrap entire string in a single node
						iEnd = i;
						Token token = new Token(all);
						DelimiterNode node = new DelimiterNodeToken(token);
						nodes.Add(node);
						return new DelimiterList(thisDelim, nodes);
					}
					if (i != iStart)
						all += " ";
					all += s;
				}
			}
			else
			{	// handle as individual tokens and nested lists
				for (int i = iStart; i < strs.Length; i++)
				{
					string s = strs[i];

					// is this the end of current set of delimited tokens?
					if (s == thisDelim.End)
					{	// end delimiter
						iEnd = i;
						return new DelimiterList(thisDelim, nodes);
					}

					// is it a stand alone starting delimiter?
					ValueDelimiter subDelim = (delims == null ? null : delims.GetDelim(s));
					if (subDelim != null)
					{	// start delimiter
						int end;
						DelimiterList sublist = Do(strs, i + 1, subDelim, delims, requestor, out end);
						if (sublist != null)
						{
							DelimiterNodeList node = new DelimiterNodeList(sublist);
							nodes.Add(node);
						}
						i = end;	// skip past the sublist
					}
					else
					{	// stand alone token
						Token token = new Token(s);
						DelimiterNode node = new DelimiterNodeToken(token);
						nodes.Add(node);
					}
				}
			}

			// didn't find closing delimiter, TODO request the next line
			if (thisDelim != ValueDelimiter.Line)
				throw new UndelimitedException(thisDelim);

			iEnd = strs.Length;
			return new DelimiterList(thisDelim, nodes);
		}
	}
}
