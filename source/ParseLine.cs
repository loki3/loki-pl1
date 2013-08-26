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
		/// 'anyToken' will be true if a token named 'start' exists at all
		/// </summary>
		ValueDelimiter GetDelim(string start, out bool anyToken);
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
			int indent = Utility.CountIndent(str);
			int end;
			return Do(indent, str, strs, 0, ValueDelimiter.Line, delims, requestor, out end);
		}

		private static DelimiterList Do(int indent, string original, string[] strs, int iStart, ValueDelimiter thisDelim,
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
				iEnd = -1;
				for (int i = iStart; i < strs.Length; i++)
				{
					if (strs[i] == thisDelim.End)
					{
						iEnd = i;
						break;
					}
				}

				// no specified end delim means take the remainder of the line
				if (thisDelim.End == "")
					iEnd = strs.Length;

				// if we found end, wrap entire string in a single node
				if (iEnd != -1)
				{
					string subStr = GetSubStr(iStart, iEnd, strs);
					Token token = new Token(subStr);
					DelimiterNode node = new DelimiterNodeToken(token);
					nodes.Add(node);
					return new DelimiterList(thisDelim, nodes, indent, strs[iStart - 1], subStr);
				}
			}
			else // Value, Array && Raw
			{	// handle as individual tokens and nested lists
				for (int i = iStart; i < strs.Length; i++)
				{
					string s = strs[i];

					// is this the end of current set of delimited tokens?
					if (s == thisDelim.End)
					{	// end delimiter
						iEnd = i;
						string subStr = GetSubStr(iStart, iEnd, strs);
						return new DelimiterList(thisDelim, nodes, indent, strs[iStart-1], subStr);
					}

					// is it a stand alone starting delimiter?
					bool bAnyToken = false;
					ValueDelimiter subDelim = (delims == null ? null : delims.GetDelim(s, out bAnyToken));
					string[] strsToUse = strs;
					bool bExtra = true;
					if (subDelim == null && !bAnyToken)
					{	// whole thing wasn't a delimiter, function, etc., how about the 1st char?
						string s1 = s.Substring(0, 1);
						subDelim = (delims == null ? null : delims.GetDelim(s1, out bAnyToken));
						if (subDelim != null)
						{	// copy across array, but break iStart into delim & remainder
							bExtra = false;
							strsToUse = new string[strs.Length + 1];
							for (int j = 0; j < i; j++)
								strsToUse[j] = strs[j];
							strsToUse[i] = s1;
							strsToUse[i + 1] = s.Substring(1);
							for (int j = i + 1; j < strs.Length; j++)
								strsToUse[j + 1] = strs[j];
						}
					}
					if (subDelim != null)
					{	// start delimiter
						int end;
						DelimiterList sublist = Do(0, original, strsToUse, i + 1, subDelim, delims, requestor, out end);
						if (sublist != null)
						{
							DelimiterNodeList node = new DelimiterNodeList(sublist);
							nodes.Add(node);
						}
						// skip past the sublist
						i = (bExtra ? end : end - 1);
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
			if (thisDelim != ValueDelimiter.Line && thisDelim.End != "")
				throw new Loki3Exception().AddMissingEndDelimiter(thisDelim);

			iEnd = strs.Length;
			string trimmed = original.TrimStart(' ', '\t');
			return new DelimiterList(thisDelim, nodes, indent, "", trimmed);
		}

		/// <summary>
		/// Get a portion of the nodes as a string
		/// TODO: recreate from the original string so whitespace is preserved
		/// </summary>
		private static string GetSubStr(int start, int end, string[] strs)
		{
			string subStr = "";
			for (int i = start; i < end; i++)
			{
				if (i != start)
					subStr += " ";
				subStr += strs[i];
			}
			return subStr;
		}
	}
}
