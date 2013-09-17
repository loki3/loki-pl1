using System;
using System.Collections.Generic;
using System.Text;

namespace loki3.core
{
	/// <summary>
	/// Break a string into an array of strings.
	/// Breaks on whitespace (including comma) and treats
	/// string delimiters specially, pretending like they
	/// were whitespace delimited.
	/// </summary>
	internal class ParseChars
	{
		internal static string[] Do(string s, IParseLineDelimiters delims)
		{
			List<string> list = new List<string>();
			StringBuilder current = new StringBuilder();
			bool atStart = true;
			bool inWord = false;
			bool inString = false;
			char endString = ' ';
			int maxDelimLen = 0;

			// find single char string delims
			//!! fetch from delims
			List<string> quotes = new List<string>();
			Dictionary<string, ValueDelimiter> stringDelims = (delims == null ? null : delims.GetStringDelims());
			if (stringDelims != null)
			{
				foreach (string key in stringDelims.Keys)
				{
					quotes.Add(key);
					if (key.Length > maxDelimLen)
						maxDelimLen = key.Length;
				}
			}

			// parse
			int len = s.Length;
			for (int i = 0; i < len; i++)
			{
				char c = s[i];
				if (inString)
				{
					if (c == endString)
					{	// end of string
						if (current.Length > 0)
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
						string cAsStr = c.ToString();
						if (quotes.Contains(cAsStr))
						{	// start of string

							// find the longest starting delim
							int additional = 0;
							for (int j = 1; j < maxDelimLen; j++)
							{
								if (i + j + 1 > len)
									break;
								string substr = s.Substring(i, j + 1);
								if (quotes.Contains(substr))
								{
									cAsStr = substr;
									additional = j;
								}
							}
							i += additional;

							list.Add(cAsStr);
							//!! should allow multi-char
							string endDelim = stringDelims[cAsStr].End;
							endString = (endDelim == null ? '\0' : endDelim[0]);
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

			if (inWord || inString)
				list.Add(current.ToString());

			return list.ToArray();
		}

		/// <summary>chars to consider as separators</summary>
		static private string s_white = " \n\r\t,";
	}
}
