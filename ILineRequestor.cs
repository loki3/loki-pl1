using System.Collections.Generic;

namespace loki3.core
{
	/// <summary>
	/// Interface for getting the following line(s) if needed
	/// </summary>
	interface ILineRequestor
	{
		/// <summary>Get line requestor is currently on, or null if none</summary>
		string GetCurrentLine();

		/// <summary>Advance to next line</summary>
		void Advance();

		/// <summary>Are we on a valid line?</summary>
		bool HasCurrent();
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
		}
		internal LineConsumer(List<string> lines)
		{
			m_count = lines.Count;
			m_lines = new string[m_count];
			int i = 0;
			foreach (string line in lines)
				m_lines[i++] = line;
		}
		internal LineConsumer(string[] lines)
		{
			m_count = lines.GetLength(0);
			m_lines = lines;
		}

		#region ILineRequestor Members
		public string GetCurrentLine()
		{
			return (m_current < m_count ? m_lines[m_current] : null);
		}

		public void Advance()
		{
			m_current++;
		}

		public bool HasCurrent()
		{
			return (m_current < m_count);
		}
		#endregion

		private string[] m_lines;
		private int m_count;
		private int m_current = 0;
	}
}
