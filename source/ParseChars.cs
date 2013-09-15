using System;
using System.Collections.Generic;
using System.Text;

namespace loki3.core
{
	interface ITemp
	{
		/// <summary>Get all the string delimiters</summary>
		ValueDelimiter[] GetStringDelims();
	}

	/// <summary>
	/// Break a string into an array of strings.
	/// Breaks on whitespace (including comma) and treats
	/// string delimiters specially, pretending like they
	/// were whitespace delimited.
	/// </summary>
	internal class ParseChars
	{
		internal static string[] Do(string s)
		{
			List<string> list = new List<string>();
			StringBuilder current = new StringBuilder();
			bool atStart = true;
			bool inWord = false;
			bool inString = false;
			char endString = '"';

			foreach (char c in s)
			{
				if (inString)
				{
					if (c == endString)
					{	// end of string
						list.Add(current.ToString());
						list.Add(c.ToString());
						inString = inWord = false;
						atStart = true;
						current = new StringBuilder();
					}
					else
					{
						current.Append(c);
					}
				}
				else if (s_white.IndexOf(c) == -1)
				{	// non-white
					if (atStart)
					{
						atStart = false;
						if (s_quotes.IndexOf(c) != -1)
						{	// start of string
							list.Add(c.ToString());
							endString = c;	//!! pull from delimiter
							inString = true;
							continue;
						}
					}
					current.Append(c);
					inWord = true;
				}
				else if (inWord)
				{	// end of word
					list.Add(current.ToString());
					inWord = false;
					atStart = true;
					current = new StringBuilder();
				}
			}

			if (inWord)
				list.Add(current.ToString());

			return list.ToArray();
		}

		/// <summary>chars to consider as separators</summary>
		static private string s_white = " \n\r\t";

		static private string s_quotes = "'\"";	//!! pass in
	}
}
