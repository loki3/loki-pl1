using System;
using System.Collections.Generic;

namespace loki3
{
	internal enum Precedence
	{
		Low,
		Medium,
		High,
	}

	/// <summary>
	/// Represents a constant value
	/// or a function that may take one or two values and returns a value
	/// </summary>
	internal class ValueFunction : Value
	{
		internal ValueFunction(bool bConsumesPrevious, bool bConsumesNext)
		{
			Init(bConsumesPrevious, bConsumesNext, Precedence.Medium);
		}
		internal ValueFunction(bool bConsumesPrevious, bool bConsumesNext, Precedence precedence)
		{
			Init(bConsumesPrevious, bConsumesNext, precedence);
		}

		private void Init(bool bConsumesPrevious, bool bConsumesNext, Precedence precedence)
		{
			Dictionary<string, Value> meta = WritableMetadata;
			meta[keyConsumesPrevious] = new ValueBool(bConsumesPrevious);
			meta[keyConsumesNext] = new ValueBool(bConsumesNext);
			meta[keyPrecedence] = new ValueInt((int)precedence);
		}

		#region Value
		internal override ValueType Type
		{
			get { return ValueType.Function; }
		}
		#endregion

		#region Keys
		internal static string keyConsumesPrevious = "l3.func.consumes-previous?";
		internal static string keyConsumesNext = "l3.func.consumes-next?";
		#endregion

		internal bool ConsumesPrevious { get { return Metadata[keyConsumesPrevious].AsBool; } }
		internal bool ConsumesNext { get { return Metadata[keyConsumesNext].AsBool; } }

		internal virtual Value Eval(DelimiterNode prev, DelimiterNode next, IFunctionRequestor functions, INodeRequestor nodes)
		{
			return null;
		}
	}
}
