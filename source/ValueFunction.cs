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
		internal static string keyCreateScope = "l3.func.createScope?";
		internal static string keyForceEval = "l3.func.forceEval?";
		#endregion

		internal virtual bool ConsumesPrevious { get { return Metadata.ContainsKey(keyPreviousPattern); } }
		internal virtual bool ConsumesNext { get { return Metadata.ContainsKey(keyNextPattern); } }
		internal virtual bool ForceEval { get { return Metadata.ContainsKey(keyForceEval); } }
		internal virtual bool ShouldCreateScope
		{
			get { return (Metadata != null && Metadata.ContainsKey(keyCreateScope) ? Metadata[keyCreateScope].AsBool : true); }
		}

		/// <summary>Retrieve the body off the argument</summary>
		internal static List<DelimiterList> ExtractBody(Value arg)
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

		/// <summary>If this is a user defined function, get the body</summary>
		internal virtual List<DelimiterList> GetBody(IScope scope) { return null; }

		internal abstract Value Eval(DelimiterNode prev, DelimiterNode next, IScope paramScope, IScope bodyScope, INodeRequestor nodes, ILineRequestor requestor);
	}

	/// <summary>
	/// Prefix function: _ func args
	/// </summary>
	internal abstract class ValueFunctionPre : ValueFunction
	{
		internal void Init(Value pattern) { Init(null, pattern); }
		internal void Init(Value pattern, Order order) { Init(null, pattern, order); }

		internal override Value Eval(DelimiterNode prev, DelimiterNode next, IScope paramScope, IScope bodyScope, INodeRequestor nodes, ILineRequestor requestor)
		{
			if (next == null)
				return this;	// without parameters, it's still a function
			Value post = EvalNode.Do(next, paramScope, nodes, requestor);
			Value match, leftover;
			if (!PatternChecker.Do(post, Metadata[keyNextPattern], false/*bShortPat*/, out match, out leftover))
				throw new Loki3Exception().AddWrongPattern(Metadata[keyNextPattern], post);
			if (leftover != null)
				// create a partial function that starts w/ match & still needs leftover
				return new PartialFunctionPre(this, match, leftover);
			Value retval = Eval(match, bodyScope);
			bodyScope.Exit();
			return retval;
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

		internal override Value Eval(DelimiterNode prev, DelimiterNode next, IScope paramScope, IScope bodyScope, INodeRequestor nodes, ILineRequestor requestor)
		{
			if (prev == null)
				return this;	// without parameters, it's still a function
			Value pre = EvalNode.Do(prev, paramScope, nodes, requestor);
			Value match, leftover;
			if (!PatternChecker.Do(pre, Metadata[keyPreviousPattern], false/*bShortPat*/, out match, out leftover))
				throw new Loki3Exception().AddWrongPattern(Metadata[keyPreviousPattern], pre);
			if (leftover != null)
				// create a partial function that starts w/ match & still needs leftover
				return new PartialFunctionPost(this, match, leftover);
			Value retval = Eval(match, bodyScope);
			bodyScope.Exit();
			return retval;
		}

		internal abstract Value Eval(Value arg, IScope scope);
	}
}
