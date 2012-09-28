using System;
using System.Collections.Generic;

namespace loki3.core
{
	internal class PatternChecker
	{
		/// <summary>
		/// Checks the contents of a value against a pattern,
		/// filling in 'match' with the results,
		/// and 'leftover' with the portions that weren't in the pattern.
		/// Returns false if there was no match at all
		/// </summary>
		/// <param name="input">input value to match against</param>
		/// <param name="pattern">pattern to apply against input value</param>
		/// <param name="match">portions that match</param>
		/// <param name="leftover">portions that don't match</param>
		internal static bool Do(Value input, Value pattern, out Value match, out Value leftover)
		{
			match = null;
			leftover = null;

			switch (input.Type)
			{
				case ValueType.Nil:
					return DoSingle(input as ValueNil, ValueType.Nil, pattern as ValueString, out match);

				case ValueType.Bool:
					return DoSingle(input as ValueBool, ValueType.Bool, pattern as ValueString, out match);
				case ValueType.Int:
					return DoSingle(input as ValueInt, ValueType.Int, pattern as ValueString, out match);
				case ValueType.Float:
					return DoSingle(input as ValueFloat, ValueType.Float, pattern as ValueString, out match);
				case ValueType.String:
					return DoSingle(input as ValueString, ValueType.String, pattern as ValueString, out match);

				case ValueType.Array:
					return Do(input as ValueArray, pattern as ValueArray, out match, out leftover);
				case ValueType.Map:
					return Do(input as ValueMap, pattern as ValueMap, out match, out leftover);

				case ValueType.Function:
					break;	// to do
				case ValueType.Delimiter:
					break;	// to do
			}

			return false;
		}

		private static bool DoSingle<T>(T input, ValueType target, ValueString pattern, out Value match) where T : Value
		{
			match = null;
			if (pattern == null)
				return false;	// this only matches against a single item
			PatternData data = new PatternData(pattern);
			ValueType type = data.ValueType;
			if (type != ValueType.Nil && type != target)
				return false;	// they asked for a different type
			match = input;
			return true;
		}

		private static bool Do(ValueArray input, ValueArray pattern, out Value match, out Value leftover)
		{
			match = null;
			leftover = null;
			if (pattern == null)
				return false;	// this only matches against another array

			List<Value> inarray = input.AsArray;
			List<Value> patarray = pattern.AsArray;
			int incount = inarray.Count;
			int patcount = patarray.Count;
			if (patcount > incount)
				return false;	// pattern has more elements than input - not a match

			// matches
			List<Value> matchlist = new List<Value>();
			for (int i = 0; i < patcount; i++)
			{
				Value submatch, subleftover;
				if (!Do(inarray[i], patarray[i], out submatch, out subleftover))
					return false;
				matchlist.Add(submatch);
			}
			match = new ValueArray(matchlist);

			// leftover
			if (incount > patcount)
			{
				List<Value> leftoverlist = new List<Value>();
				for (int i = patcount; i < incount; i++)
					leftoverlist.Add(inarray[i]);
				leftover = new ValueArray(leftoverlist);
			}

			return true;
		}

		private static bool Do(ValueMap input, ValueMap pattern, out Value match, out Value leftover)
		{
			match = null;
			leftover = null;
			if (pattern == null)
				return false;	// this only matches against another map

			return false;	// to do
		}
	}
}
