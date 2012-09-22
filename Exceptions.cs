using System;

namespace loki3.core
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

		/// <summary>Token that couldn't be parsed</summary>
		public Token Token { get { return m_token; } }

		private Token m_token;
	}

	/// <summary>
	/// Value wasn't of requested type
	/// </summary>
	internal class WrongTypeException : Exception
	{
		internal WrongTypeException(ValueType expected, ValueType actual)
		{
			m_expected = expected;
			m_actual = actual;
		}

		/// <summary>Expected type</summary>
		public ValueType Expected { get { return m_expected; } }
		/// <summary>Actual type</summary>
		public ValueType Actual { get { return m_actual; } }

		private ValueType m_expected;
		private ValueType m_actual;
	}

	/// <summary>
	/// Function requires a different number of items in array
	/// </summary>
	internal class WrongSizeArray : Exception
	{
		internal WrongSizeArray(int expected, int actual)
		{
			m_expected = expected;
			m_actual = actual;
		}

		/// <summary>Expected number</summary>
		public int Expected { get { return m_expected; } }
		/// <summary>Actual number</summary>
		public int Actual { get { return m_actual; } }

		private int m_expected;
		private int m_actual;
	}
}
