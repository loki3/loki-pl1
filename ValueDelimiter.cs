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
			meta[keyComment] = new ValueBool(false);
		}
		/// <summary>
		/// Specify the start and end strings for a delimiter
		/// and whether contents should be tokenized or left as-is
		/// </summary>
		internal ValueDelimiter(string start, string end, bool tokenize, bool comment)
		{
			Dictionary<string, Value> meta = WritableMetadata;
			meta[keyDelimStart] = new ValueString(start);
			meta[keyDelimEnd] = new ValueString(end);
			meta[keyTokenize] = new ValueBool(tokenize);
			meta[keyComment] = new ValueBool(comment);
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
		internal static string keyComment = "l3.delim.comment?";
		#endregion

		/// <summary>characters used to start delimited section</summary>
		internal string Start { get { return Metadata[keyDelimStart].AsString; } }
		/// <summary>characters used to end delimited section, empty means use rest of line</summary>
		internal string End { get { return Metadata[keyDelimEnd].AsString; } }
		/// <summary>true if section should be tokenized, false for as-is</summary>
		internal bool Tokenize { get { return Metadata[keyTokenize].AsBool; } }
		/// <summary>true if contents are a comment, i.e. they'll be ignored</summary>
		internal bool Comment { get { return Metadata[keyComment].AsBool; } }

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
