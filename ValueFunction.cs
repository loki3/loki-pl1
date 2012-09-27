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
		/// <summary>
		/// Define info on the parameters and order the function will get run in
		/// </summary>
		/// <param name="previousPattern">if func consumes previous value, this describes the pattern</param>
		/// <param name="nextPattern">if func consumes next value, this describes the pattern</param>
		/// <param name="precedence">evaluation precedence of function within a list</param>
		protected void Init(Value previousPattern, Value nextPattern, Precedence precedence)
		{
			Map meta = WritableMetadata;
			if (previousPattern != null)
				meta[keyPreviousPattern] = previousPattern;
			if (nextPattern != null)
				meta[keyNextPattern] = nextPattern;
			meta[keyPrecedence] = new ValueInt((int)precedence);
		}
		protected void Init(Value previousPattern, Value nextPattern)
		{
			Init(previousPattern, nextPattern, Precedence.Medium);
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
		#endregion

		internal bool ConsumesPrevious { get { return Metadata.ContainsKey(keyPreviousPattern); } }
		internal bool ConsumesNext { get { return Metadata.ContainsKey(keyNextPattern); } }

		internal abstract Value Eval(DelimiterNode prev, DelimiterNode next, IScope scope, INodeRequestor nodes);
	}

	/// <summary>
	/// Prefix function: _ func args
	/// </summary>
	internal abstract class ValueFunctionPre : ValueFunction
	{
		internal void Init(Value pattern) { Init(null, pattern); }
		internal void Init(Value pattern, Precedence precedence) { Init(null, pattern, precedence); }

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
		internal void Init(Value pattern) { Init(pattern, null); }
		internal void Init(Value pattern, Precedence precedence) { Init(pattern, null, precedence); }

		internal override Value Eval(DelimiterNode prev, DelimiterNode next, IScope scope, INodeRequestor nodes)
		{
			return Eval(prev, scope, nodes);
		}
		internal abstract Value Eval(DelimiterNode prev, IScope scope, INodeRequestor nodes);
	}
}
