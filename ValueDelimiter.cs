using System;
using System.Collections.Generic;

namespace loki3
{
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
			Dictionary<string, Value> meta = WritableMetadata;
			meta[keyDelimStart] = new ValueString(start);
			meta[keyDelimEnd] = new ValueString(end);
			meta[keyTokenize] = new ValueBool(true);
		}
		/// <summary>
		/// Specify the start and end strings for a delimiter
		/// and whether contents should be tokenized or left as-is
		/// </summary>
		internal ValueDelimiter(string start, string end, bool tokenize)
		{
			Dictionary<string, Value> meta = WritableMetadata;
			meta[keyDelimStart] = new ValueString(start);
			meta[keyDelimEnd] = new ValueString(end);
			meta[keyTokenize] = new ValueBool(tokenize);
		}

		#region Value
		internal override ValueType Type
		{
			get { return ValueType.Delimiter; }
		}
		#endregion

		#region Keys
		internal static string keyDelimStart = "l3.delim.start";
		internal static string keyDelimEnd = "l3.delim.end";
		internal static string keyTokenize = "l3.delim.tokenize?";
		#endregion

		internal string Start { get { return Metadata[keyDelimStart].AsString; } }
		internal string End { get { return Metadata[keyDelimEnd].AsString; } }
		internal bool Tokenize { get { return Metadata[keyTokenize].AsBool; } }

		internal virtual Value Eval(List<DelimiterNode> nodes, IFunctionRequestor functions)
		{
			return null;
		}

		/// <summary>Represents an entire line</summary>
		static internal ValueDelimiter Line { get { return m_line; } }
		/// <summary>Basic "don't eval yet" delimiters</summary>
		static internal ValueDelimiter Basic { get { return m_basic; } }

		static private ValueDelimiter m_line = new ValueDelimiter("", "");
		static private ValueDelimiter m_basic = new ValueDelimiter("(", ")");
	}
}
