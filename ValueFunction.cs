using System;
using System.Collections.Generic;

namespace loki3.core
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
	internal abstract class ValueFunction : Value
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

		internal override bool Equals(Value v)
		{
			ValueFunction other = v as ValueFunction;
			return (other == null ? false : this == other);
		}
		#endregion

		#region Keys
		internal static string keyConsumesPrevious = "l3.func.consumes-previous?";
		internal static string keyConsumesNext = "l3.func.consumes-next?";
		#endregion

		internal bool ConsumesPrevious { get { return Metadata[keyConsumesPrevious].AsBool; } }
		internal bool ConsumesNext { get { return Metadata[keyConsumesNext].AsBool; } }

		internal abstract Value Eval(DelimiterNode prev, DelimiterNode next, IScope scope, INodeRequestor nodes);
	}

	/// <summary>
	/// Prefix function: _ func args
	/// </summary>
	internal abstract class ValueFunctionPre : ValueFunction
	{
		internal ValueFunctionPre() : base(false/*bConsumesPrevious*/, true/*bConsumesNext*/) { }
		internal ValueFunctionPre(Precedence precedence) : base(false/*bConsumesPrevious*/, true/*bConsumesNext*/, precedence) { }

		internal override Value Eval(DelimiterNode prev, DelimiterNode next, IScope scope, INodeRequestor nodes)
		{
			return Eval(next, scope, nodes);
		}
		internal abstract Value Eval(DelimiterNode next, IScope scope, INodeRequestor nodes);
	}

	/// <summary>
	/// Postfix function: args func _
	/// </summary>
	internal abstract class ValueFunctionPost : ValueFunction
	{
		internal ValueFunctionPost() : base(true/*bConsumesPrevious*/, false/*bConsumesNext*/) { }
		internal ValueFunctionPost(Precedence precedence) : base(true/*bConsumesPrevious*/, false/*bConsumesNext*/, precedence) { }

		internal override Value Eval(DelimiterNode prev, DelimiterNode next, IScope scope, INodeRequestor nodes)
		{
			return Eval(prev, scope, nodes);
		}
		internal abstract Value Eval(DelimiterNode prev, IScope scope, INodeRequestor nodes);
	}

	/// <summary>
	/// Infix function: args1 func args2
	/// </summary>
	internal abstract class ValueFunctionIn : ValueFunction
	{
		internal ValueFunctionIn() : base(true/*bConsumesPrevious*/, true/*bConsumesNext*/) { }
		internal ValueFunctionIn(Precedence precedence) : base(true/*bConsumesPrevious*/, true/*bConsumesNext*/, precedence) { }
	}
}
