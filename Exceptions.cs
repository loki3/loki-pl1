using System;

namespace loki3
{
	/// <summary>
	/// A start delimiter was found without an end delimiter
	/// </summary>
	internal class UndelimitedException : Exception
	{
		internal UndelimitedException(ValueDelimiter delim)
		{
			m_delim = delim;
		}

		/// <summary>Delimiter that was missing ending</summary>
		public ValueDelimiter Delimiter { get { return m_delim; } }

		private ValueDelimiter m_delim;
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
	/// Could not parse token
	/// </summary>
	internal class UnrecognizedTokenException : Exception
	{
		internal UnrecognizedTokenException(Token token)
		{
			m_token = token;
		}

		/// <summary>Token that required an adjacent value</summary>
		public Token Token { get { return m_token; } }

		private Token m_token;
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
