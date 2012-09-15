using System;

namespace loki3
{
	/// <summary>
	/// A pair of strings that start & end a list of tokens,
	/// has an associated function
	/// </summary>
	class Delimiter
	{
		Delimiter(string start, string end)
		{
			m_start = start;
			m_end = end;
		}

		string Start { get { return m_start; } }
		string End { get { return m_end; } }

		private string m_start;
		private string m_end;
	}
}
