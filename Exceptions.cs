using System;

namespace loki3
{
	/// <summary>
	/// A start delimiter was found without an end delimiter
	/// </summary>
	internal class UndelimitedException : Exception
	{
		internal UndelimitedException(Delimiter delim)
		{
			m_delim = delim;
		}

		private Delimiter m_delim;
	}
}
