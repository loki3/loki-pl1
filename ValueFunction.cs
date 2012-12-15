using System;
using System.Collections.Generic;

namespace loki3.core
{
	/// <summary>
	/// Evaluation order
	/// </summary>
	internal enum Order
	{
		Highest,
		Higher,
		High,
		Medium,	// 3
		Low,
		Lower,
		Lowest,
	}

	/// <summary>
	/// Represents a constant value
	/// or a function that may take one or two values and returns a value
	/// </summary>
	internal abstract class ValueFunction : Value
	{
		/// <summary>
		/// Define info on the parameters and order the function will get run in
		/// </summary>
		/// <param name="previousPattern">if func consumes previous value, this describes the pattern</param>
		/// <param name="nextPattern">if func consumes next value, this describes the pattern</param>
		/// <param name="order">evaluation precedence of function within a list</param>
		protected void Init(Value previousPattern, Value nextPattern, Order order)
		{
			Map meta = WritableMetadata;
			if (previousPattern != null && !previousPattern.IsNil)
				meta[keyPreviousPattern] = previousPattern;
			if (nextPattern != null && !nextPattern.IsNil)
				meta[keyNextPattern] = nextPattern;
			meta[keyOrder] = new ValueInt((int)order);
		}
		protected void Init(Value previousPattern, Value nextPattern)
		{
			Init(previousPattern, nextPattern, Order.Medium);
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
		internal static string keyPreviousPattern = "l3.func.previous";
		internal static string keyNextPattern = "l3.func.next";
		internal static string keyBody = "l3.func.body";
		#endregion

		internal virtual bool ConsumesPrevious { get { return Metadata.ContainsKey(keyPreviousPattern); } }
		internal virtual bool ConsumesNext { get { return Metadata.ContainsKey(keyNextPattern); } }

		/// <summary>Retrieve the body off the argument</summary>
		internal static List<DelimiterList> GetBody(Value arg)
		{
			Map map = arg.AsMap;
			if (map == null || !map.ContainsKey(keyBody))
				return null;
			return map[keyBody].AsLine;
		}
		/// <summary>Does function require body?</summary>
		internal virtual bool RequiresBody()
		{
			if (!ConsumesNext)
				return false;
			Value next = Metadata[ValueFunction.keyNextPattern];
			if (!(next is ValueMap))
				return (next is ValueString && next.AsString == "l3.body");
			Map map = next.AsMap;
			return map.ContainsKey("body");
		}

		internal abstract Value Eval(DelimiterNode prev, DelimiterNode next, IScope scope, INodeRequestor nodes, ILineRequestor requestor);
	}

	/// <summary>
	/// Prefix function: _ func args
	/// </summary>
	internal abstract class ValueFunctionPre : ValueFunction
	{
		internal void Init(Value pattern) { Init(null, pattern); }
		internal void Init(Value pattern, Order order) { Init(null, pattern, order); }

		internal override Value Eval(DelimiterNode prev, DelimiterNode next, IScope scope, INodeRequestor nodes, ILineRequestor requestor)
		{
			if (next == null)
				return this;	// without parameters, it's still a function
			Value post = EvalNode.Do(next, scope, nodes, requestor);
			Value match, leftover;
			if (!PatternChecker.Do(post, Metadata[keyNextPattern], false/*bShortPat*/, out match, out leftover))
				throw new Loki3Exception().AddWrongPattern(Metadata[keyNextPattern], post);
			if (leftover != null)
				// create a partial function that starts w/ match & still needs leftover
				return new PartialFunctionPre(this, match, leftover);
			return Eval(match, scope);
		}

		internal abstract Value Eval(Value arg, IScope scope);
	}

	/// <summary>
	/// Postfix function: args func _
	/// </summary>
	internal abstract class ValueFunctionPost : ValueFunction
	{
		internal void Init(Value pattern) { Init(pattern, null); }
		internal void Init(Value pattern, Order order) { Init(pattern, null, order); }

		internal override Value Eval(DelimiterNode prev, DelimiterNode next, IScope scope, INodeRequestor nodes, ILineRequestor requestor)
		{
			if (prev == null)
				return this;	// without parameters, it's still a function
			Value pre = EvalNode.Do(prev, scope, nodes, requestor);
			Value match, leftover;
			if (!PatternChecker.Do(pre, Metadata[keyPreviousPattern], false/*bShortPat*/, out match, out leftover))
				throw new Loki3Exception().AddWrongPattern(Metadata[keyPreviousPattern], pre);
			if (leftover != null)
				// create a partial function that starts w/ match & still needs leftover
				return new PartialFunctionPost(this, match, leftover);
			return Eval(match, scope);
		}

		internal abstract Value Eval(Value arg, IScope scope);
	}
}
