using System;

namespace loki3
{
	/// <summary>
	/// A pair of strings that start & end a list of tokens,
	/// has an associated function
	/// </summary>
	internal class Delimiter
	{
		internal Delimiter(string start, string end)
		{
			m_start = start;
			m_end = end;
		}

		internal string Start { get { return m_start; } }
		internal string End { get { return m_end; } }

		private string m_start;
		private string m_end;


		/// <summary>Basic "don't eval yet" delimiters</summary>
		static internal Delimiter Basic { get { return m_basic; } }
		/// <summary>String delimiters, variation 1</summary>
		static internal Delimiter String1 { get { return m_string1; } }
		/// <summary>String delimiters, variation 2</summary>
		static internal Delimiter String2 { get { return m_string2; } }

		static private Delimiter m_basic = new Delimiter("(", ")");
		static private Delimiter m_string1 = new Delimiter("\"", "\"");
		static private Delimiter m_string2 = new Delimiter("'", "'");
	}
}
