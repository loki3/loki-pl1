using System;

namespace loki3.core
{
	internal class Loki3Exception : Exception
	{
		/// <summary>A start delimiter was found without an end delimiter</summary>
		internal Loki3Exception AddMissingEndDelimiter(ValueDelimiter delim)
		{
			m_map[keyMissingEndDelimiter] = delim;
			return this;
		}

		/// <summary>A function required a body but a body wasn't provided</summary>
		internal Loki3Exception AddMissingBody(ValueFunction function)
		{
			m_map[keyMissingBody] = function;
			return this;
		}

		/// <summary>Could not parse token</summary>
		internal Loki3Exception AddBadToken(Token token)
		{
			m_map[keyBadToken] = new ValueString(token.Value);
			return this;
		}

		/// <summary>Value wasn't of requested type</summary>
		internal Loki3Exception AddWrongType(ValueType expected, ValueType actual)
		{
			m_map[keyExpectedType] = new ValueInt((int)expected);
			m_map[keyActualType] = new ValueInt((int)actual);
			return this;
		}

		/// <summary>The passed-in value didn't match the expected pattern</summary>
		internal Loki3Exception AddWrongPattern(Value expected, Value actual)
		{
			m_map[keyExpectedPattern] = expected;
			m_map[keyActualPattern] = actual;
			return this;
		}

		/// <summary>Get collection of all errors</summary>
		internal Map Errors { get { return m_map; } }

		internal ValueType ExpectedType { get { return (ValueType)m_map.GetOptional<int>(keyExpectedType, 0); } }
		internal ValueType ActualType { get { return (ValueType)m_map.GetOptional<int>(keyActualType, 0); } }

		#region Keys
		/// <summary>Delimiter that was missing ending</summary>
		internal static string keyMissingEndDelimiter = "l3.error.missingEndDelimiter";
		/// <summary>Function that needed a body</summary>
		internal static string keyMissingBody = "l3.error.missingBody";
		/// <summary>Token that couldn't be parsed</summary>
		internal static string keyBadToken = "l3.error.badToken";
		/// <summary>Expected type</summary>
		internal static string keyExpectedType = "l3.error.expectedType";
		/// <summary>Actual type</summary>
		internal static string keyActualType = "l3.error.actualType";
		/// <summary>Expected pattern</summary>
		internal static string keyExpectedPattern = "l3.error.expectedPattern";
		/// <summary>Actual pattern</summary>
		internal static string keyActualPattern = "l3.error.actualPattern";
		#endregion

		private Map m_map = new Map();
	}
}
