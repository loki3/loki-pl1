using System.Collections.Generic;

namespace loki3.core
{
	/// <summary>
	/// Interface for getting the following line(s) if needed
	/// </summary>
	interface ILineRequestor
	{
		/// <summary>Get line requestor is currently on, or null if none</summary>
		DelimiterList GetCurrentLine(IParseLineDelimiters delims);
		int GetCurrentLineNumber();

		/// <summary>Advance to next line</summary>
		void Advance();
		/// <summary>Back up a line</summary>
		void Rewind();

		/// <summary>Are we on a valid line?</summary>
		bool HasCurrent();

		/// <summary>Is this just a subset of all lines?</summary>
		bool IsSubset();
	}


	/// <summary>Takes a list of string values and returns them one at a time</summary>
	internal class LineConsumer : ILineRequestor
	{
		internal LineConsumer(List<Value> lines)
		{
			m_count = lines.Count;
			m_lines = new string[m_count];
			int i = 0;
			foreach (Value line in lines)
				m_lines[i++] = line.AsString;
			m_parsedLines = new List<DelimiterList>();
		}
		internal LineConsumer(List<string> lines)
		{
			Init(lines);
		}
		internal LineConsumer(List<string> lines, bool isSubset)
		{
			Init(lines);
			m_isSubset = isSubset;
		}
		internal LineConsumer(string[] lines)
		{
			m_count = lines.GetLength(0);
			m_lines = lines;
			m_parsedLines = new List<DelimiterList>();
		}

		internal LineConsumer(List<DelimiterList> lines)
		{
			m_count = lines.Count;
			m_lines = null;
			m_parsedLines = lines;
		}
		internal LineConsumer(List<DelimiterList> lines, bool isSubset)
		{
			m_count = lines.Count;
			m_lines = null;
			m_parsedLines = lines;
			m_isSubset = isSubset;
		}


		private void Init(List<string> lines)
		{
			m_count = lines.Count;
			m_lines = new string[m_count];
			int i = 0;
			foreach (string line in lines)
				m_lines[i++] = line;
			m_parsedLines = new List<DelimiterList>();
		}

		#region ILineRequestor Members
		public DelimiterList GetCurrentLine(IParseLineDelimiters delims)
		{
			if (m_current >= m_count)
				return null;
			// see if line was already parsed
			DelimiterList parsedLine = (m_current < m_parsedLines.Count ? m_parsedLines[m_current] : null);
			if (parsedLine == null)
			{	// if not, parse it now
				parsedLine = ParseLine.Do(m_lines[m_current], delims);
				m_parsedLines.Add(parsedLine);
			}
			return parsedLine;
		}

		public int GetCurrentLineNumber()
		{
			return m_current + 1;
		}

		public void Advance()
		{
			m_current++;
		}

		public void Rewind()
		{
			m_current--;
		}

		public bool HasCurrent()
		{
			return (m_current < m_count);
		}

		public bool IsSubset()
		{
			return m_isSubset;
		}
		#endregion

		private string[] m_lines;
		private List<DelimiterList> m_parsedLines = null;
		private int m_count;
		private int m_current = 0;
		private bool m_isSubset = false;
	}
}
