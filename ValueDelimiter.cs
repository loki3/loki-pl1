using System;
using System.Collections.Generic;

namespace loki3.core
{
	internal enum DelimiterType
	{
		AsComment,	// ignore contents
		AsString,	// leaves contents as a string
		AsValue,	// eval contents into a single value
		AsArray,	// eval each node, creating an array of values
		AsRaw,		// don't eval yet
	}

	/// <summary>
	/// Represents a delimiter function that operates on a list of nodes
	/// and returns a value
	/// </summary>
	internal class ValueDelimiter : Value
	{
		/// <summary>
		/// Specify the start and end strings for a delimiter
		/// </summary>
		internal ValueDelimiter(string start, string end)
		{
			Init(start, end, DelimiterType.AsValue, null);
		}
		/// <summary>
		/// Specify the start and end strings for a delimiter
		/// and whether contents should be tokenized or left as-is
		/// </summary>
		internal ValueDelimiter(string start, string end, DelimiterType type)
		{
			Init(start, end, type, null);
		}
		/// <summary>
		/// Delimited contents will be passed off to function to be evaluated
		/// </summary>
		/// <param name="function">function must take a next parameter of specified type</param>
		internal ValueDelimiter(string start, string end, DelimiterType type, ValueFunction function)
		{
			Init(start, end, type, function);
		}

		private ValueDelimiter() { }

		private void Init(string start, string end, DelimiterType type, ValueFunction function)
		{
			Map meta = WritableMetadata;
			meta[keyDelimStart] = new ValueString(start);
			meta[keyDelimEnd] = new ValueString(end);
			meta[keyDelimType] = new ValueInt((int)type);
			if (function != null)
				meta[keyDelimFunction] = function;
		}

		#region Value
		internal override ValueType Type
		{
			get { return ValueType.Delimiter; }
		}

		internal override bool Equals(Value v)
		{
			ValueDelimiter other = v as ValueDelimiter;
			return (other == null ? false : this == other);
		}

		internal override Value ValueCopy() { return new ValueDelimiter(); }
		#endregion

		#region Keys
		internal static string keyDelimStart = "l3.delim.start";
		internal static string keyDelimEnd = "l3.delim.end";
		internal static string keyDelimType = "l3.delim.type";
		internal static string keyDelimFunction = "l3.delim.function";
		#endregion

		/// <summary>characters used to start delimited section</summary>
		internal string Start { get { return Metadata[keyDelimStart].AsString; } }
		/// <summary>characters used to end delimited section, empty means use rest of line</summary>
		internal string End { get { return Metadata[keyDelimEnd].AsString; } }
		/// <summary>true if section should be tokenized, false for as-is</summary>
		internal DelimiterType DelimiterType { get { return (DelimiterType)Metadata[keyDelimType].AsInt; } }

		/// <summary>optional function to run delimited value through</summary>
		internal ValueFunction Function
		{
			get
			{
				return Metadata == null ? null : Metadata.GetOptional(keyDelimFunction, null) as ValueFunction;
			}
		}

		/// <summary>Represents an entire line</summary>
		static internal ValueDelimiter Line { get { return m_line; } }
		/// <summary>Basic "don't eval yet" delimiters</summary>
		static internal ValueDelimiter Basic { get { return m_basic; } }

		static private ValueDelimiter m_line = new ValueDelimiter("", "");
		static private ValueDelimiter m_basic = new ValueDelimiter("(", ")");
	}
}
