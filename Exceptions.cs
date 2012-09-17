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

		/// <summary>Delimiter that was missing ending</summary>
		public Delimiter Delimiter { get { return m_delim; } }

		private Delimiter m_delim;
	}

	/// <summary>
	/// Token isn't a function or variable
	/// </summary>
	internal class MissingFunctionException : Exception
	{
		internal MissingFunctionException(Token token)
		{
			m_token = token;
		}

		public Token Token { get { return m_token; } }

		private Token m_token;
	}

	/// <summary>
	/// Evaluating a token would require an adjacent token that doesn't exist
	/// </summary>
	internal class MissingAdjacentValueException : Exception
	{
		internal MissingAdjacentValueException(Token token, bool bPrevious)
		{
			m_token = token;
			m_bPrevious = bPrevious;
		}

		/// <summary>Token that required an adjacent token</summary>
		public Token Token { get { return m_token; } }
		/// <summary>True if missing previous token was missing, false if it was next one</summary>
		public bool Previous { get { return m_bPrevious; } }

		private Token m_token;
		private bool m_bPrevious;
	}

	/// <summary>
	/// A value was requested, but token represents a function
	/// </summary>
	internal class NotValueException : Exception
	{
		internal NotValueException(Token token, bool bPrevious)
		{
			m_token = token;
			m_bPrevious = bPrevious;
		}

		/// <summary>Token that required an adjacent value</summary>
		public Token Token { get { return m_token; } }
		/// <summary>True if previous value was requested, false if it was next one</summary>
		public bool Previous { get { return m_bPrevious; } }

		private Token m_token;
		private bool m_bPrevious;
	}

	/// <summary>
	/// Value wasn't of requested type
	/// </summary>
	internal class WrongTypeException : Exception
	{
		internal WrongTypeException(ValueType requested, ValueType actual)
		{
			m_requested = requested;
			m_actual = actual;
		}

		/// <summary>Requested type</summary>
		public ValueType Requested { get { return m_requested; } }
		/// <summary>Actual type</summary>
		public ValueType Actual { get { return m_actual; } }

		private ValueType m_requested;
		private ValueType m_actual;
	}
}
